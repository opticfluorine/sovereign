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

using System.Threading;
using System.Threading.Tasks;
using Sovereign.ClientCore.Rendering;
using Xunit;

namespace TestClientCore.Rendering;

/// <summary>
///     Unit tests for <see cref="FrameCaptureQueue"/>.
/// </summary>
public class TestFrameCaptureQueue
{
    [Fact]
    public void RoundTripsRequestAndCompletion()
    {
        var queue = new FrameCaptureQueue();

        Assert.False(queue.TryDequeueRequest(out _));
        Assert.False(queue.TryDequeueCompletion(out _));

        queue.EnqueueRequest(7);
        Assert.True(queue.TryDequeueRequest(out var requestId));
        Assert.Equal(7u, requestId);

        var frame = new CapturedFrame
        {
            Width = 4,
            Height = 3,
            Format = CapturedPixelFormat.Rgba8,
            Pixels = new byte[48]
        };
        queue.CompleteCapture(7, frame);

        Assert.True(queue.TryDequeueCompletion(out var result));
        Assert.Equal(7u, result.RequestId);
        Assert.Same(frame, result.Frame);
    }

    [Fact]
    public void CompleteCapture_AcceptsNullFrame()
    {
        var queue = new FrameCaptureQueue();

        queue.CompleteCapture(9, null);

        Assert.True(queue.TryDequeueCompletion(out var result));
        Assert.Equal(9u, result.RequestId);
        Assert.Null(result.Frame);
    }

    [Fact]
    public async Task RoundTripsAcrossThreads()
    {
        var queue = new FrameCaptureQueue();
        var requestEnqueued = new TaskCompletionSource();
        var completionReady = new TaskCompletionSource();
        FrameCaptureResult result = default;

        // Producer thread: enqueue a request, wait for the completion to be published.
        var producer = Task.Run(() =>
        {
            queue.EnqueueRequest(21);
            requestEnqueued.SetResult();
            Assert.True(completionReady.Task.Wait(10000));
            Assert.True(queue.TryDequeueCompletion(out result));
        });

        // Consumer thread: wait for the request, then complete the capture.
        var consumer = Task.Run(async () =>
        {
            await requestEnqueued.Task.WaitAsync(TimeSpan.FromSeconds(10));
            uint requestId = 0;
            Assert.True(SpinWait.SpinUntil(() => queue.TryDequeueRequest(out requestId),
                TimeSpan.FromSeconds(10)));
            queue.CompleteCapture(requestId,
                new CapturedFrame { Width = 1, Height = 1, Pixels = new byte[4] });
            completionReady.SetResult();
        });

        await Task.WhenAll(producer, consumer);

        Assert.Equal(21u, result.RequestId);
        Assert.NotNull(result.Frame);
        Assert.Equal(1, result.Frame!.Width);
    }
}
