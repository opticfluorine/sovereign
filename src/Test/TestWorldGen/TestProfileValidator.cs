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
using System.Linq;
using Sovereign.WorldGen;
using Sovereign.WorldGen.Layout;
using Xunit;

namespace TestWorldGen;

/// <summary>
///     Unit tests for <see cref="ProfileValidator" />.
/// </summary>
public class TestProfileValidator
{
    private readonly ProfileValidator validator = new();

    [Fact]
    public void Validate_ValidProfile_HasNoIssues()
    {
        var issues = validator.Validate(TestProfiles.CreateValid());

        Assert.Empty(issues);
    }

    [Fact]
    public void Validate_WidthNotMultipleOf32_IsError()
    {
        var profile = TestProfiles.CreateValid();
        profile.Width = 100;

        AssertHasError(validator.Validate(profile), "width");
    }

    [Fact]
    public void Validate_NonPositiveDimension_IsError()
    {
        var profile = TestProfiles.CreateValid();
        profile.Height = 0;

        AssertHasError(validator.Validate(profile), "height");
    }

    [Fact]
    public void Validate_DimensionTooLarge_IsError()
    {
        var profile = TestProfiles.CreateValid();
        profile.Width = 16384 + 32;

        AssertHasError(validator.Validate(profile), "width");
    }

    [Fact]
    public void Validate_ZOrderingViolated_ReportsEachComparison()
    {
        var profile = TestProfiles.CreateValid();
        profile.BedrockZ = profile.RockFloorZ;
        profile.SeaLevelZ = profile.SurfaceMaxZ;

        var issues = validator.Validate(profile);

        Assert.Contains(issues, i => i.Severity == ProfileValidationSeverity.Error
                                     && i.Message.Contains("bedrockZ", StringComparison.Ordinal));
        Assert.Contains(issues, i => i.Severity == ProfileValidationSeverity.Error
                                     && i.Message.Contains("seaLevelZ", StringComparison.Ordinal));
    }

    [Fact]
    public void Validate_EmptyStoneBands_IsError()
    {
        var profile = TestProfiles.CreateValid();
        profile.StoneBands = new List<StoneBand>();

        AssertHasError(validator.Validate(profile), "stoneBands");
    }

    [Fact]
    public void Validate_EmptyBedrockTemplate_IsError()
    {
        var profile = TestProfiles.CreateValid();
        profile.BedrockTemplate = "  ";

        AssertHasError(validator.Validate(profile), "bedrockTemplate");
    }

    [Fact]
    public void Validate_PreviewDimensionBelowRange_IsError()
    {
        var profile = TestProfiles.CreateValid();
        profile.Preview = new PreviewOptions { MaxDimension = PreviewOptions.MinMaxDimension - 1 };

        AssertHasError(validator.Validate(profile), "preview.maxDimension");
    }

    [Fact]
    public void Validate_PreviewDimensionAboveRange_IsError()
    {
        var profile = TestProfiles.CreateValid();
        profile.Preview = new PreviewOptions { MaxDimension = PreviewOptions.MaxMaxDimension + 1 };

        AssertHasError(validator.Validate(profile), "preview.maxDimension");
    }

    [Fact]
    public void Validate_PreviewDimensionInRange_HasNoIssue()
    {
        var profile = TestProfiles.CreateValid();
        profile.Preview = new PreviewOptions { MaxDimension = PreviewOptions.MaxMaxDimension };

        Assert.DoesNotContain(validator.Validate(profile),
            i => i.Message.Contains("preview.maxDimension", StringComparison.Ordinal));
    }

    [Fact]
    public void Validate_StoneBandGap_IsError()
    {
        var profile = TestProfiles.CreateValid();
        profile.StoneBands[1].ToZ = -18;

        AssertHasError(validator.Validate(profile), "gap");
    }

    [Fact]
    public void Validate_StoneBandOverlap_IsError()
    {
        var profile = TestProfiles.CreateValid();
        profile.StoneBands[1].ToZ = -16;

        AssertHasError(validator.Validate(profile), "overlap");
    }

