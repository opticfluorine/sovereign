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
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Player;
using Sovereign.EngineCore.Systems;

namespace Sovereign.ServerCore.Systems.TemplateEntity;

/// <summary>
///     System responsible for managing server-side template entities.
/// </summary>
public class TemplateEntitySystem : ISystem
{
    private readonly TemplateEntityDataGenerator dataGenerator;
    private readonly LoggingUtil loggingUtil;
    private readonly TemplateEntityManager manager;
    private readonly PlayerRoleCheck roleCheck;

    public TemplateEntitySystem(IEventLoop eventLoop, EventCommunicator eventCommunicator,
        TemplateEntityDataGenerator dataGenerator, PlayerRoleCheck roleCheck, LoggingUtil loggingUtil,
        TemplateEntityManager manager)
    {
        this.dataGenerator = dataGenerator;
        this.roleCheck = roleCheck;
        this.loggingUtil = loggingUtil;
        this.manager = manager;
        EventCommunicator = eventCommunicator;
        eventLoop.RegisterSystem(this);
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    public EventCommunicator EventCommunicator { get; }

    public ISet<EventId> EventIdsOfInterest { get; } = new HashSet<EventId>
    {
        EventId.Core_Tick,
        EventId.Server_TemplateEntity_Update
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

                case EventId.Core_Tick:
                    OnTick();
                    break;

                default:
                    Logger.Error("Unhandled event in TemplateEntitySystem.");
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
            Logger.Error("Bad details for Update.");
            return;
        }

        if (!roleCheck.IsPlayerAdmin(requestDetails.PlayerEntityId))
        {
            Logger.ErrorFormat("[Security] Attempted Update by non-admin player {0}.",
                loggingUtil.FormatEntity(requestDetails.PlayerEntityId));
            return;
        }

        foreach (var definition in requestDetails.EntityDefinitions)
        {
            Logger.InfoFormat("Template entity {0} updated by player {1}.", definition.EntityId,
                loggingUtil.FormatEntity(requestDetails.PlayerEntityId));
            manager.UpdateExisting(definition);
        }
    }
}