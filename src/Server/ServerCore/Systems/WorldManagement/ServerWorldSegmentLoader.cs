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

using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Events;
using Sovereign.ServerCore.Systems.Persistence;
using Sovereign.WorldManagement.Systems.WorldManagement;
using Sovereign.WorldManagement.WorldSegments;

namespace Sovereign.ServerCore.Systems.WorldManagement;

/// <summary>
///     Server-side world segment loader.
/// </summary>
public class ServerWorldSegmentLoader : IWorldSegmentLoader
{
    private readonly IEventSender eventSender;
    private readonly PersistenceController persistenceController;
    private readonly WorldSegmentRegistry worldSegmentRegistry;
    private readonly WorldSegmentResolver worldSegmentResolver;

    public ServerWorldSegmentLoader(PersistenceController persistenceController,
        WorldSegmentRegistry worldSegmentRegistry,
        WorldSegmentResolver worldSegmentResolver,
        IEventSender eventSender)
    {
        this.persistenceController = persistenceController;
        this.worldSegmentRegistry = worldSegmentRegistry;
        this.worldSegmentResolver = worldSegmentResolver;
        this.eventSender = eventSender;
    }

    public void LoadSegment(GridPosition segmentIndex)
    {
        /* Skip loading if already loaded. */
        if (worldSegmentRegistry.IsLoaded(segmentIndex)) return;

        /* Load. */
        persistenceController.RetrieveWorldSegment(eventSender, segmentIndex);
        worldSegmentRegistry.OnSegmentLoaded(segmentIndex);
    }
}