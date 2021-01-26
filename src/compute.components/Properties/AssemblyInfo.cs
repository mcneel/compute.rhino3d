using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Grasshopper.Kernel;
using System.Drawing;
using Compute.Components;

[assembly: AssemblyTitle("Compute Components")]
[assembly: AssemblyDescription("Out of process solving using Rhino Compute")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Robert McNeel & Associates")]
[assembly: AssemblyProduct("Compute.Components")]
[assembly: AssemblyCopyright("Copyright ©  2020")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: Guid("82cb7266-a8bc-4d3e-99f6-b91504c7117f")]
[assembly: AssemblyVersion(GhaAssemblyInfo.AppVersion)]
[assembly: AssemblyFileVersion(GhaAssemblyInfo.AppVersion)]

namespace Compute.Components
{
    public class GhaAssemblyInfo : GH_AssemblyInfo
    {
        public const string AppVersion = "7.0.0.0";

        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
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
