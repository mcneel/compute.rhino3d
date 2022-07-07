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

        public static GrasshopperDefinition GrasshopperDefinition(string base64string, bool cacheToDisk = true)
        {
            var archive = base64string.GHArchiveFromBase64String();
            if (archive == null)
                return null;

            GrasshopperDefinition grasshopperDefinition = archive.ToGrasshopperDefinition();

            // Set inputs and outputs.
            grasshopperDefinition.SetIO();

            if (grasshopperDefinition != null)
            {
                grasshopperDefinition.CacheKey = DataCache.CreateCacheKey(base64string);

                if (cacheToDisk)
                {
                    DataCache.CacheToDisk(grasshopperDefinition.CacheKey, base64string);
                    grasshopperDefinition.StoredInCache = true;
                }
            }

            return grasshopperDefinition;
        }

        /***************************************************/

        public static GrasshopperDefinition GrasshopperDefinition(Uri scriptUrl, bool storeInCache = true)
        {
            if (scriptUrl == null)
                return null;

            string urlString = scriptUrl.ToString();

            // First check if the definition has been downloaded previously and is present in cache.
            GrasshopperDefinition rc = null;
            if (DataCache.TryGetCachedDefinition(urlString, out rc))
            {
                Log.RecordNote("Using cached definition");
                return rc;
            }

            var archive = BH.Engine.RemoteCompute.RhinoCompute.Compute.GHArchiveFromUrl(scriptUrl);
            if (archive == null)
                return null;

            rc = archive.ToGrasshopperDefinition();

            // Set inputs and outputs.
            rc.SetIO();

            rc.CacheKey = urlString;

            if (storeInCache)
                rc.StoredInCache = DataCache.CacheInMemory(urlString, rc);

            return rc;
        }

        /***************************************************/

        public static GrasshopperDefinition GrasshopperDefinition(Guid componentId, bool cache)
        {
            GrasshopperDefinition rc = ConstructAndSetIo(componentId);

            if (cache)
                rc.StoredInCache = DataCache.CacheInMemory(componentId.ToString(), rc);

            return rc;
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static GrasshopperDefinition ConstructAndSetIo(Guid componentId)
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

            foreach (var input in component.Params.Input)
            {
                rc.Inputs[input.NickName] = new InputGroup(input);
            }

            foreach (var output in component.Params.Output)
            {
                rc.Outputs[output.NickName] = output;
            }

            return rc;
        }
    }
}
