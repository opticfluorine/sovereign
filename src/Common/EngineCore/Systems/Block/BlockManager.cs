﻿/*
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

using Castle.Core.Logging;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Systems.Block.Components;
using Sovereign.EngineCore.Systems.Block.Components.Indexers;
using Sovereign.EngineCore.Systems.Block.Events;
using Sovereign.EngineCore.Systems.Movement.Components;
using Sovereign.EngineUtil.Monads;
using System;
using System.Linq;
using System.Numerics;

namespace Sovereign.EngineCore.Systems.Block
{

    /// <summary>
    /// Responsible for managing the creation and destruction of block entities.
    /// </summary>
    public sealed class BlockManager
    {

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        private readonly IEntityFactory entityFactory;
        private readonly EntityManager entityManager;
        private readonly BlockGridPositionIndexer blockPositions;
        private readonly AboveBlockComponentCollection aboveBlocks;
        private readonly PositionComponentCollection positions;

        public BlockManager(IEntityFactory entityFactory,
            EntityManager entityManager,
            BlockGridPositionIndexer blockPositions,
            AboveBlockComponentCollection aboveBlocks,
            PositionComponentCollection positions)
        {
            this.entityFactory = entityFactory;
            this.entityManager = entityManager;
            this.blockPositions = blockPositions;
            this.aboveBlocks = aboveBlocks;
            this.positions = positions;
        }

        /// <summary>
        /// Adds a new block entity.
        /// </summary>
        /// <param name="blockRecord">Block to be added.</param>
        public void AddBlock(BlockRecord blockRecord)
        {
            var blockId = CreateBlockForRecord(blockRecord);
            CoverBelowBlock(blockRecord, blockId);
        }

        /// <summary>
        /// Removes a block entity.
        /// </summary>
        /// <param name="entityId">Entity ID of the block to be removed.</param>
        public void RemoveBlock(ulong entityId)
        {
            UncoverBelowBlock(entityId);
            entityManager.RemoveEntity(entityId);
        }


        /// <summary>
        /// Creates the block described by the given record.
        /// </summary>
        /// <param name="blockRecord">Block record.</param>
        /// <returns>Entity ID of the new block.</returns>
        private ulong CreateBlockForRecord(BlockRecord blockRecord)
        {
            var hasAboveBlock = GetAboveBlock(blockRecord, out var aboveBlock);

            var builder = entityFactory.GetBuilder()
                .Positionable((Vector3)blockRecord.Position)
                .Material(blockRecord.Material, blockRecord.MaterialModifier)
                .Drawable();

            if (hasAboveBlock) builder.AboveBlock(aboveBlock);

            return builder.Build();
        }

        /// <summary>
        /// Update the AboveBlock for block below the current block (if any).
        /// </summary>
        /// <param name="blockRecord">Current block.</param>
        private void CoverBelowBlock(BlockRecord blockRecord, ulong entityId)
        {
            var pos = blockRecord.Position;
            var belowPos = new GridPosition(pos.X, pos.Y, pos.Z - 1);

            var blocks = blockPositions.GetEntitiesAtPosition(belowPos);
            if (blocks == null) return;

            foreach (var belowBlock in blocks)
            {
                aboveBlocks.AddComponent(belowBlock, entityId);
            }
        }

        /// <summary>
        /// Uncovers any block directly below the given block.
        /// </summary>
        /// <param name="entityId">Block ID.</param>
        private void UncoverBelowBlock(ulong entityId)
        {
            var pos = positions.GetComponentForEntity(entityId);
            if (!pos.HasValue)
            {
                Logger.ErrorFormat("Block (Entity ID = {0}) has no position.", entityId);
                return;
            }

            var gridPos = (GridPosition)pos.Value;
            var belowPos = new GridPosition(gridPos.X, gridPos.Y, gridPos.Z - 1);

            var belowBlocks = blockPositions.GetEntitiesAtPosition(belowPos);
            if (belowBlocks == null) return;

            foreach (var belowBlockId in belowBlocks)
            {
                aboveBlocks.RemoveComponent(belowBlockId);
            }
        }

        /// <summary>
        /// Gets the block above the given block.
        /// </summary>
        /// <param name="blockRecord">Block.</param>
        /// <param name="aboveBlock">Entity ID of the above block if any, undefined if not.</param>
        /// <returns>true if a block is above, false otherwise.</returns>
        private bool GetAboveBlock(BlockRecord blockRecord, out ulong aboveBlock)
        {
            var pos = blockRecord.Position;
            var abovePos = new GridPosition(pos.X, pos.Y, pos.Z + 1);

            var blocks = blockPositions.GetEntitiesAtPosition(abovePos);
            if (blocks != null)
                aboveBlock = blocks.First();
            else
                aboveBlock = 0;

            return blocks != null;
        }

    }

}
