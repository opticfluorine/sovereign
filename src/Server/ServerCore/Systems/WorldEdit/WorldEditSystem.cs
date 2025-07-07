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
using System.Numerics;
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Systems;
using Sovereign.EngineCore.Systems.Block;
using EventId = Sovereign.EngineCore.Events.EventId;

namespace Sovereign.ServerCore.Systems.WorldEdit;

/// <summary>
///     System responsible for server-side world editor processing.
/// </summary>
public class WorldEditSystem : ISystem
{
    private const int ResetHistoryTicks = 10;

    private readonly BlockController blockController;
    private readonly IBlockServices blockServices;

    // Add dependencies for IEntityFactory and EntityManager
    private readonly IEntityFactory entityFactory;
    private readonly EntityManager entityManager;
    private readonly EntityTable entityTable;
    private readonly IEventSender eventSender;
    private readonly ILogger<WorldEditSystem> logger;
    private readonly LoggingUtil loggingUtil;

    private readonly HashSet<GridPosition> recentAdds = new();
    private int ticksSinceAdd;

    public WorldEditSystem(EventCommunicator eventCommunicator, IEventLoop eventLoop, BlockController blockController,
        IEventSender eventSender, LoggingUtil loggingUtil, IBlockServices blockServices, EntityTable entityTable,
        IEntityFactory entityFactory, EntityManager entityManager, ILogger<WorldEditSystem> logger)
    {
        this.blockController = blockController;
        this.eventSender = eventSender;
        this.loggingUtil = loggingUtil;
        this.blockServices = blockServices;
        this.entityTable = entityTable;
        this.entityFactory = entityFactory;
        this.entityManager = entityManager;
        this.logger = logger;
        EventCommunicator = eventCommunicator;

        eventLoop.RegisterSystem(this);
    }

    public EventCommunicator EventCommunicator { get; }

    public ISet<EventId> EventIdsOfInterest { get; } = new HashSet<EventId>
    {
        EventId.Core_Tick,
        EventId.Server_WorldEdit_SetBlock,
        EventId.Server_WorldEdit_RemoveBlock,
        EventId.Server_WorldEdit_AddNpc,
        EventId.Server_WorldEdit_RemoveNpc
    };

    public int WorkloadEstimate { get; } = 20;

    public void Initialize()
    {
    }

    public void Cleanup()
    {
    }

    public int ExecuteOnce()
    {
        var processed = 0;
        while (EventCommunicator.GetIncomingEvent(out var ev))
        {
            processed++;
            switch (ev.EventId)
            {
                case EventId.Core_Tick:
                    OnTick();
                    break;
                case EventId.Server_WorldEdit_SetBlock:
                {
                    if (ev.EventDetails is not BlockAddEventDetails details)
                    {
                        logger.LogError("Received SetBlock with no details.");
                        break;
                    }

                    HandleSetBlock(details);
                    break;
                }
                case EventId.Server_WorldEdit_RemoveBlock:
                {
                    if (ev.EventDetails is not GridPositionEventDetails details)
                    {
                        logger.LogError("Received RemoveBlock with no details.");
                        break;
                    }

                    HandleRemoveBlock(details);
                    break;
                }
                case EventId.Server_WorldEdit_AddNpc:
                {
                    if (ev.EventDetails is not NpcAddEventDetails details)
                    {
                        logger.LogError("Received AddNpc with no details.");
                        break;
                    }

                    HandleAddNpc(details);
                    break;
                }
                case EventId.Server_WorldEdit_RemoveNpc:
                {
                    if (ev.EventDetails is not NpcRemoveEventDetails details)
                    {
                        logger.LogError("Received RemoveNpc with no details.");
                        break;
                    }

                    HandleRemoveNpc(details);
                    break;
                }
            }
        }

        return processed;
    }

    /// <summary>
    ///     Called once per tick.
    /// </summary>
    private void OnTick()
    {
        if (ticksSinceAdd++ > ResetHistoryTicks)
        {
            recentAdds.Clear();
            ticksSinceAdd = 0;
        }
    }

    /// <summary>
    ///     Handles a set block world edit request.
    /// </summary>
    /// <param name="details">Request details.</param>
    private void HandleSetBlock(BlockAddEventDetails details)
    {
        logger.LogDebug("Set block type {Id} at {Pos}.", loggingUtil.FormatEntity(details.BlockRecord.TemplateEntityId),
            details.BlockRecord.Position);

        // Ignore rapid duplicates.
        if (recentAdds.Contains(details.BlockRecord.Position))
        {
            logger.LogDebug("Block at {Position} recently added, skipping.", details.BlockRecord.Position);
            return;
        }

        if (blockServices.TryGetBlockAtPosition(details.BlockRecord.Position, out var entityId))
            // Block already exists, update in place.
        {
            entityTable.SetTemplate(entityId, details.BlockRecord.TemplateEntityId);
        }
        else
        {
            // Create new.
            blockController.AddBlock(eventSender, details.BlockRecord);
            recentAdds.Add(details.BlockRecord.Position);
            ticksSinceAdd = 0;
        }
    }

    /// <summary>
    ///     Handles a remove block world edit request.
    /// </summary>
    /// <param name="details">Request details.</param>
    private void HandleRemoveBlock(GridPositionEventDetails details)
    {
        logger.LogDebug("Remove block at {Pos}.", details.GridPosition);
        blockController.RemoveBlockAtPosition(eventSender, details.GridPosition);
    }

    /// <summary>
    ///     Handles an add NPC world edit request.
    /// </summary>
    /// <param name="details">Request details.</param>
    private void HandleAddNpc(NpcAddEventDetails details)
    {
        logger.LogDebug("Add NPC with template {TemplateId} at {Position}.", details.NpcTemplateId, details.Position);
        using var builder = entityFactory.GetBuilder();
        builder.Template(details.NpcTemplateId)
            .Positionable(details.Position, Vector3.Zero);
        builder.Build();
    }

    /// <summary>
    ///     Handles a remove NPC world edit request.
    /// </summary>
    /// <param name="details">Request details.</param>
    private void HandleRemoveNpc(NpcRemoveEventDetails details)
    {
        logger.LogDebug("Remove NPC entity {EntityId}.", details.NpcEntityId);
        entityManager.RemoveEntity(details.NpcEntityId);
    }
}