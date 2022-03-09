using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using BH.Engine.RemoteCompute.RhinoCompute;

using Rhino.Geometry;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Types;
using GH_IO.Serialization;

using Resthopper.IO;
using Newtonsoft.Json;
using System.Linq;
using Serilog;
using System.Reflection;

namespace compute.geometry
{
    partial class GrasshopperDefinition
    {
        public string IconBitmapString()
        {
            if (!string.IsNullOrWhiteSpace(_iconString))
                return _iconString;

            System.Drawing.Bitmap bmp = null;
            if (_singularComponent != null)
            {
                bmp = _singularComponent.Icon_24x24;
            }

            if (bmp != null)
            {
                using (var ms = new MemoryStream())
                {
                    bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    byte[] bytes = ms.ToArray();
                    string rc = Convert.ToBase64String(bytes);
                    _iconString = rc;
                    return rc;
                }
            }
            return null;
        }
    }
}
