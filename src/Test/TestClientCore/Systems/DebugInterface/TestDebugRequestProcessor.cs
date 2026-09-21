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

using System.Collections.Generic;
using System.Linq;
using Sovereign.ClientCore.Rendering;
using Sovereign.ClientCore.Systems.DebugInterface.Protocol;
using Sovereign.ClientCore.Systems.Input;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Network;
using TestClientCore.TestSupport;
using static SDL2.SDL;
using Xunit;
using EventId = Sovereign.EngineCore.Events.EventId;

namespace TestClientCore.Systems.DebugInterface;

/// <summary>
///     Unit tests for <see cref="DebugRequestProcessor"/>. A real <see cref="NetPeer"/>
///     is used so that pending screenshot peers can be verified.
/// </summary>
public class TestDebugRequestProcessor : IDisposable
{
    private readonly DebugInterfaceTestHarness harness;

    public TestDebugRequestProcessor()
    {
        harness = new DebugInterfaceTestHarness();
    }

    public void Dispose()
    {
        harness.Dispose();
    }

    [Fact]
    public void GetInputState_PopulatesResponseFromInputServices()
    {
        harness.KeyboardState.KeyDown(SDL_Keycode.SDLK_w);
        harness.MouseState.UpdateMousePosition(11, 22);
        harness.MouseState.SetButtonState(MouseButton.Left, true);
        harness.MouseState.SetButtonState(MouseButton.Right, true);
        harness.MouseState.UpdateWheel(1.5f);

        harness.Processor.HandleRequest(new DebugRequest
        {
            Type = DebugRequestType.GetInputState,
            RequestId = 5
        }, harness.ClientPeer);

        var response = DequeueResponse();
        Assert.Equal(5u, response.RequestId);
        Assert.Equal(DebugResponseStatus.Ok, response.Status);
        var details = Assert.IsType<InputStateResponseDetails>(response.Details);
        Assert.Equal(new List<int> { (int)SDL_Keycode.SDLK_w }, details.PressedKeys);
        Assert.Equal(11f, details.MouseX);
        Assert.Equal(22f, details.MouseY);
        Assert.True(details.LeftDown);
        Assert.False(details.MiddleDown);
        Assert.True(details.RightDown);
        Assert.Equal(1.5f, details.TotalScrollAmount);
    }

    [Fact]
    public void SendKeyEvent_EnqueuesSdlEventAndAcks()
    {
        harness.Processor.HandleRequest(new DebugRequest
        {
            Type = DebugRequestType.SendKeyEvent,
            RequestId = 6,
            Details = new KeyEventRequestDetails
            {
                Keycode = (int)SDL_Keycode.SDLK_w,
                Modifier = (ushort)SDL_Keymod.KMOD_LSHIFT,
                IsDown = true
            }
        }, harness.ClientPeer);

        Assert.True(harness.SdlEventInjectionQueue.TryDequeue(out var sdlEvent));
        Assert.Equal(SDL_EventType.SDL_KEYDOWN, sdlEvent.type);
        Assert.Equal(SDL_Keycode.SDLK_w, sdlEvent.key.keysym.sym);
        Assert.Equal(SDL_Keymod.KMOD_LSHIFT, sdlEvent.key.keysym.mod);

        var response = DequeueResponse();
        Assert.Equal(6u, response.RequestId);
        Assert.Equal(DebugResponseStatus.Ok, response.Status);
        Assert.IsType<EmptyResponseDetails>(response.Details);
    }

    [Theory]
    [InlineData(DebugMouseEventType.Motion)]
    [InlineData(DebugMouseEventType.Button)]
    [InlineData(DebugMouseEventType.Wheel)]
    public void SendMouseEvent_EnqueuesSdlEventAndAcks(DebugMouseEventType eventType)
    {
        harness.Processor.HandleRequest(new DebugRequest
        {
            Type = DebugRequestType.SendMouseEvent,
            RequestId = 7,
            Details = new MouseEventRequestDetails
            {
                EventType = eventType,
                X = 33f,
                Y = 44f,
                Button = 2,
                IsDown = true,
                Dx = -1.5f,
                Dy = 2.5f
            }
        }, harness.ClientPeer);

        Assert.True(harness.SdlEventInjectionQueue.TryDequeue(out var sdlEvent));
        switch (eventType)
        {
            case DebugMouseEventType.Motion:
                Assert.Equal(SDL_EventType.SDL_MOUSEMOTION, sdlEvent.type);
                Assert.Equal(33, sdlEvent.motion.x);
                Assert.Equal(44, sdlEvent.motion.y);
                break;

            case DebugMouseEventType.Button:
                Assert.Equal(SDL_EventType.SDL_MOUSEBUTTONDOWN, sdlEvent.type);
                Assert.Equal(2, sdlEvent.button.button);
                Assert.Equal(SDL_PRESSED, sdlEvent.button.state);
                break;

            case DebugMouseEventType.Wheel:
                Assert.Equal(SDL_EventType.SDL_MOUSEWHEEL, sdlEvent.type);
                Assert.Equal(-1.5f, sdlEvent.wheel.preciseX);
                Assert.Equal(2.5f, sdlEvent.wheel.preciseY);
                break;
        }

        var response = DequeueResponse();
        Assert.Equal(7u, response.RequestId);
        Assert.Equal(DebugResponseStatus.Ok, response.Status);
        Assert.IsType<EmptyResponseDetails>(response.Details);
    }

    [Fact]
    public void Exit_SendsCoreQuitAndAcks()
    {
        harness.Processor.HandleRequest(new DebugRequest
        {
            Type = DebugRequestType.Exit,
            RequestId = 8
        }, harness.ClientPeer);

        var ev = Assert.Single(harness.EventSender.SentEvents);
        Assert.Equal(EventId.Core_Quit, ev.EventId);

        var response = DequeueResponse();
        Assert.Equal(8u, response.RequestId);
        Assert.Equal(DebugResponseStatus.Ok, response.Status);
        Assert.IsType<EmptyResponseDetails>(response.Details);
    }

    [Fact]
    public void Screenshot_EnqueuesCaptureRequestAndRecordsPendingPeer()
    {
        harness.Processor.HandleRequest(new DebugRequest
        {
            Type = DebugRequestType.Screenshot,
            RequestId = 77
        }, harness.ClientPeer);

        Assert.True(harness.FrameCaptureQueue.TryDequeueRequest(out var requestId));
        Assert.Equal(77u, requestId);

        Assert.True(harness.Processor.TryTakePendingPeer(77, out var peer));
        Assert.Same(harness.ClientPeer, peer);
        Assert.False(harness.Processor.TryTakePendingPeer(77, out _));
    }

    [Fact]
    public void UnknownRequestType_ReturnsErrorResponse()
    {
        harness.Processor.HandleRequest(new DebugRequest
        {
            Type = (DebugRequestType)99,
            RequestId = 9
        }, harness.ClientPeer);

        var response = DequeueResponse();
        Assert.Equal(9u, response.RequestId);
        Assert.Equal(DebugResponseStatus.Error, response.Status);
        Assert.NotNull(response.ErrorMessage);
        Assert.Null(response.Details);
    }

    /// <summary>
    ///     Dequeues the next response produced by the processor.
    /// </summary>
    /// <returns>Deserialized response.</returns>
    private DebugResponse DequeueResponse()
    {
        Assert.True(harness.Processor.TryDequeueResponse(out var outbound));
        return MessageConfig.DeserializeMsgPack<DebugResponse>(outbound.Data);
    }
}
