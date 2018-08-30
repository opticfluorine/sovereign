using Sovereign.ClientCore.Rendering.Configuration;
using SharpDX.DXGI;
using System.Collections.Generic;

namespace Sovereign.D3D11Renderer.Rendering.Configuration
{

    /// <summary>
    /// Enumerates the available video adapters.
    /// </summary>
    public class D3D11AdapterEnumerator : IAdapterEnumerator
    {

        public IEnumerable<IVideoAdapter> EnumerateVideoAdapters()
        {
            var adapters = new List<IVideoAdapter>();

            /* Iterate over the available video adapters. */
            using (var dxgiFactory = new Factory1())
            {
                foreach (var adapter in dxgiFactory.Adapters)
                {
                    /* Ensure that the adapter supports D3D11. */
                    if (SharpDX.Direct3D11.Device.GetSupportedFeatureLevel(adapter)
                        >= SharpDX.Direct3D.FeatureLevel.Level_11_0)
                    {
                        adapters.Add(ConvertAdapter(adapter));
                    }
                }
            }

            return adapters;
        }

        /// <summary>
        /// Converts an adapter exposed by Direct3D to an IVideoAdapter.
        /// </summary>
        /// <param name="dxgiFactory">DXGI factory.</param>
        /// <param name="adapter">Video adapter.</param>
        /// <returns>Adapter.</returns>
        private IVideoAdapter ConvertAdapter(Adapter adapter)
        {
            /* Convert the adapter. */
            var converted = new D3D11VideoAdapter()
            {
                DedicatedGraphicsMemory = adapter.Description.DedicatedVideoMemory,
                DedicatedSystemMemory = adapter.Description.DedicatedSystemMemory,
                SharedSystemMemory = adapter.Description.SharedSystemMemory,
                AdapterName = adapter.Description.Description,
                InternalAdapter = adapter,
            };
            return converted;
        }

    }

}
