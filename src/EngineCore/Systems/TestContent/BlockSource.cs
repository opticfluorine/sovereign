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

using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems.Block;
using Sovereign.EngineCore.Systems.Block.Events;
using System.Collections.Generic;

namespace Sovereign.EngineCore.Systems.TestContent
{

    /// <summary>
    /// Responsible for generating blocks for initial testing.
    /// </summary>
    public sealed class BlockSource
    {
        private readonly BlockController blockController;
        private readonly IEventSender eventSender;

        public BlockSource(BlockController blockController, IEventSender eventSender)
        {
            this.blockController = blockController;
            this.eventSender = eventSender;
        }

        /// <summary>
        /// Generates blocks for initial testing.
        /// </summary>
        public void GenerateBlocks()
        {
            blockController.AddBlocks(eventSender, CreateBlockRecords);
        }

        /// <summary>
        /// Callback method to create block records.
        /// </summary>
        /// <param name="records">List to populate with block records.</param>
        private void CreateBlockRecords(IList<BlockRecord> records)
        {
            var mat = 0;
            for (int x = -50; x <= 50; ++x)
            {
                for (int y = -50; y <= 50; ++y)
                {
                    records.Add(new BlockRecord()
                    {
                        Position = new GridPosition(x, y, 0),
                        Material = mat % 2,
                        MaterialModifier = 0
                    });
                    mat++;
                }
            }
        }

    }

}
