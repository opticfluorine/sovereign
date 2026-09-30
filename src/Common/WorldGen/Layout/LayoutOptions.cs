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
using System.Linq;
using System.Text.Json.Serialization;

namespace Sovereign.WorldGen.Layout;

/// <summary>
///     Layout prior configuration within a world generation profile. All keys are optional;
///     an absent section preserves the pre-layout pipeline output.
/// </summary>
public sealed class LayoutOptions
{
    /// <summary>
    ///     Strength of the continentalness mask in [0, 1]; 0 disables the mask.
    /// </summary>
    public float Strength { get; set; }

    /// <summary>
    ///     Name of a named anchor layout preset, or null when explicit anchors are used.
    ///     Mutually exclusive with <see cref="Anchors" />.
    /// </summary>
    public string? Preset { get; set; }

    /// <summary>
    ///     Explicit anchor list, or null when a preset is used. Mutually exclusive with
    ///     <see cref="Preset" />.
    /// </summary>
    public List<LayoutAnchor>? Anchors { get; set; }

    /// <summary>
    ///     Anchor count used by the <c>random</c> preset.
    /// </summary>
    public int AnchorCount { get; set; } = 6;

    /// <summary>
    ///     Whether the section changes pipeline output at all. False when the mask is off
    ///     and no anchor carries a bias, preserving byte-identical output.
    /// </summary>
    [JsonIgnore]
    public bool IsActive => Strength > 0f && Anchors is { Count: > 0 }
                            || Preset is not null && Strength > 0f
                            || HasAnyBias;

    /// <summary>
    ///     Whether any anchor carries a nonzero bias of any kind.
    /// </summary>
    [JsonIgnore]
    public bool HasAnyBias => Anchors is not null && Anchors.Any(a =>
        a.MountainBias != 0f || a.TemperatureBias != 0f || a.MoistureBias != 0f
        || a.RoughnessBias != 0f);
}

/// <summary>
///     A single layout anchor: a smooth bump in the continentalness mask plus optional
///     regional bias contributions.
/// </summary>
public sealed class LayoutAnchor
{
    /// <summary>
    ///     Anchor center X in normalized footprint coordinates.
    /// </summary>
    public float X { get; set; }

    /// <summary>
    ///     Anchor center Y in normalized footprint coordinates.
    /// </summary>
    public float Y { get; set; }

    /// <summary>
    ///     Bump radius in normalized footprint coordinates.
    /// </summary>
    public float Radius { get; set; } = 0.1f;

    /// <summary>
    ///     Peak mask contribution of the bump before the global clamp.
    /// </summary>
    public float Weight { get; set; } = 1f;

    /// <summary>
    ///     Per-seed positional jitter magnitude in normalized coordinates.
    /// </summary>
    public float Jitter { get; set; }

    /// <summary>
    ///     Mountain bias at the anchor in [-1, 1].
    /// </summary>
    public float MountainBias { get; set; }

    /// <summary>
    ///     Temperature bias at the anchor in [-1, 1].
    /// </summary>
    public float TemperatureBias { get; set; }

    /// <summary>
    ///     Moisture bias at the anchor in [-1, 1].
    /// </summary>
    public float MoistureBias { get; set; }

    /// <summary>
    ///     Roughness bias at the anchor in [-1, 1].
    /// </summary>
    public float RoughnessBias { get; set; }
}
