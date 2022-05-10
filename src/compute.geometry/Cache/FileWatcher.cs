using System.IO;
using System.Collections.Generic;

namespace compute.geometry
{
    public static class FileWatcher
    {
        static Dictionary<string, FileSystemWatcher> _filewatchers = new Dictionary<string, FileSystemWatcher>();
        static HashSet<string> _watchedFiles = new HashSet<string>();
        public static uint WatchedFileRuntimeSerialNumber { get; private set; } = 1;

        public static void RegisterFileWatcher(string path)
        {
            if (_filewatchers == null)
                _filewatchers = new Dictionary<string, FileSystemWatcher>();

            if (!File.Exists(path))
                return;

            path = Path.GetFullPath(path);
            if (_watchedFiles.Contains(path.ToLowerInvariant()))
                return;

            _watchedFiles.Add(path.ToLowerInvariant());
            string directory = Path.GetDirectoryName(path);
            if (_filewatchers.ContainsKey(directory) || !Directory.Exists(directory))
                return;

            var fsw = new FileSystemWatcher(directory);
            fsw.NotifyFilter = NotifyFilters.Attributes |
                NotifyFilters.CreationTime |
                NotifyFilters.FileName |
                NotifyFilters.LastAccess |
                NotifyFilters.LastWrite |
                NotifyFilters.Size |
                NotifyFilters.Security;
            fsw.Changed += Fsw_Changed;
            fsw.EnableRaisingEvents = true;
            _filewatchers[directory] = fsw;
        }

        private static void Fsw_Changed(object sender, FileSystemEventArgs e)
        {
            string path = e.FullPath.ToLowerInvariant();
            if (_watchedFiles.Contains(path))
                WatchedFileRuntimeSerialNumber++;
        }
    }
}
