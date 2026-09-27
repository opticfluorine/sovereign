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
using Sovereign.WorldGen.Noise;
using Xunit;

namespace TestWorldGen;

/// <summary>
///     Unit tests for seeded noise and sub-seed derivation.
/// </summary>
public class TestSeededNoise
{
    [Fact]
    public void Sample_SameSeed_ProducesSameSequence()
    {
        var first = new SeededNoise(0xDEADBEEF);
        var second = new SeededNoise(0xDEADBEEF);

        for (var i = 0; i < 1000; ++i)
        {
            var x = i * 0.731f;
            var y = i * 1.317f;
            Assert.Equal(first.Sample(x, y), second.Sample(x, y));
        }
    }

    [Fact]
    public void Sample_DifferentSeeds_ProduceDifferentSequences()
    {
        var first = new SeededNoise(1);
        var second = new SeededNoise(2);

        var differences = 0;
        for (var i = 0; i < 100; ++i)
        {
            if (MathF.Abs(first.Sample(i * 3.1f, i * 5.7f) - second.Sample(i * 3.1f, i * 5.7f)) > 1e-4f)
            {
                ++differences;
            }
        }

        Assert.True(differences > 90, $"Only {differences}/100 samples differed between seeds.");
    }

    [Fact]
    public void Sample_Distribution_HasNearZeroMeanAndIsBounded()
    {
        var noise = new SeededNoise(42);
        double sum = 0;
        const int count = 10000;
        const float bound = 1.1f;

        for (var i = 0; i < count; ++i)
        {
            var value = noise.Sample(i * 0.137f, i * 0.291f);
            sum += value;
            Assert.InRange(value, -bound, bound);
        }

        var mean = sum / count;
        Assert.InRange(mean, -0.05, 0.05);
    }

    [Fact]
    public void Fbm_AndRidged_AreDeterministicAndBounded()
    {
        var first = new SeededNoise(7);
        var second = new SeededNoise(7);

        for (var i = 0; i < 500; ++i)
        {
            var x = i * 11.3f;
            var y = i * 7.9f;

            var fbm = first.Fbm(x, y, 256f, 5);
            Assert.Equal(fbm, second.Fbm(x, y, 256f, 5));
            Assert.InRange(fbm, -1.1f, 1.1f);

            var ridged = first.Ridged(x, y, 128f, 4);
            Assert.Equal(ridged, second.Ridged(x, y, 128f, 4));
            Assert.InRange(ridged, 0f, 1f);

            var warped = first.DomainWarpedFbm(x, y, 512f, 5);
            Assert.Equal(warped, second.DomainWarpedFbm(x, y, 512f, 5));
            Assert.InRange(warped, -1.1f, 1.1f);
        }
    }

    [Fact]
    public void DeriveSubSeed_IsDeterministic()
    {
        Assert.Equal(SeedDerivation.DeriveSubSeed(12345, "Stage"),
            SeedDerivation.DeriveSubSeed(12345, "Stage"));
    }

    [Fact]
    public void DeriveSubSeed_DistinctStageNames_YieldDistinctSeeds()
    {
        var seeds = new HashSet<ulong>();
        foreach (var stage in new[] { "TerrainFields", "TerrainShape", "FlowRouting", "RiverExtraction" })
        {
            seeds.Add(SeedDerivation.DeriveSubSeed(999, stage));
        }

        Assert.Equal(4, seeds.Count);
    }

    [Fact]
    public void DeriveSubSeed_DistinctRootSeeds_YieldDistinctSeeds()
    {
        var seeds = new HashSet<ulong>();
        for (ulong root = 0; root < 100; ++root)
        {
            seeds.Add(SeedDerivation.DeriveSubSeed(root, "TerrainFields"));
        }

        Assert.Equal(100, seeds.Count);
    }
}
