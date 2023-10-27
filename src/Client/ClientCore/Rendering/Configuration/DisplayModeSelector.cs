/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Linq;
using System.Text;
using Castle.Core.Logging;

namespace Sovereign.ClientCore.Rendering.Configuration;

/// <summary>
///     Responsible for the selection of the display mode.
/// </summary>
public class DisplayModeSelector
{
    /// <summary>
    ///     Default width.
    /// </summary>
    private const int DefaultWidth = 1280;

    /// <summary>
    ///     Default height.
    /// </summary>
    private const int DefaultHeight = 720;

    /// <summary>
    ///     Comparison tolerance for aspect ratio.
    /// </summary>
    private const double AspectRatioTolerance = 1E-1;

    /// <summary>
    ///     Display mode enumerator.
    /// </summary>
    private readonly IDisplayModeEnumerator displayModeEnumerator;

    public DisplayModeSelector(IDisplayModeEnumerator displayModeEnumerator)
    {
        this.displayModeEnumerator = displayModeEnumerator;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    /// <summary>
    ///     Selects the display mode.
    /// </summary>
    /// <param name="videoAdapter">Video adapter to be used.</param>
    /// <returns>Selected display mode.</returns>
    /// <exception cref="VideoAdapterException">
    ///     Thrown if no display modes are found for the given video adapter.
    /// </exception>
    public IDisplayMode SelectDisplayMode(IVideoAdapter videoAdapter)
    {
        /* Ensure that at least one mode is available. */
        var availableModes = displayModeEnumerator.EnumerateDisplayModes(videoAdapter);
        if (availableModes.Count() == 0)
            throw new VideoAdapterException("No display modes found for selected video adapter.");

        /* Use the preferred mode if it is available. */
        IDisplayMode selectedMode = null;
        var preferredModes = from mode in availableModes
            where mode.Width == DefaultWidth && mode.Height == DefaultHeight
            select mode;
        if (preferredModes.Count() > 0)
        {
            /* Preferred mode found, use the first matching mode. */
            selectedMode = preferredModes.First();
        }
        else
        {
            /* Preferred mode not found, use the highest resolution mode that matches the aspect ratio. */
            var aspectRatio = (double)DefaultWidth / DefaultHeight;
            var resSortedModes = from mode in availableModes
                where Math.Abs((double)mode.Width / mode.Height - aspectRatio) < AspectRatioTolerance
                orderby mode.Width * mode.Height descending
                select mode;
            try
            {
                selectedMode = resSortedModes.First();
            }
            catch
            {
                throw new VideoAdapterException("No display modes found for selected video adapter.");
            }
        }

        /* Log the decision and return. */
        Logger.Info(() => CreateLogMessageForMode(selectedMode));
        return selectedMode;
    }

    /// <summary>
    ///     Creates a log message describing a display mode.
    /// </summary>
    /// <param name="selectedMode">Mode to be described.</param>
    /// <returns>Log message.</returns>
    private string CreateLogMessageForMode(IDisplayMode selectedMode)
    {
        var sb = new StringBuilder();
        sb.Append("Selected display mode:\n")
            .Append("  Width  = ")
            .Append(selectedMode.Width)
            .Append("\n")
            .Append("  Height = ")
            .Append(selectedMode.Height)
            .Append("\n")
            .Append("  Format = ")
            .Append(selectedMode.DisplayFormat.ToString());

        return sb.ToString();
    }
}