using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Eto.Drawing;
using Eto.Forms;

namespace Hops
{
    /// <summary>
    /// Mac stand-in for the rhino.compute console window on Windows: shows the worker's output,
    /// closes when the worker exits, and stops the worker when closed.
    /// </summary>
    class WorkerLogWindow : Form
    {
        // Caps memory over a long session; trimming keeps the newest half.
        const int MaxChars = 1_000_000;
        static readonly Regex AnsiEscape = new Regex(@"\x1B\[([0-9;?]*)([A-Za-z])", RegexOptions.Compiled);
        // Dark like a terminal, since rhino.compute's colours assume a dark background.
        static readonly Color DefaultBackground = Color.FromRgb(0x1E1E1E);
        static readonly Color DefaultForeground = Color.FromRgb(0xCCCCCC);

        readonly Process process;
        readonly ConcurrentQueue<string> pending;
        readonly RichTextArea textArea;
        readonly UITimer flushTimer;
        // Colours carry over between lines, as in a terminal.
        Color? foreground;
        Color? background;
        int length;
        bool closed;

        /// <summary>Use in place of Process.Start to launch rhino.compute with its log window.</summary>
        public static Process Start(ProcessStartInfo startInfo)
        {
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
            // Output starts before the window exists, so buffer it until the window picks it up.
            var pending = new ConcurrentQueue<string>();
            process.OutputDataReceived += (s, e) => { if (e.Data != null) pending.Enqueue(e.Data); };
            process.ErrorDataReceived += (s, e) => { if (e.Data != null) pending.Enqueue(e.Data); };
            if (!process.Start())
                return null;
            // Reading must start right away, or the worker blocks once the pipe buffer fills.
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            Application.Instance.AsyncInvoke(() =>
            {
                var window = new WorkerLogWindow(process, pending);
                window.Show();
                window.UpdateLevel();
                // Matches Windows, where the console starts minimized so it doesn't cover Rhino.
                // Must come after Show; on Mac it's ignored before the window is visible.
                window.WindowState = WindowState.Minimized;
            });
            return process;
        }

        WorkerLogWindow(Process process, ConcurrentQueue<string> pending)
        {
            this.process = process;
            this.pending = pending;
            Title = "rhino.compute";
            ClientSize = new Size(800, 400);
            Resizable = true;
            Minimizable = true;
            Maximizable = true;
            ShowInTaskbar = true;

            textArea = new RichTextArea
            {
                ReadOnly = true,
                Wrap = false,
                Font = Fonts.Monospace(11),
                BackgroundColor = DefaultBackground,
                TextColor = DefaultForeground
            };
            Content = textArea;

            flushTimer = new UITimer { Interval = 0.1 };
            flushTimer.Elapsed += (s, e) => Flush();
            flushTimer.Start();

            process.Exited += (s, e) => Application.Instance.AsyncInvoke(CloseForExit);
            if (process.HasExited)
                Application.Instance.AsyncInvoke(CloseForExit);

            Application.Instance.IsActiveChanged += Application_IsActiveChanged;
        }

        void Application_IsActiveChanged(object sender, EventArgs e) => UpdateLevel();

        // Grasshopper's window floats above normal windows while Rhino is active, which hid this one
        // behind it. Float at the same level so clicking either brings it forward, and drop back
        // when Rhino isn't active so it doesn't cover other apps, as Grasshopper does.
        void UpdateLevel()
        {
            const long normalLevel = 0;
            const long floatingLevel = 3;
            // Hops doesn't reference the macOS bindings, so set the NSWindow properties by name.
            object nsWindow = ControlObject;
            SetEnumProperty(nsWindow, "Level", Application.Instance.IsActive ? floatingLevel : normalLevel);
            // Floating windows otherwise drop out of Cmd+` cycling and Mission Control.
            const long managed = 1 << 2;
            const long participatesInCycle = 1 << 5;
            SetEnumProperty(nsWindow, "CollectionBehavior", managed | participatesInCycle);
        }

        static void SetEnumProperty(object target, string name, long value)
        {
            var property = target?.GetType().GetProperty(name);
            if (property != null && property.PropertyType.IsEnum)
                property.SetValue(target, Enum.ToObject(property.PropertyType, value));
        }

