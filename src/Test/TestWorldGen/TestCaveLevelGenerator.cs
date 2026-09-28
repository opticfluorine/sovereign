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

using Sovereign.WorldGen;
using Sovereign.WorldGen.Caves;
using Sovereign.WorldGen.Noise;
using Sovereign.WorldGen.Terrain;
using Xunit;

namespace TestWorldGen;

/// <summary>
///     Tests for the per-level cave generator: floor field bounds and slope, Worley
///     porosity calibration and structure, water-proximity exclusion, connectivity repair,
///     and determinism.
/// </summary>
public class TestCaveLevelGenerator
{
    /// <summary>
    ///     Synthetic map size used by most tests.
    /// </summary>
    private const int MapSize = 256;

    [Fact]
    public void Generate_FloorsStayNearBaseAndAdjacentOpenCellsDifferByAtMostOne()
    {
        const int baseFloorZ = -16;
        var map = Generate(MapSize, baseFloorZ, 2, 0.34);

        var openCells = 0;
        for (var y = 0; y < MapSize; ++y)
        {
            for (var x = 0; x < MapSize; ++x)
            {
                if (!map.Open[x, y]) continue;
                ++openCells;

                Assert.InRange(map.FloorZ[x, y], baseFloorZ - 2, baseFloorZ + 2);

                if (x + 1 < MapSize && map.Open[x + 1, y])
                {
                    Assert.True(Math.Abs(map.FloorZ[x + 1, y] - map.FloorZ[x, y]) <= 1,
                        $"Floor step from ({x},{y}) to ({x + 1},{y}) exceeds one block.");
                }

                if (y + 1 < MapSize && map.Open[x, y + 1])
                {
                    Assert.True(Math.Abs(map.FloorZ[x, y + 1] - map.FloorZ[x, y]) <= 1,
                        $"Floor step from ({x},{y}) to ({x},{y + 1}) exceeds one block.");
                }
            }
        }

        Assert.True(openCells > 0, "The level should contain open cells.");
    }

    [Fact]
    public void Generate_RealizesConfiguredPorosityOnDryLand()
    {
        var map = Generate(MapSize, -16, 2, 0.34);

        var openCells = 0;
        for (var y = 0; y < MapSize; ++y)
        {
            for (var x = 0; x < MapSize; ++x)
            {
                if (map.Open[x, y]) ++openCells;
            }
        }

        var fraction = openCells / (double)(MapSize * MapSize);
        Assert.InRange(fraction, 0.8 * 0.34, 1.2 * 0.34);
    }

    [Fact]
    public void Generate_OpenRegionHasCorridorAndChamberStructure()
    {
        var map = Generate(MapSize, -16, 2, 0.34);

        // Bounding-box aspect of the merged open web: a Worley web spans the map in one
        // direction far more than any blob-like field would.
        var (minX, minY, maxX, maxY, openCells) = BoundsOf(map);
        Assert.True(openCells > 0, "The level should contain open cells.");
        var aspect = (maxX - minX + 1.0) / (maxY - minY + 1.0);
        Assert.InRange(aspect, 1.0 / 3.0, 3.0);

        // Chambers exist: some open cell has three or more open neighbors.
        var hasChamberCell = false;
        var hasCorridorCell = false;
        for (var y = 0; y < MapSize && !(hasChamberCell && hasCorridorCell); ++y)
        {
            for (var x = 0; x < MapSize; ++x)
            {
                if (!map.Open[x, y]) continue;

                var neighbors = 0;
                if (x > 0 && map.Open[x - 1, y]) ++neighbors;
                if (x < MapSize - 1 && map.Open[x + 1, y]) ++neighbors;
                if (y > 0 && map.Open[x, y - 1]) ++neighbors;
                if (y < MapSize - 1 && map.Open[x, y + 1]) ++neighbors;

                if (neighbors >= 3) hasChamberCell = true;
                if (neighbors <= 1) hasCorridorCell = true;
            }
        }

        Assert.True(hasChamberCell, "The Worley field should produce chamber cells.");
        Assert.True(hasCorridorCell, "The Worley field should produce corridor tips.");
    }

