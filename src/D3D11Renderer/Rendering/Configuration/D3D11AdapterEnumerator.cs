/*
 * Sovereign Engine
 * Copyright (c) 2020 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
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
                OutputCount = adapter.Outputs.Length,
                AdapterName = adapter.Description.Description,
                InternalAdapter = adapter
            };
            return converted;
        }

    }

}
