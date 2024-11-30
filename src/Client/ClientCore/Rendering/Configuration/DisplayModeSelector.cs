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
using Microsoft.Extensions.Logging;
using Sovereign.ClientCore.Configuration;

namespace Sovereign.ClientCore.Rendering.Configuration;

/// <summary>
///     Responsible for the selection of the display mode.
/// </summary>
public class DisplayModeSelector
{
    /// <summary>
    ///     Supported aspect ratio.
    /// </summary>
    private const float AspectRatio = 16.0f / 9.0f;

    /// <summary>
    ///     Comparison tolerance for aspect ratio.
    /// </summary>
    private const double AspectRatioTolerance = 1E-1;

    private readonly ClientConfigurationManager configManager;

    /// <summary>
    ///     Display mode enumerator.
    /// </summary>
    private readonly IDisplayModeEnumerator displayModeEnumerator;

    private readonly ILogger<DisplayModeSelector> logger;

    public DisplayModeSelector(IDisplayModeEnumerator displayModeEnumerator, ClientConfigurationManager configManager,
        ILogger<DisplayModeSelector> logger)
    {
        this.displayModeEnumerator = displayModeEnumerator;
        this.configManager = configManager;
        this.logger = logger;
    }

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
        IDisplayMode? selectedMode = null;
        var desiredWidth = configManager.ClientConfiguration.Display.ResolutionWidth;
        var desiredHeight = configManager.ClientConfiguration.Display.ResolutionHeight;
        var preferredModes = from mode in availableModes
            where mode.Width == desiredWidth && mode.Height == desiredHeight
            select mode;
        if (preferredModes.Count() > 0 &&
            Math.Abs((double)desiredWidth / desiredHeight - AspectRatio) < AspectRatioTolerance)
        {
            /* Preferred mode found and has the correct aspect ratio, use the first matching mode. */
            selectedMode = preferredModes.First();
        }
        else
        {
            /* Preferred mode not found, use the highest resolution mode that matches the aspect ratio. */
            var resSortedModes = from mode in availableModes
                where Math.Abs((double)mode.Width / mode.Height - AspectRatio) < AspectRatioTolerance
                orderby mode.Width * mode.Height descending
                select mode;
            try
            {
                selectedMode = resSortedModes.First();
                logger.LogWarning("Requested display size {ReqW}x{ReqH} not supported, falling back to {W}x{H}.",
                    desiredWidth, desiredHeight, selectedMode.Width, selectedMode.Height);
            }
            catch
            {
                throw new VideoAdapterException("No display modes found for selected video adapter.");
            }
        }

        /* Log the decision and return. */
        logger.LogInformation(CreateLogMessageForMode(selectedMode));
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