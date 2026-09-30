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
using Sovereign.WorldGen.Biomes;

namespace Sovereign.WorldGen;

/// <summary>
///     Performs semantic validation of a world generation profile.
/// </summary>
public sealed class ProfileValidator
{
    /// <summary>
    ///     Multiple of 32 required for world dimensions.
    /// </summary>
    private const int DimensionMultiple = 32;

    /// <summary>
    ///     Maximum world dimension in blocks.
    /// </summary>
    private const int MaxDimension = 16384;

    /// <summary>
    ///     Minimum vertical clearance in blocks between consecutive cave levels.
    /// </summary>
    private const int MinCaveLevelClearance = 10;

    /// <summary>
    ///     Minimum depth in blocks of the topmost cave floor below the top of the stone bands.
    /// </summary>
    private const int MinTopCaveDepth = 6;

    /// <summary>
    ///     Minimum allowed cave headroom in blocks.
    /// </summary>
    private const int MinHeadroom = 2;

    /// <summary>
    ///     Maximum allowed cave headroom in blocks.
    /// </summary>
    private const int MaxHeadroom = 8;

    /// <summary>
    ///     Maximum allowed cave option count.
    /// </summary>
    private const int MaxCaveCount = 64;

    /// <summary>
    ///     Minimum allowed cave porosity.
    /// </summary>
    private const double MinCavePorosity = 0.1;

    /// <summary>
    ///     Maximum allowed cave porosity.
    /// </summary>
    private const double MaxCavePorosity = 0.6;

    /// <summary>
    ///     Minimum allowed corridor carve width in blocks.
    /// </summary>
    private const int MinTunnelWidth = 1;

    /// <summary>
    ///     Maximum allowed corridor carve width in blocks.
    /// </summary>
    private const int MaxTunnelWidth = 4;

    /// <summary>
    ///     Minimum allowed mouth exclusion radius for water, in blocks.
    /// </summary>
    private const int MinMouthMinLandDistance = 4;

    /// <summary>
    ///     Minimum allowed surface mouth depth in Z blocks.
    /// </summary>
    private const int MinMaxMouthDepthZ = 8;

    /// <summary>
    ///     Maximum allowed surface mouth depth in Z blocks.
    /// </summary>
    private const int MaxMaxMouthDepthZ = 96;

    /// <summary>
    ///     Maximum allowed mouth exclusion radius for water, in blocks.
    /// </summary>
    private const int MaxMouthMinLandDistance = 256;

    /// <summary>
    ///     Maximum allowed river count.
    /// </summary>
    private const int MaxRiverCount = 500;

    /// <summary>
    ///     Minimum allowed minimum river length in blocks.
    /// </summary>
    private const int MinRiverMinLength = 16;

    /// <summary>
    ///     Maximum allowed minimum river length in blocks.
    /// </summary>
    private const int MaxRiverMinLength = 4096;

    /// <summary>
    ///     Maximum intended difference between the terrain surface maximum and sea level.
    /// </summary>
    private const int MaxIntendedSurfaceAboveSea = 24;

    /// <summary>
    ///     Minimum allowed continentalness wavelength factor.
    /// </summary>
    private const float MinWavelengthFactor = 0.5f;

    /// <summary>
    ///     Maximum allowed continentalness wavelength factor.
    /// </summary>
    private const float MaxWavelengthFactor = 4f;

    /// <summary>
    ///     Minimum allowed continentalness octave count.
    /// </summary>
    private const int MinContinentalnessOctaves = 3;

    /// <summary>
    ///     Maximum allowed continentalness octave count.
    /// </summary>
    private const int MaxContinentalnessOctaves = 9;

    /// <summary>
    ///     Maximum allowed domain warp amplitude.
    /// </summary>
    private const float MaxWarpAmplitude = 1.5f;

    /// <summary>
    ///     Minimum allowed maximum straight river run in cells.
    /// </summary>
    private const int MinMaxStraightRiverRun = 16;

    /// <summary>
    ///     Maximum allowed maximum straight river run in cells.
    /// </summary>
    private const int MaxMaxStraightRiverRun = 4096;