    [Fact]
    public void WaterExclusion_RiverOverShallowLevel_SolidifiesBeneathAndWithinFourCells()
    {
        var terrain = DryTerrain(MapSize);
        for (var y = 0; y < MapSize; ++y)
        {
            for (var x = 26; x <= 35; ++x)
            {
                // A low channel valley so the cave ceiling is within 3 blocks of the
                // water floor across the whole exclusion band.
                terrain.Heights[x, y] = -14;
            }

            terrain.IsRiver[30, y] = true;
            terrain.IsRiver[31, y] = true;
            terrain.IsBank[29, y] = true;
            terrain.IsBank[32, y] = true;
        }

        // Shallow level: ceiling (-16 to -12) within 3 blocks of the channel floor (-14).
        var shallow = new CaveLevelGenerator().Generate(
            new CaveLevel { FloorZ = -16, Headroom = 2 },
            terrain, new CaveOptions { Porosity = 0.34 }, Seed(0));

        for (var y = 0; y < MapSize; ++y)
        {
            for (var x = 26; x <= 35; ++x)
            {
                Assert.False(shallow.Map.Open[x, y],
                    $"Shallow level carved beneath or beside the river at ({x},{y}).");
            }
        }

        // Deep level under the same river may open.
        var deep = new CaveLevelGenerator().Generate(
            new CaveLevel { FloorZ = -48, Headroom = 2 },
            terrain, new CaveOptions { Porosity = 0.34 }, Seed(0));

        var deepOpenInStrip = 0;
        for (var y = 0; y < MapSize; ++y)
        {
            for (var x = 28; x <= 33; ++x)
            {
                if (deep.Map.Open[x, y]) ++deepOpenInStrip;
            }
        }

        Assert.True(deepOpenInStrip > 0,
            "The deep level should be allowed to open beneath the river.");
    }

    [Fact]
    public void WaterExclusion_ShallowLakeColumns_SolidifiedOutright()
    {
        var terrain = DryTerrain(MapSize);
        for (var y = 40; y < 80; ++y)
        {
            for (var x = 40; x < 80; ++x)
            {
                terrain.IsLake[x, y] = true;
                terrain.LakeSurfaceZ[x, y] = -12;
            }
        }

        // Shallow level: lake surface (-12) to floor (-16 to -18) is fewer than 8 blocks.
        var shallow = new CaveLevelGenerator().Generate(
            new CaveLevel { FloorZ = -16, Headroom = 2 },
            terrain, new CaveOptions { Porosity = 0.34 }, Seed(0));

        for (var y = 40; y < 80; ++y)
        {
            for (var x = 40; x < 80; ++x)
            {
                Assert.False(shallow.Map.Open[x, y],
                    $"Shallow level carved in a shallow lake column at ({x},{y}).");
            }
        }

        // Deep level under the same lake is 32 blocks below the surface and may open.
        var deep = new CaveLevelGenerator().Generate(
            new CaveLevel { FloorZ = -48, Headroom = 2 },
            terrain, new CaveOptions { Porosity = 0.34 }, Seed(0));

        var deepOpenInLake = 0;
        for (var y = 40; y < 80; ++y)
        {
            for (var x = 40; x < 80; ++x)
            {
                if (deep.Map.Open[x, y]) ++deepOpenInLake;
            }
        }

        Assert.True(deepOpenInLake > 0,
            "The deep level should be allowed to open beneath the lake.");
    }

    [Fact]
    public void ConnectivityRepair_FragmentedLevel_IsHealedIntoOneComponent()
    {
        // Low porosity fragments the Worley web into many islands; repair must merge them
        // while respecting the corridor budget and the floor slope rule.
        var result = new CaveLevelGenerator().Generate(
            new CaveLevel { FloorZ = -16, Headroom = 2 },
            DryTerrain(MapSize), new CaveOptions { Porosity = 0.25 }, Seed(2));

        Assert.True(result.ComponentsBeforeRepair > 1,
            "Low porosity should fragment the level into multiple components.");
        Assert.Equal(1, result.ComponentsAfterRepair);
        Assert.False(result.RepairBudgetExceeded);
        Assert.True(result.RepairCorridorCells > 0, "Repair corridors should have been carved.");
    }

    [Fact]
    public void ConnectivityRepair_MapSpanningWall_CannotBeHealed()
    {
        var terrain = DryTerrain(MapSize);
        for (var y = 0; y < MapSize; ++y)
        {
            for (var x = 124; x <= 127; ++x)
            {
                terrain.Heights[x, y] = -14;
                terrain.IsRiver[x, y] = true;
            }
        }

        var result = new CaveLevelGenerator().Generate(
            new CaveLevel { FloorZ = -16, Headroom = 2 },
            terrain, new CaveOptions { Porosity = 0.4 }, Seed(3));

        Assert.True(result.ComponentsAfterRepair >= 2,
            "A map-spanning wall cannot be routed around; the level stays split.");
        Assert.True(result.RepairBudgetExceeded,
            "Doomed corridors against a map-spanning wall should exhaust the budget.");
    }

    [Fact]
    public void Generate_IsDeterministic()
    {
        const int seedValue = 4;
        var first = GenerateFull(MapSize, -16, 2, 0.34, Seed(seedValue));
        var second = GenerateFull(MapSize, -16, 2, 0.34, Seed(seedValue));

        Assert.Equal(HashOpen(first.Map.Open), HashOpen(second.Map.Open));
        Assert.Equal(HashFloor(first.Map.FloorZ), HashFloor(second.Map.FloorZ));
        for (var y = 0; y < MapSize; ++y)
        {
            for (var x = 0; x < MapSize; ++x)
            {
                Assert.Equal(first.Map.CarveHeight[x, y], second.Map.CarveHeight[x, y]);
            }
        }
    }

