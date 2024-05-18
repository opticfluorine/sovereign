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
using Castle.Core.Logging;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Events;
using Sovereign.ServerCore.Configuration;
using Sovereign.ServerCore.Entities;

namespace Sovereign.ServerCore.Systems.WorldManagement;

/// <summary>
///     Synchronizes sets of entities to the client.
/// </summary>
public class EntitySynchronizer
{
    private readonly IServerConfigurationManager configManager;
    private readonly WorldManagementInternalController controller;
    private readonly EntityDefinitionGenerator definitionGenerator;
    private readonly IEventSender eventSender;

    public EntitySynchronizer(IEventSender eventSender, IServerConfigurationManager configManager,
        WorldManagementInternalController controller, EntityDefinitionGenerator definitionGenerator)
    {
        this.eventSender = eventSender;
        this.configManager = configManager;
        this.controller = controller;
        this.definitionGenerator = definitionGenerator;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

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
                .Chunk(configManager.ServerConfiguration.Network.EntitySyncBatchSize)
                .Select(batch => batch.Select(definitionGenerator.GenerateDefinition).ToList());

        // Send each batch to the client as its own event.
        foreach (var batch in definitionBatches)
        foreach (var playerEntityId in playerEntityIds)
        {
            Logger.DebugFormat("Sync {0} entities to player {1}.", batch.Count, playerEntityId);
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
        Logger.DebugFormat("Desync {0} for world segment {1}.", rootEntityId, segmentIndex);
        controller.PushDesyncEvent(eventSender, rootEntityId, segmentIndex);
    }
}