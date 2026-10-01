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
using Sovereign.WorldGen.Layout;
using Sovereign.WorldGen.Noise;
using Sovereign.WorldGen.Output;
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
    ///     Runs the full pipeline for a test profile against a fresh staging directory,
    ///     which is removed when the plan completes.
    /// </summary>
    /// <param name="profile">Profile to run.</param>
    /// <param name="profileName">Profile name.</param>
    /// <param name="seed">World seed.</param>
    /// <param name="originX">World X coordinate of the footprint origin.</param>
    /// <param name="originY">World Y coordinate of the footprint origin.</param>
    /// <param name="previewPath">Path for the preview image.</param>
    /// <returns>The completed plan.</returns>
    private static WorldGenPlan Plan(WorldGenProfile profile, string profileName, ulong seed,
        int originX, int originY, string previewPath)
    {
        var staging = Path.Combine(Path.GetTempPath(), "worldgen-test-staging",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(staging);
        try
        {
            return new WorldGenPipeline().Plan(profile, profileName, seed, originX, originY,
                previewPath, staging, TestResolvedTemplates.ForProfile(profile), null);
        }
        finally
        {
            Directory.Delete(staging, true);
        }
    }

    /// <summary>
    ///     SHA-256 hash of the golden 128x128 preview pixel buffer (decoded RGB bytes)
    ///     produced with the test baseline profile and the fixed seed above.
    ///
    ///     To regenerate after an intentional algorithm change: run this test once and read
    ///     the actual hash from the assertion failure message, then update this constant.
    ///     Hashing pixel buffers instead of PNG file bytes keeps PNG encoding choices (e.g.
    ///     compression) out of the golden contract. Generated on x64 Debian, .NET 10.0.12.
    /// </summary>
    private const string GoldenPreviewSha256 =
        "7440B3C3F91752292F632A40D89C9CF81A51F56FD2AF24EC9E9681926466518A";

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
    ///     SHA-256 hash of the golden 128x128 preview pixel buffer (decoded RGB bytes)
    ///     produced with the biome-enabled test baseline profile
    ///     (<see cref="TestProfiles.CreateSmall128Biomes" />) and the fixed seed above. The
    ///     heights hash is shared with the biome-free profile.
    ///
    ///     The biome preview palette is load-bearing for this hash: palette edits in
    ///     <see cref="Sovereign.WorldGen.Output.PreviewRenderer" /> are golden-hash edits.
    ///     To regenerate after an intentional change: run this test once and read the actual
    ///     hash from the assertion failure message, then update this constant.
    /// </summary>
    private const string GoldenBiomesPreviewSha256 =
        "1FB9C3F05CF4410BFEA45E13DC9CD87B3060BDEF96BEAA9575E21317088283D9";

    [Fact]
    public void Plan_SameSeedAndProfile_ProducesIdenticalHeightsAndPreview()
    {
        var profile = TestProfiles.CreateSmall128();
        var firstPath = TempPreviewPath("det1");
        var secondPath = TempPreviewPath("det2");

        var first = Plan(profile, "test128", Seed, 0, 0, firstPath);
        var second = Plan(profile, "test128", Seed, 0, 0, secondPath);

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
            var plan = Plan(profile, "test128", Seed, 0, 0, path);
            var image = PngReader.Read(path);
            Assert.Equal(128, image.Width);
            Assert.Equal(128, image.Height);
            var actualPreview = Sha256(image.Pixels);
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

    [Fact]
    public void Preview_PngWriterRoundTrip_PreservesPixels()
    {
        var profile = TestProfiles.CreateSmall128();
        var path = TempPreviewPath("roundtrip");

        try
        {
            Plan(profile, "test128", Seed, 0, 0, path);
            var decoded = PngReader.Read(path);

            Assert.Equal(128, decoded.Width);
            Assert.Equal(128, decoded.Height);
            Assert.Equal(128 * 128 * 3, decoded.Pixels.Length);
            Assert.NotEqual(new byte[decoded.Pixels.Length], decoded.Pixels);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Preview_PngWriterRoundTrip_MatchesRenderedPixels()
    {
        var profile = TestProfiles.CreateSmall128();
        var path = TempPreviewPath("roundtrip2");

        try
        {
            var (map, continentalness) = BuildTerrain(profile);
            var rendered = new PreviewRenderer().Render(map, continentalness, profile,
                PreviewOptions.DefaultMaxDimension);

            PngWriter.WritePng(path, rendered.Width, rendered.Height, rendered.Pixels);
            var decoded = PngReader.Read(path);

            Assert.Equal(rendered.Width, decoded.Width);
            Assert.Equal(rendered.Height, decoded.Height);
            Assert.Equal(rendered.Pixels, decoded.Pixels);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Preview_MaxDimensionKnob_GovernsOutputSize()
    {
        var downscaledProfile = TestProfiles.CreateSmall128();
        downscaledProfile.Width = 512;
        downscaledProfile.Height = 512;
        downscaledProfile.Preview = new PreviewOptions { MaxDimension = 256 };

        var fullProfile = TestProfiles.CreateSmall128();
        fullProfile.Width = 512;
        fullProfile.Height = 512;

        var downscaledPath = TempPreviewPath("knob256");
        var fullPath = TempPreviewPath("knobmax");

        try
        {
            var downscaled = Plan(downscaledProfile, "test512", Seed, 0, 0, downscaledPath);
            var full = Plan(fullProfile, "test512", Seed, 0, 0, fullPath);

            Assert.Equal(512, downscaled.Statistics.Width);
            var downscaledImage = PngReader.Read(downscaledPath);
            var fullImage = PngReader.Read(fullPath);
            Assert.Equal(256, downscaledImage.Width);
            Assert.Equal(256, downscaledImage.Height);
            Assert.Equal(512, fullImage.Width);
            Assert.Equal(512, fullImage.Height);
        }
        finally
        {
            File.Delete(downscaledPath);
            File.Delete(fullPath);
        }
    }

    /// <summary>
    ///     Runs the terrain stages for renderer tests.
    /// </summary>
    /// <param name="profile">Profile to run.</param>
    /// <returns>Shaped terrain map and its continentalness classification.</returns>
    private static (TerrainMap Map, ContinentalnessResult Continentalness) BuildTerrain(
        WorldGenProfile profile)
    {
        var fields = new TerrainFieldStack().Sample(profile.Width, profile.Height,
            SeedDerivation.DeriveSubSeed(Seed, "TerrainFields"), profile.Terrain);
        var continentalness = new ContinentalnessStage().Apply(fields, profile.Width,
            profile.Height, profile.Terrain);
        var map = new TerrainShapeStage().Apply(fields, continentalness, profile,
            SeedDerivation.DeriveSubSeed(Seed, "TerrainShape")).Map;
        return (map, continentalness);
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
            var plan = Plan(profile, "test128", Seed, 5, -7, path);

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
            var plan = Plan(profile, "test128", Seed, 0, 0, path);
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
            var plan = Plan(profile, "test128biomes", Seed, 0, 0, path);

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
            var first = Plan(profile, "test128biomes", Seed, 0, 0, firstPath);
            var second = Plan(profile, "test128biomes", Seed, 0, 0, secondPath);

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
            Plan(profile, "test128biomes", Seed, 0, 0, path);
            var image = PngReader.Read(path);
            Assert.Equal(128, image.Width);
            Assert.Equal(128, image.Height);
            var actual = Sha256(image.Pixels);

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
    public void Plan_WithLayout_ProducesLayoutReportAndOverlay()
    {
        var profile = TestProfiles.CreateSmall128ThreeLands();
        var maskedPath = TempPreviewPath("layout");
        var plainPath = TempPreviewPath("layoutplain");

        try
        {
            var plan = Plan(profile, "threelands", Seed, 0, 0, maskedPath);
            var plain = Plan(TestProfiles.CreateSmall128Biomes(), "test128biomes", Seed, 0, 0,
                plainPath);

            Assert.NotNull(plan.Statistics.Layout);
            var report = plan.Statistics.Layout!;
            Assert.Equal(3, report.Anchors.Count);
            Assert.Equal(3, report.SignificantAnchorCount);
            Assert.Contains("Layout:", plan.Statistics.Format());

            // The anchor overlay changes preview pixels relative to the unmasked plan.
            Assert.NotEqual(Sha256(PngReader.Read(plainPath).Pixels),
                Sha256(PngReader.Read(maskedPath).Pixels));
        }
        finally
        {
            File.Delete(maskedPath);
            File.Delete(plainPath);
        }
    }

    [Fact]
    public void Preview_AnchorOverlay_IsControllable()
    {
        var profile = TestProfiles.CreateSmall128();
        var (map, continentalness) = BuildTerrain(profile);
        var options = new LayoutOptions
        {
            Strength = 1f,
            Anchors = new List<LayoutAnchor>
            {
                new() { X = 0.5f, Y = 0.5f, Radius = 0.2f, Weight = 1f }
            }
        };
        var layout = new LayoutMaskStage().Build(profile.Width, profile.Height, options, Seed);
        var renderer = new PreviewRenderer();

        var noLayout = renderer.Render(map, continentalness, profile,
            PreviewOptions.DefaultMaxDimension);
        var withOverlay = renderer.Render(map, continentalness, profile,
            PreviewOptions.DefaultMaxDimension, null, null, layout);
        var withoutOverlay = renderer.Render(map, continentalness, profile,
            PreviewOptions.DefaultMaxDimension, null, null, layout, null, false);

        // The overlay changes pixels, while disabling it reproduces the plain render exactly.
        Assert.NotEqual(noLayout.Pixels, withOverlay.Pixels);
        Assert.Equal(noLayout.Pixels, withoutOverlay.Pixels);
    }

    [Fact]
    public void Plan_WithLayoutAnchorOverlayDisabled_DropsOverlay()
    {
        var withOverlay = TestProfiles.CreateSmall128ThreeLands();
        var withoutOverlay = TestProfiles.CreateSmall128ThreeLands();
        withoutOverlay.Preview = new PreviewOptions { ShowAnchorOverlay = false };
        var overlayPath = TempPreviewPath("overlayon");
        var plainPath = TempPreviewPath("overlayoff");

        try
        {
            Plan(withOverlay, "threelands", Seed, 0, 0, overlayPath);
            Plan(withoutOverlay, "threelands", Seed, 0, 0, plainPath);

            Assert.NotEqual(Sha256(PngReader.Read(overlayPath).Pixels),
                Sha256(PngReader.Read(plainPath).Pixels));
        }
        finally
        {
            File.Delete(overlayPath);
            File.Delete(plainPath);
        }
    }

    [Fact]
    public void Plan_WithLayout_IsDeterministicAcrossRuns()
    {
        var profile = TestProfiles.CreateSmall128ThreeLands();
        var firstPath = TempPreviewPath("layoutdet1");
        var secondPath = TempPreviewPath("layoutdet2");

        try
        {
            var first = Plan(profile, "threelands", Seed, 0, 0, firstPath);
            var second = Plan(profile, "threelands", Seed, 0, 0, secondPath);

            Assert.Equal(Sha256(HeightsBytes(first.Terrain)), Sha256(HeightsBytes(second.Terrain)));
            Assert.Equal(Sha256(File.ReadAllBytes(firstPath)),
                Sha256(File.ReadAllBytes(secondPath)));
            Assert.Equal(first.Statistics.Layout!.Anchors.Count,
                second.Statistics.Layout!.Anchors.Count);
        }
        finally
        {
            File.Delete(firstPath);
            File.Delete(secondPath);
        }
    }

    [Fact]
    public void Plan_StageCallbacks_CarryPercentages()
    {
        var profile = TestProfiles.CreateSmall128Biomes();
        var messages = new List<string>();
        var staging = Path.Combine(Path.GetTempPath(), "worldgen-test-staging",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(staging);
        var path = TempPreviewPath("progress");

        try
        {
            new WorldGenPipeline().Plan(profile, "baseline", Seed, 0, 0, path, staging,
                TestResolvedTemplates.ForProfile(profile), messages.Add);

            Assert.Contains(messages, m =>
                m.StartsWith("Terrain:", StringComparison.Ordinal) && m.EndsWith("%)"));
            Assert.Contains(messages, m =>
                m.StartsWith("Hydrology:", StringComparison.Ordinal) && m.EndsWith("%)"));
            Assert.Contains(messages, m =>
                m.StartsWith("Rendering preview", StringComparison.Ordinal)
                && m.EndsWith("%)"));
            Assert.Contains(messages, m =>
                m.StartsWith("Assembly:", StringComparison.Ordinal) && m.EndsWith("%)"));
        }
        finally
        {
            File.Delete(path);
            Directory.Delete(staging, true);
        }
    }


    [Fact]
    public void Plan_WithInactiveLayout_IsByteIdenticalToCard5()
    {
        var profile = TestProfiles.CreateSmall128Biomes();
        profile.Layout = new LayoutOptions
        {
            Strength = 0f,
            Anchors = new List<LayoutAnchor>
            {
                new() { X = 0.5f, Y = 0.5f, Radius = 0.2f, Weight = 1f }
            }
        };
        var path = TempPreviewPath("layoutinactive");

        try
        {
            var plan = Plan(profile, "test128biomes", Seed, 0, 0, path);

            Assert.Null(plan.Statistics.Layout);
            Assert.Equal(GoldenHeightsSha256,
                Sha256(HeightsBytes(plan.Terrain)).ToUpperInvariant());
            Assert.Equal(GoldenBiomesPreviewSha256,
                Sha256(PngReader.Read(path).Pixels).ToUpperInvariant());
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Plan_WithUnmatchedLayoutAnchor_ResamplesAndWarns()
    {
        var profile = TestProfiles.CreateSmall128();
        profile.Layout = new LayoutOptions
        {
            Strength = 0.05f,
            Anchors = new List<LayoutAnchor>
            {
                new() { X = 0.1f, Y = 0.1f, Radius = 0.1f, Weight = 1f, Jitter = 0.1f }
            }
        };
        var path = TempPreviewPath("layoutwarn");

        try
        {
            var plan = Plan(profile, "test128", Seed, 0, 0, path);

            Assert.NotNull(plan.Statistics.Layout);
            var report = plan.Statistics.Layout!;
            Assert.False(report.AllSignificantAnchorsMatched);
            Assert.Contains(report.Warnings, w => w.Contains("unmatched", StringComparison.Ordinal));
            Assert.Contains("Layout warning", plan.Statistics.Format());
            Assert.True(File.Exists(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Plan_WithSeparateLayout_IsDeterministic()
    {
        var profile = TestProfiles.CreateSmall128ThreeLands();
        profile.Layout!.Connectivity = LayoutConnectivity.Separate;
        var firstPath = TempPreviewPath("separatedet1");
        var secondPath = TempPreviewPath("separatedet2");

        try
        {
            var first = Plan(profile, "threelands", Seed, 0, 0, firstPath);
            var second = Plan(profile, "threelands", Seed, 0, 0, secondPath);

            Assert.Equal(Sha256(HeightsBytes(first.Terrain)),
                Sha256(HeightsBytes(second.Terrain)));
            Assert.Equal(Sha256(File.ReadAllBytes(firstPath)),
                Sha256(File.ReadAllBytes(secondPath)));
            Assert.Equal(first.Statistics.Layout!.Anchors.Count,
                second.Statistics.Layout!.Anchors.Count);
        }
        finally
        {
            File.Delete(firstPath);
            File.Delete(secondPath);
        }
    }

    [Fact]
    public void Plan_WithStrictUnmatchableLayout_Throws()
    {
        var profile = CreateAllOceanProfile(LayoutConnectivity.Strict);
        var path = TempPreviewPath("strictfail");

        try
        {
            var exception = Assert.Throws<LayoutValidationException>(
                () => Plan(profile, "test128", Seed, 0, 0, path));

            Assert.Contains("layout validation failed", exception.Message);
            Assert.Contains("unmatched", exception.Message);
            Assert.Contains("#1", exception.Message);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void Plan_WithSeparateUnmatchableLayout_WarnsButCompletes()
    {
        var profile = CreateAllOceanProfile(LayoutConnectivity.Separate);
        var path = TempPreviewPath("separatefail");

        try
        {
            var plan = Plan(profile, "test128", Seed, 0, 0, path);

            Assert.NotNull(plan.Statistics.Layout);
            var report = plan.Statistics.Layout!;
            Assert.False(report.AllSignificantAnchorsMatched);
            Assert.Contains(report.Warnings,
                w => w.Contains("unmatched", StringComparison.Ordinal));
            Assert.Contains("Layout warning", plan.Statistics.Format());
            Assert.True(File.Exists(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Plan_WithForcedJitterResolvingIdentically_SkipsRemainingAttempts()
    {
        var options = new LayoutOptions
        {
            Strength = 1f,
            Anchors = new List<LayoutAnchor>
            {
                new() { X = 0.02f, Y = 0.02f, Radius = 0.1f, Weight = 1f }
            }
        };

        // Find a seed whose first forced-jitter resample clamps back to the same anchor
        // centre as the initial attempt; that resample cannot change the terrain.
        ulong? collisionSeed = null;
        for (ulong candidate = 1; candidate <= 500 && collisionSeed is null; ++candidate)
        {
            var initial = LayoutMaskStage.ResolveAnchors(options,
                SeedDerivation.DeriveSubSeed(candidate, "Layout"), 0f);
            var resample = LayoutMaskStage.ResolveAnchors(options,
                SeedDerivation.DeriveSubSeed(candidate, "Layout.Resample1"), 0.02f);
            if (LayoutMaskStage.AnchorsEquivalent(initial, resample)) collisionSeed = candidate;
        }

        Assert.NotNull(collisionSeed);

        var profile = CreateAllOceanProfile(LayoutConnectivity.None);
        profile.Layout = options;
        var path = TempPreviewPath("skipattempt");

        try
        {
            var plan = Plan(profile, "test128", collisionSeed.Value, 0, 0, path);

            Assert.NotNull(plan.Statistics.Layout);
            var report = plan.Statistics.Layout!;
            Assert.False(report.AllSignificantAnchorsMatched);
            Assert.Equal(1, report.AttemptCount);
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>
    ///     Creates an all-ocean profile so that layout validation fails deterministically,
    ///     isolating the connectivity and resample behavior under test. The thresholds exceed
    ///     1 so that even the layout mask cannot raise any cell to land.
    /// </summary>
    /// <param name="connectivity">Connectivity mode for the layout.</param>
    /// <returns>Profile whose terrain is entirely ocean.</returns>
    private static WorldGenProfile CreateAllOceanProfile(LayoutConnectivity connectivity)
    {
        var profile = TestProfiles.CreateSmall128();
        profile.Terrain.Thresholds = new ContinentalnessThresholdOptions
        {
            Ocean = 2f, Coast = 3f, Inland = 4f
        };
        profile.Layout = new LayoutOptions
        {
            Strength = 1f,
            Connectivity = connectivity,
            Anchors = new List<LayoutAnchor>
            {
                new() { X = 0.5f, Y = 0.5f, Radius = 0.1f, Weight = 1f }
            }
        };
        return profile;
    }

    /// <summary>
    ///     SHA-256 hash of the golden 128x128 preview pixel buffer produced with the
    ///     continents3 layout test profile and the fixed seed above.
    ///
    ///     To regenerate after an intentional change: run this test once and read the actual
    ///     hash from the assertion failure message, then update this constant. Regenerated
    ///     for the worldgen 7b per-mass climate thresholds (mountain bias lowers the
    ///     alpine/snowcap line) on x64 Debian, .NET 10.0.12.
    /// </summary>
    private const string GoldenLayoutPreviewSha256 =
        "8579D9E980D04D944756892487EB4A0D90B12C71FC5443C9842F936DEC19495C";

    /// <summary>
    ///     SHA-256 hash of the masked 128x128 heightmap produced with the continents3 layout
    ///     test profile and the fixed seed above. Card 7b shifts biome classification only, so
    ///     this hash must match the worldgen 7 value.
    ///
    ///     To regenerate after an intentional terrain change: run this test once and read the
    ///     actual hash from the assertion failure message, then update this constant.
    /// </summary>
    private const string GoldenLayoutHeightsSha256 =
        "81C13F064694438EF065908B77135EEC12D40DCCF9696551887ADD681CB35EEF";

    [Fact]
    public void Plan_WithLayout_MaskedHeightsAreUnchangedByClimateBias()
    {
        var profile = TestProfiles.CreateSmall128ThreeLands();
        var path = TempPreviewPath("layoutheights");

        try
        {
            var plan = Plan(profile, "threelands", Seed, 0, 0, path);
            Assert.Equal(GoldenLayoutHeightsSha256,
                Sha256(HeightsBytes(plan.Terrain)).ToUpperInvariant());
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Plan_WithLayout_GoldenPreviewMatchesHash()
    {
        var profile = TestProfiles.CreateSmall128ThreeLands();
        var path = TempPreviewPath("layoutgolden");

        try
        {
            Plan(profile, "threelands", Seed, 0, 0, path);
            var image = PngReader.Read(path);
            Assert.Equal(128, image.Width);
            Assert.Equal(128, image.Height);
            var actual = Sha256(image.Pixels);

            Assert.True(string.Equals(actual, GoldenLayoutPreviewSha256,
                    StringComparison.OrdinalIgnoreCase),
                $"Golden layout preview hash mismatch: expected {GoldenLayoutPreviewSha256}, " +
                $"actual {actual}. If the change was intentional, regenerate the constant as " +
                "described in its documentation.");
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
            var plan = Plan(profile, "test128", Seed, 0, 0, path);

            Assert.Null(plan.Biomes);
            Assert.Null(plan.Materials);
            Assert.Null(plan.Decorations);
            Assert.Null(plan.Statistics.BiomePercentages);
            Assert.Null(plan.Statistics.DecorationCounts);
            Assert.Equal(0, plan.Statistics.DecorationsTotal);
            Assert.Contains("Biomes: not configured", plan.Statistics.Format());

            // The biome-free preview hash is unchanged from card 2.
            Assert.Equal(GoldenPreviewSha256, Sha256(PngReader.Read(path).Pixels), ignoreCase: true);
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
            plan = Plan(profile, "test128", Seed, 0, 0, path);

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
            Assert.Equal(GoldenPreviewSha256, Sha256(PngReader.Read(path).Pixels), ignoreCase: true);
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
            var first = Plan(profile, "test128", Seed, 0, 0, firstPath);
            var second = Plan(profile, "test128", Seed, 0, 0, secondPath);

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
            var plan = Plan(profile, "test128", Seed, 0, 0, path);

            Assert.Null(plan.Caves);
            Assert.Empty(plan.CavePreviewPaths);
            Assert.Null(plan.Statistics.Caves);
            Assert.DoesNotContain("Cave level", plan.Statistics.Format());
            Assert.Equal(GoldenPreviewSha256, Sha256(PngReader.Read(path).Pixels), ignoreCase: true);
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
        "32E994AFDEF0BE6679C6ABD1734047335F7E37C0CBAE54AF61731E4A24EF88DA";

    [Fact]
    public void Plan_CaveGoldenPreview_MatchesHash()
    {
        var profile = TestProfiles.CreateSmall128();
        profile.Caves = new CaveOptions();
        var path = TempPreviewPath("cavegolden");
        WorldGenPlan? plan = null;

        try
        {
            plan = Plan(profile, "test128", Seed, 0, 0, path);
            var cavePreview = Assert.Single(plan.CavePreviewPaths);
            var caveImage = PngReader.Read(cavePreview);
            Assert.Equal(128, caveImage.Width);
            Assert.Equal(128, caveImage.Height);
            var actual = Sha256(caveImage.Pixels);

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
