using Engine8.EngineCore.Resources;
using System.IO;

namespace Engine8.ClientCore.Resources
{

    /// <summary>
    /// Builds resource paths for the client.
    /// </summary>
    public class ClientResourcePathBuilder : IResourcePathBuilder
    {

        /// <summary>
        /// Top-level resource directory.
        /// </summary>
        private const string ResourceRoot = ".";

        public string BuildPathToResource(ResourceType resourceType, string resourceFilename)
        {
            return Path.Combine(GetBaseDirectoryForResource(resourceType), resourceFilename);
        }

        public string GetBaseDirectoryForResource(ResourceType resourceType)
        {
            return Path.Combine(ResourceRoot, resourceType.ToString());
        }

    }

}
