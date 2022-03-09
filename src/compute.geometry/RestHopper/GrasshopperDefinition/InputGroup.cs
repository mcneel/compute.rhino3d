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
        class InputGroup
        {
            object _default = null;
            public IGH_Param Param { get; }

            public InputGroup(IGH_Param param)
            {
                Param = param;
                _default = param.DefaultValue();
                if (_default is GH_Number ghNumber)
                {
                    _default = ghNumber.Value;
                }
                else if (_default is GH_Boolean ghBoolean)
                {
                    _default = ghBoolean.Value;
                }
            }

            public string GetDescription()
            {
                IGH_ContextualParameter contextualParameter = Param as IGH_ContextualParameter;
                if (contextualParameter != null)
                {
                    return contextualParameter.Prompt;
                }
                return null;
            }

            public int GetAtLeast()
            {
                IGH_ContextualParameter contextualParameter = Param as IGH_ContextualParameter;
                if (contextualParameter != null)
                {
                    return contextualParameter.AtLeast;
                }
                return 1;
            }

            public int GetAtMost()
            {
                IGH_ContextualParameter contextualParameter = Param as IGH_ContextualParameter;
                if (contextualParameter != null)
                {
                    return contextualParameter.AtMost;
                }
                if (Param is GH_NumberSlider)
                    return 1;
                return int.MaxValue;
            }

            public object GetDefault()
            {
                return _default;
            }

            public object GetMinimum()
            {
                var p = Param;
                if (p is IGH_ContextualParameter && p.Sources.Count == 1)
                {
                    p = p.Sources[0];
                }

                if (p is GH_NumberSlider paramSlider)
                    return paramSlider.Slider.Minimum;
                return null;
            }

            public object GetMaximum()
            {
                var p = Param;
                if (p is IGH_ContextualParameter && p.Sources.Count == 1)
                {
                    p = p.Sources[0];
                }

                if (p is GH_NumberSlider paramSlider)
                    return paramSlider.Slider.Maximum;

                return null;
            }

            public bool AlreadySet(DataTree<ResthopperObject> tree)
            {
                if (_tree == null)
                    return false;

                var oldDictionary = _tree.InnerTree;
                var newDictionary = tree.InnerTree;

                if (!oldDictionary.Keys.SequenceEqual(newDictionary.Keys))
                {
                    return false;
                }

                foreach (var kvp in oldDictionary)
                {
                    var oldValue = kvp.Value;
                    if (!newDictionary.TryGetValue(kvp.Key, out List<ResthopperObject> newValue))
                        return false;

                    if (!newValue.SequenceEqual(oldValue))
                    {
                        return false;
                    }
                }

                return true;
            }

            public void CacheTree(DataTree<ResthopperObject> tree)
            {
                _tree = tree;
            }

            DataTree<ResthopperObject> _tree;
        }
    }
}
