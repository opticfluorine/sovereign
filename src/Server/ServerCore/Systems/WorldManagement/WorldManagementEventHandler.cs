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
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;

namespace Sovereign.ServerCore.Systems.WorldManagement;

/// <summary>
///     Event handler for WorldManagementSystem.
/// </summary>
public sealed class WorldManagementEventHandler
{
    private readonly IWorldSegmentLoadedHandler loadedHandler;
    private readonly IWorldSegmentLoader loader;
    private readonly IWorldSegmentUnloader unloader;

    public WorldManagementEventHandler(IWorldSegmentLoader loader,
        IWorldSegmentUnloader unloader,
        IWorldSegmentLoadedHandler loadedHandler)
    {
        this.loader = loader;
        this.unloader = unloader;
        this.loadedHandler = loadedHandler;
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
            case EventId.Server_WorldManagement_LoadSegment:
            {
                var details = (WorldSegmentEventDetails)ev.EventDetails;
                OnLoadSegment(details.SegmentIndex);
            }
                break;

            case EventId.Server_WorldManagement_UnloadSegment:
            {
                var details = (WorldSegmentEventDetails)ev.EventDetails;
                OnUnloadSegment(details.SegmentIndex);
            }
                break;

            case EventId.Server_WorldManagement_WorldSegmentLoaded:
            {
                var details = (WorldSegmentEventDetails)ev.EventDetails;
                OnWorldSegmentLoaded(details.SegmentIndex);
            }
                break;

            default:
                Logger.ErrorFormat("Unhandled event ID {0}.", ev.EventId);
                break;
        }
    }

    /// <summary>
    ///     Handles a load segment request.
    /// </summary>
    /// <param name="segmentIndex">Segment index.</param>
    private void OnLoadSegment(GridPosition segmentIndex)
    {
        loader.LoadSegment(segmentIndex);
    }

    /// <summary>
    ///     Handles an unload segment request.
    /// </summary>
    /// <param name="segmentIndex">Segment to unload.</param>
    private void OnUnloadSegment(GridPosition segmentIndex)
    {
        unloader.UnloadSegment(segmentIndex);
    }

    /// <summary>
    ///     Handles a segment loaded event.
    /// </summary>
    /// <param name="segmentIndex">Segment index.</param>
    private void OnWorldSegmentLoaded(GridPosition segmentIndex)
    {
        loadedHandler.OnWorldSegmentLoaded(segmentIndex);
    }
}