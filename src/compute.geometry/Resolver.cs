using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using compute.geometry;
using Serilog;

namespace RhinoInside
{
    public class Resolver
    {
        /// <summary>
        /// Set up an assembly resolver to load RhinoCommon and other Rhino
        /// assemblies from where Rhino is installed
        /// </summary>
        public static void Initialize()
        {
            if (System.IntPtr.Size != 8)
                throw new Exception("Only 64 bit applications can use RhinoInside");
            AppDomain.CurrentDomain.AssemblyResolve += ResolveForRhinoAssemblies;
        }

        static string _rhinoSystemDirectory;

        /// <summary>
        /// Directory used by assembly resolver to attempt load core Rhino assemblies. If not manually set,
        /// this will be determined by inspecting the registry
        /// 
        /// This is the C:/Program Files/Rhino 8/System directory on Windows
        /// This is the Rhinoceros.app/Contents/Frameworks directory on Mac
        /// </summary>
        public static string RhinoSystemDirectory
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_rhinoSystemDirectory))
                    _rhinoSystemDirectory = FindRhinoSystemDirectory();
                return _rhinoSystemDirectory;
            }
            set
            {
                _rhinoSystemDirectory = value;
            }
        }

        /// <summary>
        /// Whether or not to use the newest installation of Rhino on the system. By default the resolver will only use an
        /// installation with a matching major version.
        /// </summary>
        public static bool UseLatest { get; set; } = true;

        public static string AssemblyPathFromName(string systemDirectory, string name)
        {
            if (name == null || name.EndsWith(".resources", StringComparison.OrdinalIgnoreCase))
                return null;

            // load Microsoft.macOS in the default context as xamarin initialization requires it there
            if (name == "Microsoft.macOS")
                return null;

            // only use the plain name to resolve assemblies, not the full name.
            var assemblyName = new AssemblyName(name);
            name = assemblyName.Name;

            string path = null;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                path = Path.Combine(systemDirectory, "RhCore.framework/Resources", name + ".dll");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                path = Path.Combine(systemDirectory, "netcore", name + ".dll");
                if (!File.Exists(path))
                    path = Path.Combine(systemDirectory, name + ".dll");
                //if (!File.Exists(path))
                //{
                //    var intPath = typeof(int).Assembly.Location;
                //    string directory = System.IO.Path.GetDirectoryName(intPath);
                //    path = Path.Combine(directory, name + ".dll");
                //    if (!File.Exists(path) || name.Contains(".Drawing") || name.Contains("WindowsBase"))
                //    {
                //        int index = directory.IndexOf("NETCORE", StringComparison.OrdinalIgnoreCase);
                //        directory = directory.Substring(0, index) + "WindowsDesktop" + directory.Substring(index + "NETCORE".Length);
                //        path = Path.Combine(directory, name + ".dll");
                //    }
                //}
            }
            return path;
        }

        static Assembly ResolveForRhinoAssemblies(object sender, ResolveEventArgs args)
        {
            string path = AssemblyPathFromName(RhinoSystemDirectory, args.Name);
            if (File.Exists(path))
                return Assembly.LoadFrom(path);
            return null;
        }

        static string FindRhinoSystemDirectory()
        {
            
            var major = Assembly.GetExecutingAssembly().GetName().Version.Major;

            if (RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                string baseName = @"SOFTWARE\McNeel\Rhinoceros";
                using (var baseKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(baseName))
                {
                    string[] children = baseKey.GetSubKeyNames();
                    Array.Sort(children);
                    string versionName = "";
                    for (int i = children.Length - 1; i >= 0; i--)
                    {
                        // 20 Jan 2020 S. Baer (https://github.com/mcneel/rhino.inside/issues/248)
                        // A generic double.TryParse is failing when run under certain locales.
                        if (double.TryParse(children[i], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double d))
                        {
                            if (d < 8.0)
                                continue;

                            versionName = children[i];

                            if (!UseLatest && (int)Math.Floor(d) != major)
                                continue;

                            using (var installKey = baseKey.OpenSubKey($"{versionName}\\Install"))
                            {
                                string corePath = installKey.GetValue("CoreDllPath") as string;
                                if (System.IO.File.Exists(corePath))
                                {
                                    return System.IO.Path.GetDirectoryName(corePath);
                                }
                            }
                        }
                    }
                }
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // TODO: detect the app location
                var path = "/Applications/Rhino 8.app";
                // var path = "/Users/curtis/Library/Developer/Xcode/DerivedData/MacRhino-dalqjlsjnqqsltdayygnhqhgntxb/Build/Products/Debug/Rhinoceros.app";
                
                path = Path.Combine(path, "Contents", "Frameworks");
                if (Directory.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }

        public static bool RelaunchIfNeeded()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return false;

            const string RHCORE_LIB = "RhCore.framework/Versions/A/RhCore";
            bool found = false;
            var libPaths = Environment.GetEnvironmentVariable("DYLD_LIBRARY_PATH")?.Split(";").ToList();
            if (libPaths != null)
            {
                foreach (var libPath in libPaths)
                {
                    if (File.Exists(Path.Combine(libPath, RHCORE_LIB)))
                    {
                        // found Rhino! Let's use it.
                        RhinoSystemDirectory = libPath;
                        Console.WriteLine($"Using Rhino from {RhinoSystemDirectory}");
                        found = true;
                        break;
                    }
                }
            }
            else
            {
                Console.WriteLine("DYLD_LIBRARY_PATH is null");
            }
            
            if (!found)
            {
                Console.WriteLine("DYLD_LIBRARY_PATH not set, launching as child process");
                
                string systemDirectory = RhinoSystemDirectory;
                if (!File.Exists(Path.Combine(systemDirectory, RHCORE_LIB)))
                {
                    Console.WriteLine("Could not find Rhino");
                    return true;
                }
                
                // executable has the same name without the .dll extension
                var executable = Assembly.GetEntryAssembly().Location;
                if (executable.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                    executable = executable.Substring(0, executable.Length - 4);
                var startInfo = new ProcessStartInfo
                {
                    UseShellExecute = false,
                    FileName = executable
                };
                foreach (var arg in Environment.GetCommandLineArgs())
                {
                    startInfo.ArgumentList.Add(arg);
                }
                startInfo.Environment.Add("DYLD_LIBRARY_PATH", systemDirectory);
                var process = Process.Start(startInfo);
                process.WaitForExit();
                return true;
            }
            return false;
        }

        public static void LoadRhino()
        {
            string systemDirectory = RhinoSystemDirectory;
            SetupXamarin(systemDirectory);
            SetupDefaultResolver(systemDirectory);
            /*
            var rhinoContext = new RhinoLoadContext(systemDirectory);
            // load System.Drawing in the Rhino context so it doesn't fall back to default when loading System.Drawing.Primitives
            var systemDrawingPath = FindSystemAssembly("System.Drawing.dll");
            if (systemDrawingPath != null)
                rhinoContext.LoadFromAssemblyPath(systemDrawingPath);
            */

            nint rhinoLibraryHandle = 0;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                rhinoLibraryHandle = NativeLibrary.Load(Path.Combine(systemDirectory, "RhinoLibrary.dll"));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                rhinoLibraryHandle = NativeLibrary.Load(Path.Combine(systemDirectory, "RhinoLibrary.framework/Versions/A/RhinoLibrary"));
                AssemblyLoadContext.Default.ResolvingUnmanagedDll += ResolvingUnmanagedDll;
            }
            else
            {
                throw new Exception("Unsupported platform");
            }

            IntPtr handle = NativeLibrary.GetExport(rhinoLibraryHandle, "RhLibRegisterDotNetInitializer");
            var setLoaderProc = Marshal.GetDelegateForFunctionPointer<SetLoaderProc>(handle);

            //Action load = () => ExecuteLoadProc(rhinoContext);
            Action load = () => ExecuteLoadProc(AssemblyLoadContext.Default);
            GCHandle functionPointer = GCHandle.Alloc(load);
            setLoaderProc(load);
        }

        private static IntPtr ResolvingUnmanagedDll(Assembly assembly, string unmanagedDllName)
        {
            var systemDirectory = RhinoSystemDirectory;
            if (unmanagedDllName == "RhinoLibrary")
                return NativeLibrary.Load(Path.Combine(systemDirectory, "RhinoLibrary.framework/Versions/A/RhinoLibrary"));

            return IntPtr.Zero;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate void SetLoaderProc(Action p);

        delegate int GetCLRRuntimeHost(ref Guid ptr, out IntPtr handle);
        static void SetupXamarin(string frameworks)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var libCoreClr = NativeLibrary.Load("libcoreclr.dylib");
                var getClrRuntimeHostPtr = NativeLibrary.GetExport(libCoreClr, "GetCLRRuntimeHost");
                var getClrRuntimeHost = Marshal.GetDelegateForFunctionPointer<GetCLRRuntimeHost>(getClrRuntimeHostPtr);
                // var hostId = new Guid(0x90F1A06C, 0x7712, 0x4762, 0x86, 0xB5, 0x7A, 0x5E, 0xBA, 0x6B, 0xDB, 0x02);
                var hostId4 = new Guid(0x64F6D366, 0xD7C2, 0x4F1F, 0xB4, 0xB2, 0xE8, 0x16, 0x0C, 0xAC, 0x43, 0xAF);
                var result = getClrRuntimeHost(ref hostId4, out var coreclr_handle);
                if (result != 0 || coreclr_handle == IntPtr.Zero)
                    throw new InvalidOperationException("Could not get CLR Runtime Host");

                // set handle/domain id for xamarin.mac to use
                var libXamarin = NativeLibrary.Load(Path.Combine(frameworks, "libxamarin-dotnet-coreclr.dylib"));
                var coreClrHandlePtr = NativeLibrary.GetExport(libXamarin, "coreclr_handle");
                var coreClrDomainIdPtr = NativeLibrary.GetExport(libXamarin, "coreclr_domainId");
                Marshal.WriteIntPtr(coreClrHandlePtr, coreclr_handle);
                Marshal.WriteInt32(coreClrDomainIdPtr, AppDomain.CurrentDomain.Id);
            }
        }

        static void ExecuteLoadProc(AssemblyLoadContext context)
        {
            var assembly = context.LoadFromAssemblyName(new AssemblyName("dotnetstart"));
            Type? programType = assembly?.GetType("dotnetstart.DotNetInitialization");
            MethodInfo? method = programType?.GetMethod("Start");
            method?.Invoke(null, new object[] { "headless" });
        }

        private static void SetupDefaultResolver(string systemDirectory)
        {
            AssemblyLoadContext.Default.Resolving += (ctx, arg) =>
            {
                string path = AssemblyPathFromName(systemDirectory, arg.Name);
                if (File.Exists(path))
                    return ctx.LoadFromAssemblyPath(path);

                return null;
            };
        }
    }
    /*
    class RhinoLoadContext : AssemblyLoadContext
    {
        string _systemDirectory;
        public RhinoLoadContext(string systemDirectory) : base("Rhino")
        {
            _systemDirectory = systemDirectory;
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            string path = Resolver.AssemblyPathFromName(_systemDirectory, assemblyName.Name);
            if (File.Exists(path))
                return LoadFromAssemblyPath(path);

            return null;
        }

        static List<(string prefix, string suffix)> s_nativeVariations = new List<(string prefix, string suffix)>
        {
            ("lib", ".dylib"),
            (string.Empty, ".dylib"),
            ("lib", string.Empty),
            (string.Empty, string.Empty)
        };


        private IntPtr FindUnmanagedDll(string unmanagedDllName)
        {
            foreach (var variation in s_nativeVariations)
            {
                var path = Path.Combine(_systemDirectory, variation.prefix + unmanagedDllName + variation.suffix);
                if (File.Exists(path) && NativeLibrary.TryLoad(path, out var handle))
                {
                    s_unmanagedMap.TryAdd(unmanagedDllName, handle);
                    // Console.WriteLine($"Loading native library {unmanagedDllName} from {path}");
                    return handle;
                }
            }
            return IntPtr.Zero;
        }

        static ConcurrentDictionary<string, IntPtr> s_unmanagedMap = new ConcurrentDictionary<string, IntPtr>();
        protected override nint LoadUnmanagedDll(string unmanagedDllName) => s_unmanagedMap.GetOrAdd(unmanagedDllName, FindUnmanagedDll);
    }
*/
}
