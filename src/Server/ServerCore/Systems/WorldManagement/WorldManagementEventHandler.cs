/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using Castle.Core.Logging;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;

namespace Sovereign.ServerCore.Systems.WorldManagement;

/// <summary>
///     Event handler for WorldManagementSystem.
/// </summary>
public sealed class WorldManagementEventHandler
{
    private readonly WorldSegmentActivationManager activationManager;
    private readonly WorldSegmentBlockDataManager blockDataManager;
    private readonly WorldSegmentSubscriptionManager subscriptionManager;
    private readonly WorldSegmentSynchronizationManager syncManager;

    public WorldManagementEventHandler(WorldSegmentActivationManager activationManager,
        WorldSegmentSynchronizationManager syncManager,
        WorldSegmentBlockDataManager blockDataManager,
        WorldSegmentSubscriptionManager subscriptionManager)
    {
        this.activationManager = activationManager;
        this.syncManager = syncManager;
        this.blockDataManager = blockDataManager;
        this.subscriptionManager = subscriptionManager;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    /// <summary>
    ///     Handles an incoming event.
    /// </summary>
    /// <param name="ev">Event.</param>
    public void HandleEvent(Event ev)
    {
        switch (ev.EventId)
        {
            case EventId.Server_WorldManagement_WorldSegmentLoaded:
            {
                if (ev.EventDetails is not WorldSegmentEventDetails details)
                {
                    Logger.Error("Received WorldSegmentLoaded without details.");
                    break;
                }

                activationManager.OnWorldSegmentLoaded(details.SegmentIndex);
                syncManager.OnWorldSegmentLoaded(details.SegmentIndex);
                blockDataManager.AddWorldSegment(details.SegmentIndex);
            }
                break;

            case EventId.Core_WorldManagement_EntityLeaveWorldSegment:
            {
                if (ev.EventDetails is not EntityChangeWorldSegmentEventDetails details)
                {
                    Logger.Error("Received EntityLeaveWorldSegment without details.");
                    break;
                }

                subscriptionManager.OnEntityChangeSegment(details.EntityId);
            }
                break;

            default:
                Logger.ErrorFormat("Unhandled event ID {0}.", ev.EventId);
                break;
        }
    }
}