using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel;

namespace Resthopper.IO
{
    public enum GHTypeCodes
    {
        Boolean = 101,
        Point = 102, 
        Vector = 103,
        Integer = 104,
        Number = 105,
        Text = 106,
        Line = 107,
        Curve = 108,
        Circle = 109,
        PLane = 110,
        Rectangle = 111,
        Box = 112,
        Surface = 113,
        Brep = 114,
        Mesh = 115,

        Slider = 201,
        BooleanToggle = 202,

        Panel = 301

        //gh_boolean = 1,
        //gh_byte = 2,
        //gh_int = 3,
        //gh_long = 4,
        //gh_float = 5,
        //gh_double = 6,
        //gh_decimal = 7,
        //gh_DateTime = 8,
        //gh_Guid = 9,
        //gh_string = 10,
        //gh_byteArray = 20,
        //gh_doubleArray = 21,
        //gh_SystemDrawingPoint = 30,
        //gh_SystemDrawingPointF = 31,
        //gh_SystemDrawingSize = 32,
        //gh_SystemDrawngSizeF = 33,
        //gh_SystemDrawingRectangleF = 35,
        //gh_SystemDrawingColor = 36,
        //gh_SystemDrawingBitmap = 37,
        //gh_GH_IO_2dPoint = 50,
        //gh_GH_IO_3dPoint = 51,
        //gh_GH_IO_4dPoint = 52,
        //gh_GH_IO_1dInterval = 60,
        //gh_GH_IO_2dInterval = 61,
        //gh_GH_IO_line = 70,
        //gh_GH_IO_BoundingBox = 71,
        //gh_GH_IO_Plane = 72,
        //gh_GH_IO_version = 80

    }
}
