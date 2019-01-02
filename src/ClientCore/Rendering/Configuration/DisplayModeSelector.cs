/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using Castle.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Rendering.Configuration
{

    /// <summary>
    /// Responsible for the selection of the display mode.
    /// </summary>
    public class DisplayModeSelector
    {

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        /// <summary>
        /// Default width.
        /// </summary>
        private const int DefaultWidth = 1366;

        /// <summary>
        /// Default height.
        /// </summary>
        private const int DefaultHeight = 768;

        /// <summary>
        /// Display mode enumerator.
        /// </summary>
        private readonly IDisplayModeEnumerator displayModeEnumerator;

        public DisplayModeSelector(IDisplayModeEnumerator displayModeEnumerator)
        {
            this.displayModeEnumerator = displayModeEnumerator;
        }

        /// <summary>
        /// Selects the display mode.
        /// </summary>
        /// <param name="videoAdapter">Video adapter to be used.</param>
        /// <returns>Selected display mode.</returns>
        /// <exception cref="VideoAdapterException">
        /// Thrown if no display modes are found for the given video adapter.
        /// </exception>
        public IDisplayMode SelectDisplayMode(IVideoAdapter videoAdapter)
        {
            /* Ensure that at least one mode is available. */
            var availableModes = displayModeEnumerator.EnumerateDisplayModes(videoAdapter);
            if (availableModes.Count() == 0)
            {
                throw new VideoAdapterException("No display modes found for selected video adapter.");
            }

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
                /* Preferred mode not found, use the highest resolution mode. */
                var resSortedModes = from mode in availableModes
                                     orderby mode.Width * mode.Height descending
                                     select mode;
                selectedMode = resSortedModes.First();
            }

            /* Log the decision and return. */
            Logger.Info(() => CreateLogMessageForMode(selectedMode));
            return selectedMode;
        }

        /// <summary>
        /// Creates a log message describing a display mode.
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

}
