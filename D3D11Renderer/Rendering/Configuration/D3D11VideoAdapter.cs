using Sovereign.ClientCore.Rendering.Configuration;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.D3D11Renderer.Rendering.Configuration
{

    /// <summary>
    /// Video adapter summary linked back to the underlying D3D11 representation.
    /// </summary>
    public class D3D11VideoAdapter : IVideoAdapter
    {
        public long DedicatedSystemMemory { get; internal set; }

        public long DedicatedGraphicsMemory { get; internal set; }

        public long SharedSystemMemory { get; internal set; }

        public string AdapterName { get; internal set; }

        /// <summary>
        /// Internal DXGI video adapter described by this IVideoAdapter.
        /// </summary>
        internal Adapter InternalAdapter { get; set; }

        public override String ToString()
        {
            var sb = new StringBuilder();
            sb.Append(nameof(D3D11VideoAdapter)).Append(" / ").Append(AdapterName);
            return sb.ToString();
        }

    }

}
