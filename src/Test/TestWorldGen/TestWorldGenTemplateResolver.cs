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
using Sovereign.ServerCore.Systems.WorldGeneration;
using Xunit;

namespace TestWorldGen;

/// <summary>
///     Tests of plan-time template resolution against the template name indexer.
/// </summary>
public class TestWorldGenTemplateResolver
{
    [Fact]
    public void Resolve_AllNamesPresent_ProducesIdsOnPlan()
    {
        var profile = TestProfiles.CreateSmall128Biomes();
        var templates = TestTemplateIndexers.Create(new Dictionary<string, ulong>
        {
            ["Bedrock"] = 0x7FFE000000000010,
            ["Water"] = 0x7FFE000000000001,
            ["Shale"] = 0x7FFE000000000009,
            ["Granite"] = 0x7FFE00000000000A,
            ["Basalt"] = 0x7FFE00000000000B,
            ["Gravel"] = 0x7FFE00000000000C,
            ["Sand"] = 0x7FFE000000000004,
            ["Grass"] = 0x7FFE000000000000,
            ["Dirt"] = 0x7FFE000000000003,
            ["Snow"] = 0x7FFE00000000000D,
            ["Oak Tree"] = 0x7FFE000000000011,
            ["Boulder"] = 0x7FFE000000000014,
            ["Pine Tree"] = 0x7FFE000000000012,
            ["Acacia Tree"] = 0x7FFE000000000013,
            ["Dead Bush"] = 0x7FFE000000000017
        });
        var resolver = new WorldGenTemplateResolver(templates);

        var resolved = resolver.Resolve(profile, "test");

        Assert.Equal(0x7FFE000000000000UL, resolved.IdsByName["Grass"]);
        Assert.Equal(0x7FFE000000000011UL, resolved.IdsByName["Oak Tree"]);
        Assert.Equal(0x7FFE000000000010UL, resolved.IdsByName["Bedrock"]);
        Assert.True(resolved.TryGetId("oak tree", out var caseInsensitive));
        Assert.Equal(0x7FFE000000000011UL, caseInsensitive);
    }

    [Fact]
    public void Resolve_UnknownNames_FailWithConsolidatedError()
    {
        var profile = TestProfiles.CreateSmall128Biomes();
        profile.BedrockTemplate = "Missing Rock";
        var templates = TestTemplateIndexers.Create(new Dictionary<string, ulong>
        {
            ["Bedrock"] = 0x7FFE000000000010,
            ["Water"] = 0x7FFE000000000001,
            ["Shale"] = 0x7FFE000000000009,
            ["Granite"] = 0x7FFE00000000000A,
            ["Basalt"] = 0x7FFE00000000000B,
            ["Gravel"] = 0x7FFE00000000000C,
            ["Sand"] = 0x7FFE000000000004,
            ["Grass"] = 0x7FFE000000000000,
            ["Dirt"] = 0x7FFE000000000003
            // Decoration names deliberately missing.
        });
        var resolver = new WorldGenTemplateResolver(templates);

        var exception = Assert.Throws<WorldGenTemplateResolutionException>(
            () => resolver.Resolve(profile, "test"));

        var message = exception.Message;
        Assert.Contains("Unknown templates:", message);
        // Every missing name is reported, not just the first.
        Assert.Contains("\"Oak Tree\"", message);
        Assert.Contains("\"Boulder\"", message);
        Assert.Contains("\"Pine Tree\"", message);
        Assert.Contains("\"Dead Bush\"", message);
        Assert.DoesNotContain("\"Grass\"", message);
        Assert.Contains("Data/Worldgen/test.json", message);
    }

    [Fact]
    public void Resolve_DuplicateNames_ResolvedOnce()
    {
        var profile = TestProfiles.CreateSmall128();
        var templates = TestTemplateIndexers.Create(new Dictionary<string, ulong>
        {
            ["Bedrock"] = 0x7FFE000000000010,
            ["Water"] = 0x7FFE000000000001,
            ["Shale"] = 0x7FFE000000000009,
            ["Granite"] = 0x7FFE00000000000A,
            ["Basalt"] = 0x7FFE00000000000B
        });
        var resolver = new WorldGenTemplateResolver(templates);

        var resolved = resolver.Resolve(profile, "test");

        Assert.Equal(5, resolved.IdsByName.Count);
    }
}
