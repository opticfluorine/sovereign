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
using System.Collections.Concurrent;
using System.Collections.Generic;
using LiteNetLib;
using Microsoft.Extensions.Logging;
using SDL2;
using Sovereign.ClientCore.Rendering;
using Sovereign.ClientCore.Systems.DebugInterface.Protocol;
using Sovereign.ClientCore.Systems.Input;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Main;
using Sovereign.EngineCore.Network;

namespace Sovereign.ClientCore.Systems.DebugInterface;

/// <summary>
///     An outbound response waiting to be sent to a debug client.
/// </summary>
/// <param name="Peer">Destination peer.</param>
/// <param name="Data">Serialized response.</param>
public readonly record struct DebugOutboundResponse(NetPeer Peer, byte[] Data);

/// <summary>
///     Routes debug requests received by the debug interface server to the
///     appropriate engine services.
/// </summary>
public class DebugRequestProcessor
{
    private readonly CoreController coreController;
    private readonly FrameCaptureQueue frameCaptureQueue;
    private readonly InputServices inputServices;
    private readonly IEventSender eventSender;
    private readonly ILogger<DebugRequestProcessor> logger;
    private readonly SdlEventInjectionQueue sdlEventInjectionQueue;

    private readonly ConcurrentQueue<DebugOutboundResponse> outboundResponses = new();

    /// <summary>
    ///     Peers waiting for the completion of each pending screenshot request,
    ///     keyed by request ID. Only accessed on the debug interface system thread.
    /// </summary>
    private readonly Dictionary<uint, NetPeer> pendingScreenshotPeers = new();

    public DebugRequestProcessor(InputServices inputServices,
        SdlEventInjectionQueue sdlEventInjectionQueue, FrameCaptureQueue frameCaptureQueue,
        IEventSender eventSender, CoreController coreController,
        ILogger<DebugRequestProcessor> logger)
    {
        this.inputServices = inputServices;
        this.sdlEventInjectionQueue = sdlEventInjectionQueue;
        this.frameCaptureQueue = frameCaptureQueue;
        this.eventSender = eventSender;
        this.coreController = coreController;
        this.logger = logger;
    }

    /// <summary>
    ///     Handles a debug request received from a debug client.
    /// </summary>
    /// <param name="request">Debug request.</param>
    /// <param name="peer">Peer that sent the request.</param>
    public void HandleRequest(DebugRequest request, NetPeer peer)
    {
        try
        {
            switch (request.Type)
            {
                case DebugRequestType.Screenshot:
                    HandleScreenshotRequest(request, peer);
                    break;

                case DebugRequestType.GetInputState:
                    HandleGetInputStateRequest(request, peer);
                    break;

                case DebugRequestType.SendKeyEvent:
                    HandleSendKeyEventRequest(request, peer);
                    break;

                case DebugRequestType.SendMouseEvent:
                    HandleSendMouseEventRequest(request, peer);
                    break;

                case DebugRequestType.Exit:
                    HandleExitRequest(request, peer);
                    break;

                default:
                    SendErrorResponse(peer, request.RequestId,
                        $"Unknown request type {request.Type}.");
                    break;
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error processing debug request {RequestId}.",
                request.RequestId);
            SendErrorResponse(peer, request.RequestId,
                "Internal error while processing the request.");
        }
    }

    /// <summary>
    ///     Takes the peer waiting on the completion of a screenshot request.
    /// </summary>
    /// <param name="requestId">Request identifier.</param>
    /// <param name="peer">Peer waiting on the request.</param>
    /// <returns>true if a peer is waiting on the request, false otherwise.</returns>
    public bool TryTakePendingPeer(uint requestId, out NetPeer peer)
    {
        NetPeer? pendingPeer;
        var found = pendingScreenshotPeers.Remove(requestId, out pendingPeer);
        peer = pendingPeer!;
        return found;
    }

    /// <summary>
    ///     Enqueues a serialized response to be sent to a debug client.
    /// </summary>
    /// <param name="peer">Destination peer.</param>
    /// <param name="responseBytes">Serialized response.</param>
    public void EnqueueResponse(NetPeer peer, byte[] responseBytes)
    {
        outboundResponses.Enqueue(new DebugOutboundResponse(peer, responseBytes));
    }

