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
using System.Collections.Generic;
using System.Linq;
using Sovereign.EngineUtil.Collections;
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
    private static LinkedHashSet<string> CollectReferencedNames(WorldGenProfile profile)
    {
        var names = new LinkedHashSet<string>(StringComparer.Ordinal);
        AddName(names, profile.BedrockTemplate);
        AddName(names, WaterTemplateName);

        foreach (var band in profile.StoneBands) AddName(names, band.Template);

        if (profile.Biomes is { } biomes)
        {
            AddName(names, biomes.OceanFloorTemplate);
            AddName(names, biomes.WaterFloorTemplate);
            if (biomes.Swamp is { } swamp) AddName(names, swamp.Template);

            foreach (var definition in biomes.Definitions.Values)
            {
                AddName(names, definition.SurfaceTemplate);
                AddName(names, definition.SubSurfaceTemplate);
                if (definition.Decorations is null) continue;
                foreach (var decoration in definition.Decorations)
                {
                    AddName(names, decoration.Template);
                }
            }
        }

        return names;
    }

    /// <summary>
    ///     Adds a template name to the set if it is not blank.
    /// </summary>
    /// <param name="names">Template names collected so far.</param>
    /// <param name="name">Template name to add.</param>
    private static void AddName(LinkedHashSet<string> names, string? name)
    {
        if (!string.IsNullOrWhiteSpace(name)) names.Add(name!);
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
}
