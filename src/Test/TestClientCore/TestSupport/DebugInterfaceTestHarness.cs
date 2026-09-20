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
using System.Threading;
using LiteNetLib;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Sovereign.ClientCore.Configuration;
using Sovereign.ClientCore.Rendering;
using Sovereign.ClientCore.Systems.DebugInterface;
using Sovereign.ClientCore.Systems.DebugInterface.Protocol;
using Sovereign.ClientCore.Systems.Input;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Main;
using Sovereign.EngineCore.Network;
using Sovereign.EngineCore.Configuration;

namespace TestClientCore.TestSupport;

/// <summary>
///     Test harness that wires up the debug interface with real InputServices and a
///     real loopback <see cref="DebugInterfaceServer"/>, plus an in-process LiteNetLib
///     debug client for exercising the interface end to end.
/// </summary>
public sealed class DebugInterfaceTestHarness : IDisposable
{
    private const int EphemeralPort = 0;

    private readonly NetManager clientManager;

    /// <summary>
    ///     Responses received by the debug client, in arrival order.
    /// </summary>
    public ConcurrentQueue<DebugResponse> ClientResponses { get; } = new();

    /// <summary>
    ///     Client-side peer connected to the debug interface server.
    /// </summary>
    public NetPeer ClientPeer { get; private set; } = null!;

    /// <summary>
    ///     Event sender used by the debug interface.
    /// </summary>
    public TestEventSender EventSender { get; } = new();

    public FrameCaptureQueue FrameCaptureQueue { get; } = new();

    public InputServices InputServices { get; }

    /// <summary>
    ///     Debug interface system under test.
    /// </summary>
    public DebugInterfaceSystem System { get; }

    public SdlEventInjectionQueue SdlEventInjectionQueue { get; } = new();

    public KeyboardState KeyboardState { get; } = new();

    public MouseState MouseState { get; }

    public DebugRequestProcessor Processor { get; }

    /// <summary>
    ///     Creates the harness, starts the debug interface server on an ephemeral
    ///     loopback port, and waits for the debug client to connect.
    /// </summary>
    public DebugInterfaceTestHarness()
    {
        MouseState = new MouseState(new InputInternalController(), EventSender,
            NullLogger<MouseState>.Instance);
        InputServices = new InputServices(MouseState, KeyboardState);
        Processor = new DebugRequestProcessor(InputServices, SdlEventInjectionQueue,
            FrameCaptureQueue, EventSender, new CoreController(),
            NullLogger<DebugRequestProcessor>.Instance);

        var serverOptions = new DebugInterfaceOptions
        {
            Enabled = true,
            Host = "127.0.0.1",
            Port = EphemeralPort
        };
        var server = new DebugInterfaceServer(Processor, Options.Create(serverOptions),
            Options.Create(new PerformanceOptions()),
            NullLogger<DebugInterfaceServer>.Instance);
        System = new DebugInterfaceSystem(new EventCommunicator(), server, Processor,
            FrameCaptureQueue, Options.Create(serverOptions),
            NullLogger<DebugInterfaceSystem>.Instance);

        System.Initialize();

        var clientListener = new EventBasedNetListener();
        clientListener.NetworkReceiveEvent += ClientListener_OnNetworkReceive;
        clientManager = new NetManager(clientListener);
        clientManager.Start();
        ClientPeer = clientManager.Connect("127.0.0.1", server.LocalPort, "")
            ?? throw new InvalidOperationException("Failed to begin connection.");

        PollUntil(() => clientManager.FirstPeer.ConnectionState == ConnectionState.Connected);
    }

    public void Dispose()
    {
        clientManager.Stop();
        System.Cleanup();
    }

    /// <summary>
    ///     Sends a request to the debug interface server.
    /// </summary>
    /// <param name="request">Request to send.</param>
    public void SendRequest(DebugRequest request)
    {
        ClientPeer.Send(MessageConfig.SerializeMsgPack(request),
            DeliveryMethod.ReliableOrdered);
    }

    /// <summary>
    ///     Pumps the debug interface system and the debug client until the given
    ///     condition is met or a timeout expires.
    /// </summary>
    /// <param name="condition">Completion condition.</param>
    /// <exception cref="TimeoutException">Thrown if the condition is not met in time.</exception>
    public void PollUntil(Func<bool> condition)
    {
        const int timeoutMs = 10000;
        var deadline = Environment.TickCount64 + timeoutMs;
        while (Environment.TickCount64 < deadline)
        {
            System.ExecuteOnce();
            clientManager.PollEvents();
            if (condition()) return;
            Thread.Sleep(10);
        }

        throw new TimeoutException("Condition was not met before the timeout expired.");
    }

    /// <summary>
    ///     Dequeues the next response received by the debug client, waiting for its
    ///     arrival if necessary.
    /// </summary>
    /// <returns>The next received response.</returns>
    public DebugResponse ReceiveResponse()
    {
        DebugResponse? response = null;
        PollUntil(() => ClientResponses.TryDequeue(out response));
        return response!;
    }

    private void ClientListener_OnNetworkReceive(NetPeer peer, NetPacketReader reader,
        byte channelNumber, DeliveryMethod deliveryMethod)
    {
        try
        {
            ClientResponses.Enqueue(MessageConfig.DeserializeMsgPack<DebugResponse>(
                reader.GetRemainingBytes()));
        }
        finally
        {
            reader.Recycle();
        }
    }
}
