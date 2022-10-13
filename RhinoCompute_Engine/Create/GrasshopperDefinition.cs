using System;
using BH.Engine.Computing;
using BH.oM.Computing;
using BH.oM.Computing.RhinoCompute;
using Grasshopper.Kernel;

namespace BH.Engine.Computing.RhinoCompute
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static GrasshopperDefinition GrasshopperDefinitionFromBase64String(string base64string, GHScriptConfig gHScriptConfig)
        {
            string cacheKey = base64string.CacheKey();

            GrasshopperDefinition grasshopperDefinition = base64string.ToGrasshopperDefinition(gHScriptConfig);

            return grasshopperDefinition;
        }

        /***************************************************/

        public static GrasshopperDefinition GrasshopperDefinitionFromUri(Uri scriptUrl, GHScriptConfig gHScriptConfig)
        {
            if (scriptUrl == null)
                return null;

            string urlString = scriptUrl.ToString();

            // First check if the definition has been downloaded previously and is present in cache.
            GrasshopperDefinition gDef = null;
            if (DataCache.TryGetCachedDefinition(urlString, out gDef))
            {
                Log.RecordNote("Using cached definition");
                return gDef;
            }

            var archive = BH.Engine.Computing.RhinoCompute.Compute.GHArchiveFromUrl(scriptUrl);
            if (archive == null)
                return null;

            gDef = archive.ToGrasshopperDefinition(gHScriptConfig);

            return gDef;
        }

        /***************************************************/

        public static GrasshopperDefinition GrasshopperDefinitionFromComponent(Guid componentId, GHScriptConfig gHScriptConfig)
        {
            GH_Component component = Grasshopper.Instances.ComponentServer.EmitObject(componentId) as GH_Component;
            if (component == null)
                return null;

            GH_Document gh_document = new GH_Document();
            gh_document.AddObject(component, false);

            try
            {
                // raise DocumentServer.DocumentAdded event (used by some plug-ins)
                Grasshopper.Instances.DocumentServer.AddDocument(gh_document);
            }
            catch (Exception e)
            {
                BH.Engine.Computing.Log.RecordWarning($"Exception in DocumentAdded event handler:\n\t{e.Message}");
            }

            GrasshopperDefinition rc = new GrasshopperDefinition(gh_document, gHScriptConfig);
            rc.SingularComponent = component;

            foreach (var inputParam in component.Params.Input)
            {
                rc.Inputs[inputParam.NickName] = new Input(inputParam.NickName, inputParam, inputParam.Description());
            }

            foreach (var outputParam in component.Params.Output)
            {
                rc.Outputs[outputParam.NickName] = new Output(outputParam.NickName, outputParam, outputParam.Description());
            }

            return rc;
        }
    }
}
