using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace compute.geometry
{
    public class compute.grasshopperInfo : GH_AssemblyInfo
  {
    public override string Name
    {
        get
        {
            return "ghcompute";
        }
    }
    public override Bitmap Icon
    {
        get
        {
            //Return a 24x24 pixel bitmap to represent this GHA library.
            return null;
        }
    }
    public override string Description
    {
        get
        {
            //Return a short string describing the purpose of this GHA library.
            return "";
        }
    }
    public override Guid Id
    {
        get
        {
            return new Guid("09155073-8a71-4667-ab10-753493e60311");
        }
    }

    public override string AuthorName
    {
        get
        {
            //Return a string identifying you or your company.
            return "";
        }
    }
    public override string AuthorContact
    {
        get
        {
            //Return a string representing your preferred contact details.
            return "";
        }
    }
}
}
