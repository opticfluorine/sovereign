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

using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Castle.Core.Logging;
using LiteNetLib;
using Sovereign.EngineCore.Events;
using Sovereign.NetworkCore.Network;
using Sovereign.NetworkCore.Network.Infrastructure;
using Sovereign.NetworkCore.Network.Pipeline.Outbound;
using Sovereign.ServerNetwork.Configuration;
using Sovereign.ServerNetwork.Network.Connections;
using Sovereign.ServerNetwork.Network.Rest;
using Sovereign.ServerNetwork.Network.ServerNetwork;

namespace Sovereign.ServerNetwork.Network.Infrastructure;

/// <summary>
///     Server-side network manager.
/// </summary>
public sealed class ServerNetworkManager : INetworkManager
{
    private readonly IServerNetworkConfiguration config;
    private readonly NetworkConnectionManager connectionManager;
    private readonly IEventSender eventSender;

    /// <summary>
    ///     Backing LiteNetLib NetListener.
    /// </summary>
    private readonly EventBasedNetListener netListener;

    /// <summary>
    ///     Backing LiteNetLib NetManager.
    /// </summary>
    private readonly NetManager netManager;

    private readonly ServerNetworkController networkController;

    private readonly NewConnectionProcessor newConnectionProcessor;

    /// <summary>
    ///     Queue of outbound events from the outbound pipeline.
    /// </summary>
    private readonly ConcurrentQueue<OutboundEventInfo> outboundEventQueue = new();

    private readonly RestServer restServer;
    private readonly NetworkSerializer serializer;

    public ServerNetworkManager(IServerNetworkConfiguration config,
        NetworkConnectionManager connectionManager,
        NetworkSerializer serializer,
        RestServer restServer,
        NewConnectionProcessor newConnectionProcessor,
        ServerNetworkController networkController,
        IEventSender eventSender)
    {
        /* Dependency injection. */
        this.config = config;
        this.connectionManager = connectionManager;
        this.serializer = serializer;
        this.restServer = restServer;
        this.newConnectionProcessor = newConnectionProcessor;
        this.networkController = networkController;
        this.eventSender = eventSender;

        /* Connect the network event plumbing. */
        netListener = new EventBasedNetListener();
        netListener.NetworkErrorEvent += NetListener_NetworkErrorEvent;
        netListener.ConnectionRequestEvent += NetListener_ConnectionRequestEvent;
        netListener.NetworkReceiveEvent += NetListener_NetworkReceiveEvent;
        netListener.PeerDisconnectedEvent += NetListener_PeerDisconnectedEvent;

        /* Create the network manager, but defer startup. */
        netManager = new NetManager(netListener);

#if DEBUG
        // For debug builds only, max out the disconnect timeout so that the connection survives
        // hitting a breakpoint.
        netManager.DisconnectTimeout = int.MaxValue;
#endif
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    public event OnNetworkReceive? OnNetworkReceive;

    public void Initialize()
    {
        /* Start the network manager in server mode. */
        var sb = new StringBuilder();
        sb.Append("Starting server on ")
            .Append(config.NetworkInterfaceIPv4)
            .Append(" / ")
            .Append(config.NetworkInterfaceIPv6)
            .Append(" port ")
            .Append(config.Port)
            .Append(".");
        Logger.Info(sb.ToString());

        var result = netManager.Start(config.NetworkInterfaceIPv4,
            config.NetworkInterfaceIPv6,
            config.Port);
        if (!result)
            /* Something went wrong, probably failed to bind. */
            throw new NetworkException("Failed to bind socket.");

        Logger.Info("Server started.");

        // Start the REST server.
        restServer.Initialize();
    }

    public void Dispose()
    {
        // Stop the REST server.
        restServer.Dispose();

        /* Clean up the network manager. */
        Logger.Info("Stopping server.");
        netManager.Stop();
    }

    public void Poll()
    {
        // Send whatever is in the outbound event queue.
        while (outboundEventQueue.TryDequeue(out var evInfo))
            try
            {
                evInfo.Connection?.SendEvent(evInfo.Event, evInfo.DeliveryMethod, serializer);
            }
            catch (Exception e)
            {
                Logger.Error("Failed to send event to client.", e);
            }

        // Handle incoming messages, connections, etc.
        netManager.PollEvents();
    }

    public void EnqueueEvent(OutboundEventInfo evInfo)
    {
        outboundEventQueue.Enqueue(evInfo);
    }

    public void Disconnect(int connectionId)
    {
        // TODO Clean up any resources associated to the connection

        connectionManager.RemoveConnection(connectionId);
    }

    /// <summary>
    ///     Callback invoked when a peer has disconnected.
    /// </summary>
    /// <param name="peer">Disconnected peer.</param>
    /// <param name="disconnectInfo">Info.</param>
    private void NetListener_PeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        CloseConnection(peer);
    }

    /// <summary>
    ///     Called when a packet is received from a connected peer.
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
            var conn = connectionManager.GetConnection(peer.Id);
            var ev = serializer.DeserializeEvent(conn, reader.GetRemainingBytes());

            OnNetworkReceive?.Invoke(ev, conn);
        }
        catch (Exception e)
        {
            // Log error.
            var sb = new StringBuilder();
            sb.Append("Error receiving from ")
                .Append(peer.EndPoint)
                .Append(".");
            Logger.Error(sb.ToString(), e);

            // Terminate connection.
            CloseConnection(peer);
        }
        finally
        {
            reader.Recycle();
        }
    }

    /// <summary>
    ///     Called when a connection request is received.
    /// </summary>
    /// <param name="request">Connection request.</param>
    private void NetListener_ConnectionRequestEvent(ConnectionRequest request)
    {
        try
        {
            var sb = new StringBuilder();
            sb.Append("New connection request from ")
                .Append(request.RemoteEndPoint)
                .Append(".");
            Logger.Info(sb.ToString());

            // Hand off to detailed request processing.
            newConnectionProcessor.ProcessConnectionRequest(request);
        }
        catch (Exception e)
        {
            var sb = new StringBuilder();
            sb.Append("Error accepting connection request from ")
                .Append(request.RemoteEndPoint)
                .Append(".");
            Logger.Error(sb.ToString(), e);
        }
    }

    /// <summary>
    ///     Called when a network error occurs.
    /// </summary>
    /// <param name="endPoint">Remote endpoint.</param>
    /// <param name="socketError">Error.</param>
    private void NetListener_NetworkErrorEvent(IPEndPoint endPoint, SocketError socketError)
    {
        Logger.ErrorFormat("Network error from {0}: {1}.",
            endPoint.ToString(), socketError.ToString());
    }

    /// <summary>
    ///     Closes a connection as gracefully as possible.
    /// </summary>
    /// <param name="peer">Connected peer.</param>
    private void CloseConnection(NetPeer peer)
    {
        try
        {
            connectionManager.RemoveConnection(peer.Id);
            networkController.ClientDisconnected(eventSender, peer.Id);
            Logger.InfoFormat("Connection closed from {0}.",
                peer.EndPoint.ToString());
        }
        catch (Exception e)
        {
            var sb = new StringBuilder();
            sb.Append("Error removing closed connection from ")
                .Append(peer.EndPoint).Append(".");
            Logger.Error(sb.ToString(), e);
        }
    }
}