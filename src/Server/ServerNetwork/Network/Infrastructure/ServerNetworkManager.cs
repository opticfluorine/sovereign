/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Castle.Core.Logging;
using LiteNetLib;
using Sovereign.NetworkCore.Network;
using Sovereign.NetworkCore.Network.Infrastructure;
using Sovereign.NetworkCore.Network.Pipeline.Outbound;
using Sovereign.ServerNetwork.Configuration;
using Sovereign.ServerNetwork.Network.Connections;
using Sovereign.ServerNetwork.Network.Rest;

namespace Sovereign.ServerNetwork.Network.Infrastructure;

/// <summary>
///     Server-side network manager.
/// </summary>
public sealed class ServerNetworkManager : INetworkManager
{
    private readonly IServerNetworkConfiguration config;
    private readonly NetworkConnectionManager connectionManager;

    /// <summary>
    ///     Backing LiteNetLib NetListener.
    /// </summary>
    private readonly EventBasedNetListener netListener;

    /// <summary>
    ///     Backing LiteNetLib NetManager.
    /// </summary>
    private readonly NetManager netManager;

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
        NewConnectionProcessor newConnectionProcessor)
    {
        /* Dependency injection. */
        this.config = config;
        this.connectionManager = connectionManager;
        this.serializer = serializer;
        this.restServer = restServer;
        this.newConnectionProcessor = newConnectionProcessor;

        /* Connect the network event plumbing. */
        netListener = new EventBasedNetListener();
        netListener.NetworkErrorEvent += NetListener_NetworkErrorEvent;
        netListener.ConnectionRequestEvent += NetListener_ConnectionRequestEvent;
        netListener.NetworkReceiveEvent += NetListener_NetworkReceiveEvent;
        netListener.PeerDisconnectedEvent += NetListener_PeerDisconnectedEvent;

        /* Create the network manager, but defer startup. */
        netManager = new NetManager(netListener);
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    public event OnNetworkReceive OnNetworkReceive;

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
        netManager.PollEvents();
    }

    public void EnqueueEvent(OutboundEventInfo evInfo)
    {
        outboundEventQueue.Enqueue(evInfo);
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

            OnNetworkReceive(ev, conn);
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