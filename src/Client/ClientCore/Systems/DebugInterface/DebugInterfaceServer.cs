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
using System.Net;
using LiteNetLib;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sovereign.ClientCore.Configuration;
using Sovereign.ClientCore.Systems.DebugInterface.Protocol;
using Sovereign.EngineCore.Configuration;
using Sovereign.EngineCore.Network;

namespace Sovereign.ClientCore.Systems.DebugInterface;

/// <summary>
///     MessagePack-over-LiteNetLib server for the client debug interface. All
///     NetManager interaction occurs on the debug interface system thread.
/// </summary>
public sealed class DebugInterfaceServer
{
    private readonly ILogger<DebugInterfaceServer> logger;
    private readonly EventBasedNetListener netListener;
    private readonly NetManager netManager;
    private readonly DebugInterfaceOptions options;
    private readonly DebugRequestProcessor requestProcessor;

    private bool running;

    public DebugInterfaceServer(DebugRequestProcessor requestProcessor,
        IOptions<DebugInterfaceOptions> options,
        IOptions<PerformanceOptions> performanceOptions,
        ILogger<DebugInterfaceServer> logger)
    {
        this.requestProcessor = requestProcessor;
        this.options = options.Value;
        this.logger = logger;

        netListener = new EventBasedNetListener();
        netListener.ConnectionRequestEvent += NetListener_ConnectionRequestEvent;
        netListener.NetworkReceiveEvent += NetListener_NetworkReceiveEvent;

        netManager = new NetManager(netListener)
        {
            UpdateTime = performanceOptions.Value.NetworkUpdateTimeMs
        };
#if DEBUG
        // For debug builds only, max out the disconnect timeout so that the connection
        // survives hitting a breakpoint.
        netManager.DisconnectTimeout = int.MaxValue;
#endif
    }

    /// <summary>
    ///     Local UDP port bound by the server. Only meaningful while the server is running.
    /// </summary>
    public int LocalPort => netManager.LocalPort;

    /// <summary>
    ///     Starts the debug interface server.
    /// </summary>
    public void Start()
    {
        if (running) return;

        /*
         * Bind only the IPv4 socket so that the server listens strictly on the
         * configured host address, which is the loopback address by default.
         */
        var address = IPAddress.Parse(options.Host);
        netManager.IPv6Enabled = false;
        if (!netManager.Start(address, address, options.Port))
        {
            logger.LogError(
                "Failed to bind the debug interface to {Host}:{Port}; debug interface is disabled.",
                options.Host, options.Port);
            return;
        }

        running = true;
        logger.LogInformation("Debug interface listening on {Host}:{Port}.",
            options.Host, netManager.LocalPort);
    }

    /// <summary>
    ///     Sends any queued responses and polls for network events.
    /// </summary>
    public void Poll()
    {
        if (!running) return;

        while (requestProcessor.TryDequeueResponse(out var response))
        {
            if (response.Peer.ConnectionState != ConnectionState.Connected) continue;
            response.Peer.Send(response.Data, DeliveryMethod.ReliableOrdered);
        }

        netManager.PollEvents();
    }

    /// <summary>
    ///     Stops the debug interface server.
    /// </summary>
    public void Stop()
    {
        if (!running) return;

        running = false;
        logger.LogInformation("Stopping debug interface.");
        netManager.Stop();
    }

    /// <summary>
    ///     Called when a connection request is received.
    /// </summary>
    /// <param name="request">Connection request.</param>
    private void NetListener_ConnectionRequestEvent(ConnectionRequest request)
    {
        /*
         * Accept all connection requests; the debug socket is bound to the loopback
         * interface by default, which is the security boundary for the interface.
         */
        request.Accept();
    }

    /// <summary>
    ///     Called when a message is received from a connected debug client.
    /// </summary>
    /// <param name="peer">Remote peer.</param>
    /// <param name="reader">Packet reader.</param>
    /// <param name="channelNumber">Channel number.</param>
    /// <param name="deliveryMethod">Delivery method.</param>
    private void NetListener_NetworkReceiveEvent(NetPeer peer, NetPacketReader reader,
        byte channelNumber, DeliveryMethod deliveryMethod)
    {
        try
        {
            var request = MessageConfig.DeserializeMsgPack<DebugRequest>(
                reader.GetRemainingBytes());
            requestProcessor.HandleRequest(request, peer);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Malformed debug request from {Address}.", peer.Address);
        }
        finally
        {
            reader.Recycle();
        }
    }
}
