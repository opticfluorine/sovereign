// Sovereign Engine
// Copyright (c) 2023 opticfluorine
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

using System;
using System.Collections.Generic;
using System.Numerics;
using Microsoft.Extensions.Logging;
using Sovereign.Accounts.Accounts.Services;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.World;
using Sovereign.EngineUtil.Monads;
using Sovereign.NetworkCore.Network.Pipeline.Outbound;
using Sovereign.ServerNetwork.Network.Pipeline.Outbound.ConnectionMappers;
using EventId = Sovereign.EngineCore.Events.EventId;

namespace Sovereign.ServerNetwork.Network.Pipeline.Outbound;

/// <summary>
///     Maps events to connections in the server-side outbound pipeline.
/// </summary>
public class ServerConnectionMappingOutboundPipelineStage : IConnectionMappingOutboundPipelineStage
{
    private readonly ILogger<ServerConnectionMappingOutboundPipelineStage> logger;

    /// <summary>
    ///     Map of outbound events to specialized connection mappers.
    /// </summary>
    private readonly Dictionary<EventId, ISpecificConnectionMapper> specificMappers = new();

    private IOutboundPipelineStage? nextStage;

    public ServerConnectionMappingOutboundPipelineStage(GlobalConnectionMapper globalMapper,
        SingleEntityConnectionMapperFactory singleConnMapperFactory,
        EntityWorldSegmentConnectionMapperFactory entityWorldSegmentMapperFactory,
        WorldSegmentConnectionMapperFactory worldSegmentMapperFactory,
        GenericChatConnectionMapper genericChatMapper,
        AccountServices accountServices, WorldSegmentResolver resolver,
        ILogger<ServerConnectionMappingOutboundPipelineStage> logger)
    {
        this.logger = logger;
        // Create delegate mappers.
        var worldSubEventMapper = singleConnMapperFactory.Create(evInfo =>
        {
            if (evInfo.Event.EventDetails is not WorldSegmentSubscriptionEventDetails details) return new Maybe<int>();
            return accountServices.GetConnectionIdForPlayer(details.EntityId);
        });

        var entityDefMapper = singleConnMapperFactory.Create(evInfo =>
        {
            if (evInfo.Event.EventDetails is not EntityDefinitionEventDetails details) return new Maybe<int>();
            return accountServices.GetConnectionIdForPlayer(details.PlayerEntityId);
        });

        var moveMapper = entityWorldSegmentMapperFactory.Create(evInfo =>
        {
            if (evInfo.Event.EventDetails is not MoveEventDetails details) return new Maybe<ulong>();
            return new Maybe<ulong>(details.EntityId);
        });

        var entityGridMapper = worldSegmentMapperFactory.Create(evInfo =>
        {
            if (evInfo.Event.EventDetails is not EntityChangeWorldSegmentEventDetails details)
                return new Maybe<GridPosition>();
            return new Maybe<GridPosition>(details.PreviousSegmentIndex);
        });

        var desyncMapper = worldSegmentMapperFactory.Create(evInfo =>
        {
            if (evInfo.Event.EventDetails is not EntityDesyncEventDetails details)
                return new Maybe<GridPosition>();
            return new Maybe<GridPosition>(details.WorldSegmentIndex);
        });

        var localChatMapper = worldSegmentMapperFactory.Create(evInfo =>
        {
            if (evInfo.Event.EventDetails is not LocalChatEventDetails details)
                return new Maybe<GridPosition>();
            return new Maybe<GridPosition>(details.SegmentIndex);
        });

        var systemChatMapper = singleConnMapperFactory.Create(evInfo =>
        {
            if (evInfo.Event.EventDetails is not SystemChatEventDetails details) return new Maybe<int>();
            return accountServices.GetConnectionIdForPlayer(details.TargetEntityId);
        });

        var blockAddMapper = worldSegmentMapperFactory.Create(evInfo =>
        {
            if (evInfo.Event.EventDetails is not BlockAddEventDetails details) return new Maybe<GridPosition>();
            return new Maybe<GridPosition>(resolver.GetWorldSegmentForPosition((Vector3)details.BlockRecord.Position));
        });

        var blockPosMapper = worldSegmentMapperFactory.Create(evInfo =>
        {
            if (evInfo.Event.EventDetails is not GridPositionEventDetails details) return new Maybe<GridPosition>();
            return new Maybe<GridPosition>(resolver.GetWorldSegmentForPosition((Vector3)details.GridPosition));
        });

        var teleportNoticeMapper = worldSegmentMapperFactory.Create(evInfo =>
        {
            if (evInfo.Event.EventDetails is not TeleportNoticeEventDetails details) return new Maybe<GridPosition>();
            return new Maybe<GridPosition>(details.FromWorldSegment);
        });

        // Configure specific connection mappers.
        specificMappers[EventId.Core_Ping_Ping] = globalMapper;
        specificMappers[EventId.Core_WorldManagement_Subscribe] = worldSubEventMapper;
        specificMappers[EventId.Core_WorldManagement_Unsubscribe] = worldSubEventMapper;
        specificMappers[EventId.Core_EntitySync_Sync] = entityDefMapper;
        specificMappers[EventId.Core_EntitySync_Desync] = desyncMapper;
        specificMappers[EventId.Core_EntitySync_SyncTemplate] = globalMapper;
        specificMappers[EventId.Core_Movement_Move] = moveMapper;
        specificMappers[EventId.Core_Movement_TeleportNotice] = teleportNoticeMapper;
        specificMappers[EventId.Core_WorldManagement_EntityLeaveWorldSegment] = entityGridMapper;
        specificMappers[EventId.Core_Chat_Local] = localChatMapper;
        specificMappers[EventId.Core_Chat_Global] = globalMapper;
        specificMappers[EventId.Core_Chat_System] = systemChatMapper;
        specificMappers[EventId.Core_Chat_Generic] = genericChatMapper;
        specificMappers[EventId.Core_Block_ModifyNotice] = blockAddMapper;
        specificMappers[EventId.Core_Block_RemoveNotice] = blockPosMapper;
        specificMappers[EventId.Core_Time_Clock] = globalMapper;
    }

    public void Process(OutboundEventInfo evInfo)
    {
        // Dispatch to the correct mapping strategy.
        if (specificMappers.TryGetValue(evInfo.Event.EventId, out var mapper))
            mapper.Process(evInfo);
        else
            // No mapper found for this event type.
            logger.LogError("No connection mapper available for event ID {Id}.", evInfo.Event.EventId);
    }

    public IOutboundPipelineStage NextStage
    {
        get
        {
            if (nextStage == null) throw new InvalidOperationException("Next stage not set.");
            return nextStage;
        }
        set
        {
            nextStage = value;
            foreach (var mapper in specificMappers.Values) mapper.NextStage = nextStage;
        }
    }
}