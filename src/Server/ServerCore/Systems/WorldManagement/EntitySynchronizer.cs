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
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.ServerCore.Configuration;

namespace Sovereign.ServerCore.Systems.WorldManagement;

/// <summary>
///     Synchronizes sets of entities to the client.
/// </summary>
public class EntitySynchronizer
{
    private readonly IServerConfigurationManager configManager;
    private readonly WorldManagementInternalController controller;
    private readonly IEventSender eventSender;

    public EntitySynchronizer(IEventSender eventSender, IServerConfigurationManager configManager,
        WorldManagementInternalController controller)
    {
        this.eventSender = eventSender;
        this.configManager = configManager;
        this.controller = controller;
    }

    /// <summary>
    ///     Synchronizes the given set of entities to a client.
    /// </summary>
    /// <param name="playerEntityId">Player to synchronize with.</param>
    /// <param name="entities">Entities to synchronize.</param>
    public void Synchronize(ulong playerEntityId, IEnumerable<ulong> entities)
    {
        // Batch the entities and generate definitions for each.
        var definitionBatches =
            entities
                .Chunk(configManager.ServerConfiguration.Network.EntitySyncBatchSize)
                .Select(batch => batch.Select(GenerateDefinition).ToList());

        // Send each batch to the client as its own event.
        foreach (var batch in definitionBatches)
        {
            controller.PushSyncEvent(eventSender, playerEntityId, batch);
        }
    }

    public EntityDefinition GenerateDefinition(ulong entityId)
    {
        return new EntityDefinition();
    }
}