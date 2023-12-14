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

using System.Collections.Generic;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Events;
using Sovereign.ServerCore.Systems.Persistence;

namespace Sovereign.ServerCore.Systems.WorldManagement;

/// <summary>
///     Manages the activation and deactivation of world segments.
/// </summary>
public class WorldSegmentActivationManager
{
    private readonly IEventSender eventSender;

    private readonly PersistenceController persistenceController;

    /// <summary>
    ///     Reference counts for each currently active world segment.
    /// </summary>
    private readonly Dictionary<GridPosition, int> segmentRefCounts = new();

    public WorldSegmentActivationManager(IEventSender eventSender, PersistenceController persistenceController)
    {
        this.eventSender = eventSender;
        this.persistenceController = persistenceController;
    }

    /// <summary>
    ///     Processes a set of changes to the activation counts of a set of world segments.
    /// </summary>
    /// <param name="changes">Dictionary mapping a segment index to the change in the number of subscribers.</param>
    public void ProcessChanges(Dictionary<GridPosition, int> changes)
    {
        foreach (var segmentIndex in changes.Keys)
            // Is this a new activation?
            if (!segmentRefCounts.ContainsKey(segmentIndex))
            {
                // New activation.
                segmentRefCounts.Add(segmentIndex, changes[segmentIndex]);
                persistenceController.RetrieveWorldSegment(eventSender, segmentIndex);
            }
            else
            {
                // Change in reference count to an existing activation.
                segmentRefCounts[segmentIndex] += changes[segmentIndex];
            }
    }
}