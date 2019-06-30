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

using Castle.Core.Logging;
using LiteNetLib;
using Sovereign.ServerNetwork.Configuration;
using Sovereign.NetworkCore.Network;
using Sovereign.NetworkCore.Network.Infrastructure;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Sovereign.ServerNetwork.Network.Infrastructure
{

    /// <summary>
    /// Server-side network manager.
    /// </summary>
    public sealed class ServerNetworkManager : INetworkManager
    {
        private readonly IServerNetworkConfiguration config;

        /// <summary>
        /// Backing LiteNetLib NetManager.
        /// </summary>
        private readonly NetManager netManager;

        /// <summary>
        /// Backing LiteNetLib NetListener.
        /// </summary>
        private readonly EventBasedNetListener netListener;

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        public event OnConnectionRequest OnConnectionRequest;
        public event OnConnected OnConnected;
        public event OnDisconnected OnDisconnected;
        public event OnNetworkError OnNetworkError;
        public event OnNetworkReceive OnNetworkReceive;
        public event OnNetworkReceiveUnconnected OnNetworkReceiveUnconnected;

        public ServerNetworkManager(IServerNetworkConfiguration config)
        {
            /* Dependency injection. */
            this.config = config;

            /* Connect the network event plumbing. */
            netListener = new EventBasedNetListener();
            netListener.NetworkErrorEvent += NetListener_NetworkErrorEvent;
            netListener.ConnectionRequestEvent += NetListener_ConnectionRequestEvent;
            netListener.NetworkReceiveEvent += NetListener_NetworkReceiveEvent;
            netListener.NetworkReceiveUnconnectedEvent += NetListener_NetworkReceiveUnconnectedEvent;
            netListener.NetworkLatencyUpdateEvent += NetListener_NetworkLatencyUpdateEvent;
            netListener.PeerConnectedEvent += NetListener_PeerConnectedEvent;
            netListener.PeerDisconnectedEvent += NetListener_PeerDisconnectedEvent;

            /* Create the network manager, but defer startup. */
            netManager = new NetManager(netListener);
        }

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
            {
                /* Something went wrong, probably failed to bind. */
                throw new NetworkException("Failed to bind socket.");
            }

            Logger.Info("Server started.");
        }

        public void Dispose()
        {
            /* Clean up the network manager. */
            Logger.Info("Stopping server.");
            netManager.Stop();
        }

        /// <summary>
        /// Callback invoked when a peer has disconnected.
        /// </summary>
        /// <param name="peer">Disconnected peer.</param>
        /// <param name="disconnectInfo">Info.</param>
        private void NetListener_PeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectInfo)
        {
        }

        /// <summary>
        /// Callback invoked when a peer has connected.
        /// </summary>
        /// <param name="peer">Connected peer.</param>
        private void NetListener_PeerConnectedEvent(NetPeer peer)
        {
        }

        /// <summary>
        /// Called when the network latency to a peer is updated.
        /// </summary>
        /// <param name="peer">Remote peer.</param>
        /// <param name="latency">New latency value.</param>
        private void NetListener_NetworkLatencyUpdateEvent(NetPeer peer, int latency)
        {
        }

        /// <summary>
        /// Called when a packet is received from an unconnected remote endpoint. 
        /// </summary>
        /// <param name="remoteEndPoint">Remote endpoint.</param>
        /// <param name="reader">Packet reader.</param>
        /// <param name="messageType">Message type.</param>
        private void NetListener_NetworkReceiveUnconnectedEvent(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
        }

        /// <summary>
        /// Called when a packet is received from a connected peer.
        /// </summary>
        /// <param name="peer">Remote peer.</param>
        /// <param name="reader">Packet reader.</param>
        /// <param name="deliveryMethod">Delivery method.</param>
        private void NetListener_NetworkReceiveEvent(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
        }

        /// <summary>
        /// Called when a connection request is received.
        /// </summary>
        /// <param name="request">Connection request.</param>
        private void NetListener_ConnectionRequestEvent(ConnectionRequest request)
        {
        }

        /// <summary>
        /// Called when a network error occurs.
        /// </summary>
        /// <param name="endPoint">Remote endpoint.</param>
        /// <param name="socketError">Error.</param>
        private void NetListener_NetworkErrorEvent(IPEndPoint endPoint, SocketError socketError)
        {
        }

    }
}
