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
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Player;
using Sovereign.EngineCore.Systems;
using EventId = Sovereign.EngineCore.Events.EventId;

namespace Sovereign.ServerCore.Systems.TemplateEntity;

/// <summary>
///     System responsible for managing server-side template entities.
/// </summary>
public class TemplateEntitySystem : ISystem
{
    private readonly TemplateEntityDataGenerator dataGenerator;
    private readonly ILogger<TemplateEntitySystem> logger;
    private readonly LoggingUtil loggingUtil;
    private readonly TemplateEntityManager manager;
    private readonly PlayerRoleCheck roleCheck;

    public TemplateEntitySystem(IEventLoop eventLoop, EventCommunicator eventCommunicator,
        TemplateEntityDataGenerator dataGenerator, PlayerRoleCheck roleCheck, LoggingUtil loggingUtil,
        TemplateEntityManager manager, ILogger<TemplateEntitySystem> logger)
    {
        this.dataGenerator = dataGenerator;
        this.roleCheck = roleCheck;
        this.loggingUtil = loggingUtil;
        this.manager = manager;
        this.logger = logger;
        EventCommunicator = eventCommunicator;
        eventLoop.RegisterSystem(this);
    }

    public EventCommunicator EventCommunicator { get; }

    public ISet<EventId> EventIdsOfInterest { get; } = new HashSet<EventId>
    {
        EventId.Core_Tick,
        EventId.Server_TemplateEntity_Update,
        EventId.Server_TemplateEntity_UpdateKeyed
    };

    public int WorkloadEstimate => 10;

    public void Initialize()
    {
    }

    public void Cleanup()
    {
    }

    public int ExecuteOnce()
    {
        var eventCount = 0;
        while (EventCommunicator.GetIncomingEvent(out var ev))
        {
            eventCount++;
            switch (ev.EventId)
            {
                case EventId.Server_TemplateEntity_Update:
                    OnUpdate(ev.EventDetails);
                    break;

                case EventId.Server_TemplateEntity_UpdateKeyed:
                {
                    if (ev.EventDetails is not KeyedEntityDefinitionEventDetails details)
                    {
                        logger.LogError("Bad details for UpdateKeyed.");
                        break;
                    }

                    OnUpdateKeyed(details.EntityDefinition, details.EntityKeyValuePairs);
                    break;
                }

                case EventId.Core_Tick:
                    OnTick();
                    break;

                default:
                    logger.LogError("Unhandled event in TemplateEntitySystem.");
                    break;
            }
        }

        return eventCount;
    }

    /// <summary>
    ///     Handles a Tick event by synchronizing any pending changes.
    /// </summary>
    private void OnTick()
    {
        if (manager.TrySyncPendingTemplates()) dataGenerator.OnTemplatesChanged();
    }

    /// <summary>
    ///     Handles an Update event.
    /// </summary>
    /// <param name="details">Details.</param>
    private void OnUpdate(IEventDetails? details)
    {
        if (details is not EntityDefinitionEventDetails requestDetails)
        {
            logger.LogError("Bad details for Update.");
            return;
        }

        if (!roleCheck.IsPlayerAdmin(requestDetails.PlayerEntityId))
        {
            logger.LogError("[Security] Attempted Update by non-admin player {Player}.",
                loggingUtil.FormatEntity(requestDetails.PlayerEntityId));
            return;
        }

        foreach (var definition in requestDetails.EntityDefinitions)
        {
            logger.LogInformation("Template entity {EntityId:X} updated by player {Player}.", definition.EntityId,
                loggingUtil.FormatEntity(requestDetails.PlayerEntityId));
            manager.UpdateExisting(definition);
        }
    }

    /// <summary>
    ///     Handles an UpdateKeyed event.
    /// </summary>
    /// <param name="definition">Entity definition.</param>
    /// <param name="keyValuePairs">Complete entity key-value dzta.</param>
    private void OnUpdateKeyed(EntityDefinition definition, Dictionary<string, string> keyValuePairs)
    {
        manager.UpdateKeyed(definition, keyValuePairs);
    }
}