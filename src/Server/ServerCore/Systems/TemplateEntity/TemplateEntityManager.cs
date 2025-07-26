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
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems.Data;

namespace Sovereign.ServerCore.Systems.TemplateEntity;

/// <summary>
///     Responsible for the management of the server-side set of template entities.
/// </summary>
public class TemplateEntityManager
{
    private readonly IDataController dataController;
    private readonly EntityDefinitionProcessor definitionProcessor;
    private readonly IEventSender eventSender;
    private readonly TemplateEntityInternalController internalController;
    private readonly ILogger<TemplateEntityManager> logger;

    private readonly List<ulong> pendingSync = new();

    public TemplateEntityManager(IEventSender eventSender,
        TemplateEntityInternalController internalController, EntityDefinitionProcessor definitionProcessor,
        ILogger<TemplateEntityManager> logger, IDataController dataController)
    {
        this.eventSender = eventSender;
        this.internalController = internalController;
        this.definitionProcessor = definitionProcessor;
        this.logger = logger;
        this.dataController = dataController;
    }

    /// <summary>
    ///     Creates or updates a template entity based on an entity definition.
    /// </summary>
    /// <param name="definition">Entity definition.</param>
    public void UpdateExisting(EntityDefinition definition)
    {
        if (definition.EntityId is < EntityConstants.FirstTemplateEntityId or > EntityConstants.LastTemplateEntityId)
        {
            logger.LogError("Received update for non-template entity ID {Id:X}; skipping.", definition.EntityId);
            return;
        }

        definitionProcessor.ProcessDefinition(definition);
        pendingSync.Add(definition.EntityId);
    }

    /// <summary>
    ///     Synchronizes any pending template entity updates to the clients.
    /// </summary>
    /// <returns>true if one or more template entities were synced, false otherwise.</returns>
    public bool TrySyncPendingTemplates()
    {
        var synced = false;
        foreach (var templateEntityId in pendingSync)
        {
            internalController.SyncTemplateEntity(eventSender, templateEntityId);
            synced = true;
        }

        pendingSync.Clear();
        return synced;
    }

    /// <summary>
    ///     Creates or updates a template entity based on its definition and a set of key-value pairs.
    /// </summary>
    /// <param name="definition"></param>
    /// <param name="keyValuePairs"></param>
    public void UpdateKeyed(EntityDefinition definition, Dictionary<string, string> keyValuePairs)
    {
        if (definition.EntityId is < EntityConstants.FirstTemplateEntityId or > EntityConstants.LastTemplateEntityId)
        {
            logger.LogError("Received update for non-template entity ID {Id:X}; skipping.", definition.EntityId);
            return;
        }

        definitionProcessor.ProcessDefinition(definition);
        dataController.ClearEntityKeyValuesSync(definition.EntityId);
        foreach (var kvp in keyValuePairs)
        {
            if (kvp.Value == string.Empty) continue;
            dataController.SetEntityKeyValueSync(definition.EntityId, kvp.Key, kvp.Value);
        }

        pendingSync.Add(definition.EntityId);
    }
}