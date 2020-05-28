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
using System;
using System.Text;

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

        public int OutputCount { get; internal set; }

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
