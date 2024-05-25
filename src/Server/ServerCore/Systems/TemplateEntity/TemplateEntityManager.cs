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
using Castle.Core.Logging;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;

namespace Sovereign.ServerCore.Systems.TemplateEntity;

/// <summary>
///     Responsible for the management of the server-side set of template entities.
/// </summary>
public class TemplateEntityManager
{
    private readonly EntityDefinitionProcessor definitionProcessor;
    private readonly IEntityFactory entityFactory;
    private readonly IEventSender eventSender;
    private readonly TemplateEntityInternalController internalController;

    private readonly List<ulong> pendingSync = new();

    public TemplateEntityManager(IEntityFactory entityFactory, IEventSender eventSender,
        TemplateEntityInternalController internalController, EntityDefinitionProcessor definitionProcessor)
    {
        this.entityFactory = entityFactory;
        this.eventSender = eventSender;
        this.internalController = internalController;
        this.definitionProcessor = definitionProcessor;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    /// <summary>
    ///     Creates or updates a template entity based on an entity definition.
    /// </summary>
    /// <param name="definition">Entity definition.</param>
    public void UpdateExisting(EntityDefinition definition)
    {
        if (definition.EntityId < EntityConstants.FirstTemplateEntityId ||
            definition.EntityId > EntityConstants.LastTemplateEntityId)
        {
            Logger.ErrorFormat("Received update for non-template entity ID {0}; skipping.", definition.EntityId);
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
}