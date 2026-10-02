using System;
using System.IO;
using System.Runtime.InteropServices;

namespace compute.geometry
{
    // Where compute.meter.agent listens: a named pipe on Windows, a Unix domain socket on Linux. Also compiled into
    // rhino.compute.
    static class MeterAgentPipe
    {
        public const string PIPE_NAME = "compute.meter.agent";
        public const string SOCKET_PATH = "/run/rhino-compute/meter.sock";

        // Checks without connecting: opening a pipe's path would take one of the agent's connections.
        public static bool Listening()
        {
            if (OperatingSystem.IsWindows())
                return WaitNamedPipe($@"\\.\pipe\{PIPE_NAME}", 1) || Marshal.GetLastWin32Error() == ERROR_SEM_TIMEOUT;
            return OperatingSystem.IsLinux() && File.Exists(SOCKET_PATH);
        }

        const int ERROR_SEM_TIMEOUT = 121;

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool WaitNamedPipe(string name, uint timeout);
    }
}
