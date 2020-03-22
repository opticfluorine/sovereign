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
