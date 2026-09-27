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
}
