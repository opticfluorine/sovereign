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
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Player;
using Sovereign.ServerCore.Configuration;

namespace Sovereign.ServerCore.Systems.WorldManagement;

/// <summary>
///     Synchronizes sets of entities to the client.
/// </summary>
public class EntitySynchronizer
{
    private const int DefaultBufferSize = 256;
    private const int DefaultBatchBufferSize = 32;
    private readonly List<List<EntityDefinition>> adminBatches = new(DefaultBatchBufferSize);
    private readonly WorldManagementInternalController controller;
    private readonly EntityDefinitionGenerator definitionGenerator;
    private readonly IEventSender eventSender;
    private readonly List<List<EntityDefinition>> everyoneBatches = new(DefaultBatchBufferSize);
    private readonly List<ulong> forAdmins = new(DefaultBufferSize);
    private readonly List<ulong> forEveryone = new(DefaultBufferSize);
    private readonly ILogger<EntitySynchronizer> logger;
    private readonly NetworkOptions networkOptions;
    private readonly PlayerRoleCheck roleCheck;
    private readonly ServerOnlyTagCollection serverOnly;
    private readonly List<ulong> singletonList = [0];
    private readonly Lock syncLock = new();

    public EntitySynchronizer(IEventSender eventSender, IOptions<NetworkOptions> networkOptions,
        WorldManagementInternalController controller, EntityDefinitionGenerator definitionGenerator,
        ServerOnlyTagCollection serverOnly, PlayerRoleCheck roleCheck, ILogger<EntitySynchronizer> logger)
    {
        this.eventSender = eventSender;
        this.controller = controller;
        this.definitionGenerator = definitionGenerator;
        this.serverOnly = serverOnly;
        this.roleCheck = roleCheck;
        this.logger = logger;
        this.networkOptions = networkOptions.Value;
    }

    /// <summary>
    ///     Synchronizes the given set of entities to a client. This method is thread-safe.
    /// </summary>
    /// <param name="playerEntityId">Player to synchronize with.</param>
    /// <param name="entities">Entities to synchronize.</param>
    public void Synchronize(ulong playerEntityId, List<ulong> entities)
    {
        lock (syncLock)
        {
            singletonList[0] = playerEntityId;
            Synchronize(singletonList, entities);
        }
    }

    /// <summary>
    ///     Synchronizes the given set of entities to a client. This method is thread-safe.
    /// </summary>
    /// <param name="playerEntityIds">Players to synchronize with.</param>
    /// <param name="entities">Entities to synchronize.</param>
    public void Synchronize(List<ulong> playerEntityIds, List<ulong> entities)
    {
        // This lock shouldn't have much contention - only two threads access it (the main thread where component
        // updates are committed, and the executor thread for the WorldManagement system), and the access will
        // typically be out of phase.
        lock (syncLock)
        {
            // Group the entities by audience.
            forEveryone.Clear();
            forAdmins.Clear();
            foreach (var entityId in entities)
                if (!serverOnly.HasTagForEntity(entityId) || EntityUtil.IsTemplateEntity(entityId))
                    forEveryone.Add(entityId);
                else forAdmins.Add(entityId);

            // Separate the entities into small batches to reduce network overhead.
            everyoneBatches.Clear();
            MakeBatches(forEveryone, everyoneBatches);

            if (forAdmins.Count > 0)
            {
                adminBatches.Clear();
                MakeBatches(forAdmins, adminBatches);
            }

            // Send each batch to the client as its own event.
            foreach (var playerEntityId in playerEntityIds)
            {
                foreach (var batch in everyoneBatches)
                    controller.PushSyncEvent(eventSender, playerEntityId, batch);

                if (!roleCheck.IsPlayerAdmin(playerEntityId) && forAdmins.Count > 0)
                    logger.LogDebug("Admin sync issue!");

                if (forAdmins.Count == 0 || !roleCheck.IsPlayerAdmin(playerEntityId)) continue;
                foreach (var batch in adminBatches)
                    controller.PushSyncEvent(eventSender, playerEntityId, batch);
            }
        }
    }

    /// <summary>
    ///     Desynchronizes an entity tree across a world segment. This method is thread-safe.
    /// </summary>
    /// <param name="rootEntityId">Root of the entity tree to be desynchronized.</param>
    /// <param name="segmentIndex">World segment index of the entity.</param>
    public void Desynchronize(ulong rootEntityId, GridPosition segmentIndex)
    {
        logger.LogDebug("Desync {Id:X} for world segment {SegmentIndex}.", rootEntityId, segmentIndex);
        controller.PushDesyncEvent(eventSender, rootEntityId, segmentIndex);
    }

    /// <summary>
    ///     Groups entities into entity definition batches.
    /// </summary>
    /// <param name="entities">Entities to batch.</param>
    /// <param name="batches">List to populate with batches.</param>
    private void MakeBatches(List<ulong> entities, List<List<EntityDefinition>> batches)
    {
        List<EntityDefinition>? currentBatch = null;
        for (var i = 0; i < entities.Count; ++i)
        {
            if (i % networkOptions.EntitySyncBatchSize == 0)
            {
                currentBatch = new List<EntityDefinition>(networkOptions.EntitySyncBatchSize);
                batches.Add(currentBatch);
            }

            currentBatch!.Add(definitionGenerator.GenerateDefinition(entities[i]));
        }
    }
}