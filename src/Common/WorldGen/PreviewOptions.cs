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

namespace Sovereign.WorldGen;

/// <summary>
///     Preview rendering options of a world generation profile.
/// </summary>
public sealed class PreviewOptions
{
    /// <summary>
    ///     Maximum preview dimension in pixels applied when the profile has no preview
    ///     section.
    /// </summary>
    public const int DefaultMaxDimension = 1024;

    /// <summary>
    ///     Minimum allowed maximum preview dimension in pixels.
    /// </summary>
    public const int MinMaxDimension = 256;

    /// <summary>
    ///     Maximum allowed maximum preview dimension in pixels.
    /// </summary>
    public const int MaxMaxDimension = 8192;

    /// <summary>
    ///     Longest allowed side of a rendered preview image in pixels, in
    ///     <c>[256, 8192]</c>. Longer footprints are box-downscaled to fit.
    /// </summary>
    public int MaxDimension { get; set; } = DefaultMaxDimension;

    /// <summary>
    ///     Whether layout anchor highlighting is drawn over the preview when a layout is
    ///     configured. Enabled by default; disable for a clean terrain image.
    /// </summary>
    public bool ShowAnchorOverlay { get; set; } = true;

    /// <summary>
    ///     Gets the effective maximum preview dimension of a profile: the configured knob
    ///     when present, otherwise the shipped default.
    /// </summary>
    /// <param name="profile">World generation profile.</param>
    /// <returns>Effective maximum preview dimension in pixels.</returns>
    public static int EffectiveMaxDimension(WorldGenProfile profile)
    {
        return profile.Preview?.MaxDimension ?? DefaultMaxDimension;
    }

    /// <summary>
    ///     Gets whether anchor highlighting is enabled for a profile: the configured knob
    ///     when present, otherwise enabled.
    /// </summary>
    /// <param name="profile">World generation profile.</param>
    /// <returns>true when the anchor overlay should be drawn.</returns>
    public static bool EffectiveShowAnchorOverlay(WorldGenProfile profile)
    {
        return profile.Preview?.ShowAnchorOverlay ?? true;
    }
}
