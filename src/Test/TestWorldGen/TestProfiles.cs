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
using Sovereign.WorldGen;

namespace TestWorldGen;

/// <summary>
///     Builder for valid baseline world generation profiles.
/// </summary>
internal static class TestProfiles
{
    /// <summary>
    ///     Creates a fully valid baseline profile.
    /// </summary>
    /// <returns>Profile.</returns>
    public static WorldGenProfile CreateValid()
    {
        return new WorldGenProfile
        {
            Width = 2048,
            Height = 2048,
            SeaLevelZ = 12,
            SurfaceMaxZ = 28,
            RockFloorZ = -63,
            BedrockZ = -64,
            StoneBands = new List<StoneBand>
            {
                new() { FromZ = -16, ToZ = -1, Template = "Shale" },
                new() { FromZ = -40, ToZ = -17, Template = "Granite" },
                new() { FromZ = -63, ToZ = -41, Template = "Basalt" }
            },
            CaveLevels = new List<CaveLevel>
            {
                new() { FloorZ = -16, Headroom = 2 },
                new() { FloorZ = -32, Headroom = 2 },
                new() { FloorZ = -48, Headroom = 2 }
            },
            Rivers = new RiverOptions { MaxCount = 40, MinLength = 64 },
            Caves = new CaveOptions { ShaftsPerLevelPair = 3, SurfaceMouths = 2 }
        };
    }

    /// <summary>
    ///     Creates the 128x128 baseline profile used by pipeline tests. Cave data is parsed
    ///     but unused until the cave generation card.
    /// </summary>
    /// <returns>Profile.</returns>
    public static WorldGenProfile CreateSmall128()
    {
        return new WorldGenProfile
        {
            Width = 128,
            Height = 128,
            SeaLevelZ = 12,
            SurfaceMaxZ = 28,
            RockFloorZ = -63,
            BedrockZ = -64,
            StoneBands = new List<StoneBand>
            {
                new() { FromZ = -16, ToZ = -1, Template = "Shale" },
                new() { FromZ = -40, ToZ = -17, Template = "Granite" },
                new() { FromZ = -63, ToZ = -41, Template = "Basalt" }
            },
            CaveLevels = new List<CaveLevel>
            {
                new() { FloorZ = -32, Headroom = 2 }
            },
            Rivers = new RiverOptions { MaxCount = 8, MinLength = 16 }
        };
    }

    /// <summary>
    ///     Creates the 128x128 baseline profile without river options; river stages are skipped.
    /// </summary>
    /// <returns>Profile.</returns>
    public static WorldGenProfile CreateSmall128WithoutRivers()
    {
        var profile = CreateSmall128();
        profile.Rivers = null;
        return profile;
    }
}
