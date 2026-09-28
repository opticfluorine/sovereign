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
using System.IO;
using System.Security.Cryptography;
using Sovereign.WorldGen;
using Sovereign.WorldGen.Biomes;
using Sovereign.WorldGen.Caves;
using Sovereign.WorldGen.Decorations;
using Sovereign.WorldGen.Terrain;
using Xunit;

namespace TestWorldGen;

/// <summary>
///     End-to-end pipeline tests: determinism of the heightmap and preview PNG, the golden
///     preview image hash, and sanity of plan statistics.
/// </summary>
public class TestWorldGenPipeline
{
    /// <summary>
    ///     Fixed seed used by the end-to-end tests.
    /// </summary>
    private const ulong Seed = 12345;

    /// <summary>
    ///     SHA-256 hash of the golden 128x128 preview PNG produced with the test baseline
    ///     profile and the fixed seed above.
    ///
    ///     To regenerate after an intentional algorithm change: run this test once and read
    ///     the actual hash from the assertion failure message, then update this constant.
    ///     The preview PNG uses stored (uncompressed) deflate blocks per RFC 1951, which are
    ///     fully specified by the format, so the hash is stable across machines and runtime
    ///     versions. Generated on x64 Debian, .NET 10.0.12.
    /// </summary>
    private const string GoldenPreviewSha256 =
        "35CF4EE3A6F06406C3360DD0C073BE93DD2D70FF93093F7C383326E05908DFB3";

    /// <summary>
    ///     SHA-256 hash of the golden 128x128 heightmap: the TerrainMap heights serialized
    ///     row-major as 32-bit little-endian integers.
    ///
    ///     Pinned independently of the preview hash so that a cross-machine mismatch
    ///     distinguishes drift in the generation math (this hash) from drift in the
    ///     preview encoding (the preview hash). To regenerate: run this test once and
    ///     read the actual hash from the assertion failure message. Regenerated for the
    ///     worldgen surface tuning card (terrain section, reduced warp amplitudes, extra
    ///     continentalness octave, contrast stretch) on x64 Debian, .NET 10.0.12.
    /// </summary>
    private const string GoldenHeightsSha256 =
        "7C972F970465185C3DBC80607A6BE824CDD784E78D27371F509B678A23FD55AE";

    /// <summary>
    ///     SHA-256 hash of the golden 128x128 preview PNG produced with the biome-enabled
    ///     test baseline profile (<see cref="TestProfiles.CreateSmall128Biomes" />) and the
    ///     fixed seed above. The heights hash is shared with the biome-free profile.
    ///
    ///     The biome preview palette is load-bearing for this hash: palette edits in
    ///     <see cref="Sovereign.WorldGen.Output.PreviewRenderer" /> are golden-hash edits.
    ///     To regenerate after an intentional change: run this test once and read the actual
    ///     hash from the assertion failure message, then update this constant.
    /// </summary>
    private const string GoldenBiomesPreviewSha256 =
        "D088771A7F2A2E893D9A0D5DCEF1077D239A44F765BE1BFCD51742CD193A642A";

    [Fact]
    public void Plan_SameSeedAndProfile_ProducesIdenticalHeightsAndPreview()
    {
        var profile = TestProfiles.CreateSmall128();
        var firstPath = TempPreviewPath("det1");
        var secondPath = TempPreviewPath("det2");

        var first = new WorldGenPipeline().Plan(profile, "test128", Seed, 0, 0, firstPath, null);
        var second = new WorldGenPipeline().Plan(profile, "test128", Seed, 0, 0, secondPath, null);

        try
        {
            Assert.Equal(first.Terrain.Width, second.Terrain.Width);
            Assert.Equal(first.Terrain.Height, second.Terrain.Height);
            for (var y = 0; y < profile.Height; ++y)
            {
                for (var x = 0; x < profile.Width; ++x)
                {
                    Assert.Equal(first.Terrain.Heights[x, y], second.Terrain.Heights[x, y]);
                }
            }

            var firstHash = Sha256(File.ReadAllBytes(firstPath));
            var secondHash = Sha256(File.ReadAllBytes(secondPath));
            Assert.Equal(firstHash, secondHash);
        }
        finally
        {
            File.Delete(firstPath);
            File.Delete(secondPath);
        }
    }

