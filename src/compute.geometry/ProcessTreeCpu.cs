using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using Serilog;

namespace compute.geometry
{
    // CPU time of this process plus the processes it starts: its own Job Object on Windows, waited-for
    // children on Linux, otherwise this process alone.
    static class ProcessTreeCpu
    {
        const int JOB_OBJECT_BASIC_ACCOUNTING_INFORMATION = 1;
        const double LINUX_CLOCK_TICKS_PER_SECOND = 100;

        static IntPtr job = IntPtr.Zero;

        public static void Initialize()
        {
            if (!OperatingSystem.IsWindows() || job != IntPtr.Zero)
                return;
            IntPtr created = CreateJobObject(IntPtr.Zero, null);
            if (created != IntPtr.Zero && AssignProcessToJobObject(created, GetCurrentProcess()))
            {
                job = created;
                return;
            }
            Log.Warning("Metering: could not place compute.geometry in a Job Object (error {Error}); CPU of processes it starts won't be counted", Marshal.GetLastWin32Error());
            if (created != IntPtr.Zero)
                CloseHandle(created);
        }

        public static TimeSpan Total()
        {
            try
            {
                if (job != IntPtr.Zero && QueryInformationJobObject(job, JOB_OBJECT_BASIC_ACCOUNTING_INFORMATION, out var info, Marshal.SizeOf<JobAccounting>(), IntPtr.Zero))
                    return TimeSpan.FromTicks(info.TotalUserTime + info.TotalKernelTime);
                if (OperatingSystem.IsLinux())
                    return LinuxTotal();
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is FormatException)
            {
            }
            return Environment.CpuUsage.TotalTime;
        }

        // /proc/self/stat fields after "(comm)": utime, stime, cutime and cstime are at 11..14.
        static TimeSpan LinuxTotal()
        {
            string stat = File.ReadAllText("/proc/self/stat");
            string[] fields = stat.Substring(stat.LastIndexOf(')') + 2).Split(' ');
            long ticks = 0;
            for (int i = 11; i <= 14; i++)
                ticks += long.Parse(fields[i], CultureInfo.InvariantCulture);
            return TimeSpan.FromSeconds(ticks / LINUX_CLOCK_TICKS_PER_SECOND);
        }

        [StructLayout(LayoutKind.Sequential)]
        struct JobAccounting
        {
            public long TotalUserTime;
            public long TotalKernelTime;
            public long ThisPeriodTotalUserTime;
            public long ThisPeriodTotalKernelTime;
            public uint TotalPageFaultCount;
            public uint TotalProcesses;
            public uint ActiveProcesses;
            public uint TotalTerminatedProcesses;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern IntPtr CreateJobObject(IntPtr jobAttributes, string name);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AssignProcessToJobObject(IntPtr job, IntPtr process);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool QueryInformationJobObject(IntPtr job, int infoClass, out JobAccounting info, int length, IntPtr returnLength);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll")]
        static extern bool CloseHandle(IntPtr handle);
    }
}
