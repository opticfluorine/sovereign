// Sovereign Engine
// Copyright (c) 2026 opticfluorine
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sovereign.WorldGen;

namespace Sovereign.ServerCore.Systems.WorldGeneration;

/// <summary>
///     Source of the live named template entities used for plan-time template resolution.
/// </summary>
public interface IWorldGenTemplateSource
{
    /// <summary>
    ///     Gets every named template entity available for resolution.
    /// </summary>
    /// <returns>Template names mapped to template entity IDs.</returns>
    IReadOnlyDictionary<string, ulong> GetNamedTemplates();
}

/// <summary>
///     Resolves the template names referenced by a world generation profile against the
///     live in-memory template entity set at plan time. Resolution is all-or-nothing:
///     any missing name fails the plan with a single consolidated error.
/// </summary>
public sealed class WorldGenTemplateResolver
{
    /// <summary>
    ///     Name of the water block template used to fill ocean, lake, and river columns.
    /// </summary>
    public const string WaterTemplateName = "Water";

    private readonly IWorldGenTemplateSource templateSource;

    public WorldGenTemplateResolver(IWorldGenTemplateSource templateSource)
    {
        this.templateSource = templateSource;
    }

    /// <summary>
    ///     Resolves every template name referenced by the profile.
    /// </summary>
    /// <param name="profile">Validated profile to resolve.</param>
    /// <param name="profileName">Name of the profile, for error reporting.</param>
    /// <returns>Resolved template set carried on the plan.</returns>
    /// <exception cref="WorldGenTemplateResolutionException">Thrown if any template name
    ///     cannot be resolved; the exception names every missing template.</exception>
    public WorldGenResolvedTemplates Resolve(WorldGenProfile profile, string profileName)
    {
        var referenced = CollectReferencedNames(profile);
        var available = AvailableTemplatesByName();
        var resolved = new List<(string Name, ulong TemplateEntityId)>(referenced.Count);
        var missing = new List<string>();

        foreach (var name in referenced)
        {
            if (available.TryGetValue(name, out var id))
                resolved.Add((name, id));
            else
                missing.Add(name);
        }

        if (missing.Count > 0)
        {
            var quoted = string.Join(", ", missing.Select(name => $"\"{name}\""));
            throw new WorldGenTemplateResolutionException(
                $"Unknown templates: {quoted}. Check Data/Worldgen/{profileName}.json against " +
                "the installed template entities.");
        }

        return new WorldGenResolvedTemplates(resolved);
    }

    /// <summary>
    ///     Collects every distinct template name referenced by the profile, in profile
    ///     declaration order.
    /// </summary>
    /// <param name="profile">Profile to scan.</param>
    /// <returns>Referenced template names.</returns>
    private static LinkedHashSet CollectReferencedNames(WorldGenProfile profile)
    {
        var names = new LinkedHashSet { profile.BedrockTemplate, WaterTemplateName };

        foreach (var band in profile.StoneBands) names.Add(band.Template);

        if (profile.Biomes is { } biomes)
        {
            names.Add(biomes.OceanFloorTemplate);
            names.Add(biomes.WaterFloorTemplate);
            if (biomes.Swamp is { } swamp) names.Add(swamp.Template);

            foreach (var definition in biomes.Definitions.Values)
            {
                names.Add(definition.SurfaceTemplate);
                names.Add(definition.SubSurfaceTemplate);
                if (definition.Decorations is null) continue;
                foreach (var decoration in definition.Decorations) names.Add(decoration.Template);
            }
        }

        return names;
    }

    /// <summary>
    ///     Builds the case-insensitive name-to-ID map of all named template entities.
    /// </summary>
    /// <returns>Map from lowercased template name to template entity ID.</returns>
    private Dictionary<string, ulong> AvailableTemplatesByName()
    {
        var available = new Dictionary<string, ulong>(StringComparer.OrdinalIgnoreCase);
        foreach (var (name, id) in templateSource.GetNamedTemplates())
        {
            available[name] = id;
        }

        return available;
    }

    /// <summary>
    ///     Insertion-ordered set of distinct strings.
    /// </summary>
    private sealed class LinkedHashSet : IEnumerable<string>
    {
        private readonly List<string> order = new();
        private readonly HashSet<string> seen = new(StringComparer.Ordinal);

        /// <summary>
        ///     Adds a name if it has not been seen and is not blank.
        /// </summary>
        /// <param name="name">Name to add.</param>
        public void Add(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            if (!seen.Add(name)) return;
            order.Add(name);
        }

        /// <summary>
        ///     Names in insertion order.
        /// </summary>
        public IReadOnlyList<string> Order => order;

        /// <summary>
        ///     Number of distinct names.
        /// </summary>
        public int Count => order.Count;

        /// <summary>
        ///     Enumerates the names in insertion order.
        /// </summary>
        /// <returns>Enumerator.</returns>
        public IEnumerator<string> GetEnumerator() => order.GetEnumerator();

        /// <summary>
        ///     Enumerates the names in insertion order.
        /// </summary>
        /// <returns>Enumerator.</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
