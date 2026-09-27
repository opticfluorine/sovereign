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
        return issues;
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

        var hasCaveLevels = profile.CaveLevels is { Count: > 0 };
        if (!hasCaveLevels && (caves.ShaftsPerLevelPair != 0 || caves.SurfaceMouths != 0))
            issues.Add(new ProfileValidationIssue(ProfileValidationSeverity.Error,
                "caves must be absent or all-zero when caveLevels is empty."));
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
}
