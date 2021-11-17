using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using Grasshopper.Kernel;
using Hops;

[assembly: AssemblyTitle("Hops")]
[assembly: AssemblyDescription("Out of process solving using Rhino Compute")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Robert McNeel & Associates")]
[assembly: AssemblyProduct("Hops")]
[assembly: AssemblyCopyright("Copyright ©  2021")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: Guid("82cb7266-a8bc-4d3e-99f6-b91504c7117f")]
[assembly: AssemblyVersion(GhaAssemblyInfo.AppVersion)]
[assembly: AssemblyFileVersion(GhaAssemblyInfo.AppVersion)]

namespace Hops
{
    public class GhaAssemblyInfo : GH_AssemblyInfo
    {
        public static GhaAssemblyInfo TheAssemblyInfo { get; private set; }
        public GhaAssemblyInfo()
        {
            TheAssemblyInfo = this;
        }

        public const string AppVersion = "0.10.1.0";

        public override Bitmap Icon
        {
            get
            {
                var stream = GetType().Assembly.GetManifestResourceStream("Hops.resources.Hops_24x24.png");
                return new System.Drawing.Bitmap(stream);
            }
        }

        public override string Name
        {
            get
            {
                var attr = GetType().Assembly.GetCustomAttribute<AssemblyTitleAttribute>();
                return attr.Title;
            }
        }

        public override string Description
        {
            get
            {
                var attr = GetType().Assembly.GetCustomAttribute<AssemblyDescriptionAttribute>();
                return attr.Description;
            }
        }

        public override Guid Id
        {
            get
            {
                var attr = GetType().Assembly.GetCustomAttribute<GuidAttribute>();
                return new Guid(attr.Value);
            }
        }

        public override string AuthorName
        {
            get
            {
                var attr = GetType().Assembly.GetCustomAttribute<AssemblyCompanyAttribute>();
                return attr.Company;
            }
        }

        public override string AuthorContact => "https://github.com/mcneel/compute.rhino3d";

        public override string AssemblyVersion
        {
            get
            {
                var t = typeof(GhaAssemblyInfo).Assembly.GetName().Version;
                return $"{t.Major}.{t.Minor}.{t.Build}";
            }
        }
    }
}
