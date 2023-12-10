﻿/*
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
using Sovereign.EngineCore.Events.Details;

namespace Sovereign.EngineCore.Systems.WorldManagement;

/// <summary>
///     Event controller for the world management system.
/// </summary>
public sealed class WorldManagementController
{
    /// <summary>
    ///     Loads the given world segment if it is not already loaded.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="segmentIndex">World segment index.</param>
    public void LoadSegment(IEventSender eventSender, GridPosition segmentIndex)
    {
        var details = new WorldSegmentEventDetails
        {
            SegmentIndex = segmentIndex
        };
        var ev = new Event(EventId.Server_WorldManagement_LoadSegment, details);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Unloads the given world segment if it is already loaded.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="segmentIndex">World segment index.</param>
    public void UnloadSegment(IEventSender eventSender, GridPosition segmentIndex)
    {
        var details = new WorldSegmentEventDetails
        {
            SegmentIndex = segmentIndex
        };
        var ev = new Event(EventId.Server_WorldManagement_UnloadSegment, details);
        eventSender.SendEvent(ev);
    }
}