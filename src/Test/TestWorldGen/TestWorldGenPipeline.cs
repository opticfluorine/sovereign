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
using System.IO;
using System.Security.Cryptography;
using Sovereign.WorldGen;
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
    ///     The pipeline is deterministic, so the hash is stable across runs on a given runtime.
    /// </summary>
    private const string GoldenPreviewSha256 =
        "4CF2E2F70BE8379BD5CD6CFFFDCA1EA75253DC88D28308BE51536E0D5DD3B568";

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
            new WorldGenPipeline().Plan(profile, "test128", Seed, 0, 0, path, null);
            var actual = Sha256(File.ReadAllBytes(path));

            if (GoldenPreviewSha256.StartsWith("REPLACE"))
            {
                Assert.Fail(
                    $"Golden hash is not set; copy this value into GoldenPreviewSha256: {actual}");
            }

            Assert.True(string.Equals(actual, GoldenPreviewSha256, StringComparison.OrdinalIgnoreCase),
                $"Golden preview hash mismatch: expected {GoldenPreviewSha256}, actual {actual}. " +
                "If the generation algorithms changed intentionally, regenerate the constant " +
                "as described in its documentation.");
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Plan_ReportsSaneStatistics()
    {
        var path = TempPreviewPath("stats");

        try
        {
            var plan = new WorldGenPipeline().Plan(TestProfiles.CreateSmall128(), "test128", Seed,
                5, -7, path, null);

            var stats = plan.Statistics;
            Assert.Equal(128, stats.Width);
            Assert.Equal(128, stats.Height);
            Assert.Equal(128L * 128, stats.LandCells + stats.WaterCells);
            Assert.True(stats.LandCells > 0, "The plan should contain land.");
            Assert.True(stats.WaterCells > 0, "The plan should contain water.");
            Assert.True(stats.RiverCount > 0, "The plan should contain rivers.");
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
