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
    /// Display mode available through Direct3D 11.
    /// </summary>
    public class D3D11DisplayMode : IDisplayMode
    {

        public int Width { get; internal set; }

        public int Height { get; internal set; }

        public DisplayFormat DisplayFormat { get; internal set; }

        /// <summary>
        /// DXGI mode description.
        /// </summary>
        internal ModeDescription InternalModeDescription { get; set; }

    }

}
