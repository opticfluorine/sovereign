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

using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.ServerCore.Configuration;

namespace Sovereign.ServerCore.Systems.WorldManagement;

/// <summary>
///     Synchronizes sets of entities to the client.
/// </summary>
public class EntitySynchronizer
{
    private readonly WorldManagementInternalController controller;
    private readonly EntityDefinitionGenerator definitionGenerator;
    private readonly IEventSender eventSender;
    private readonly ILogger<EntitySynchronizer> logger;
    private readonly NetworkOptions networkOptions;

    public EntitySynchronizer(IEventSender eventSender, IOptions<NetworkOptions> networkOptions,
        WorldManagementInternalController controller, EntityDefinitionGenerator definitionGenerator,
        ILogger<EntitySynchronizer> logger)
    {
        this.eventSender = eventSender;
        this.controller = controller;
        this.definitionGenerator = definitionGenerator;
        this.logger = logger;
        this.networkOptions = networkOptions.Value;
    }

    /// <summary>
    ///     Synchronizes the given set of entities to a client.
    /// </summary>
    /// <param name="playerEntityId">Player to synchronize with.</param>
    /// <param name="entities">Entities to synchronize.</param>
    public void Synchronize(ulong playerEntityId, IEnumerable<ulong> entities)
    {
        Synchronize(Enumerable.Repeat(playerEntityId, 1), entities);
    }

    /// <summary>
    ///     Synchronizes the given set of entities to a client.
    /// </summary>
    /// <param name="playerEntityIds">Players to synchronize with.</param>
    /// <param name="entities">Entities to synchronize.</param>
    public void Synchronize(IEnumerable<ulong> playerEntityIds, IEnumerable<ulong> entities)
    {
        // Batch the entities and generate definitions for each.
        var definitionBatches =
            entities
                .Chunk(networkOptions.EntitySyncBatchSize)
                .Select(batch => batch.Select(definitionGenerator.GenerateDefinition).ToList());

        // Send each batch to the client as its own event.
        foreach (var batch in definitionBatches)
        foreach (var playerEntityId in playerEntityIds)
        {
            logger.LogDebug("Sync {Count} entities to player {Player}.", batch.Count, playerEntityId);
            controller.PushSyncEvent(eventSender, playerEntityId, batch);
        }
    }

    /// <summary>
    ///     Desynchronizes an entity tree across a world segment.
    /// </summary>
    /// <param name="rootEntityId">Root of the entity tree to be desynchronized.</param>
    /// <param name="segmentIndex">World segment index of the entity.</param>
    public void Desynchronize(ulong rootEntityId, GridPosition segmentIndex)
    {
        logger.LogDebug("Desync {Id:X} for world segment {SegmentIndex}.", rootEntityId, segmentIndex);
        controller.PushDesyncEvent(eventSender, rootEntityId, segmentIndex);
    }
}