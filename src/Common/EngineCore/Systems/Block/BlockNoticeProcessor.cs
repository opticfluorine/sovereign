// Sovereign Engine
// Copyright (c) 2024 opticfluorine
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events.Details;

namespace Sovereign.EngineCore.Systems.Block;

/// <summary>
///     Processes block notice events (modify/remove) from the server.
/// </summary>
public class BlockNoticeProcessor
{
    private readonly BlockManager blockManager;
    private readonly BlockServices blockServices;
    private readonly EntityTable entityTable;
    private readonly ILogger<BlockNoticeProcessor> logger;

    public BlockNoticeProcessor(BlockServices blockServices, BlockManager blockManager, EntityTable entityTable,
        ILogger<BlockNoticeProcessor> logger)
    {
        this.blockServices = blockServices;
        this.blockManager = blockManager;
        this.entityTable = entityTable;
        this.logger = logger;
    }

    /// <summary>
    ///     Processes a block modification notice from the server, overwriting any existing
    ///     block data at the given position with the data found in the notice.
    /// </summary>
    /// <param name="blockRecord">Block record from the modification notice.</param>
    public void ProcessModifyNotice(BlockRecord blockRecord)
    {
        logger.LogDebug("Modify {Pos} => {Id}", blockRecord.Position, blockRecord.TemplateEntityId);

        if (blockServices.TryGetBlockAtPosition(blockRecord.Position, out var entityId))
            // Block already exists, update in place.
            entityTable.SetTemplate(entityId, blockRecord.TemplateEntityId);
        else
            // Block is new, create it.
            blockManager.AddBlock(blockRecord, false);
    }

    /// <summary>
    ///     Processes a block removal notice from the server, removing the existing block (if any)
    ///     at the given position.
    /// </summary>
    /// <param name="blockPosition">Block position from the removal notice.</param>
    public void ProcessRemoveNotice(GridPosition blockPosition)
    {
        logger.LogDebug("Remove {BlockPosition}", blockPosition);

        if (!blockServices.TryGetBlockAtPosition(blockPosition, out var entityId)) return;
        blockManager.RemoveBlock(entityId);
    }
}