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
using Sovereign.WorldGen.Layout;
using Xunit;

namespace TestWorldGen;

/// <summary>
///     Unit tests for the named layout presets.
/// </summary>
public class TestLayoutPresets
{
    private const ulong Seed = 987654321;

    [Fact]
    public void Presets_Continents3_HasDocumentedGeometry()
    {
        var anchors = LayoutPresets.Resolve("continents3", 6, Seed);

        Assert.Equal(3, anchors.Count);
        Assert.Equal(0.18f, anchors[0].X, 3);
        Assert.Equal(0.50f, anchors[1].X, 3);
        Assert.Equal(0.82f, anchors[2].X, 3);
        Assert.Equal(0.11f, anchors[1].Radius, 3);
        Assert.Equal(0.7f, anchors[1].Weight, 3);
        Assert.Equal(1.0f, anchors[1].MountainBias, 3);
    }

    [Fact]
    public void Presets_Pangaea_HasSingleCentralAnchor()
    {
        var anchors = LayoutPresets.Resolve("pangaea", 6, Seed);

        var anchor = Assert.Single(anchors);
        Assert.Equal(0.5f, anchor.X, 3);
        Assert.Equal(0.5f, anchor.Y, 3);
        Assert.Equal(0.30f, anchor.Radius, 3);
        Assert.Equal(1.0f, anchor.Weight, 3);
    }

    [Fact]
    public void Presets_EastWest_HasTwoStraddlingAnchors()
    {
        var anchors = LayoutPresets.Resolve("eastwest", 6, Seed);

        Assert.Equal(2, anchors.Count);
        Assert.Equal(0.28f, anchors[0].X, 3);
        Assert.Equal(0.72f, anchors[1].X, 3);
        Assert.All(anchors, a => Assert.Equal(0.17f, a.Radius, 3));
    }

    [Fact]
    public void Presets_Archipelago_RespectsCountGeometryAndSeparation()
    {
        var anchors = LayoutPresets.Resolve("archipelago", 6, Seed);

        Assert.InRange(anchors.Count, 8, 12);
        foreach (var anchor in anchors)
        {
            Assert.InRange(anchor.Radius, 0.06f, 0.10f);
            Assert.InRange(anchor.Weight, 0.5f, 0.8f);
            Assert.InRange(anchor.X, 0.15f, 0.85f);
            Assert.InRange(anchor.Y, 0.15f, 0.85f);
        }

        for (var i = 0; i < anchors.Count; ++i)
        {
            for (var j = i + 1; j < anchors.Count; ++j)
            {
                var dx = anchors[i].X - anchors[j].X;
                var dy = anchors[i].Y - anchors[j].Y;
                var distance = MathF.Sqrt(dx * dx + dy * dy);
                var required = 2f * MathF.Max(anchors[i].Radius, anchors[j].Radius);
                Assert.True(distance >= required - 1e-3f,
                    $"Archipelago anchors {i} and {j} are too close: {distance} < {required}.");
            }
        }
    }

    [Fact]
    public void Presets_Random_IsDeterministicAndRespectsCount()
    {
        var first = LayoutPresets.Resolve("random", 10, Seed);
        var second = LayoutPresets.Resolve("random", 10, Seed);

        Assert.Equal(10, first.Count);
        Assert.Equal(first.Count, second.Count);
        for (var i = 0; i < first.Count; ++i)
        {
            Assert.Equal(first[i].X, second[i].X);
            Assert.Equal(first[i].Y, second[i].Y);
            Assert.Equal(first[i].Radius, second[i].Radius);
            Assert.InRange(first[i].Radius, 0.10f, 0.16f);
        }
    }

    [Fact]
    public void Presets_Random_DiffersAcrossSeeds()
    {
        var first = LayoutPresets.Resolve("random", 6, Seed);
        var second = LayoutPresets.Resolve("random", 6, Seed + 1);

        var identical = first.Count == second.Count;
        if (identical)
        {
            for (var i = 0; i < first.Count; ++i)
            {
                identical &= first[i].X == second[i].X && first[i].Y == second[i].Y;
            }
        }

        Assert.False(identical, "Random anchors should depend on the seed.");
    }

    [Fact]
    public void Presets_IsKnown_RecognizesAllNames()
    {
        foreach (var name in LayoutPresets.Names) Assert.True(LayoutPresets.IsKnown(name));
        Assert.False(LayoutPresets.IsKnown("bogus"));
        Assert.False(LayoutPresets.IsKnown(null));
    }

    [Fact]
    public void Presets_UnknownName_Throws()
    {
        Assert.Throws<ArgumentException>(() => LayoutPresets.Resolve("bogus", 6, Seed));
    }
}
