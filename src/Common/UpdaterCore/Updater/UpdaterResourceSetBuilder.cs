// Sovereign Engine
// Copyright (c) 2024 opticfluorine
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.Security.Cryptography;
using System.Text;
using Sovereign.EngineCore.Resources;

namespace Sovereign.UpdaterCore.Updater;

/// <summary>
///     Builds UpdaterResourceSet objects that can be published to an update server.
/// </summary>
public class UpdaterResourceSetBuilder
{
    /// <summary>
    ///     Builds an UpdaterResourceSet object using all resources found from the given path builder.
    /// </summary>
    /// <param name="pathBuilder">Resource path builder.</param>
    /// <returns>UpdaterResourceSet.</returns>
    public UpdaterResourceSet Build(IResourcePathBuilder pathBuilder)
    {
        var resourceSet = new UpdaterResourceSet();
        resourceSet.ReleaseId = Guid.NewGuid();

        AddSprites(resourceSet, pathBuilder);

        return resourceSet;
    }

    /// <summary>
    ///     Adds all Sprite resources to the resource set.
    /// </summary>
    /// <param name="resourceSet">Resource set.</param>
    /// <param name="pathBuilder">Path builder.</param>
    private void AddSprites(UpdaterResourceSet resourceSet, IResourcePathBuilder pathBuilder)
    {
        var baseDir = pathBuilder.GetBaseDirectoryForResource(ResourceType.Sprite);
        
    }

    /// <summary>
    ///     Adds all Spritesheet resources to the resource set.
    /// </summary>
    /// <param name="resourceSet">Resource set.</param>
    /// <param name="pathBuilder">Path builder.</param>
    private void AddSpritesheet(UpdaterResourceSet resourceSet, IResourcePathBuilder pathBuilder)
    {
    }

    /// <summary>
    ///     Adds all World resources to the resource set.
    /// </summary>
    /// <param name="resourceSet">Resource set.</param>
    /// <param name="pathBuilder">Path builder.</param>
    private void AddWorld(UpdaterResourceSet resourceSet, IResourcePathBuilder pathBuilder)
    {
    }

    /// <summary>
    ///     Adds files of a given resource type if they pass a test function.
    /// </summary>
    /// <param name="resourceSet">Resource set.</param>
    /// <param name="pathBuilder">Path builder.</param>
    /// <param name="resourceType">Resource type.</param>
    /// <param name="shouldInclude">Test function; if returns true, include the given filename.</param>
    private void AddFiles(UpdaterResourceSet resourceSet, IResourcePathBuilder pathBuilder,
        ResourceType resourceType, Func<string, bool> shouldInclude)
    {
        var includedFiles = Directory.EnumerateFiles(pathBuilder.GetBaseDirectoryForResource(resourceType))
            .Select(fullPath => Path.GetFileName(fullPath))
            .Where(shouldInclude);

        foreach (var filename in includedFiles)
        {
            AddResource(resourceSet, pathBuilder, resourceType, filename);
        }
    }

    /// <summary>
    ///     Adds an individual resource to the resource set.
    /// </summary>
    /// <param name="resourceSet">Resource set.</param>
    /// <param name="pathBuilder">Path builder.</param>
    /// <param name="resourceType">Resource type.</param>
    /// <param name="filename">Resource filename.</param>
    private void AddResource(UpdaterResourceSet resourceSet, IResourcePathBuilder pathBuilder,
        ResourceType resourceType, string filename)
    {
        // Computer SHA512 hash of file.
        var fullPath = pathBuilder.BuildPathToResource(resourceType, filename);
        var hash = HashFile(fullPath);
        
        // Record file.
        resourceSet.Resources.Add(new UpdaterResource()
        {
            Filename = filename,
            ResourceType = resourceType,
            Hash = hash
        });
    }

    /// <summary>
    ///     Hashes a file.
    /// </summary>
    /// <param name="fullPath">Full path to file.</param>
    /// <returns>Hash encoded as a hex string.</returns>
    private string HashFile(string fullPath)
    {
        using var fs = File.Open(fullPath, FileMode.Open, FileAccess.Read);
        return Convert.ToHexString(SHA512.Create().ComputeHash(fs));
    }
}