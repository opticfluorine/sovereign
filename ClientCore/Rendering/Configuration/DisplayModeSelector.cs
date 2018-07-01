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
            /* Select the highest resolution mode. */
            try
            {
                var selectedMode = displayModeEnumerator.EnumerateDisplayModes(videoAdapter)
                    .OrderByDescending(mode => mode.Width * mode.Height)
                    .First();
                Logger.Info(() => CreateLogMessageForMode(selectedMode));
                return selectedMode;
            } catch (Exception e)
            {
                throw new VideoAdapterException("No display modes found for selected video adapter.",  e);
            }
        }

        /// <summary>
        /// Creates a log message describing a display mode.
        /// </summary>
        /// <param name="selectedMode">Mode to be described.</param>
        /// <returns>Log message.</returns>
        private String CreateLogMessageForMode(IDisplayMode selectedMode)
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
