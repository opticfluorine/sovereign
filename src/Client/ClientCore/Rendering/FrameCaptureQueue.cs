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

namespace Sovereign.ClientCore.Rendering;

/// <summary>
///     A completed frame capture request.
/// </summary>
/// <param name="RequestId">Request identifier of the capture request.</param>
/// <param name="Frame">Captured frame, or null if the capture failed.</param>
public readonly record struct FrameCaptureResult(uint RequestId, CapturedFrame? Frame);

/// <summary>
///     Thread-safe request/completion queue for frame captures. Requests are enqueued
///     by the debug interface system and consumed by the render thread; completions
///     travel in the opposite direction.
/// </summary>
public class FrameCaptureQueue
{
    private readonly ConcurrentQueue<uint> requests = new();
    private readonly ConcurrentQueue<FrameCaptureResult> completions = new();

    /// <summary>
    ///     Enqueues a frame capture request.
    /// </summary>
    /// <param name="requestId">Request identifier.</param>
    public void EnqueueRequest(uint requestId)
    {
        requests.Enqueue(requestId);
    }

    /// <summary>
    ///     Dequeues the next frame capture request, if any.
    /// </summary>
    /// <param name="requestId">Dequeued request identifier.</param>
    /// <returns>true if a request was dequeued, false if the queue is empty.</returns>
    public bool TryDequeueRequest(out uint requestId)
    {
        return requests.TryDequeue(out requestId);
    }

    /// <summary>
    ///     Publishes the completion of a frame capture request.
    /// </summary>
    /// <param name="requestId">Request identifier.</param>
    /// <param name="frame">Captured frame, or null if the capture failed.</param>
    public void CompleteCapture(uint requestId, CapturedFrame? frame)
    {
        completions.Enqueue(new FrameCaptureResult(requestId, frame));
    }

    /// <summary>
    ///     Dequeues the next completed frame capture, if any.
    /// </summary>
    /// <param name="result">Dequeued capture result.</param>
    /// <returns>true if a completion was dequeued, false if the queue is empty.</returns>
    public bool TryDequeueCompletion(out FrameCaptureResult result)
    {
        return completions.TryDequeue(out result);
    }
}