    /// <summary>
    ///     Maximum allowed subsurface depth of a biome definition.
    /// </summary>
    private const int MaxSubSurfaceDepth = 16;

    /// <summary>
    ///     Maximum allowed decorations per biome pool.
    /// </summary>
    private const int MaxDecorationsPerPool = 8;

    /// <summary>
    ///     Maximum allowed decoration maximum slope.
    /// </summary>
    private const int MaxDecorationSlope = 2;

    /// <summary>
    ///     Moisture band keys of the Whittaker table rows, in band order.
    /// </summary>
    private static readonly string[] MoistureBands = { "dry", "temperate", "wet" };

    /// <summary>
    ///     Validates a world generation profile.
    /// </summary>
    /// <param name="profile">Profile to validate.</param>
    /// <returns>List of issues found; any Error issue blocks world generation planning.</returns>
    public IReadOnlyList<ProfileValidationIssue> Validate(WorldGenProfile profile)
    {
        var issues = new List<ProfileValidationIssue>();
        ValidateDimensions(profile, issues);
        ValidateZOrdering(profile, issues);
        ValidateSurfaceHeight(profile, issues);
        ValidateStoneBands(profile, issues);
        ValidateCaveLevels(profile, issues);
        ValidateCaveOptions(profile, issues);
        ValidateRiverOptions(profile, issues);
        ValidateBiomeOptions(profile, issues);
        ValidateTerrainOptions(profile, issues);
        ValidatePreviewOptions(profile, issues);
        return issues;
    }