    [Fact]
    public void Validate_StoneBandsOutOfOrder_IsError()
    {
        var profile = TestProfiles.CreateValid();
        (profile.StoneBands[0], profile.StoneBands[1]) = (profile.StoneBands[1], profile.StoneBands[0]);

        AssertHasError(validator.Validate(profile), "ordered");
    }

    [Fact]
    public void Validate_DeepestBandNotAtRockFloor_IsError()
    {
        var profile = TestProfiles.CreateValid();
        profile.StoneBands[2].FromZ = -62;

        AssertHasError(validator.Validate(profile), "rockFloorZ");
    }

    [Fact]
    public void Validate_EmptyStoneBandTemplate_IsError()
    {
        var profile = TestProfiles.CreateValid();
        profile.StoneBands[0].Template = "";

        AssertHasError(validator.Validate(profile), "template");
    }

    [Fact]
    public void Validate_SingleStoneBand_IsWarningOnly()
    {
        var profile = TestProfiles.CreateValid();
        profile.CaveLevels = null;
        profile.Caves = null;
        profile.StoneBands = new List<StoneBand>
        {
            new() { FromZ = -63, ToZ = -1, Template = "Basalt" }
        };

        var issues = validator.Validate(profile);

        Assert.DoesNotContain(issues, i => i.Severity == ProfileValidationSeverity.Error);
        Assert.Contains(issues, i => i.Severity == ProfileValidationSeverity.Warning
                                     && i.Message.Contains("stone band", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_CaveLevelsNotDescending_IsError()
    {
        var profile = TestProfiles.CreateValid();
        profile.CaveLevels![1].FloorZ = profile.CaveLevels![0].FloorZ;

        AssertHasError(validator.Validate(profile), "descending");
    }

    [Fact]
    public void Validate_CaveLevelBelowBedrock_IsError()
    {
        var profile = TestProfiles.CreateValid();
        profile.CaveLevels![2].FloorZ = profile.BedrockZ - 1;

        AssertHasError(validator.Validate(profile), "bedrockZ");
    }

    [Fact]
    public void Validate_CaveLevelAboveStoneBands_IsError()
    {
        var profile = TestProfiles.CreateValid();
        profile.CaveLevels![0].FloorZ = profile.StoneBands[0].ToZ + 1;

        AssertHasError(validator.Validate(profile), "between");
    }

    [Fact]
    public void Validate_CaveLevelClearanceTooSmall_IsError()
    {
        var profile = TestProfiles.CreateValid();
        profile.CaveLevels![1].FloorZ = -20;

        AssertHasError(validator.Validate(profile), "clearance");
    }

    [Fact]
    public void Validate_TopmostCaveLevelTooShallow_IsError()
    {
        var profile = TestProfiles.CreateValid();
        profile.CaveLevels![0].FloorZ = -5;

        AssertHasError(validator.Validate(profile), "topmost");
    }

    [Fact]
    public void Validate_CaveHeadroomTooSmall_IsError()
    {
        var profile = TestProfiles.CreateValid();
        profile.CaveLevels![0].Headroom = 1;

        AssertHasError(validator.Validate(profile), "headroom");
    }

    [Fact]
    public void Validate_CaveHeadroomTooLarge_IsError()
    {
        var profile = TestProfiles.CreateValid();
        profile.CaveLevels![0].Headroom = 9;

        AssertHasError(validator.Validate(profile), "headroom");
    }

    [Fact]
    public void Validate_CaveCountOutOfRange_IsError()
    {
        var profile = TestProfiles.CreateValid();
        profile.Caves!.ShaftsPerLevelPair = 65;
        profile.Caves.SurfaceMouths = -1;

        var issues = validator.Validate(profile);

        Assert.Equal(2, issues.Count(i => i.Severity == ProfileValidationSeverity.Error));
    }

    [Fact]
    public void Validate_NonZeroCavesWithEmptyCaveLevels_IsError()
    {
        var profile = TestProfiles.CreateValid();
        profile.CaveLevels = new List<CaveLevel>();
        profile.Caves = new CaveOptions { ShaftsPerLevelPair = 1, SurfaceMouths = 0 };

        AssertHasError(validator.Validate(profile), "caveLevels");
    }

    [Fact]
    public void Validate_ZeroCavesWithEmptyCaveLevels_IsAllowed()
    {
        var profile = TestProfiles.CreateValid();
        profile.CaveLevels = null;
        profile.Caves = new CaveOptions { ShaftsPerLevelPair = 0, SurfaceMouths = 0 };

        Assert.Empty(validator.Validate(profile));
    }

    [Fact]
    public void Validate_PartialCavesSection_UsesDefaultsAndIsValid()
    {
        var profile = TestProfiles.CreateValid();
        profile.Caves = new CaveOptions
        {
            ShaftsPerLevelPair = 0,
            SurfaceMouths = 0,
            Porosity = 0.5
        };

        Assert.Empty(validator.Validate(profile));
        Assert.Equal(CaveOptions.DefaultMinTunnelWidth, profile.Caves.MinTunnelWidth);
        Assert.Equal(CaveOptions.DefaultMouthMinLandDistance, profile.Caves.MouthMinLandDistance);
    }

    [Fact]
    public void Validate_CavePorosityOutOfRange_IsError()
    {
        var profile = TestProfiles.CreateValid();
        profile.Caves!.Porosity = 0.05;
        AssertHasError(validator.Validate(profile), "porosity");

        profile.Caves.Porosity = 0.9;
        AssertHasError(validator.Validate(profile), "porosity");
    }

    [Fact]
    public void Validate_CaveMinTunnelWidthOutOfRange_IsError()
    {
        var profile = TestProfiles.CreateValid();
        profile.Caves!.MinTunnelWidth = 0;
        AssertHasError(validator.Validate(profile), "minTunnelWidth");

        profile.Caves.MinTunnelWidth = 5;
        AssertHasError(validator.Validate(profile), "minTunnelWidth");
    }

    [Fact]
    public void Validate_CaveMouthMinLandDistanceOutOfRange_IsError()
    {
        var profile = TestProfiles.CreateValid();
        profile.Caves!.MouthMinLandDistance = 3;
        AssertHasError(validator.Validate(profile), "mouthMinLandDistance");

        profile.Caves.MouthMinLandDistance = 300;
        AssertHasError(validator.Validate(profile), "mouthMinLandDistance");
    }

    [Fact]
    public void Validate_RiverCountOutOfRange_IsError()
    {
        var profile = TestProfiles.CreateValid();
        profile.Rivers!.MaxCount = 501;

        AssertHasError(validator.Validate(profile), "maxCount");
    }

    [Fact]
    public void Validate_RiverMinLengthOutOfRange_IsError()
    {
        var profile = TestProfiles.CreateValid();
        profile.Rivers!.MinLength = 8;

        AssertHasError(validator.Validate(profile), "minLength");
    }

    [Fact]
    public void Validate_DefaultTerrain_HasNoIssues()
    {
        var profile = TestProfiles.CreateValid();
        profile.Terrain = new TerrainOptions();

        AssertHasNoError(validator.Validate(profile), "terrain.");
    }

    [Fact]
    public void Validate_TerrainBoundaryValues_HasNoIssues()
    {
        var profile = TestProfiles.CreateValid();
        profile.Terrain = new TerrainOptions
        {
            ContinentalnessWavelengthFactor = 0.5f,
            ContinentalnessOctaves = 3,
            WarpAmplitudeInner = 0f,
            WarpAmplitudeOuter = 1.5f,
            Thresholds = new ContinentalnessThresholdOptions
                { Ocean = 0.25f, Coast = 0.5f, Inland = 0.75f },
            MaxStraightRiverRun = 16
        };

        AssertHasNoError(validator.Validate(profile), "terrain.");
    }

    [Fact]
    public void Validate_WavelengthFactorOutOfRange_IsError()
    {
        var profile = TestProfiles.CreateValid();
        profile.Terrain.ContinentalnessWavelengthFactor = 0.4f;

        AssertHasError(validator.Validate(profile), "continentalnessWavelengthFactor");

        profile.Terrain.ContinentalnessWavelengthFactor = 4.5f;
        AssertHasError(validator.Validate(profile), "continentalnessWavelengthFactor");
    }

    [Fact]
    public void Validate_ContinentOctavesOutOfRange_IsError()
    {
        var profile = TestProfiles.CreateValid();
        profile.Terrain.ContinentalnessOctaves = 2;

        AssertHasError(validator.Validate(profile), "continentalnessOctaves");

        profile.Terrain.ContinentalnessOctaves = 10;
        AssertHasError(validator.Validate(profile), "continentalnessOctaves");
    }

    [Fact]
    public void Validate_WarpAmplitudeOutOfRange_IsError()
    {
        var profile = TestProfiles.CreateValid();
        profile.Terrain.WarpAmplitudeInner = -0.1f;

        AssertHasError(validator.Validate(profile), "warpAmplitudeinner");

        profile.Terrain.WarpAmplitudeOuter = 1.6f;
        AssertHasError(validator.Validate(profile), "warpAmplitudeouter");
    }

    [Fact]
    public void Validate_ThresholdOutOfRange_IsError()
    {
        var profile = TestProfiles.CreateValid();
        profile.Terrain.Thresholds.Ocean = 0f;

        AssertHasError(validator.Validate(profile), "thresholds.ocean");

        profile.Terrain.Thresholds.Ocean = 0.4f;
        profile.Terrain.Thresholds.Coast = 1.0f;
        AssertHasError(validator.Validate(profile), "thresholds.coast");
    }

    [Fact]
    public void Validate_ThresholdOrderingViolated_IsError()
    {
        var profile = TestProfiles.CreateValid();
        profile.Terrain.Thresholds = new ContinentalnessThresholdOptions
            { Ocean = 0.5f, Coast = 0.48f, Inland = 0.62f };

        AssertHasError(validator.Validate(profile), "ocean < coast < inland");

        profile.Terrain.Thresholds = new ContinentalnessThresholdOptions
            { Ocean = 0.40f, Coast = 0.48f, Inland = 0.42f };
        AssertHasError(validator.Validate(profile), "ocean < coast < inland");
    }

    [Fact]
    public void Validate_MaxStraightRiverRunOutOfRange_IsError()
    {
        var profile = TestProfiles.CreateValid();
        profile.Terrain.MaxStraightRiverRun = 15;

        AssertHasError(validator.Validate(profile), "maxStraightRiverRun");

        profile.Terrain.MaxStraightRiverRun = 4097;
        AssertHasError(validator.Validate(profile), "maxStraightRiverRun");
    }

    [Fact]
    public void Validate_SurfaceFarAboveSea_IsWarning()
    {
        var profile = TestProfiles.CreateValid();
        profile.SurfaceMaxZ = profile.SeaLevelZ + 25;

        var issues = validator.Validate(profile);

        Assert.DoesNotContain(issues, i => i.Severity == ProfileValidationSeverity.Error);
        Assert.Contains(issues, i => i.Severity == ProfileValidationSeverity.Warning
                                     && i.Message.Contains("surfaceMaxZ", StringComparison.Ordinal));
    }

    /// <summary>
    ///     Asserts that the given issues contain at least one Error whose message mentions the
    ///     given text.
    /// </summary>
    /// <param name="issues">Issues to search.</param>
    /// <param name="messageFragment">Text that must appear in the error message.</param>
    private static void AssertHasError(IReadOnlyList<ProfileValidationIssue> issues, string messageFragment)
    {
        Assert.Contains(issues, i => i.Severity == ProfileValidationSeverity.Error
                                     && i.Message.Contains(messageFragment, StringComparison.Ordinal));
    }

    /// <summary>
    ///     Asserts that no error issue message contains the given fragment.
    /// </summary>
    /// <param name="issues">Issues to search.</param>
    /// <param name="messageFragment">Text that must not appear in any error message.</param>
    private static void AssertHasNoError(IReadOnlyList<ProfileValidationIssue> issues,
        string messageFragment)
    {
        Assert.DoesNotContain(issues, i => i.Severity == ProfileValidationSeverity.Error
                                           && i.Message.Contains(messageFragment, StringComparison.Ordinal));
    }

    /// <summary>
    ///     Creates a valid profile with the shared biomes section attached.
    /// </summary>
    /// <returns>Profile.</returns>
    private static WorldGenProfile CreateValidWithBiomes()
    {
        var profile = TestProfiles.CreateValid();
        profile.Biomes = TestProfiles.CreateSmall128Biomes().Biomes;
        return profile;
    }

    [Fact]
    public void Validate_ValidBiomesSection_HasNoIssues()
    {
        Assert.Empty(validator.Validate(CreateValidWithBiomes()));
    }

    [Fact]
    public void Validate_AbsentBiomesSection_HasNoIssues()
    {
        Assert.Empty(validator.Validate(TestProfiles.CreateValid()));
    }

    [Fact]
    public void Validate_AlpineAtOrAboveSnowcap_IsError()
    {
        var profile = CreateValidWithBiomes();
        profile.Biomes!.AlpineZ = profile.Biomes.SnowcapZ;

        AssertHasError(validator.Validate(profile), "alpineZ");
    }

    [Fact]
    public void Validate_SnowcapAboveSurfaceMax_IsError()
    {
        var profile = CreateValidWithBiomes();
        profile.Biomes!.SnowcapZ = profile.SurfaceMaxZ + 1;

        AssertHasError(validator.Validate(profile), "snowcapZ");
    }

    [Fact]
    public void Validate_AlpineAtOrBelowSeaLevel_IsError()
    {
        var profile = CreateValidWithBiomes();
        profile.Biomes!.AlpineZ = profile.SeaLevelZ;

        AssertHasError(validator.Validate(profile), "alpineZ");
    }

    [Fact]
    public void Validate_TableReferenceToUnknownBiome_IsError()
    {
        var profile = CreateValidWithBiomes();
        profile.Biomes!.Table.Mild.Temperate = "Bogus";

        AssertHasError(validator.Validate(profile), "Bogus");
    }

    [Fact]
    public void Validate_MissingTableCellReference_IsError()
    {
        var profile = CreateValidWithBiomes();
        profile.Biomes!.Table.Hot.Dry = null!;

        AssertHasError(validator.Validate(profile), "table.hot.dry");
    }

    [Fact]
    public void Validate_TableReferencedBiomeWithoutDefinition_IsError()
    {
        var profile = CreateValidWithBiomes();
        profile.Biomes!.Definitions.Remove("Savanna");

        AssertHasError(validator.Validate(profile), "Savanna");
    }

    [Fact]
    public void Validate_MissingBeachDefinition_IsError()
    {
        var profile = CreateValidWithBiomes();
        profile.Biomes!.Definitions.Remove("Beach");

        AssertHasError(validator.Validate(profile), "Beach");
    }

    [Fact]
    public void Validate_MissingSnowcapDefinition_IsError()
    {
        var profile = CreateValidWithBiomes();
        profile.Biomes!.Definitions.Remove("Snowcap");

        AssertHasError(validator.Validate(profile), "Snowcap");
    }

    [Fact]
    public void Validate_SwampOverrideWithoutSwampDefinition_IsError()
    {
        var profile = CreateValidWithBiomes();
        profile.Biomes!.Definitions.Remove("Swamp");

        AssertHasError(validator.Validate(profile), "Swamp");
    }

    [Fact]
    public void Validate_UnknownDefinitionName_IsError()
    {
        var profile = CreateValidWithBiomes();
        profile.Biomes!.Definitions["Bogus"] = TestTerrainMaps.Definition("Grass", "Dirt", 1);

        AssertHasError(validator.Validate(profile), "Bogus");
    }

    [Fact]
    public void Validate_EmptySurfaceTemplate_IsError()
    {
        var profile = CreateValidWithBiomes();
        profile.Biomes!.Definitions["Grassland"].SurfaceTemplate = "";

        AssertHasError(validator.Validate(profile), "surfaceTemplate");
    }

    [Fact]
    public void Validate_EmptySubSurfaceTemplate_IsError()
    {
        var profile = CreateValidWithBiomes();
        profile.Biomes!.Definitions["Grassland"].SubSurfaceTemplate = " ";

        AssertHasError(validator.Validate(profile), "subSurfaceTemplate");
    }

    [Fact]
    public void Validate_SubSurfaceDepthOutOfRange_IsError()
    {
        var profile = CreateValidWithBiomes();
        profile.Biomes!.Definitions["Grassland"].SubSurfaceDepth = 17;

        AssertHasError(validator.Validate(profile), "subSurfaceDepth");

        profile.Biomes.Definitions["Grassland"].SubSurfaceDepth = -1;
        AssertHasError(validator.Validate(profile), "subSurfaceDepth");
    }

    [Fact]
    public void Validate_DecorationWeightOutOfRange_IsError()
    {
        var profile = CreateValidWithBiomes();
        var pool = profile.Biomes!.Definitions["Grassland"].Decorations!;
        pool[0].Weight = 0.0;

        AssertHasError(validator.Validate(profile), "weight");

        pool[0].Weight = 1.5;
        AssertHasError(validator.Validate(profile), "weight");
    }

    [Fact]
    public void Validate_DecorationMinSpacingBelowOne_IsError()
    {
        var profile = CreateValidWithBiomes();
        profile.Biomes!.Definitions["Grassland"].Decorations![0].MinSpacing = 0;

        AssertHasError(validator.Validate(profile), "minSpacing");
    }

    [Fact]
    public void Validate_DecorationMaxSlopeOutOfRange_IsError()
    {
        var profile = CreateValidWithBiomes();
        profile.Biomes!.Definitions["Grassland"].Decorations![0].MaxSlope = 3;

        AssertHasError(validator.Validate(profile), "maxSlope");
    }

    [Fact]
    public void Validate_EmptyDecorationTemplate_IsError()
    {
        var profile = CreateValidWithBiomes();
        profile.Biomes!.Definitions["Grassland"].Decorations![0].Template = "";

        AssertHasError(validator.Validate(profile), "template");
    }

    [Fact]
    public void Validate_TooManyDecorationsInPool_IsError()
    {
        var profile = CreateValidWithBiomes();
        var pool = new List<DecorationOptions>();
        for (var i = 0; i < 9; ++i)
        {
            pool.Add(new DecorationOptions
            {
                Template = $"Tree{i}",
                Weight = 0.01,
                MinSpacing = 2,
                MaxSlope = 1
            });
        }

        profile.Biomes!.Definitions["Grassland"].Decorations = pool;

        AssertHasError(validator.Validate(profile), "decorations");
    }

    [Fact]
    public void Validate_EmptyDecorationPool_IsAllowed()
    {
        var profile = CreateValidWithBiomes();
        profile.Biomes!.Definitions["Grassland"].Decorations = new List<DecorationOptions>();

        var issues = validator.Validate(profile);

        Assert.DoesNotContain(issues, i => i.Severity == ProfileValidationSeverity.Error);
    }

    [Fact]
    public void Validate_TableBiomeWithoutDecorations_IsWarningOnly()
    {
        var profile = CreateValidWithBiomes();
        profile.Biomes!.Definitions["Savanna"].Decorations = null;

        var issues = validator.Validate(profile);

        Assert.DoesNotContain(issues, i => i.Severity == ProfileValidationSeverity.Error);
        Assert.Contains(issues, i => i.Severity == ProfileValidationSeverity.Warning
                                     && i.Message.Contains("Savanna", StringComparison.Ordinal));
    }

    [Fact]
    public void Validate_NonTableBiomeWithoutDecorations_IsSilent()
    {
        var profile = CreateValidWithBiomes();
        profile.Biomes!.Definitions["Alpine"].Decorations = null;

        var issues = validator.Validate(profile);

        Assert.DoesNotContain(issues, i => i.Message.Contains("Alpine", StringComparison.Ordinal));
    }

    [Fact]
    public void Validate_EmptyFloorTemplate_IsError()
    {
        var profile = CreateValidWithBiomes();
        profile.Biomes!.OceanFloorTemplate = "";

        AssertHasError(validator.Validate(profile), "oceanFloorTemplate");

        profile.Biomes.OceanFloorTemplate = "Gravel";
        profile.Biomes.WaterFloorTemplate = "";
        AssertHasError(validator.Validate(profile), "waterFloorTemplate");
    }

    [Fact]
    public void Validate_EmptySwampTemplate_IsError()
    {
        var profile = CreateValidWithBiomes();
        profile.Biomes!.Swamp = new SwampOptions { Template = "", MaxHeightZ = 14 };

        AssertHasError(validator.Validate(profile), "swamp.template");
    }

    [Fact]
    public void Validate_ValidLayout_HasNoIssues()
    {
        Assert.Empty(validator.Validate(CreateValidWithLayout()));
    }

    [Fact]
    public void Validate_LayoutStrengthOutOfRange_IsError()
    {
        var profile = CreateValidWithLayout();
        profile.Layout!.Strength = 1.5f;

        AssertHasError(validator.Validate(profile), "layout.strength");

        profile.Layout.Strength = -0.1f;
        AssertHasError(validator.Validate(profile), "layout.strength");
    }

    [Fact]
    public void Validate_UnknownLayoutPreset_IsError()
    {
        var profile = CreateValidWithLayout();
        profile.Layout!.Preset = "bogus";

        AssertHasError(validator.Validate(profile), "preset");
    }

    [Fact]
    public void Validate_PresetAndAnchorsTogether_IsError()
    {
        var profile = CreateValidWithLayout();
        profile.Layout!.Preset = "continents3";

        AssertHasError(validator.Validate(profile), "mutually exclusive");
    }

    [Fact]
    public void Validate_LayoutAnchorCoordinateOutOfRange_IsError()
    {
        var profile = CreateValidWithLayout();
        profile.Layout!.Anchors![0].X = 0.0f;

        AssertHasError(validator.Validate(profile), "anchors[0].x");
    }

    [Fact]
    public void Validate_LayoutAnchorRadiusOutOfRange_IsError()
    {
        var profile = CreateValidWithLayout();
        profile.Layout!.Anchors![0].Radius = 0.01f;

        AssertHasError(validator.Validate(profile), "radius");
    }

    [Fact]
    public void Validate_LayoutAnchorWeightOutOfRange_IsError()
    {
        var profile = CreateValidWithLayout();
        profile.Layout!.Anchors![0].Weight = 0.05f;

        AssertHasError(validator.Validate(profile), "weight");
    }

    [Fact]
    public void Validate_LayoutAnchorJitterOutOfRange_IsError()
    {
        var profile = CreateValidWithLayout();
        profile.Layout!.Anchors![0].Jitter = 0.25f;

        AssertHasError(validator.Validate(profile), "jitter");
    }

    [Fact]
    public void Validate_LayoutAnchorBiasOutOfRange_IsError()
    {
        var profile = CreateValidWithLayout();
        profile.Layout!.Anchors![0].MountainBias = 1.5f;

        AssertHasError(validator.Validate(profile), "mountainBias");
    }

    [Fact]
    public void Validate_LayoutAnchorCountOutOfRange_IsError()
    {
        var profile = CreateValidWithLayout();
        profile.Layout!.AnchorCount = 0;

        AssertHasError(validator.Validate(profile), "anchorCount");
    }

    [Fact]
    public void Validate_LayoutStrengthWithoutAnchors_IsWarning()
    {
        var profile = CreateValidWithLayout();
        profile.Layout!.Anchors = new List<LayoutAnchor>();

        var issues = validator.Validate(profile);

        Assert.DoesNotContain(issues, i => i.Severity == ProfileValidationSeverity.Error);
        Assert.Contains(issues, i => i.Severity == ProfileValidationSeverity.Warning
                                     && i.Message.Contains("no-op", StringComparison.Ordinal));
    }

    /// <summary>
    ///     Creates a valid profile with a valid layout section attached.
    /// </summary>
    /// <returns>Profile.</returns>
    private static WorldGenProfile CreateValidWithLayout()
    {
        var profile = TestProfiles.CreateValid();
        profile.Layout = new LayoutOptions
        {
            Strength = 1f,
            Anchors = new List<LayoutAnchor>
            {
                new() { X = 0.5f, Y = 0.5f, Radius = 0.2f, Weight = 1f, MountainBias = 0.5f }
            }
        };
        return profile;
    }
}