    /// <summary>
    ///     Generates a level and returns just its carved map.
    /// </summary>
    /// <param name="size">Map size in blocks.</param>
    /// <param name="baseFloorZ">Base floor Z.</param>
    /// <param name="headroom">Level headroom.</param>
    /// <param name="porosity">Configured porosity.</param>
    /// <returns>Carved level map.</returns>
    private static CaveLevelMap Generate(int size, int baseFloorZ, int headroom, double porosity)
    {
        return GenerateFull(size, baseFloorZ, headroom, porosity, Seed(0)).Map;
    }

    /// <summary>
    ///     Generates a level on dry synthetic terrain.
    /// </summary>
    /// <param name="size">Map size in blocks.</param>
    /// <param name="baseFloorZ">Base floor Z.</param>
    /// <param name="headroom">Level headroom.</param>
    /// <param name="porosity">Configured porosity.</param>
    /// <param name="seed">Level sub-seed.</param>
    /// <returns>Full level result.</returns>
    private static CaveLevelResult GenerateFull(int size, int baseFloorZ, int headroom,
        double porosity, ulong seed)
    {
        var terrain = DryTerrain(size);
        return new CaveLevelGenerator().Generate(
            new CaveLevel { FloorZ = baseFloorZ, Headroom = headroom },
            terrain, new CaveOptions { Porosity = porosity }, seed);
    }

    /// <summary>
    ///     Creates a dry synthetic terrain map: all land at height 28, no water flags.
    /// </summary>
    /// <param name="size">Map size in blocks.</param>
    /// <returns>Terrain map.</returns>
    private static TerrainMap DryTerrain(int size)
    {
        var terrain = TestTerrainMaps.Create(size, size);
        for (var y = 0; y < size; ++y)
        {
            for (var x = 0; x < size; ++x)
            {
                terrain.Heights[x, y] = 28;
            }
        }

        return terrain;
    }

    /// <summary>
    ///     Computes the bounding box of the open mask and the open-cell count.
    /// </summary>
    /// <param name="map">Carved level map.</param>
    /// <returns>Bounds and open-cell count.</returns>
    private static (int MinX, int MinY, int MaxX, int MaxY, int OpenCells) BoundsOf(
        CaveLevelMap map)
    {
        var minX = int.MaxValue;
        var minY = int.MaxValue;
        var maxX = -1;
        var maxY = -1;
        var openCells = 0;
        for (var y = 0; y < map.Height; ++y)
        {
            for (var x = 0; x < map.Width; ++x)
            {
                if (!map.Open[x, y]) continue;

                ++openCells;
                minX = Math.Min(minX, x);
                minY = Math.Min(minY, y);
                maxX = Math.Max(maxX, x);
                maxY = Math.Max(maxY, y);
            }
        }

        return (minX, minY, maxX, maxY, openCells);
    }

    /// <summary>
    ///     Serializes an open mask row-major for hashing.
    /// </summary>
    /// <param name="open">Open mask.</param>
    /// <returns>Serialized bytes.</returns>
    private static byte[] HashOpen(bool[,] open)
    {
        var width = open.GetLength(0);
        var height = open.GetLength(1);
        var bytes = new byte[width * height];
        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                bytes[y * width + x] = open[x, y] ? (byte)1 : (byte)0;
            }
        }

        return bytes;
    }

    /// <summary>
    ///     Serializes a floor field row-major as 32-bit little-endian integers for hashing.
    /// </summary>
    /// <param name="floorZ">Floor field.</param>
    /// <returns>Serialized bytes.</returns>
    private static byte[] HashFloor(int[,] floorZ)
    {
        var width = floorZ.GetLength(0);
        var height = floorZ.GetLength(1);
        var bytes = new byte[width * height * sizeof(int)];
        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                var offset = (y * width + x) * sizeof(int);
                var value = floorZ[x, y];
                bytes[offset] = (byte)value;
                bytes[offset + 1] = (byte)(value >> 8);
                bytes[offset + 2] = (byte)(value >> 16);
                bytes[offset + 3] = (byte)(value >> 24);
            }
        }

        return bytes;
    }

    /// <summary>
    ///     Builds a level sub-seed from an integer, matching the stage's derivation scheme.
    /// </summary>
    /// <param name="index">Level index.</param>
    /// <returns>Sub-seed.</returns>
    private static ulong Seed(int index)
    {
        return SeedDerivation.DeriveSubSeed(42UL, $"Caves.Level{index}");
    }
}
