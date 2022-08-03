using System;
using BH.Engine.RemoteCompute;
using BH.oM.RemoteCompute.RhinoCompute;
using Grasshopper.Kernel;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static GrasshopperDefinition GrasshopperDefinitionFromBase64String(string base64string)
        {
            string cacheKey = base64string.CacheKey();

            GrasshopperDefinition grasshopperDefinition = base64string.ToGrasshopperDefinition();

            return grasshopperDefinition;
        }

        /***************************************************/

        public static GrasshopperDefinition GrasshopperDefinitionFromUri(Uri scriptUrl)
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

            var archive = BH.Engine.RemoteCompute.RhinoCompute.Compute.GHArchiveFromUrl(scriptUrl);
            if (archive == null)
                return null;

            gDef = archive.ToGrasshopperDefinition();

            return gDef;
        }

        /***************************************************/

        public static GrasshopperDefinition GrasshopperDefinitionFromComponent(Guid componentId)
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
                BH.Engine.RemoteCompute.Log.RecordWarning($"Exception in DocumentAdded event handler:\n\t{e.Message}");
            }

            GrasshopperDefinition rc = new GrasshopperDefinition(gh_document);
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
