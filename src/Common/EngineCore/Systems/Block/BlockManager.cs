/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Linq;
using Castle.Core.Logging;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events.Details;

namespace Sovereign.EngineCore.Systems.Block;

/// <summary>
///     Responsible for managing the creation and destruction of block entities.
/// </summary>
public sealed class BlockManager
{
    private readonly AboveBlockComponentCollection aboveBlocks;
    private readonly BlockGridPositionIndexer blockPositionIndexer;
    private readonly BlockPositionComponentCollection blockPositions;

    private readonly IEntityFactory entityFactory;
    private readonly EntityManager entityManager;

    public BlockManager(IEntityFactory entityFactory,
        EntityManager entityManager,
        BlockGridPositionIndexer blockPositionIndexer,
        AboveBlockComponentCollection aboveBlocks,
        BlockPositionComponentCollection blockPositions)
    {
        this.entityFactory = entityFactory;
        this.entityManager = entityManager;
        this.blockPositionIndexer = blockPositionIndexer;
        this.aboveBlocks = aboveBlocks;
        this.blockPositions = blockPositions;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    /// <summary>
    ///     Adds a new block entity.
    /// </summary>
    /// <param name="blockRecord">Block to be added.</param>
    /// <param name="isLoad">If true, treat as load instead of add.</param>
    public void AddBlock(BlockRecord blockRecord, bool isLoad)
    {
        var blockId = CreateBlockForRecord(blockRecord, isLoad);
        CoverBelowBlock(blockRecord, blockId);
    }

    /// <summary>
    ///     Removes a block entity.
    /// </summary>
    /// <param name="entityId">Entity ID of the block to be removed.</param>
    public void RemoveBlock(ulong entityId)
    {
        UncoverBelowBlock(entityId);
        entityManager.RemoveEntity(entityId);
    }


    /// <summary>
    ///     Creates the block described by the given record.
    /// </summary>
    /// <param name="blockRecord">Block record.</param>
    /// <param name="isLoad">If true, treat as load instead of add.</param>
    /// <returns>Entity ID of the new block.</returns>
    private ulong CreateBlockForRecord(BlockRecord blockRecord, bool isLoad)
    {
        var hasAboveBlock = GetAboveBlock(blockRecord, out var aboveBlock);

        var builder = entityFactory.GetBuilder(EntityType.Block, isLoad)
            .BlockPositionable(blockRecord.Position)
            .Template(blockRecord.TemplateEntityId);

        if (hasAboveBlock) builder.AboveBlock(aboveBlock);

        return builder.Build();
    }

    /// <summary>
    ///     Update the AboveBlock for block below the current block (if any).
    /// </summary>
    /// <param name="blockRecord">Current block.</param>
    /// <param name="entityId">Entity ID.</param>
    private void CoverBelowBlock(BlockRecord blockRecord, ulong entityId)
    {
        var pos = blockRecord.Position;
        var belowPos = new GridPosition(pos.X, pos.Y, pos.Z - 1);

        var blocks = blockPositionIndexer.GetEntitiesAtPosition(belowPos);
        if (blocks == null) return;

        foreach (var belowBlock in blocks.Keys) aboveBlocks.AddComponent(belowBlock, entityId);
    }

    /// <summary>
    ///     Uncovers any block directly below the given block.
    /// </summary>
    /// <param name="entityId">Block ID.</param>
    private void UncoverBelowBlock(ulong entityId)
    {
        if (!blockPositions.HasComponentForEntity(entityId))
        {
            Logger.ErrorFormat("Block (Entity ID = {0}) has no position.", entityId);
            return;
        }

        var gridPos = blockPositions[entityId];
        var belowPos = gridPos with { Z = gridPos.Z - 1 };

        var belowBlocks = blockPositionIndexer.GetEntitiesAtPosition(belowPos);
        if (belowBlocks == null) return;

        foreach (var belowBlockId in belowBlocks.Keys) aboveBlocks.RemoveComponent(belowBlockId);
    }

    /// <summary>
    ///     Gets the block above the given block.
    /// </summary>
    /// <param name="blockRecord">Block.</param>
    /// <param name="aboveBlock">Entity ID of the above block if any, undefined if not.</param>
    /// <returns>true if a block is above, false otherwise.</returns>
    private bool GetAboveBlock(BlockRecord blockRecord, out ulong aboveBlock)
    {
        var pos = blockRecord.Position;
        var abovePos = new GridPosition(pos.X, pos.Y, pos.Z + 1);

        var blocks = blockPositionIndexer.GetEntitiesAtPosition(abovePos);
        var hasAbove = blocks != null && blocks.Count > 0;
        if (hasAbove)
            aboveBlock = blocks!.Keys.First();
        else
            aboveBlock = 0;

        return hasAbove;
    }
}