using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BH.oM.RemoteCompute.RhinoCompute;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Types;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Query
    {
        public static object DefaultValue(this IGH_Param param, int depth = 0)
        {
            object defaultValue = null;

            switch (param)
            {
                case Grasshopper.Kernel.Special.GH_Relay _:
                    {
                        if (param.Sources.Count == 1)
                        {
                            var sourceParam = param.Sources[0];
                            defaultValue = DefaultValue(sourceParam, depth + 1);
                        }
                    }
                    break;
                case Grasshopper.Kernel.IGH_ContextualParameter _:
                    {
                        if (0 == depth && param.Sources.Count == 1)
                        {
                            var sourceParam = param.Sources[0];
                            defaultValue = DefaultValue(sourceParam, depth + 1);
                        }
                    }
                    break;
                case Param_Arc _:
                    break;
                case Param_Boolean paramBool:
                    if (paramBool.PersistentDataCount == 1)
                        defaultValue = paramBool.ToListOfLists().SimplifiedListOfLists();
                    break;
                case Param_Box _:
                    break;
                case Param_Brep _:
                    break;
                case Param_Circle _:
                    break;
                case Param_Colour _:
                    break;
                case Param_Complex _:
                    break;
                case Param_Culture _:
                    break;
                case Param_Curve _:
                    break;
                case Param_Field _:
                    break;
                case Param_FilePath _:
                    break;
                case Param_GenericObject _:
                    break;
                case Param_Geometry _:
                    break;
                case Param_Group _:
                    break;
                case Param_Guid _:
                    break;
                case Param_Integer paramInt:
                    defaultValue = paramInt.ToListOfLists().SimplifiedListOfLists();
                    break;
                case Param_Interval _:
                    break;
                case Param_Interval2D _:
                    break;
                case Param_LatLonLocation _:
                    break;
                case Param_Line _:
                    break;
                case Param_Matrix _:
                    break;
                case Param_Mesh _:
                    break;
                case Param_MeshFace _:
                    break;
                case Param_MeshParameters _:
                    break;
                case Param_Number paramNumber:
                    {
                        if (paramNumber.PersistentDataCount == 1)
                        {
                            defaultValue = paramNumber.ToListOfLists().SimplifiedListOfLists();
                            break;
                        }

                        break;
                    }
                //case Param_OGLShader:
                case Param_Plane paramPlane:
                    defaultValue = paramPlane.ToListOfLists().SimplifiedListOfLists();
                    break;
                case Param_Point paramPoint:
                    defaultValue = paramPoint.ToListOfLists().SimplifiedListOfLists();
                    break;
                case Param_Rectangle _:
                    break;
                //case Param_ScriptVariable _:
                case Param_String paramString:
                    break;
                case Param_StructurePath _:
                    break;
                case Param_SubD _:
                    break;
                case Param_Surface _:
                    break;
                case Param_Time _:
                    break;
                case Param_Transform _:
                    break;
                case Param_Vector paramVector:
                    if (paramVector.PersistentDataCount == 1)
                        defaultValue = paramVector.ToListOfLists().SimplifiedListOfLists();
                    break;
                case GH_NumberSlider paramSlider:
                    return paramSlider.CurrentValue;
                case GH_Panel paramPanel:
                    return paramPanel.UserText;
                case GH_ValueList paramValueList:
                    defaultValue = paramValueList.FirstSelectedItem.Value;
                    break;
            }

            if (defaultValue is GH_Number ghNumber)
                return ghNumber.Value;

            if (defaultValue is GH_Boolean ghBoolean)
                return ghBoolean.Value;

            if (param.Sources != null && param.Sources.Count == 1)
                return DefaultValue(param.Sources[0], depth + 1);

            return param.PersistentDataAsListOfLists().SimplifiedListOfLists();
        }



      

    }
}
