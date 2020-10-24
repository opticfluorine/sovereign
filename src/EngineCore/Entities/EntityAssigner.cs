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

namespace Sovereign.EngineCore.Entities
{

    /// <summary>
    /// Responsible for assigning entity IDs within a block.
    /// </summary>
    ///
    /// This class is not thread-safe. Consider using one or more unique
    /// assigners for each ISystem.
    public class EntityAssigner
    {

        /// <summary>
        /// First block ID (upper 32 bits) for persisted entity IDs.
        /// </summary>
        public const ulong PersistedBlock = 0x7FFF0000;

        /// <summary>
        /// First volatile entity ID.
        /// </summary>
        public const ulong FirstVolatileId = 0;

        /// <summary>
        /// First persisted entity ID.
        /// </summary>
        public const ulong FirstPersistedId = PersistedBlock << 32;

        /// <summary>
        /// Entity block assigned to this assigner.
        /// </summary>
        public ulong Block { get; private set; }
        
        /// <summary>
        /// Block ID shifted to the upper dword
        /// </summary>
        private readonly ulong shiftBlock;

        /// <summary>
        /// Counter for the local segment of the id.
        /// </summary>
        private ulong localIdCounter = 0;

        /// <summary>
        /// Creates an assigner for the given block.
        /// </summary>
        /// <param name="block">Block.</param>
        public EntityAssigner(ulong block)
        {
            Block = block;
            shiftBlock = block << 32;
        }

        /// <summary>
        /// Gets the next ID from this assigner.
        /// </summary>
        /// <returns>Next ID.</returns>
        public ulong GetNextId()
        {
            return shiftBlock | localIdCounter++;
        }

    }

}
