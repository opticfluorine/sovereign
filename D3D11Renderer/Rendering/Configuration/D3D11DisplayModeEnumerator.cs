using Engine8.ClientCore.Rendering.Configuration;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine8.D3D11Renderer.Rendering.Configuration
{

    /// <summary>
    /// Enumerates the display modes available to an adapter.
    /// </summary>
    public class D3D11DisplayModeEnumerator : IDisplayModeEnumerator
    {

        /// <summary>
        /// Display format to be used.
        /// </summary>
        private const Format DefaultFormat = Format.R8G8B8A8_UNorm;

        /// <summary>
        /// Engine display format corresponding to the display format to be used.
        /// </summary>
        private const DisplayFormat DefaultDisplayFormat = DisplayFormat.R8G8B8A8_UNorm;

        public IEnumerable<IDisplayMode> EnumerateDisplayModes(IVideoAdapter adapter)
        {
            var videoAdapter = (D3D11VideoAdapter)adapter;
            return GetNativeModes(videoAdapter.InternalAdapter)
                .Select(ConvertMode);
        }

        /// <summary>
        /// Gets the D3D11 display modes available to the given adapter.
        /// </summary>
        /// <param name="adapter">D3D11 video adapter.</param>
        /// <returns>Available display modes.</returns>
        private IEnumerable<ModeDescription> GetNativeModes(Adapter adapter)
        {
            return adapter.Outputs.SelectMany(
                output => output.GetDisplayModeList(DefaultFormat, 0))
                .Distinct();
        }

        private D3D11DisplayMode ConvertMode(ModeDescription modeDescription)
        {
            var mode = new D3D11DisplayMode
            {
                Width = modeDescription.Width,
                Height = modeDescription.Height,
                DisplayFormat = DefaultDisplayFormat,
                InternalModeDescription = modeDescription,
            };
            return mode;
        }

    }

}
