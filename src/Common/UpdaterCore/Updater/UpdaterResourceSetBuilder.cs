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

using System.Text.RegularExpressions;
using Sovereign.EngineCore.Resources;

namespace Sovereign.UpdaterCore.Updater;

/// <summary>
///     Builds UpdaterResourceSet objects that can be published to an update server.
/// </summary>
public partial class UpdaterResourceSetBuilder
{
    private readonly UpdaterHash updaterHash;

    public UpdaterResourceSetBuilder(UpdaterHash updaterHash)
    {
        this.updaterHash = updaterHash;
    }

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
        AddSpritesheet(resourceSet, pathBuilder);
        AddWorld(resourceSet, pathBuilder);

        return resourceSet;
    }

    /// <summary>
    ///     Adds all Sprite resources to the resource set.
    /// </summary>
    /// <param name="resourceSet">Resource set.</param>
    /// <param name="pathBuilder">Path builder.</param>
    private void AddSprites(UpdaterResourceSet resourceSet, IResourcePathBuilder pathBuilder)
    {
        AddFiles(resourceSet, pathBuilder, ResourceType.Sprite, f => IsJsonRegex().IsMatch(f));
    }

    /// <summary>
    ///     Adds all Spritesheet resources to the resource set.
    /// </summary>
    /// <param name="resourceSet">Resource set.</param>
    /// <param name="pathBuilder">Path builder.</param>
    private void AddSpritesheet(UpdaterResourceSet resourceSet, IResourcePathBuilder pathBuilder)
    {
        AddFiles(resourceSet, pathBuilder, ResourceType.Spritesheet,
            f => IsPngRegex().IsMatch(f) || IsYamlRegex().IsMatch(f));
    }

    /// <summary>
    ///     Adds all World resources to the resource set.
    /// </summary>
    /// <param name="resourceSet">Resource set.</param>
    /// <param name="pathBuilder">Path builder.</param>
    private void AddWorld(UpdaterResourceSet resourceSet, IResourcePathBuilder pathBuilder)
    {
        AddFiles(resourceSet, pathBuilder, ResourceType.World, f => IsJsonRegex().IsMatch(f));
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
        var hash = updaterHash.Hash(fullPath);

        // Record file.
        resourceSet.Resources.Add(new UpdaterResource
        {
            Filename = filename,
            ResourceType = resourceType,
            Hash = hash
        });
    }

    /// <summary>
    ///     Generated regex that matches .png filenames.
    /// </summary>
    /// <returns>Regex.</returns>
    [GeneratedRegex(@".+\.png$")]
    private static partial Regex IsPngRegex();

    /// <summary>
    ///     Generated regex that matches .yaml filenames.
    /// </summary>
    /// <returns>Regex.</returns>
    [GeneratedRegex(@".+\.yaml$")]
    private static partial Regex IsYamlRegex();

    /// <summary>
    ///     Generated regex that matches .json filenames.
    /// </summary>
    /// <returns>Regex.</returns>
    [GeneratedRegex(@".+\.json$")]
    private static partial Regex IsJsonRegex();
}