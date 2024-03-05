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
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Systems;

namespace Sovereign.ClientCore.Systems.EntitySynchronization;

public class EntitySynchronizationSystem : ISystem
{
    private readonly IEventLoop eventLoop;
    private readonly EntityDefinitionProcessor processor;
    private readonly ClientEntityUnloader unloader;
    private ulong playerEntityId;

    public EntitySynchronizationSystem(EventCommunicator eventCommunicator,
        IEventLoop eventLoop, EntityDefinitionProcessor processor,
        ClientEntityUnloader unloader)
    {
        this.eventLoop = eventLoop;
        this.processor = processor;
        this.unloader = unloader;
        EventCommunicator = eventCommunicator;

        eventLoop.RegisterSystem(this);
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    public EventCommunicator EventCommunicator { get; }

    public ISet<EventId> EventIdsOfInterest { get; } = new HashSet<EventId>
    {
        EventId.Client_EntitySynchronization_Sync,
        EventId.Client_EntitySynchronization_Desync,
        EventId.Core_WorldManagement_Subscribe,
        EventId.Core_WorldManagement_Unsubscribe,
        EventId.Client_Network_PlayerEntitySelected,
        EventId.Core_WorldManagement_EntityLeaveWorldSegment
    };

    public int WorkloadEstimate => 50;

    public void Initialize()
    {
    }

    public void Cleanup()
    {
    }

    public int ExecuteOnce()
    {
        var eventsProcessed = 0;
        while (EventCommunicator.GetIncomingEvent(out var ev))
        {
            eventsProcessed++;
            switch (ev.EventId)
            {
                case EventId.Client_EntitySynchronization_Sync:
                    if (ev.EventDetails is not EntityDefinitionEventDetails)
                    {
                        Logger.Error("Received Sync event without details.");
                        break;
                    }

                    HandleSync((EntityDefinitionEventDetails)ev.EventDetails);
                    break;

                case EventId.Client_EntitySynchronization_Desync:
                    if (ev.EventDetails is not EntityDesyncEventDetails)
                    {
                        Logger.Error("Received Desync event without details.");
                        break;
                    }

                    HandleDesync((EntityDesyncEventDetails)ev.EventDetails);
                    break;

                case EventId.Core_WorldManagement_Subscribe:
                    if (ev.EventDetails is not WorldSegmentSubscriptionEventDetails)
                    {
                        Logger.Error("Received Subscribe event without details.");
                        break;
                    }

                    HandleSubscribe((WorldSegmentSubscriptionEventDetails)ev.EventDetails);
                    break;

                case EventId.Core_WorldManagement_Unsubscribe:
                    if (ev.EventDetails is not WorldSegmentSubscriptionEventDetails)
                    {
                        Logger.Error("Received Unsubscribe event without details.");
                        break;
                    }

                    HandleUnsubscribe((WorldSegmentSubscriptionEventDetails)ev.EventDetails);
                    break;

                case EventId.Client_Network_PlayerEntitySelected:
                    if (ev.EventDetails is not EntityEventDetails)
                    {
                        Logger.Error("Received PlayerEntitySelected event without details.");
                        break;
                    }

                    HandlePlayerSelect((EntityEventDetails)ev.EventDetails);
                    break;

                case EventId.Core_WorldManagement_EntityLeaveWorldSegment:
                    if (ev.EventDetails is not EntityChangeWorldSegmentEventDetails)
                    {
                        Logger.Error("Received EntityLeaveWorldSegment event without details.");
                        break;
                    }

                    HandleChangeWorldSegment((EntityChangeWorldSegmentEventDetails)ev.EventDetails);
                    break;
            }
        }

        return eventsProcessed;
    }

    private void HandleSubscribe(WorldSegmentSubscriptionEventDetails details)
    {
        unloader.OnSubscribe(details.SegmentIndex);
    }

    /// <summary>
    ///     Handles a world segment unsubscribe event.
    /// </summary>
    /// <param name="details">Event details.</param>
    private void HandleUnsubscribe(WorldSegmentSubscriptionEventDetails details)
    {
        unloader.OnUnsubscribe(details.SegmentIndex);
    }

    /// <summary>
    ///     Handles notification of an entity leaving a world segment.
    /// </summary>
    /// <param name="details">Event details.</param>
    private void HandleChangeWorldSegment(EntityChangeWorldSegmentEventDetails details)
    {
        if (details.EntityId == playerEntityId)
            Logger.DebugFormat("Player entered world segment {0}.", details.NewSegmentIndex);

        unloader.OnEntityChangeWorldSegment(details.EntityId,
            details.NewSegmentIndex);
    }

    /// <summary>
    ///     Handles a player select event from the server.
    /// </summary>
    /// <param name="details">Event details.</param>
    private void HandlePlayerSelect(EntityEventDetails details)
    {
        playerEntityId = details.EntityId;
        unloader.SetPlayer(details.EntityId);
    }

    /// <summary>
    ///     Handles an entity synchronization update.
    /// </summary>
    /// <param name="details">Update details.</param>
    private void HandleSync(EntityDefinitionEventDetails details)
    {
        Logger.DebugFormat("Processing {0} entity definitions.", details.EntityDefinitions.Count);
        foreach (var definition in details.EntityDefinitions)
            processor.ProcessDefinition(definition);
    }

    /// <summary>
    ///     Handles an entity desynchronization.
    /// </summary>
    /// <param name="details">Desync details.</param>
    private void HandleDesync(EntityDesyncEventDetails details)
    {
        Logger.DebugFormat("Desynchronizing entity ID {0} and descendants.", details.EntityId);
        unloader.OnDesync(details.EntityId);
    }
}