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