    /// <summary>
    ///     Validates that the bedrock template name is non-empty and that the preview knob,
    ///     when present, lies in its allowed range.
    /// </summary>
    /// <param name="profile">Profile to validate.</param>
    /// <param name="issues">List to append issues to.</param>
    private static void ValidatePreviewOptions(WorldGenProfile profile,
        List<ProfileValidationIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(profile.BedrockTemplate))
            issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                "bedrockTemplate must be non-empty."));

        if (profile.Preview is not { } preview) return;

        if (preview.MaxDimension is < PreviewOptions.MinMaxDimension
            or > PreviewOptions.MaxMaxDimension)
            issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                $"preview.maxDimension ({preview.MaxDimension}) must be between " +
                $"{PreviewOptions.MinMaxDimension} and {PreviewOptions.MaxMaxDimension}."));
    }

    /// <summary>
    ///     Validates that the world dimensions are positive multiples of 32 and at most 16384.
    /// </summary>
    /// <param name="profile">Profile to validate.</param>
    /// <param name="issues">List to append issues to.</param>
    private static void ValidateDimensions(WorldGenProfile profile, List<ProfileValidationIssue> issues)
    {
        ValidateDimension("width", profile.Width, issues);
        ValidateDimension("height", profile.Height, issues);
    }

    /// <summary>
    ///     Validates a single world dimension.
    /// </summary>
    /// <param name="name">Dimension name for error messages.</param>
    /// <param name="value">Dimension value.</param>
    /// <param name="issues">List to append issues to.</param>
    private static void ValidateDimension(string name, int value, List<ProfileValidationIssue> issues)
    {
        if (value <= 0)
            issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                $"{name} must be positive, but is {value}."));
        else if (value % DimensionMultiple != 0)
            issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                $"{name} must be a multiple of {DimensionMultiple}, but is {value}."));
        else if (value > MaxDimension)
            issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                $"{name} must be at most {MaxDimension}, but is {value}."));
    }

    /// <summary>
    ///     Validates that the Z ordering <c>bedrockZ &lt; rockFloorZ &lt; seaLevelZ &lt; surfaceMaxZ</c> holds.
    /// </summary>
    /// <param name="profile">Profile to validate.</param>
    /// <param name="issues">List to append issues to.</param>
    private static void ValidateZOrdering(WorldGenProfile profile, List<ProfileValidationIssue> issues)
    {
        if (profile.BedrockZ >= profile.RockFloorZ)
            issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                $"bedrockZ ({profile.BedrockZ}) must be less than rockFloorZ ({profile.RockFloorZ})."));
        if (profile.RockFloorZ >= profile.SeaLevelZ)
            issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                $"rockFloorZ ({profile.RockFloorZ}) must be less than seaLevelZ ({profile.SeaLevelZ})."));
        if (profile.SeaLevelZ >= profile.SurfaceMaxZ)
            issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                $"seaLevelZ ({profile.SeaLevelZ}) must be less than surfaceMaxZ ({profile.SurfaceMaxZ})."));
    }

    /// <summary>
    ///     Emits a warning if the terrain surface maximum rises far above sea level.
    /// </summary>
    /// <param name="profile">Profile to validate.</param>
    /// <param name="issues">List to append issues to.</param>
    private static void ValidateSurfaceHeight(WorldGenProfile profile, List<ProfileValidationIssue> issues)
    {
        if (profile.SurfaceMaxZ - profile.SeaLevelZ > MaxIntendedSurfaceAboveSea)
            issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Warning,
                $"surfaceMaxZ - seaLevelZ ({profile.SurfaceMaxZ - profile.SeaLevelZ}) exceeds " +
                $"{MaxIntendedSurfaceAboveSea}; unlikely to be intended."));
    }

    /// <summary>
    ///     Validates that the stone bands are non-empty, ordered, contiguous, and cover down to
    ///     the rock floor.
    /// </summary>
    /// <param name="profile">Profile to validate.</param>
    /// <param name="issues">List to append issues to.</param>
    private static void ValidateStoneBands(WorldGenProfile profile, List<ProfileValidationIssue> issues)
    {
        var bands = profile.StoneBands;
        if (bands is not { Count: > 0 })
        {
            issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                "stoneBands must contain at least one band."));
            return;
        }

        for (var i = 0; i < bands.Count; ++i)
        {
            var band = bands[i];
            if (band.FromZ >= band.ToZ)
                issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                    $"stone band {i + 1}: fromZ ({band.FromZ}) must be less than toZ ({band.ToZ})."));
            if (string.IsNullOrWhiteSpace(band.Template))
                issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                    $"stone band {i + 1}: template must be non-empty."));
        }

        for (var i = 1; i < bands.Count; ++i)
        {
            if (bands[i].FromZ >= bands[i - 1].FromZ)
                issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                    $"stone band {i + 1}: bands must be ordered from top to bottom " +
                    $"(fromZ {bands[i].FromZ} does not precede {bands[i - 1].FromZ})."));
            else if (bands[i].ToZ < bands[i - 1].FromZ - 1)
                issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                    $"stone band {i + 1}: gap above band (toZ {bands[i].ToZ} is below {bands[i - 1].FromZ - 1})."));
            else if (bands[i].ToZ > bands[i - 1].FromZ - 1)
                issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                    $"stone band {i + 1}: overlaps preceding band (toZ {bands[i].ToZ} is above {bands[i - 1].FromZ - 1})."));
        }

        var deepestBand = bands[^1];
        if (deepestBand.FromZ != profile.RockFloorZ)
            issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                $"deepest stone band starts at {deepestBand.FromZ}, but rockFloorZ is {profile.RockFloorZ}."));

        if (bands.Count < 2)
            issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Warning,
                "Fewer than two stone bands; mining progression will be flat."));
    }

    /// <summary>
    ///     Validates the optional cave levels for ordering, placement, and clearance.
    /// </summary>
    /// <param name="profile">Profile to validate.</param>
    /// <param name="issues">List to append issues to.</param>
    private static void ValidateCaveLevels(WorldGenProfile profile, List<ProfileValidationIssue> issues)
    {
        var levels = profile.CaveLevels;
        if (levels is not { Count: > 0 }) return;

        for (var i = 0; i < levels.Count; ++i)
        {
            var level = levels[i];
            if (level.Headroom is < MinHeadroom or > MaxHeadroom)
                issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                    $"cave level {i + 1}: headroom ({level.Headroom}) must be between " +
                    $"{MinHeadroom} and {MaxHeadroom}."));
        }

        for (var i = 1; i < levels.Count; ++i)
        {
            if (levels[i].FloorZ >= levels[i - 1].FloorZ)
                issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                    $"cave level {i + 1}: floorZ values must be strictly descending " +
                    $"({levels[i].FloorZ} does not precede {levels[i - 1].FloorZ})."));
            else if (levels[i - 1].FloorZ - levels[i].FloorZ < MinCaveLevelClearance)
                issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                    $"cave level {i + 1}: insufficient vertical clearance below the previous level " +
                    $"({levels[i - 1].FloorZ - levels[i].FloorZ} < {MinCaveLevelClearance})."));
        }

        var bandTopZ = profile.StoneBands is { Count: > 0 } bands ? (int?)bands[0].ToZ : null;
        if (bandTopZ is null) return;

        for (var i = 0; i < levels.Count; ++i)
        {
            if (levels[i].FloorZ <= profile.BedrockZ || levels[i].FloorZ > bandTopZ)
                issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                    $"cave level {i + 1}: floorZ ({levels[i].FloorZ}) must lie between bedrockZ " +
                    $"({profile.BedrockZ}) and the top of the stone bands ({bandTopZ})."));
        }

        if (levels[0].FloorZ > bandTopZ - MinTopCaveDepth)
            issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                $"topmost cave level floorZ ({levels[0].FloorZ}) must be at least {MinTopCaveDepth} " +
                $"below the top of the stone bands ({bandTopZ})."));
    }

    /// <summary>
    ///     Validates the optional cave generation options.
    /// </summary>
    /// <param name="profile">Profile to validate.</param>
    /// <param name="issues">List to append issues to.</param>
    private static void ValidateCaveOptions(WorldGenProfile profile, List<ProfileValidationIssue> issues)
    {
        var caves = profile.Caves;
        if (caves is null) return;

        ValidateCaveCount("shaftsPerLevelPair", caves.ShaftsPerLevelPair, issues);
        ValidateCaveCount("surfaceMouths", caves.SurfaceMouths, issues);
        ValidateCaveTunings(caves, issues);

        var hasCaveLevels = profile.CaveLevels is { Count: > 0 };
        if (!hasCaveLevels && (caves.ShaftsPerLevelPair != 0 || caves.SurfaceMouths != 0))
            issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                "caves must be absent or all-zero when caveLevels is empty."));
    }

    /// <summary>
    ///     Validates the cave generation tuning options: porosity, tunnel width, and the
    ///     mouth water exclusion radius.
    /// </summary>
    /// <param name="caves">Cave options to validate.</param>
    /// <param name="issues">List to append issues to.</param>
    private static void ValidateCaveTunings(CaveOptions caves, List<ProfileValidationIssue> issues)
    {
        if (caves.Porosity is < MinCavePorosity or > MaxCavePorosity)
            issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                $"caves.porosity ({caves.Porosity}) must be between {MinCavePorosity} " +
                $"and {MaxCavePorosity}."));
        if (caves.MinTunnelWidth is < MinTunnelWidth or > MaxTunnelWidth)
            issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                $"caves.minTunnelWidth ({caves.MinTunnelWidth}) must be between " +
                $"{MinTunnelWidth} and {MaxTunnelWidth}."));
        if (caves.MouthMinLandDistance is < MinMouthMinLandDistance or > MaxMouthMinLandDistance)
            issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                $"caves.mouthMinLandDistance ({caves.MouthMinLandDistance}) must be between " +
                $"{MinMouthMinLandDistance} and {MaxMouthMinLandDistance}."));
        if (caves.MaxMouthDepthZ is < MinMaxMouthDepthZ or > MaxMaxMouthDepthZ)
            issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                $"caves.maxMouthDepthZ ({caves.MaxMouthDepthZ}) must be between " +
                $"{MinMaxMouthDepthZ} and {MaxMaxMouthDepthZ}."));
    }

    /// <summary>
    ///     Validates a single cave option count.
    /// </summary>
    /// <param name="name">Option name for error messages.</param>
    /// <param name="value">Option value.</param>
    /// <param name="issues">List to append issues to.</param>
    private static void ValidateCaveCount(string name, int value, List<ProfileValidationIssue> issues)
    {
        if (value is < 0 or > MaxCaveCount)
            issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                $"caves.{name} ({value}) must be between 0 and {MaxCaveCount}."));
    }

    /// <summary>
    ///     Validates the optional river generation options.
    /// </summary>
    /// <param name="profile">Profile to validate.</param>
    /// <param name="issues">List to append issues to.</param>
    private static void ValidateRiverOptions(WorldGenProfile profile, List<ProfileValidationIssue> issues)
    {
        var rivers = profile.Rivers;
        if (rivers is null) return;

        if (rivers.MaxCount is < 0 or > MaxRiverCount)
            issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                $"rivers.maxCount ({rivers.MaxCount}) must be between 0 and {MaxRiverCount}."));
        if (rivers.MinLength is < MinRiverMinLength or > MaxRiverMinLength)
            issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                $"rivers.minLength ({rivers.MinLength}) must be between {MinRiverMinLength} " +
                $"and {MaxRiverMinLength}."));
    }

    /// <summary>
    ///     Validates the terrain section: wavelength factor, octave count, warp amplitudes,
    ///     band thresholds, and the maximum straight river run.
    /// </summary>
    /// <param name="profile">Profile to validate.</param>
    /// <param name="issues">List to append issues to.</param>
    private static void ValidateTerrainOptions(WorldGenProfile profile,
        List<ProfileValidationIssue> issues)
    {
        var terrain = profile.Terrain;

        if (terrain.ContinentalnessWavelengthFactor is < MinWavelengthFactor or > MaxWavelengthFactor)
            issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                $"terrain.continentalnessWavelengthFactor " +
                $"({terrain.ContinentalnessWavelengthFactor}) must be between " +
                $"{MinWavelengthFactor} and {MaxWavelengthFactor}."));

        if (terrain.ContinentalnessOctaves is < MinContinentalnessOctaves or > MaxContinentalnessOctaves)
            issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                $"terrain.continentalnessOctaves ({terrain.ContinentalnessOctaves}) must be " +
                $"between {MinContinentalnessOctaves} and {MaxContinentalnessOctaves}."));

        ValidateWarpAmplitude("inner", terrain.WarpAmplitudeInner, issues);
        ValidateWarpAmplitude("outer", terrain.WarpAmplitudeOuter, issues);
        ValidateThresholds(terrain.Thresholds, issues);

        if (terrain.MaxStraightRiverRun is < MinMaxStraightRiverRun or > MaxMaxStraightRiverRun)
            issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                $"terrain.maxStraightRiverRun ({terrain.MaxStraightRiverRun}) must be between " +
                $"{MinMaxStraightRiverRun} and {MaxMaxStraightRiverRun}."));
    }

    /// <summary>
    ///     Validates a single domain warp amplitude.
    /// </summary>
    /// <param name="name">Amplitude name for error messages.</param>
    /// <param name="value">Amplitude value.</param>
    /// <param name="issues">List to append issues to.</param>
    private static void ValidateWarpAmplitude(string name, float value,
        List<ProfileValidationIssue> issues)
    {
        if (value is < 0f or > MaxWarpAmplitude)
            issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                $"terrain.warpAmplitude{name} ({value}) must be between 0 and " +
                $"{MaxWarpAmplitude}."));
    }

    /// <summary>
    ///     Validates the continentalness band thresholds: each lies in (0, 1) and the
    ///     ordering <c>ocean &lt; coast &lt; inland</c> holds.
    /// </summary>
    /// <param name="thresholds">Thresholds to validate.</param>
    /// <param name="issues">List to append issues to.</param>
    private static void ValidateThresholds(ContinentalnessThresholdOptions thresholds,
        List<ProfileValidationIssue> issues)
    {
        ValidateThreshold("ocean", thresholds.Ocean, issues);
        ValidateThreshold("coast", thresholds.Coast, issues);
        ValidateThreshold("inland", thresholds.Inland, issues);

        if (thresholds.Ocean < thresholds.Coast && thresholds.Coast < thresholds.Inland) return;

        issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
            $"terrain.thresholds must satisfy ocean < coast < inland " +
            $"(ocean {thresholds.Ocean}, coast {thresholds.Coast}, inland {thresholds.Inland})."));
    }

    /// <summary>
    ///     Validates a single continentalness band threshold.
    /// </summary>
    /// <param name="name">Threshold name for error messages.</param>
    /// <param name="value">Threshold value.</param>
    /// <param name="issues">List to append issues to.</param>
    private static void ValidateThreshold(string name, float value,
        List<ProfileValidationIssue> issues)
    {
        if (value is <= 0f or >= 1f)
            issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                $"terrain.thresholds.{name} ({value}) must lie strictly between 0 and 1."));
    }

    /// <summary>
    ///     Validates the optional biomes section: snow line ordering, table completeness,
    ///     definitions, decorations, and floor templates.
    /// </summary>
    /// <param name="profile">Profile to validate.</param>
    /// <param name="issues">List to append issues to.</param>
    private static void ValidateBiomeOptions(WorldGenProfile profile, List<ProfileValidationIssue> issues)
    {
        var biomes = profile.Biomes;
        if (biomes is null) return;

        ValidateSnowLine(profile, biomes, issues);
        ValidateTemplateNames(biomes, issues);
        ValidateSwamp(biomes, issues);

        var tableBiomes = ValidateTable(biomes, issues);
        ValidateDefinitions(biomes, tableBiomes, issues);
    }

    /// <summary>
    ///     Validates that alpineZ is less than snowcapZ and both lie within
    ///     <c>(seaLevelZ, surfaceMaxZ]</c>.
    /// </summary>
    /// <param name="profile">Profile to validate.</param>
    /// <param name="biomes">Biome options to validate.</param>
    /// <param name="issues">List to append issues to.</param>
    private static void ValidateSnowLine(WorldGenProfile profile, BiomeOptions biomes,
        List<ProfileValidationIssue> issues)
    {
        if (biomes.AlpineZ >= biomes.SnowcapZ)
            issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                $"biomes.alpineZ ({biomes.AlpineZ}) must be less than biomes.snowcapZ " +
                $"({biomes.SnowcapZ})."));

        ValidateSnowLineZ(profile, "alpineZ", biomes.AlpineZ, issues);
        ValidateSnowLineZ(profile, "snowcapZ", biomes.SnowcapZ, issues);
    }

    /// <summary>
    ///     Validates that one snow line Z lies within <c>(seaLevelZ, surfaceMaxZ]</c>.
    /// </summary>
    /// <param name="profile">Profile to validate.</param>
    /// <param name="name">Option name for error messages.</param>
    /// <param name="value">Z value to validate.</param>
    /// <param name="issues">List to append issues to.</param>
    private static void ValidateSnowLineZ(WorldGenProfile profile, string name, int value,
        List<ProfileValidationIssue> issues)
    {
        if (value <= profile.SeaLevelZ || value > profile.SurfaceMaxZ)
            issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                $"biomes.{name} ({value}) must lie between seaLevelZ ({profile.SeaLevelZ}) " +
                $"and surfaceMaxZ ({profile.SurfaceMaxZ}]."));
    }

    /// <summary>
    ///     Validates that the floor and swamp template names are non-empty.
    /// </summary>
    /// <param name="biomes">Biome options to validate.</param>
    /// <param name="issues">List to append issues to.</param>
    private static void ValidateTemplateNames(BiomeOptions biomes, List<ProfileValidationIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(biomes.OceanFloorTemplate))
            issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                "biomes.oceanFloorTemplate must be non-empty."));
        if (string.IsNullOrWhiteSpace(biomes.WaterFloorTemplate))
            issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                "biomes.waterFloorTemplate must be non-empty."));
    }

    /// <summary>
    ///     Validates the optional swamp override configuration.
    /// </summary>
    /// <param name="biomes">Biome options to validate.</param>
    /// <param name="issues">List to append issues to.</param>
    private static void ValidateSwamp(BiomeOptions biomes, List<ProfileValidationIssue> issues)
    {
        if (biomes.Swamp is not { } swamp) return;

        if (string.IsNullOrWhiteSpace(swamp.Template))
            issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                "biomes.swamp.template must be non-empty."));
    }

    /// <summary>
    ///     Validates that the Whittaker table is complete and only references defined biomes.
    /// </summary>
    /// <param name="biomes">Biome options to validate.</param>
    /// <param name="issues">List to append issues to.</param>
    /// <returns>The set of biome names referenced by the table.</returns>
    private static HashSet<string> ValidateTable(BiomeOptions biomes,
        List<ProfileValidationIssue> issues)
    {
        var referenced = new HashSet<string>();
        AddTableReferences(biomes, biomes.Table.Cold, "cold", referenced, issues);
        AddTableReferences(biomes, biomes.Table.Mild, "mild", referenced, issues);
        AddTableReferences(biomes, biomes.Table.Hot, "hot", referenced, issues);
        return referenced;
    }

    /// <summary>
    ///     Validates one Whittaker table row: every moisture band must reference a defined
    ///     biome.
    /// </summary>
    /// <param name="biomes">Biome options being validated.</param>
    /// <param name="row">Row to validate.</param>
    /// <param name="rowName">Row name for error messages.</param>
    /// <param name="referenced">Set collecting referenced biome names.</param>
    /// <param name="issues">List to append issues to.</param>
    private static void AddTableReferences(BiomeOptions biomes, BiomeTableRow row, string rowName,
        HashSet<string> referenced, List<ProfileValidationIssue> issues)
    {
        CollectReference(biomes, row.Dry, rowName, "dry", referenced, issues);
        CollectReference(biomes, row.Temperate, rowName, "temperate", referenced, issues);
        CollectReference(biomes, row.Wet, rowName, "wet", referenced, issues);
    }

    /// <summary>
    ///     Collects one table cell reference, reporting an error if the referenced biome is
    ///     not defined.
    /// </summary>
    /// <param name="biomes">Biome options being validated.</param>
    /// <param name="biomeName">Referenced biome name.</param>
    /// <param name="rowName">Row name for error messages.</param>
    /// <param name="bandName">Moisture band name for error messages.</param>
    /// <param name="referenced">Set collecting referenced biome names.</param>
    /// <param name="issues">List to append issues to.</param>
    private static void CollectReference(BiomeOptions biomes, string biomeName, string rowName,
        string bandName, HashSet<string> referenced, List<ProfileValidationIssue> issues)
    {
        if (IsDefinedBiome(biomes, biomeName))
        {
            referenced.Add(biomeName);
        }
        else
        {
            issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                $"biomes.table.{rowName}.{bandName} references \"{biomeName}\", which is not " +
                "defined in biomes.definitions."));
        }
    }

    /// <summary>
    ///     Validates the biome definitions: names, templates, depths, decorations, and
    ///     required coverage.
    /// </summary>
    /// <param name="biomes">Biome options to validate.</param>
    /// <param name="tableBiomes">Biome names referenced by the table.</param>
    /// <param name="issues">List to append issues to.</param>
    private static void ValidateDefinitions(BiomeOptions biomes, HashSet<string> tableBiomes,
        List<ProfileValidationIssue> issues)
    {
        foreach (var (name, definition) in biomes.Definitions)
        {
            if (!Enum.TryParse<BiomeId>(name, out _))
                issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                    $"biomes.definitions[\"{name}\"] is not a known biome name."));

            ValidateDefinition(name, definition, issues);
            if ((definition.Decorations is null || definition.Decorations.Count == 0)
                && tableBiomes.Contains(name))
            {
                issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Warning,
                    $"biome \"{name}\" is referenced by biomes.table but has no decorations; " +
                    "it will be silent."));
            }
        }

        foreach (var required in RequiredDefinitions(biomes, tableBiomes))
        {
            if (biomes.Definitions.ContainsKey(required)) continue;
            issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                $"biomes.definitions must define \"{required}\", which is used by the " +
                "material stages."));
        }
    }

    /// <summary>
    ///     Validates a single biome definition.
    /// </summary>
    /// <param name="name">Biome name for error messages.</param>
    /// <param name="definition">Definition to validate.</param>
    /// <param name="issues">List to append issues to.</param>
    private static void ValidateDefinition(string name, BiomeDefinition definition,
        List<ProfileValidationIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(definition.SurfaceTemplate))
            issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                $"biomes.definitions[\"{name}\"].surfaceTemplate must be non-empty."));
        if (string.IsNullOrWhiteSpace(definition.SubSurfaceTemplate))
            issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                $"biomes.definitions[\"{name}\"].subSurfaceTemplate must be non-empty."));
        if (definition.SubSurfaceDepth is < 0 or > MaxSubSurfaceDepth)
            issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                $"biomes.definitions[\"{name}\"].subSurfaceDepth " +
                $"({definition.SubSurfaceDepth}) must be between 0 and {MaxSubSurfaceDepth}."));

        ValidateDecorations(name, definition, issues);
    }

    /// <summary>
    ///     Validates the decoration pool of a biome definition.
    /// </summary>
    /// <param name="name">Biome name for error messages.</param>
    /// <param name="definition">Definition owning the pool.</param>
    /// <param name="issues">List to append issues to.</param>
    private static void ValidateDecorations(string name, BiomeDefinition definition,
        List<ProfileValidationIssue> issues)
    {
        var decorations = definition.Decorations;
        if (decorations is null) return;

        if (decorations.Count > MaxDecorationsPerPool)
            issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                $"biomes.definitions[\"{name}\"] has {decorations.Count} decorations; at most " +
                $"{MaxDecorationsPerPool} are allowed."));

        for (var i = 0; i < decorations.Count; ++i)
        {
            var decoration = decorations[i];
            if (string.IsNullOrWhiteSpace(decoration.Template))
                issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                    $"biomes.definitions[\"{name}\"].decorations[{i}].template must be non-empty."));
            if (decoration.Weight is <= 0.0 or > 1.0)
                issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                    $"biomes.definitions[\"{name}\"].decorations[{i}].weight " +
                    $"({decoration.Weight}) must be between 0 and 1, exclusive of 0."));
            if (decoration.MinSpacing < 1)
                issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                    $"biomes.definitions[\"{name}\"].decorations[{i}].minSpacing " +
                    $"({decoration.MinSpacing}) must be at least 1."));
            if (decoration.MaxSlope is < 0 or > MaxDecorationSlope)
                issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                    $"biomes.definitions[\"{name}\"].decorations[{i}].maxSlope " +
                    $"({decoration.MaxSlope}) must be between 0 and {MaxDecorationSlope}."));
        }
    }

    /// <summary>
    ///     Determines whether a table cell references a biome that counts as defined. Swamp
    ///     counts as defined when the swamp override block is present.
    /// </summary>
    /// <param name="biomes">Biome options being validated.</param>
    /// <param name="biomeName">Referenced biome name.</param>
    /// <returns>true if the biome is defined, false otherwise.</returns>
    private static bool IsDefinedBiome(BiomeOptions biomes, string? biomeName)
    {
        return biomeName is not null && (biomes.Definitions.ContainsKey(biomeName)
               || biomeName == nameof(BiomeId.Swamp) && biomes.Swamp is not null);
    }

    /// <summary>
    ///     Computes the set of biome names that must have definitions because the material
    ///     stages can encounter them.
    /// </summary>
    /// <param name="biomes">Biome options being validated.</param>
    /// <param name="tableBiomes">Biome names referenced by the table.</param>
    /// <returns>Required biome names.</returns>
    private static IEnumerable<string> RequiredDefinitions(BiomeOptions biomes,
        HashSet<string> tableBiomes)
    {
        foreach (var name in tableBiomes) yield return name;
        yield return nameof(BiomeId.Beach);
        yield return nameof(BiomeId.Alpine);
        yield return nameof(BiomeId.Snowcap);
        if (biomes.Swamp is not null) yield return nameof(BiomeId.Swamp);
    }
}
