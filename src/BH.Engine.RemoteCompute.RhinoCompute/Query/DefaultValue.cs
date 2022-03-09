using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Query
    {
        public static object DefaultValue(this IGH_Param param, int depth = 0)
        {
            switch (param)
            {
                case Grasshopper.Kernel.Special.GH_Relay _:
                    {
                        if (param.Sources.Count == 1)
                        {
                            var sourceParam = param.Sources[0];
                            return DefaultValue(sourceParam, depth + 1);
                        }
                    }
                    break;
                case Grasshopper.Kernel.IGH_ContextualParameter _:
                    {
                        if (0 == depth && param.Sources.Count == 1)
                        {
                            var sourceParam = param.Sources[0];
                            return DefaultValue(sourceParam, depth + 1);
                        }
                    }
                    break;
                case Grasshopper.Kernel.Parameters.Param_Arc _:
                    break;
                case Grasshopper.Kernel.Parameters.Param_Boolean paramBool:
                    if (paramBool.PersistentDataCount == 1)
                        return paramBool.PersistentData[0][0].Value;
                    break;
                case Grasshopper.Kernel.Parameters.Param_Box _:
                    break;
                case Grasshopper.Kernel.Parameters.Param_Brep _:
                    break;
                case Grasshopper.Kernel.Parameters.Param_Circle _:
                    break;
                case Grasshopper.Kernel.Parameters.Param_Colour _:
                    break;
                case Grasshopper.Kernel.Parameters.Param_Complex _:
                    break;
                case Grasshopper.Kernel.Parameters.Param_Culture _:
                    break;
                case Grasshopper.Kernel.Parameters.Param_Curve _:
                    break;
                case Grasshopper.Kernel.Parameters.Param_Field _:
                    break;
                case Grasshopper.Kernel.Parameters.Param_FilePath _:
                    break;
                case Grasshopper.Kernel.Parameters.Param_GenericObject _:
                    break;
                case Grasshopper.Kernel.Parameters.Param_Geometry _:
                    break;
                case Grasshopper.Kernel.Parameters.Param_Group _:
                    break;
                case Grasshopper.Kernel.Parameters.Param_Guid _:
                    break;
                case Grasshopper.Kernel.Parameters.Param_Integer paramInt:
                    if (paramInt.PersistentDataCount == 1)
                        return paramInt.PersistentData[0][0].Value;
                    break;
                case Grasshopper.Kernel.Parameters.Param_Interval _:
                    break;
                case Grasshopper.Kernel.Parameters.Param_Interval2D _:
                    break;
                case Grasshopper.Kernel.Parameters.Param_LatLonLocation _:
                    break;
                case Grasshopper.Kernel.Parameters.Param_Line _:
                    break;
                case Grasshopper.Kernel.Parameters.Param_Matrix _:
                    break;
                case Grasshopper.Kernel.Parameters.Param_Mesh _:
                    break;
                case Grasshopper.Kernel.Parameters.Param_MeshFace _:
                    break;
                case Grasshopper.Kernel.Parameters.Param_MeshParameters _:
                    break;
                case Grasshopper.Kernel.Parameters.Param_Number paramNumber:
                    if (paramNumber.PersistentDataCount == 1)
                        return paramNumber.PersistentData[0][0].Value;
                    break;
                //case Grasshopper.Kernel.Parameters.Param_OGLShader:
                case Grasshopper.Kernel.Parameters.Param_Plane paramPlane:
                    if (paramPlane.PersistentDataCount == 1)
                        return paramPlane.PersistentData[0][0].Value;
                    break;
                case Grasshopper.Kernel.Parameters.Param_Point paramPoint:
                    if (paramPoint.PersistentDataCount == 1)
                        return paramPoint.PersistentData[0][0].Value;
                    break;
                case Grasshopper.Kernel.Parameters.Param_Rectangle _:
                    break;
                //case Grasshopper.Kernel.Parameters.Param_ScriptVariable _:
                case Grasshopper.Kernel.Parameters.Param_String paramString:
                    if (paramString.PersistentDataCount == 1)
                        return paramString.PersistentData[0][0].Value;
                    break;
                case Grasshopper.Kernel.Parameters.Param_StructurePath _:
                    break;
                case Grasshopper.Kernel.Parameters.Param_SubD _:
                    break;
                case Grasshopper.Kernel.Parameters.Param_Surface _:
                    break;
                case Grasshopper.Kernel.Parameters.Param_Time _:
                    break;
                case Grasshopper.Kernel.Parameters.Param_Transform _:
                    break;
                case Grasshopper.Kernel.Parameters.Param_Vector paramVector:
                    if (paramVector.PersistentDataCount == 1)
                        return paramVector.PersistentData[0][0].Value;
                    break;
                case Grasshopper.Kernel.Special.GH_NumberSlider paramSlider:
                    return paramSlider.CurrentValue;
                case Grasshopper.Kernel.Special.GH_Panel paramPanel:
                    return paramPanel.UserText;
                case Grasshopper.Kernel.Special.GH_ValueList paramValueList:
                    return paramValueList.FirstSelectedItem.Value;
            }
            return null;
        }
    }
}
