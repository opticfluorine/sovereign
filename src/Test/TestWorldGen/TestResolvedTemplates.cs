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

using System.Collections.Generic;
using System.Linq;
using Sovereign.WorldGen;

namespace TestWorldGen;

/// <summary>
///     Builds resolved template sets for test profiles, assigning sequential placeholder
///     template entity IDs to every referenced template name.
/// </summary>
public static class TestResolvedTemplates
{
    /// <summary>
    ///     Template entity ID of the first placeholder template.
    /// </summary>
    public const ulong FirstTemplateEntityId = 0x7FFE000000000000;

    /// <summary>
    ///     Resolves every template name referenced by the profile to a sequential
    ///     placeholder template entity ID.
    /// </summary>
    /// <param name="profile">Profile to scan.</param>
    /// <returns>Resolved template set.</returns>
    public static WorldGenResolvedTemplates ForProfile(WorldGenProfile profile)
    {
        var names = new List<string> { profile.BedrockTemplate, "Water" };
        names.AddRange(profile.StoneBands.Select(band => band.Template));

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
                names.AddRange(definition.Decorations.Select(decoration => decoration.Template));
            }
        }

        var resolved = names.Distinct()
            .Select((name, index) => (name, (ulong)(FirstTemplateEntityId + (uint)index)))
            .ToList();
        return new WorldGenResolvedTemplates(resolved);
    }
}
