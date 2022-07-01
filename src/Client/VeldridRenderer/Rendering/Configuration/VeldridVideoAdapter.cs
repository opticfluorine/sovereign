/*
 * Sovereign Engine
 * Copyright (c) 2021 opticfluorine
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

namespace Sovereign.VeldridRenderer.Rendering.Configuration
{

    /// <summary>
    /// Dummy implementation of IVideoAdapter for the Veldrid renderer.
    /// </summary>
    /// Since the Veldrid library doesn't provide a direct method for querying
    /// (or selecting) specific video adapters, only one dummy adapter will be
    /// presented for selection.
    public class VeldridVideoAdapter : IVideoAdapter
    {
        public long DedicatedSystemMemory => 0;
        public long DedicatedGraphicsMemory => 0;
        public long SharedSystemMemory => 0;
        public int OutputCount => 1;
        public string AdapterName => "Veldrid Default";
    }
}