    /// <summary>
    ///     Dequeues the next outbound response, if any.
    /// </summary>
    /// <param name="response">Dequeued response.</param>
    /// <returns>true if a response was dequeued, false if the queue is empty.</returns>
    public bool TryDequeueResponse(out DebugOutboundResponse response)
    {
        return outboundResponses.TryDequeue(out response);
    }

    /// <summary>
    ///     Handles a Screenshot request by recording the pending peer and requesting
    ///     a capture on the next rendered frame.
    /// </summary>
    /// <param name="request">Debug request.</param>
    /// <param name="peer">Peer that sent the request.</param>
    private void HandleScreenshotRequest(DebugRequest request, NetPeer peer)
    {
        pendingScreenshotPeers[request.RequestId] = peer;
        frameCaptureQueue.EnqueueRequest(request.RequestId);
    }

    /// <summary>
    ///     Handles a GetInputState request by building the response from InputServices.
    /// </summary>
    /// <param name="request">Debug request.</param>
    /// <param name="peer">Peer that sent the request.</param>
    private void HandleGetInputStateRequest(DebugRequest request, NetPeer peer)
    {
        var mousePosition = inputServices.GetMousePosition();
        var details = new InputStateResponseDetails
        {
            PressedKeys = ToKeycodeList(inputServices.GetPressedKeys()),
            MouseX = mousePosition.X,
            MouseY = mousePosition.Y,
            LeftDown = inputServices.IsMouseButtonDown(MouseButton.Left),
            MiddleDown = inputServices.IsMouseButtonDown(MouseButton.Middle),
            RightDown = inputServices.IsMouseButtonDown(MouseButton.Right),
            TotalScrollAmount = inputServices.GetCumulativeMouseScroll()
        };

        SendResponse(peer, new DebugResponse
        {
            RequestId = request.RequestId,
            Status = DebugResponseStatus.Ok,
            Details = details
        });
    }

    /// <summary>
    ///     Handles a SendKeyEvent request by injecting the corresponding SDL event.
    /// </summary>
    /// <param name="request">Debug request.</param>
    /// <param name="peer">Peer that sent the request.</param>
    private void HandleSendKeyEventRequest(DebugRequest request, NetPeer peer)
    {
        if (request.Details is not KeyEventRequestDetails details)
        {
            SendErrorResponse(peer, request.RequestId, "Missing key event details.");
            return;
        }

        var sdlEvent = new SDL.SDL_Event
        {
            type = details.IsDown
                ? SDL.SDL_EventType.SDL_KEYDOWN
                : SDL.SDL_EventType.SDL_KEYUP
        };
        sdlEvent.key.state = details.IsDown ? SDL.SDL_PRESSED : SDL.SDL_RELEASED;
        sdlEvent.key.keysym.sym = (SDL.SDL_Keycode)details.Keycode;
        sdlEvent.key.keysym.mod = (SDL.SDL_Keymod)details.Modifier;

        sdlEventInjectionQueue.Enqueue(sdlEvent);
        SendAck(peer, request.RequestId);
    }

    /// <summary>
    ///     Handles a SendMouseEvent request by injecting the corresponding SDL event.
    /// </summary>
    /// <param name="request">Debug request.</param>
    /// <param name="peer">Peer that sent the request.</param>
    private void HandleSendMouseEventRequest(DebugRequest request, NetPeer peer)
    {
        if (request.Details is not MouseEventRequestDetails details)
        {
            SendErrorResponse(peer, request.RequestId, "Missing mouse event details.");
            return;
        }

        var sdlEvent = details.EventType switch
        {
            DebugMouseEventType.Motion => BuildMouseMotionEvent(details),
            DebugMouseEventType.Button => BuildMouseButtonEvent(details),
            DebugMouseEventType.Wheel => BuildMouseWheelEvent(details),
            _ => throw new ArgumentException(
                $"Unknown mouse event type {details.EventType}.")
        };

        sdlEventInjectionQueue.Enqueue(sdlEvent);
        SendAck(peer, request.RequestId);
    }