    [Fact]
    public void Plan_GoldenPreview_MatchesHash()
    {
        var profile = TestProfiles.CreateSmall128();
        var path = TempPreviewPath("golden");

        try
        {
            var plan = new WorldGenPipeline().Plan(profile, "test128", Seed, 0, 0, path, null);
            var actualPreview = Sha256(File.ReadAllBytes(path));
            var actualHeights = Sha256(HeightsBytes(plan.Terrain));

            Assert.True(string.Equals(actualHeights, GoldenHeightsSha256, StringComparison.OrdinalIgnoreCase),
                $"Golden heights hash mismatch: expected {GoldenHeightsSha256}, actual {actualHeights}. " +
                "The generation math has drifted from the pinned baseline. If the change was " +
                "intentional, regenerate the constant as described in its documentation.");
            Assert.True(string.Equals(actualPreview, GoldenPreviewSha256, StringComparison.OrdinalIgnoreCase),
                $"Golden preview hash mismatch: expected {GoldenPreviewSha256}, actual {actualPreview}. " +
                "The preview encoding has drifted from the pinned baseline (the heightmap hash " +
                "passed, so the math is intact). If the change was intentional, regenerate the " +
                "constant as described in its documentation.");
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>
    ///     Serializes the heightmap row-major as 32-bit little-endian integers for hashing.
    /// </summary>
    /// <param name="map">Terrain map to serialize.</param>
    /// <returns>Serialized bytes.</returns>
    private static byte[] HeightsBytes(TerrainMap map)
    {
        var bytes = new byte[map.Width * map.Height * sizeof(int)];
        for (var y = 0; y < map.Height; ++y)
        {
            for (var x = 0; x < map.Width; ++x)
            {
                var offset = (y * map.Width + x) * sizeof(int);
                var value = map.Heights[x, y];
                bytes[offset] = (byte)value;
                bytes[offset + 1] = (byte)(value >> 8);
                bytes[offset + 2] = (byte)(value >> 16);
                bytes[offset + 3] = (byte)(value >> 24);
            }
        }

        return bytes;
    }

    [Fact]
    public void Plan_ReportsSaneStatistics()
    {
        var profile = TestProfiles.CreateSmall128();
        var path = TempPreviewPath("stats");

        try
        {
            var plan = new WorldGenPipeline().Plan(profile, "test128", Seed,
                5, -7, path, null);

            var stats = plan.Statistics;
            Assert.Equal(128, stats.Width);
            Assert.Equal(128, stats.Height);
            Assert.Equal(128L * 128, stats.LandCells + stats.WaterCells);
            Assert.True(stats.LandCells > 0, "The plan should contain land.");
            Assert.True(stats.WaterCells > 0, "The plan should contain water.");
            Assert.True(stats.RiverCount > 0, "The plan should contain rivers.");
            Assert.InRange(stats.LongestStraightRiverRun, 0, profile.Terrain.MaxStraightRiverRun);
            Assert.True(stats.TotalMs >= 0);

            Assert.Equal(Seed, plan.Seed);
            Assert.Equal("test128", plan.ProfileName);
            Assert.Equal(5, plan.OriginX);
            Assert.Equal(-7, plan.OriginY);
            Assert.Equal(path, plan.PreviewPath);
            Assert.True(File.Exists(plan.PreviewPath), "The preview PNG should exist.");
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Plan_HeightsStayWithinProfileBounds()
    {
        var profile = TestProfiles.CreateSmall128();
        var path = TempPreviewPath("bounds");

        try
        {
            var plan = new WorldGenPipeline().Plan(profile, "test128", Seed, 0, 0, path, null);
            var map = plan.Terrain;

            for (var y = 0; y < profile.Height; ++y)
            {
                for (var x = 0; x < profile.Width; ++x)
                {
                    Assert.InRange(map.Heights[x, y], profile.RockFloorZ + 8, profile.SurfaceMaxZ);
                }
            }
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Plan_WithBiomes_PopulatesBiomeMaterialAndDecorationOutputs()
    {
        var profile = TestProfiles.CreateSmall128Biomes();
        var path = TempPreviewPath("biomes");

        try
        {
            var plan = new WorldGenPipeline().Plan(profile, "test128biomes", Seed, 0, 0, path, null);

            Assert.NotNull(plan.Biomes);
            Assert.NotNull(plan.Materials);
            Assert.NotNull(plan.Decorations);

            // Every cell is classified and the heights golden hash is unchanged.
            var biomeMap = plan.Biomes!;
            var total = 0;
            for (var y = 0; y < profile.Height; ++y)
            {
                for (var x = 0; x < profile.Width; ++x)
                {
                    Assert.True(Enum.IsDefined(biomeMap.Biome[x, y]));
                    ++total;
                }
            }

            Assert.Equal((long)profile.Width * profile.Height, total);
            Assert.Equal(GoldenHeightsSha256,
                Sha256(HeightsBytes(plan.Terrain)).ToUpperInvariant());

            // Biome percentages sum to one within tolerance.
            var percentages = plan.Statistics.BiomePercentages!;
            var sum = 0.0;
            foreach (var fraction in percentages.Values) sum += fraction;
            Assert.InRange(sum, 1.0 - 0.001, 1.0 + 0.001);

            // Materials cover the footprint.
            var materials = plan.Materials!;
            Assert.Equal(profile.Width, materials.Width);
            Assert.Equal(profile.Height, materials.Height);
            Assert.False(string.IsNullOrEmpty(materials.SurfaceTemplate[0, 0]));

            // Decoration placements sit on the surface top of their column.
            foreach (var placement in plan.Decorations!)
            {
                Assert.InRange(placement.X, 0, profile.Width - 1);
                Assert.InRange(placement.Y, 0, profile.Height - 1);
                Assert.Equal(plan.Terrain.Heights[placement.X, placement.Y] + 1, placement.Z);
            }

            Assert.Equal(plan.Decorations.Count, plan.Statistics.DecorationsTotal);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Plan_WithBiomes_IsDeterministicAcrossRuns()
    {
        var profile = TestProfiles.CreateSmall128Biomes();
        var firstPath = TempPreviewPath("biomesdet1");
        var secondPath = TempPreviewPath("biomesdet2");

        try
        {
            var first = new WorldGenPipeline().Plan(profile, "test128biomes", Seed, 0, 0,
                firstPath, null);
            var second = new WorldGenPipeline().Plan(profile, "test128biomes", Seed, 0, 0,
                secondPath, null);

            for (var y = 0; y < profile.Height; ++y)
            {
                for (var x = 0; x < profile.Width; ++x)
                {
                    Assert.Equal(first.Biomes!.Biome[x, y], second.Biomes!.Biome[x, y]);
                    Assert.Equal(first.Materials!.SurfaceTemplate[x, y],
                        second.Materials!.SurfaceTemplate[x, y]);
                    Assert.Equal(first.Materials.SubSurfaceTemplate[x, y],
                        second.Materials.SubSurfaceTemplate[x, y]);
                    Assert.Equal(first.Materials.SubSurfaceDepth[x, y],
                        second.Materials.SubSurfaceDepth[x, y]);
                    Assert.Equal(first.Materials.SurfaceModifier[x, y],
                        second.Materials.SurfaceModifier[x, y]);
                }
            }

            Assert.Equal(first.Decorations!.Count, second.Decorations!.Count);
            for (var i = 0; i < first.Decorations.Count; ++i)
            {
                var a = first.Decorations[i];
                var b = second.Decorations[i];
                Assert.Equal(a.TemplateName, b.TemplateName);
                Assert.Equal(a.X, b.X);
                Assert.Equal(a.Y, b.Y);
                Assert.Equal(a.Z, b.Z);
                Assert.Equal(a.Biome, b.Biome);
                Assert.Equal(a.PoolIndex, b.PoolIndex);
            }

            Assert.Equal(Sha256(File.ReadAllBytes(firstPath)), Sha256(File.ReadAllBytes(secondPath)));
        }
        finally
        {
            File.Delete(firstPath);
            File.Delete(secondPath);
        }
    }

    [Fact]
    public void Plan_WithBiomes_GoldenPreviewMatchesHash()
    {
        var profile = TestProfiles.CreateSmall128Biomes();
        var path = TempPreviewPath("biomesgolden");

        try
        {
            new WorldGenPipeline().Plan(profile, "test128biomes", Seed, 0, 0, path, null);
            var actual = Sha256(File.ReadAllBytes(path));

            Assert.True(string.Equals(actual, GoldenBiomesPreviewSha256,
                    StringComparison.OrdinalIgnoreCase),
                $"Golden biome preview hash mismatch: expected {GoldenBiomesPreviewSha256}, " +
                $"actual {actual}. If the change (e.g. a palette edit) was intentional, " +
                "regenerate the constant as described in its documentation.");
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Plan_WithoutBiomes_SkipsBiomeStagesAndSaysSo()
    {
        var profile = TestProfiles.CreateSmall128();
        var path = TempPreviewPath("nobiomes");

        try
        {
            var plan = new WorldGenPipeline().Plan(profile, "test128", Seed, 0, 0, path, null);

            Assert.Null(plan.Biomes);
            Assert.Null(plan.Materials);
            Assert.Null(plan.Decorations);
            Assert.Null(plan.Statistics.BiomePercentages);
            Assert.Null(plan.Statistics.DecorationCounts);
            Assert.Equal(0, plan.Statistics.DecorationsTotal);
            Assert.Contains("Biomes: not configured", plan.Statistics.Format());

            // The biome-free preview hash is unchanged from card 2.
            Assert.Equal(GoldenPreviewSha256, Sha256(File.ReadAllBytes(path)));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Plan_WithCaveLevels_ProducesCaveMapAndCavePreviews()
    {
        var profile = TestProfiles.CreateSmall128();
        profile.Caves = new CaveOptions { ShaftsPerLevelPair = 2, SurfaceMouths = 2 };
        var path = TempPreviewPath("caves");
        WorldGenPlan? plan = null;

        try
        {
            plan = new WorldGenPipeline().Plan(profile, "test128", Seed, 0, 0, path, null);

            Assert.NotNull(plan.Caves);
            var cave = plan.Caves!;
            Assert.Single(cave.Levels);
            Assert.True(cave.LevelOpenCounts[0] > 0, "The cave level should contain open cells.");
            Assert.NotNull(plan.Statistics.Caves);
            Assert.Contains("Cave level 1", plan.Statistics.Format());

            var cavePreview = Assert.Single(plan.CavePreviewPaths);
            Assert.True(File.Exists(cavePreview), "The cave preview PNG should exist.");
            Assert.Contains("caves_", cavePreview);

            // The surface preview is byte-identical to the 3b golden image: caves must not
            // perturb surface pixels.
            Assert.Equal(GoldenPreviewSha256, Sha256(File.ReadAllBytes(path)));
        }
        finally
        {
            File.Delete(path);
            if (plan is not null)
            {
                foreach (var cavePreview in plan.CavePreviewPaths)
                {
                    File.Delete(cavePreview);
                }
            }
        }
    }

    [Fact]
    public void Plan_WithCaves_IsDeterministic()
    {
        var profile = TestProfiles.CreateSmall128Biomes();
        profile.Caves = new CaveOptions { ShaftsPerLevelPair = 2, SurfaceMouths = 1 };
        var firstPath = TempPreviewPath("cavedet1");
        var secondPath = TempPreviewPath("cavedet2");

        try
        {
            var first = new WorldGenPipeline().Plan(profile, "test128", Seed, 0, 0, firstPath, null);
            var second = new WorldGenPipeline().Plan(profile, "test128", Seed, 0, 0, secondPath, null);

            Assert.Equal(HashCaveMap(first.Caves!), HashCaveMap(second.Caves!));
            Assert.Equal(Sha256(File.ReadAllBytes(firstPath)), Sha256(File.ReadAllBytes(secondPath)));
            Assert.Equal(first.CavePreviewPaths.Count, second.CavePreviewPaths.Count);
            for (var i = 0; i < first.CavePreviewPaths.Count; ++i)
            {
                Assert.Equal(Sha256(File.ReadAllBytes(first.CavePreviewPaths[i])),
                    Sha256(File.ReadAllBytes(second.CavePreviewPaths[i])));
            }
        }
        finally
        {
            File.Delete(firstPath);
            File.Delete(secondPath);
            foreach (var path in new[] { firstPath, secondPath })
            {
                var directory = Path.GetDirectoryName(path)!;
                var prefix = Path.GetFileNameWithoutExtension(path);
                foreach (var file in Directory.GetFiles(directory, prefix + "*"))
                {
                    File.Delete(file);
                }
            }
        }
    }

    [Fact]
    public void Plan_WithoutCaveLevels_ProducesNoCaveOutputs()
    {
        var profile = TestProfiles.CreateSmall128();
        profile.CaveLevels = null;
        profile.Caves = null;
        var path = TempPreviewPath("nocaves");

        try
        {
            var plan = new WorldGenPipeline().Plan(profile, "test128", Seed, 0, 0, path, null);

            Assert.Null(plan.Caves);
            Assert.Empty(plan.CavePreviewPaths);
            Assert.Null(plan.Statistics.Caves);
            Assert.DoesNotContain("Cave level", plan.Statistics.Format());
            Assert.Equal(GoldenPreviewSha256, Sha256(File.ReadAllBytes(path)));
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>
    ///     SHA-256 hash of the golden 128x128 cave preview PNG for the shallowest cave
    ///     level, produced with the test baseline profile, its single cave level, and the
    ///     fixed seed above.
    ///
    ///     To regenerate after an intentional change: run this test once and read the
    ///     actual hash from the assertion failure message, then update this constant.
    ///     Pinned for the worldgen caves card on x64 Debian, .NET 10.0.12.
    /// </summary>
    private const string GoldenCavePreviewSha256 =
        "26C28ED922F59F4D6E9484256EB8608B461605C80214137B65F8952FA0F1729E";

    [Fact]
    public void Plan_CaveGoldenPreview_MatchesHash()
    {
        var profile = TestProfiles.CreateSmall128();
        profile.Caves = new CaveOptions();
        var path = TempPreviewPath("cavegolden");
        WorldGenPlan? plan = null;

        try
        {
            plan = new WorldGenPipeline().Plan(profile, "test128", Seed, 0, 0, path, null);
            var cavePreview = Assert.Single(plan.CavePreviewPaths);
            var actual = Sha256(File.ReadAllBytes(cavePreview));

            Assert.True(string.Equals(actual, GoldenCavePreviewSha256,
                    StringComparison.OrdinalIgnoreCase),
                $"Golden cave preview hash mismatch: expected {GoldenCavePreviewSha256}, " +
                $"actual {actual}. If the change (e.g. a palette or shading edit) was " +
                "intentional, regenerate the constant as described in its documentation.");
        }
        finally
        {
            File.Delete(path);
            if (plan is not null)
            {
                foreach (var cavePreview in plan.CavePreviewPaths) File.Delete(cavePreview);
            }
        }
    }

    /// <summary>
    ///     Serializes a cave map's open, floor, and carve arrays row-major for hashing.
    /// </summary>
    /// <param name="caves">Cave map to serialize.</param>
    /// <returns>Serialized bytes.</returns>
    private static byte[] HashCaveMap(CaveMap caves)
    {
        var bytes = new List<byte>();
        foreach (var level in caves.Levels)
        {
            for (var y = 0; y < caves.Height; ++y)
            {
                for (var x = 0; x < caves.Width; ++x)
                {
                    bytes.Add(level.Open[x, y] ? (byte)1 : (byte)0);
                    bytes.Add(level.CarveHeight[x, y]);
                    var value = level.FloorZ[x, y];
                    bytes.Add((byte)value);
                    bytes.Add((byte)(value >> 8));
                    bytes.Add((byte)(value >> 16));
                    bytes.Add((byte)(value >> 24));
                }
            }
        }

        return bytes.ToArray();
    }

    /// <summary>
    ///     Computes the SHA-256 hash of a byte array as lowercase hex.
    /// </summary>
    /// <param name="bytes">Bytes to hash.</param>
    /// <returns>Hex hash string.</returns>
    private static string Sha256(byte[] bytes)
    {
        return Convert.ToHexString(SHA256.HashData(bytes));
    }

    /// <summary>
    ///     Returns a temporary preview path.
    /// </summary>
    private static string TempPreviewPath(string tag)
    {
        return Path.Combine(Path.GetTempPath(), $"worldgen-test-{tag}-{Guid.NewGuid():N}.png");
    }
}
