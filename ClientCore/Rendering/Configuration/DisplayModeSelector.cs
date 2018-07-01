using Castle.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine8.ClientCore.Rendering.Configuration
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
        private const int DefaultWidth = 1440;

        /// <summary>
        /// Default height.
        /// </summary>
        private const int DefaultHeight = 900;

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
