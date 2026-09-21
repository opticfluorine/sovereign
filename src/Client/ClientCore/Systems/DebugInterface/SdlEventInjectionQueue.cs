// Sovereign Engine
// Copyright (c) 2026 opticfluorine
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

using System.Collections.Concurrent;
using SDL2;

namespace Sovereign.ClientCore.Systems.DebugInterface;

/// <summary>
///     Thread-safe queue of SDL events to be injected into the normal SDL event
///     processing path, including GUI event processing.
/// </summary>
public class SdlEventInjectionQueue
{
    private readonly ConcurrentQueue<SDL.SDL_Event> events = new();

    /// <summary>
    ///     Enqueues an SDL event for injection.
    /// </summary>
    /// <param name="sdlEvent">SDL event.</param>
    public void Enqueue(SDL.SDL_Event sdlEvent)
    {
        events.Enqueue(sdlEvent);
    }

    /// <summary>
    ///     Dequeues the next injected SDL event, if any.
    /// </summary>
    /// <param name="sdlEvent">Dequeued SDL event.</param>
    /// <returns>true if an event was dequeued, false if the queue is empty.</returns>
    public bool TryDequeue(out SDL.SDL_Event sdlEvent)
    {
        return events.TryDequeue(out sdlEvent);
    }
}
