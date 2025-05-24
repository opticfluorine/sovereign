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

using System.Collections.Generic;
using Sovereign.EngineCore.Events;
using Sovereign.NetworkCore.Network.Service;

namespace Sovereign.NetworkCore.Systems.Network;

/// <summary>
///     Event adapter that injects events received and processed from the network.
/// </summary>
public sealed class NetworkEventAdapter : IEventAdapter
{
    private readonly ReceivedEventQueue eventQueue;
    private readonly Queue<Event> localQueue = new();

    public NetworkEventAdapter(ReceivedEventQueue eventQueue,
        EventAdapterManager adapterManager)
    {
        this.eventQueue = eventQueue;

        adapterManager.RegisterEventAdapter(this);
    }

    public bool PollEvent(out Event? ev)
    {
        return localQueue.TryDequeue(out ev);
    }

    public void PrepareEvents()
    {
        // Rapidly drain the shared queue to minimize locked time.
        while (eventQueue.ReceivedEvents.TryDequeue(out var ev))
        {
            localQueue.Enqueue(ev);
        }
    }
}