// Sovereign Engine
// Copyright (c) 2025 opticfluorine
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
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Sovereign.EngineCore.Events;

/// <summary>
///     Provides an API for asynchronously logging events.
/// </summary>
public class EventLogger
{
    /// <summary>
    ///     Maximum time to wait before checking.
    /// </summary>
    private const int WaitTimeoutMs = 1000;

    private readonly ConcurrentQueue<Event> eventQueue = new();
    private readonly EventWaitHandle eventWaitHandle = new(false, EventResetMode.AutoReset);
    private readonly WaitHandle[] waitHandles = new WaitHandle[2];

    /// <summary>
    ///     Logs an event.
    /// </summary>
    /// <param name="ev">Event.</param>
    public void LogEvent(Event ev)
    {
        eventQueue.Enqueue(ev);
        eventWaitHandle.Set();
    }

    /// <summary>
    ///     Blocks the calling thread until at least one event is available.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    public void WaitForEvents(CancellationToken cancellationToken)
    {
        waitHandles[0] = eventWaitHandle;
        waitHandles[1] = cancellationToken.WaitHandle;
        WaitHandle.WaitAny(waitHandles);
    }

    /// <summary>
    ///     Takes the next event from the logging queue.
    /// </summary>
    /// <param name="ev">Event.</param>
    /// <returns>true if the queue contained an event, false otherwise.</returns>
    public bool TryTakeEvent([NotNullWhen(true)] out Event? ev)
    {
        return eventQueue.TryDequeue(out ev);
    }
}