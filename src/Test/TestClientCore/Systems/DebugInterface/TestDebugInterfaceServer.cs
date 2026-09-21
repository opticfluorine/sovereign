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

using System;
using Sovereign.ClientCore.Rendering;
using Sovereign.ClientCore.Systems.DebugInterface.Protocol;
using Sovereign.EngineCore.Events;
using TestClientCore.TestSupport;
using static SDL2.SDL;
using Xunit;
using EventId = Sovereign.EngineCore.Events.EventId;

namespace TestClientCore.Systems.DebugInterface;

/// <summary>
///     End-to-end integration tests for the client debug interface over a real
///     loopback LiteNetLib connection.
/// </summary>
public class TestDebugInterfaceServer : IDisposable
{
    private readonly DebugInterfaceTestHarness harness;

    public TestDebugInterfaceServer()
    {
        harness = new DebugInterfaceTestHarness();
    }

    public void Dispose()
    {
        harness.Dispose();
    }

    /// <summary>
    ///     Waits for the debug interface to request a frame capture.
    /// </summary>
    /// <returns>Request identifier of the capture request.</returns>
    private uint ReceiveCaptureRequest()
    {
        uint requestId = 0;
        harness.PollUntil(() => harness.FrameCaptureQueue.TryDequeueRequest(out requestId));
        return requestId;
    }

    [Fact]
    public void GetInputState_EndToEnd()
    {
        harness.KeyboardState.KeyDown(SDL_Keycode.SDLK_w);
        harness.MouseState.UpdateMousePosition(12, 34);

        harness.SendRequest(new DebugRequest
        {
            Type = DebugRequestType.GetInputState,
            RequestId = 100
        });

        var response = harness.ReceiveResponse();
        Assert.Equal(100u, response.RequestId);
        Assert.Equal(DebugResponseStatus.Ok, response.Status);
        var details = Assert.IsType<InputStateResponseDetails>(response.Details);
        Assert.Contains((int)SDL_Keycode.SDLK_w, details.PressedKeys);
        Assert.Equal(12f, details.MouseX);
        Assert.Equal(34f, details.MouseY);
    }

    [Fact]
    public void SendKeyEvent_EndToEnd()
    {
        harness.SendRequest(new DebugRequest
        {
            Type = DebugRequestType.SendKeyEvent,
            RequestId = 101,
            Details = new KeyEventRequestDetails
            {
                Keycode = (int)SDL_Keycode.SDLK_w,
                IsDown = true
            }
        });

        var response = harness.ReceiveResponse();
        Assert.Equal(101u, response.RequestId);
        Assert.Equal(DebugResponseStatus.Ok, response.Status);
        Assert.IsType<EmptyResponseDetails>(response.Details);

        Assert.True(harness.SdlEventInjectionQueue.TryDequeue(out var sdlEvent));
        Assert.Equal(SDL_EventType.SDL_KEYDOWN, sdlEvent.type);
        Assert.Equal(SDL_Keycode.SDLK_w, sdlEvent.key.keysym.sym);
    }

    [Fact]
    public void Screenshot_EndToEnd_ReturnsFrame()
    {
        harness.SendRequest(new DebugRequest
        {
            Type = DebugRequestType.Screenshot,
            RequestId = 102
        });

        // Simulate the render thread capturing the requested frame.
        var requestId = ReceiveCaptureRequest();
        var frame = new CapturedFrame
        {
            Width = 320,
            Height = 64,
            Format = CapturedPixelFormat.Bgra8,
            Pixels = new byte[320 * 64 * 4]
        };
        new Random(5678).NextBytes(frame.Pixels);
        harness.FrameCaptureQueue.CompleteCapture(requestId, frame);

        var response = harness.ReceiveResponse();
        Assert.Equal(102u, response.RequestId);
        Assert.Equal(DebugResponseStatus.Ok, response.Status);
        var details = Assert.IsType<ScreenshotResponseDetails>(response.Details);
        Assert.Equal(frame.Width, details.Width);
        Assert.Equal(frame.Height, details.Height);
        Assert.Equal(frame.Format, details.Format);
        Assert.Equal(frame.Pixels, details.Pixels);
    }

    [Fact]
    public void Screenshot_EndToEnd_LargeFrameSurvivesFragmentation()
    {
        harness.SendRequest(new DebugRequest
        {
            Type = DebugRequestType.Screenshot,
            RequestId = 103
        });

        var requestId = ReceiveCaptureRequest();
        harness.FrameCaptureQueue.CompleteCapture(requestId, new CapturedFrame
        {
            Width = 1024,
            Height = 512,
            Format = CapturedPixelFormat.Bgra8Srgb,
            Pixels = new byte[1024 * 512 * 4]
        });

        var response = harness.ReceiveResponse();
        Assert.Equal(103u, response.RequestId);
        Assert.Equal(DebugResponseStatus.Ok, response.Status);
        var details = Assert.IsType<ScreenshotResponseDetails>(response.Details);
        Assert.Equal(1024 * 512 * 4, details.Pixels.Length);
    }

    [Fact]
    public void Screenshot_EndToEnd_FailedCaptureReturnsError()
    {
        harness.SendRequest(new DebugRequest
        {
            Type = DebugRequestType.Screenshot,
            RequestId = 104
        });

        var requestId = ReceiveCaptureRequest();
        harness.FrameCaptureQueue.CompleteCapture(requestId, null);

        var response = harness.ReceiveResponse();
        Assert.Equal(104u, response.RequestId);
        Assert.Equal(DebugResponseStatus.Error, response.Status);
        Assert.NotNull(response.ErrorMessage);
    }

    [Fact]
    public void Exit_EndToEnd()
    {
        harness.SendRequest(new DebugRequest
        {
            Type = DebugRequestType.Exit,
            RequestId = 105
        });

        var response = harness.ReceiveResponse();
        Assert.Equal(105u, response.RequestId);
        Assert.Equal(DebugResponseStatus.Ok, response.Status);

        harness.PollUntil(() => harness.EventSender.SentEvents.Count > 0);
        Assert.Contains(harness.EventSender.SentEvents,
            ev => ev.EventId == EventId.Core_Quit);
    }
}