    /// <summary>
    ///     Handles an Exit request by quitting the engine.
    /// </summary>
    /// <param name="request">Debug request.</param>
    /// <param name="peer">Peer that sent the request.</param>
    private void HandleExitRequest(DebugRequest request, NetPeer peer)
    {
        coreController.Quit(eventSender);
        SendAck(peer, request.RequestId);
    }

    /// <summary>
    ///     Builds an SDL mouse motion event.
    /// </summary>
    /// <param name="details">Request details.</param>
    /// <returns>SDL event.</returns>
    private static SDL.SDL_Event BuildMouseMotionEvent(MouseEventRequestDetails details)
    {
        var sdlEvent = new SDL.SDL_Event
        {
            type = SDL.SDL_EventType.SDL_MOUSEMOTION
        };
        sdlEvent.motion.x = (int)details.X;
        sdlEvent.motion.y = (int)details.Y;
        return sdlEvent;
    }

    /// <summary>
    ///     Builds an SDL mouse button event.
    /// </summary>
    /// <param name="details">Request details.</param>
    /// <returns>SDL event.</returns>
    private static SDL.SDL_Event BuildMouseButtonEvent(MouseEventRequestDetails details)
    {
        var sdlEvent = new SDL.SDL_Event
        {
            type = details.IsDown
                ? SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN
                : SDL.SDL_EventType.SDL_MOUSEBUTTONUP
        };
        sdlEvent.button.button = details.Button;
        sdlEvent.button.state = details.IsDown ? SDL.SDL_PRESSED : SDL.SDL_RELEASED;
        sdlEvent.button.clicks = 1;
        return sdlEvent;
    }

    /// <summary>
    ///     Builds an SDL mouse wheel event.
    /// </summary>
    /// <param name="details">Request details.</param>
    /// <returns>SDL event.</returns>
    private static SDL.SDL_Event BuildMouseWheelEvent(MouseEventRequestDetails details)
    {
        var sdlEvent = new SDL.SDL_Event
        {
            type = SDL.SDL_EventType.SDL_MOUSEWHEEL
        };
        sdlEvent.wheel.x = (int)details.Dx;
        sdlEvent.wheel.y = (int)details.Dy;
        sdlEvent.wheel.preciseX = details.Dx;
        sdlEvent.wheel.preciseY = details.Dy;
        return sdlEvent;
    }

    /// <summary>
    ///     Converts a collection of pressed keys to the wire format.
    /// </summary>
    /// <param name="pressedKeys">Pressed keys.</param>
    /// <returns>SDL keycodes of the pressed keys.</returns>
    private static IList<int> ToKeycodeList(
        IEnumerable<SDL.SDL_Keycode> pressedKeys)
    {
        var keycodes = new List<int>();
        foreach (var key in pressedKeys)
        {
            keycodes.Add((int)key);
        }

        return keycodes;
    }

    /// <summary>
    ///     Sends an acknowledgement response.
    /// </summary>
    /// <param name="peer">Destination peer.</param>
    /// <param name="requestId">Request identifier.</param>
    private void SendAck(NetPeer peer, uint requestId)
    {
        SendResponse(peer, new DebugResponse
        {
            RequestId = requestId,
            Status = DebugResponseStatus.Ok,
            Details = new EmptyResponseDetails()
        });
    }

    /// <summary>
    ///     Sends an error response.
    /// </summary>
    /// <param name="peer">Destination peer.</param>
    /// <param name="requestId">Request identifier.</param>
    /// <param name="errorMessage">Error description.</param>
    private void SendErrorResponse(NetPeer peer, uint requestId, string errorMessage)
    {
        SendResponse(peer, new DebugResponse
        {
            RequestId = requestId,
            Status = DebugResponseStatus.Error,
            ErrorMessage = errorMessage
        });
    }

    /// <summary>
    ///     Serializes and enqueues a response for delivery.
    /// </summary>
    /// <param name="peer">Destination peer.</param>
    /// <param name="response">Response to send.</param>
    private void SendResponse(NetPeer peer, DebugResponse response)
    {
        EnqueueResponse(peer, MessageConfig.SerializeMsgPack(response));
    }
}
