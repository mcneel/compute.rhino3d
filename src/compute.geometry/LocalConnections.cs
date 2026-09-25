using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;

namespace compute.geometry
{
    // Which process opened a TCP connection that arrived from this machine: the owner of its client end,
    // from the TCP table on Windows and the socket's inode in /proc on Linux. Also compiled into rhino.compute.
    static class LocalConnections
    {
        public static bool FromThisMachine(IPAddress remote)
        {
            if (remote == null)
                return false;
            return IPAddress.IsLoopback(remote.IsIPv4MappedToIPv6 ? remote.MapToIPv4() : remote);
        }

        // For a connection from clientPort to serverPort on this machine: the candidate that opened it, or when
        // orStartedBy, the candidate that started (directly or not) the process that opened it. Null otherwise.
        public static int? OpenedBy(int clientPort, int serverPort, ICollection<int> candidates, bool orStartedBy)
        {
            try
            {
                if (OperatingSystem.IsWindows())
                    return WindowsOpenedBy(clientPort, serverPort, candidates, orStartedBy);
                if (OperatingSystem.IsLinux())
                    return LinuxOpenedBy(clientPort, serverPort, candidates, orStartedBy);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is FormatException)
            {
            }
            return null;
        }

        static int? WindowsOpenedBy(int clientPort, int serverPort, ICollection<int> candidates, bool orStartedBy)
        {
            int owner = WindowsOwner(clientPort, serverPort);
            if (owner == 0)
                return null;
            if (candidates.Contains(owner))
                return owner;
            if (!orStartedBy)
                return null;
            var seen = new HashSet<int>();
            for (int pid = owner; seen.Add(pid);)
            {
                if (!WindowsParent(pid, out int parent) || parent == 0)
                    return null;
                if (candidates.Contains(parent))
                    return parent;
                pid = parent;
            }
            return null;
        }

        // A parent that started after its child is a newer process reusing the id of one that has exited.
        static bool WindowsParent(int pid, out int parent)
        {
            parent = 0;
            if (!WindowsProcessInfo(pid, out int parentId, out long created) || !WindowsProcessInfo(parentId, out _, out long parentCreated))
                return false;
            if (parentCreated > created)
                return false;
            parent = parentId;
            return true;
        }

        static bool WindowsProcessInfo(int pid, out int parent, out long created)
        {
            parent = 0;
            created = 0;
            IntPtr process = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, false, pid);
            if (process == IntPtr.Zero)
                return false;
            try
            {
                var info = new ProcessBasicInformation();
                if (NtQueryInformationProcess(process, 0, ref info, Marshal.SizeOf<ProcessBasicInformation>(), out _) != 0 ||
                    !GetProcessTimes(process, out created, out _, out _, out _))
                    return false;
                parent = (int)info.InheritedFromUniqueProcessId;
                return true;
            }
            finally
            {
                CloseHandle(process);
            }
        }

        const int AF_INET = 2;
        const int AF_INET6 = 23;
        const int TCP_TABLE_OWNER_PID_CONNECTIONS = 4;
        const int ERROR_INSUFFICIENT_BUFFER = 122;

        // MIB_TCPROW_OWNER_PID and MIB_TCP6ROW_OWNER_PID layouts; ports are in network byte order.
        static readonly (int Family, int RowSize, int LocalPort, int RemotePort, int Pid)[] tcpTables =
        {
            (AF_INET, 24, 8, 16, 20),
            (AF_INET6, 56, 20, 44, 52),
        };

