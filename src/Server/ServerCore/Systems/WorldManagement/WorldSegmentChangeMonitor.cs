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

using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems.WorldManagement.Components.Indexers;

namespace Sovereign.ServerCore.Systems.WorldManagement;

/// <summary>
///     Monitors world segment changes for entities and publishes corresponding events.
/// </summary>
/// <remarks>
///     This class is not responsible for monitoring initial and final world segments in the case of
///     newly added and removed/unloaded entities. Other events should be used to cover the synchronization
///     of these behaviors.
/// </remarks>
public class WorldSegmentChangeMonitor
{
    private readonly IEventSender eventSender;
    private readonly WorldManagementInternalController internalController;

    public WorldSegmentChangeMonitor(WorldSegmentIndexer indexer, WorldManagementInternalController internalController,
        IEventSender eventSender)
    {
        this.internalController = internalController;
        this.eventSender = eventSender;
        indexer.OnChangeWorldSegment += OnWorldSegmentChange;
    }

    /// <summary>
    ///     Called when an entity changes world segments.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="previousSegmentIndex">Previous world segment index.</param>
    /// <param name="newSegmentIndex">New world segment index.</param>
    public void OnWorldSegmentChange(ulong entityId, GridPosition previousSegmentIndex, GridPosition newSegmentIndex)
    {
        internalController.PushChangeSegment(eventSender, entityId, previousSegmentIndex, newSegmentIndex);
    }
}