using BH.Engine.RemoteCompute.RhinoCompute.Objects;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using BH.oM.RemoteCompute.RhinoCompute;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Query
    {
        public static object DefaultValue(this InputGroup inputGroup)
        {
            return inputGroup.Param.DefaultValue();
        }

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
                case Grasshopper.Kernel.Parameters.Param_Arc _:
                    break;
                case Grasshopper.Kernel.Parameters.Param_Boolean paramBool:
                    if (paramBool.PersistentDataCount == 1)
                        defaultValue = paramBool.PersistentData[0][0].Value;
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
                        defaultValue = paramInt.PersistentData[0][0].Value;
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
                        defaultValue = paramNumber.PersistentData[0][0].Value;
                    break;
                //case Grasshopper.Kernel.Parameters.Param_OGLShader:
                case Grasshopper.Kernel.Parameters.Param_Plane paramPlane:
                    if (paramPlane.PersistentDataCount == 1)
                        defaultValue = paramPlane.PersistentData[0][0].Value;
                    break;
                case Grasshopper.Kernel.Parameters.Param_Point paramPoint:
                    if (paramPoint.PersistentDataCount == 1)
                        defaultValue = paramPoint.PersistentData[0][0].Value;
                    break;
                case Grasshopper.Kernel.Parameters.Param_Rectangle _:
                    break;
                //case Grasshopper.Kernel.Parameters.Param_ScriptVariable _:
                case Grasshopper.Kernel.Parameters.Param_String paramString:
                    if (paramString.PersistentDataCount == 1)
                        defaultValue = paramString.PersistentData[0][0].Value;
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
                        defaultValue = paramVector.PersistentData[0][0].Value;
                    break;
                case Grasshopper.Kernel.Special.GH_NumberSlider paramSlider:
                    defaultValue = paramSlider.CurrentValue;
                    break;
                case Grasshopper.Kernel.Special.GH_Panel paramPanel:
                    defaultValue = paramPanel.UserText;
                    break;
                case Grasshopper.Kernel.Special.GH_ValueList paramValueList:
                    defaultValue = paramValueList.FirstSelectedItem.Value;
                    break;
            }

            if (defaultValue is GH_Number ghNumber)
            {
                defaultValue = ghNumber.Value;
            }
            else if (defaultValue is GH_Boolean ghBoolean)
            {
                defaultValue = ghBoolean.Value;
            }

            return defaultValue;
        }
    }
}
