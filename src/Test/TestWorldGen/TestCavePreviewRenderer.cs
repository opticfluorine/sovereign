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
using Sovereign.WorldGen.Biomes;
using Sovereign.WorldGen.Caves;
using Sovereign.WorldGen.Output;
using Sovereign.WorldGen.Terrain;
using Xunit;

namespace TestWorldGen;

/// <summary>
///     Tests for the cave preview renderer and the surface preview mouth dots.
/// </summary>
public class TestCavePreviewRenderer
{
    /// <summary>
    ///     Synthetic map size used by the tests.
    /// </summary>
    private const int MapSize = 64;

    [Fact]
    public void Render_ProducesOneImagePerLevel()
    {
        var caves = TestCaveMap();

        var images = new CavePreviewRenderer().Render(caves);

        Assert.Equal(2, images.Count);
        foreach (var image in images)
        {
            Assert.Equal(MapSize, image.Width);
            Assert.Equal(MapSize, image.Height);
            Assert.Equal(MapSize * MapSize * 3, image.Pixels.Length);
        }
    }

    [Fact]
    public void Render_OpenCellsAreDarkGrayAndDistinctFromRock()
    {
        var caves = TestCaveMap();

        var image = new CavePreviewRenderer().Render(caves)[0];

        // Cell (5,5) is open dark gray; cell (0,0) is solid rock.
        var open = PixelAt(image, 5, 5);
        Assert.True(open.R > 40 && open.R < 160, $"Open cell should be dark gray, got {open}.");
        var rock = PixelAt(image, 0, 0);
        Assert.Equal(rock.R, rock.G);
        Assert.NotEqual(open, rock);
    }

    [Fact]
    public void Render_MouthRingsAppearOnlyOnShallowestLevel()
    {
        var caves = TestCaveMap();

        var images = new CavePreviewRenderer().Render(caves);

        // The mouth at (30,30) draws its ring centered on (31,31).
        Assert.Equal(((byte)220, (byte)40, (byte)40), PixelAt(images[0], 31, 31));
        Assert.NotEqual(((byte)220, (byte)40, (byte)40), PixelAt(images[1], 31, 31));
    }

    [Fact]
    public void Render_SurfacePreviewDots_AlterOnlyMouthPixels()
    {
        var map = TestTerrainMaps.Create(MapSize, MapSize);
        var continentalness = TestTerrainMaps.AllLand(MapSize, MapSize);
        var profile = TestProfiles.CreateSmall128();
        var renderer = new PreviewRenderer();

        var plain = renderer.Render(map, continentalness, profile);
        var dotted = renderer.Render(map, continentalness, profile, null,
            new List<(int X, int Y)> { (10, 10) });

        for (var y = 0; y < MapSize; ++y)
        {
            for (var x = 0; x < MapSize; ++x)
            {
                var offset = (y * MapSize + x) * 3;
                if ((x, y) == (10, 10))
                {
                    Assert.Equal((220, 40, 40), (
                        dotted.Pixels[offset],
                        dotted.Pixels[offset + 1],
                        dotted.Pixels[offset + 2]));
                    continue;
                }

                Assert.Equal(
                    (plain.Pixels[offset], plain.Pixels[offset + 1], plain.Pixels[offset + 2]),
                    (dotted.Pixels[offset], dotted.Pixels[offset + 1], dotted.Pixels[offset + 2]));
            }
        }
    }

    /// <summary>
    ///     Creates a small two-level cave map with a shaft and a mouth.
    /// </summary>
    /// <returns>Cave map.</returns>
    private static CaveMap TestCaveMap()
    {
        const int size = MapSize;
        var levels = new List<CaveLevelMap>();
        for (var i = 0; i < 2; ++i)
        {
            var open = new bool[size, size];
            var floorZ = new int[size, size];
            var carveHeight = new byte[size, size];
            var worley = new byte[size, size];
            var waterExcluded = new bool[size, size];
            for (var y = 0; y < size; ++y)
            {
                for (var x = 0; x < size; ++x)
                {
                    var isOpen = x > 2 && y > 2;
                    open[x, y] = isOpen;
                    floorZ[x, y] = -16 - 16 * i;
                    carveHeight[x, y] = isOpen ? (byte)2 : (byte)0;
                    worley[x, y] = isOpen ? (byte)200 : (byte)0;
                }
            }

            levels.Add(new CaveLevelMap
            {
                Width = size,
                Height = size,
                BaseFloorZ = -16 - 16 * i,
                Headroom = 2,
                Open = open,
                FloorZ = floorZ,
                CarveHeight = carveHeight,
                Worley = worley,
                WaterExcluded = waterExcluded
            });
        }

        return new CaveMap
        {
            Width = size,
            Height = size,
            Levels = levels,
            Shafts = new List<CaveShaft>
            {
                new()
                {
                    UpperLevel = 0,
                    LowerLevel = 1,
                    CenterX = 32,
                    CenterY = 32,
                    LowerFloorZ = -32,
                    UpperFloorZ = -16,
                    Headroom = 2,
                    Columns = new List<CaveShaftColumn>
                    {
                        new() { X = 32, Y = 31, StepZs = new[] { -31, -23 } }
                    },
                    Ring = new List<(int X, int Y)> { (30, 30), (34, 34) }
                }
            },
            MouthList = new List<CaveMouth>
            {
                new()
                {
                    X = 30,
                    Y = 30,
                    SurfaceZ = -4,
                    FloorZ = -16,
                    Headroom = 2,
                    Columns = new List<CaveShaftColumn>()
                }
            }
        };
    }

    /// <summary>
    ///     Reads the pixel at an image coordinate.
    /// </summary>
    /// <param name="image">Preview image.</param>
    /// <param name="x">Pixel X coordinate.</param>
    /// <param name="y">Pixel Y coordinate.</param>
    /// <returns>Pixel color.</returns>
    private static (byte R, byte G, byte B) PixelAt(PreviewImage image, int x, int y)
    {
        var offset = (y * image.Width + x) * 3;
        return (image.Pixels[offset], image.Pixels[offset + 1], image.Pixels[offset + 2]);
    }
}
