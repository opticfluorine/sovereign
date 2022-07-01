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

using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sovereign.WorldLib.World.Domain.Block
{

    /// <summary>
    /// Describes a single block within a domain.
    /// </summary>
    [MessagePackObject]
    public class DomainBlock
    {

        /// <summary>
        /// X coordinate of the top-northwest corner of the block.
        /// </summary>
        [Key(0)]
        public int X { get; set; }

        /// <summary>
        /// Y coordinate of the top-northwest corner of the block.
        /// </summary>
        [Key(1)]
        public int Y { get; set; }

        /// <summary>
        /// Z coordinate of the top-northwest corner of the block.
        /// </summary>
        [Key(2)]
        public int Z { get; set; }

        /// <summary>
        /// Material ID of this block.
        /// </summary>
        [Key(3)]
        public uint MaterialId { get; set; }

        /// <summary>
        /// Material modifier of this block.
        /// </summary>
        [Key(4)]
        public uint MaterialModifier { get; set; }

    }
}