        static int WindowsOwner(int clientPort, int serverPort)
        {
            foreach (var layout in tcpTables)
            {
                int size = 0;
                int result = GetExtendedTcpTable(IntPtr.Zero, ref size, false, layout.Family, TCP_TABLE_OWNER_PID_CONNECTIONS, 0);
                IntPtr table = IntPtr.Zero;
                try
                {
                    while (result == ERROR_INSUFFICIENT_BUFFER)
                    {
                        Marshal.FreeHGlobal(table);
                        table = Marshal.AllocHGlobal(size);
                        result = GetExtendedTcpTable(table, ref size, false, layout.Family, TCP_TABLE_OWNER_PID_CONNECTIONS, 0);
                    }
                    if (result != 0 || table == IntPtr.Zero)
                        continue;
                    int count = Marshal.ReadInt32(table);
                    for (int i = 0; i < count; i++)
                    {
                        IntPtr row = table + 4 + i * layout.RowSize;
                        if (Port(row, layout.LocalPort) == clientPort && Port(row, layout.RemotePort) == serverPort)
                            return Marshal.ReadInt32(row, layout.Pid);
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(table);
                }
            }
            return 0;
        }

        static int Port(IntPtr row, int offset) => (Marshal.ReadByte(row, offset) << 8) | Marshal.ReadByte(row, offset + 1);

        static int? LinuxOpenedBy(int clientPort, int serverPort, ICollection<int> candidates, bool orStartedBy)
        {
            string socket = LinuxSocket(clientPort, serverPort);
            if (socket == null)
                return null;
            foreach (int pid in candidates)
            {
                if (LinuxOwns(pid, socket))
                    return pid;
            }
            if (!orStartedBy)
                return null;

            var parents = new Dictionary<int, int>();
            foreach (string dir in Directory.GetDirectories("/proc"))
            {
                if (int.TryParse(Path.GetFileName(dir), out int pid) && LinuxParent(pid) is int parent)
                    parents[pid] = parent;
            }
            foreach (var process in parents)
            {
                var seen = new HashSet<int>();
                for (int pid = process.Key; parents.TryGetValue(pid, out int parent) && parent != 0 && seen.Add(pid); pid = parent)
                {
                    if (candidates.Contains(parent))
                    {
                        if (LinuxOwns(process.Key, socket))
                            return parent;
                        break;
                    }
                }
            }
            return null;
        }

        // "socket:[inode]" for the client end, from /proc/net/tcp and tcp6 (addresses are hex, ports big-endian).
        static string LinuxSocket(int clientPort, int serverPort)
        {
            foreach (string file in new[] { "/proc/net/tcp", "/proc/net/tcp6" })
            {
                if (!File.Exists(file))
                    continue;
                foreach (string line in File.ReadLines(file))
                {
                    string[] fields = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (fields.Length < 10 || !fields[1].Contains(':'))
                        continue;
                    if (HexPort(fields[1]) == clientPort && HexPort(fields[2]) == serverPort && fields[9] != "0")
                        return $"socket:[{fields[9]}]";
                }
            }
            return null;
        }

        static int HexPort(string address) =>
            int.TryParse(address.Substring(address.LastIndexOf(':') + 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int port) ? port : -1;

        static bool LinuxOwns(int pid, string socket)
        {
            try
            {
                foreach (string fd in Directory.GetFiles($"/proc/{pid}/fd"))
                {
                    if (new FileInfo(fd).LinkTarget == socket)
                        return true;
                }
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
            }
            return false;
        }

        // /proc/<pid>/stat: the parent id is the second field after "(comm)".
        static int? LinuxParent(int pid)
        {
            try
            {
                string stat = File.ReadAllText($"/proc/{pid}/stat");
                string[] fields = stat.Substring(stat.LastIndexOf(')') + 2).Split(' ');
                return int.Parse(fields[1], CultureInfo.InvariantCulture);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is FormatException || ex is IndexOutOfRangeException || ex is ArgumentOutOfRangeException)
            {
                return null;
            }
        }

        const uint PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;

        [StructLayout(LayoutKind.Sequential)]
        struct ProcessBasicInformation
        {
            public IntPtr ExitStatus;
            public IntPtr PebBaseAddress;
            public IntPtr AffinityMask;
            public IntPtr BasePriority;
            public IntPtr UniqueProcessId;
            public IntPtr InheritedFromUniqueProcessId;
        }

        [DllImport("iphlpapi.dll")]
        static extern int GetExtendedTcpTable(IntPtr table, ref int size, bool sort, int family, int tableClass, int reserved);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr OpenProcess(uint access, bool inheritHandle, int processId);

        [DllImport("ntdll.dll")]
        static extern int NtQueryInformationProcess(IntPtr process, int infoClass, ref ProcessBasicInformation info, int length, out int returnLength);

        [DllImport("kernel32.dll")]
        static extern bool GetProcessTimes(IntPtr process, out long creation, out long exit, out long kernel, out long user);

        [DllImport("kernel32.dll")]
        static extern bool CloseHandle(IntPtr handle);
    }
}
