
namespace Sovereign.EngineCore.Resources
{

    /// <summary>
    /// Builds paths to resources.
    /// </summary>
    public interface IResourcePathBuilder
    {

        /// <summary>
        /// Builds the path to the base directory for the given resource type.
        /// </summary>
        /// <param name="resourceType">Resource type.</param>
        /// <returns>Path to the base directory for the given resource type.</returns>
        string GetBaseDirectoryForResource(ResourceType resourceType);

        /// <summary>
        /// Builds the path to the given resource file.
        /// </summary>
        /// <param name="resourceType">Resource type.</param>
        /// <param name="resourceFilename">Name of the resource file.</param>
        /// <returns>Path to the resource file.</returns>
        string BuildPathToResource(ResourceType resourceType, string resourceFilename);

    }

}
