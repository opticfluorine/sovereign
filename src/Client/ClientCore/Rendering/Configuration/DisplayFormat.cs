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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Rendering.Configuration
{

    /// <summary>
    /// Enumeration of possible display formats. These are also used for
    /// internal pixel formats, so not all are suitable for display.
    /// </summary>
    public enum DisplayFormat
    {

        /// <summary>
        /// RGBA, 8 bits per channel, normalized unsigned integer.
        /// </summary>
        R8G8B8A8_UNorm,

        /// <summary>
        /// BGRA, 8 bits per channel, normalized unsigned integer.
        /// </summary>
        B8G8R8A8_UNorm,

        /// <summary>
        /// Represents any SDL-supported pixel format that is not a valid 
        /// display format. These occur in loading sprite resources; conversion
        /// to a display-suitable format occurs when the spritesheets are
        /// blitted onto a new texture atlas surface.
        /// </summary>
        CpuUseOnly,

    }

}