        void Flush()
        {
            if (pending.IsEmpty)
                return;
            var text = new StringBuilder();
            var runs = new List<(int start, int length, Color? fg, Color? bg)>();
            while (pending.TryDequeue(out string line))
            {
                AppendLine(line, text, runs);
            }

            var buffer = textArea.Buffer;
            textArea.Append(text.ToString(), false);
            foreach (var run in runs)
            {
                var range = Range.FromLength(length + run.start, run.length);
                buffer.SetForeground(range, run.fg ?? DefaultForeground);
                if (run.bg != null)
                    buffer.SetBackground(range, run.bg.Value);
            }
            length += text.Length;
            if (length > MaxChars)
            {
                // Cut at a line break so the first line shown isn't a partial one.
                string all = textArea.Text;
                int cut = all.IndexOf('\n', all.Length - MaxChars / 2) + 1;
                buffer.Delete(Range.FromLength(0, cut));
                length -= cut;
            }
            textArea.ScrollToEnd();
        }

        void AppendLine(string line, StringBuilder text, List<(int, int, Color?, Color?)> runs)
        {
            int pos = 0;
            foreach (Match match in AnsiEscape.Matches(line))
            {
                AddRun(line.Substring(pos, match.Index - pos), text, runs);
                if (match.Groups[2].Value == "m")
                    ApplySgr(match.Groups[1].Value);
                pos = match.Index + match.Length;
            }
            AddRun(line.Substring(pos) + "\n", text, runs);
        }

        void AddRun(string segment, StringBuilder text, List<(int, int, Color?, Color?)> runs)
        {
            if (segment.Length == 0)
                return;
            runs.Add((text.Length, segment.Length, foreground, background));
            text.Append(segment);
        }

        // Handles the colour codes rhino.compute's logging uses: 16, 256 and 24-bit colours plus resets.
        void ApplySgr(string parameters)
        {
            var codes = parameters.Length == 0 ? new[] { "0" } : parameters.Split(';');
            for (int i = 0; i < codes.Length; i++)
            {
                int.TryParse(codes[i], out int code);
                if (code == 0) { foreground = null; background = null; }
                else if (code >= 30 && code <= 37) foreground = Palette(code - 30);
                else if (code >= 90 && code <= 97) foreground = Palette(code - 90 + 8);
                else if (code == 39) foreground = null;
                else if (code >= 40 && code <= 47) background = Palette(code - 40);
                else if (code >= 100 && code <= 107) background = Palette(code - 100 + 8);
                else if (code == 49) background = null;
                else if (code == 38 || code == 48)
                {
                    Color? color = null;
                    if (i + 2 < codes.Length && codes[i + 1] == "5")
                    {
                        int.TryParse(codes[i + 2], out int index);
                        color = Palette(index);
                        i += 2;
                    }
                    else if (i + 4 < codes.Length && codes[i + 1] == "2")
                    {
                        int.TryParse(codes[i + 2], out int r);
                        int.TryParse(codes[i + 3], out int g);
                        int.TryParse(codes[i + 4], out int b);
                        color = Color.FromArgb(r, g, b);
                        i += 4;
                    }
                    if (code == 38) foreground = color;
                    else background = color;
                }
            }
        }

        // Standard xterm 256-colour palette, with the first 16 tuned for a dark background.
        static readonly int[] Basic16 =
        {
            0x000000, 0xCD3131, 0x0DBC79, 0xE5E510, 0x2472C8, 0xBC3FBC, 0x11A8CD, 0xE5E5E5,
            0x666666, 0xF14C4C, 0x23D18B, 0xF5F543, 0x3B8EEA, 0xD670D6, 0x29B8DB, 0xFFFFFF
        };

        static Color Palette(int index)
        {
            if (index < 16)
                return Color.FromRgb(Basic16[Math.Max(0, index)]);
            if (index < 232)
            {
                index -= 16;
                int Level(int v) => v == 0 ? 0 : 55 + v * 40;
                return Color.FromArgb(Level(index / 36), Level(index / 6 % 6), Level(index % 6));
            }
            int gray = 8 + (Math.Min(index, 255) - 232) * 10;
            return Color.FromArgb(gray, gray, gray);
        }

        void CloseForExit()
        {
            if (!closed)
                Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            closed = true;
            flushTimer.Stop();
            Application.Instance.IsActiveChanged -= Application_IsActiveChanged;
            // rhino.compute's compute.geometry children shut themselves down once it's gone.
            try
            {
                if (!process.HasExited)
                    process.Kill();
            }
            catch (InvalidOperationException)
            {
                // Exited between the check and the kill.
            }
            base.OnClosed(e);
        }
    }
}
