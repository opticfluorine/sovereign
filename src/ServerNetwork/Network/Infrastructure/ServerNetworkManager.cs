/*
 * Sovereign Engine
 * Copyright (c) 2020 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
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
using Sovereign.ServerNetwork.Network.Connections;
using Sovereign.ServerNetwork.Network.Rest;

namespace Sovereign.ServerNetwork.Network.Infrastructure
{
    /// <summary>
    /// Server-side network manager.
    /// </summary>
    public sealed class ServerNetworkManager : INetworkManager
    {
        private readonly IServerNetworkConfiguration config;
        private readonly NetworkConnectionManager connectionManager;
        private readonly NetworkSerializer serializer;
        private readonly RestServer restServer;
        private readonly NewConnectionProcessor newConnectionProcessor;

        /// <summary>
        /// Backing LiteNetLib NetManager.
        /// </summary>
        private readonly NetManager netManager;

        /// <summary>
        /// Backing LiteNetLib NetListener.
        /// </summary>
        private readonly EventBasedNetListener netListener;

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        public event OnNetworkReceive OnNetworkReceive;

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

        /// <summary>
        /// Callback invoked when a peer has disconnected.
        /// </summary>
        /// <param name="peer">Disconnected peer.</param>
        /// <param name="disconnectInfo">Info.</param>
        private void NetListener_PeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            CloseConnection(peer);
        }

        /// <summary>
        /// Called when a packet is received from a connected peer.
        /// </summary>
        /// <param name="peer">Remote peer.</param>
        /// <param name="reader">Packet reader.</param>
        /// <param name="deliveryMethod">Delivery method.</param>
        private void NetListener_NetworkReceiveEvent(NetPeer peer, NetPacketReader reader,
            DeliveryMethod deliveryMethod)
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
                    .Append(peer.EndPoint.ToString())
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
        /// Called when a connection request is received.
        /// </summary>
        /// <param name="request">Connection request.</param>
        private void NetListener_ConnectionRequestEvent(ConnectionRequest request)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append("New connection request from ")
                    .Append(request.RemoteEndPoint.ToString())
                    .Append(".");
                Logger.Info(sb.ToString());

                // First round of validation.
                var shouldAccept = request.Type == ConnectionRequestType.Incoming;
                if (!shouldAccept)
                {
                    request.Reject();
                    return;
                }

                // Hand off to detailed request processing.
                newConnectionProcessor.ProcessConnectionRequest(request);
            }
            catch (Exception e)
            {
                var sb = new StringBuilder();
                sb.Append("Error accepting connection request from ")
                    .Append(request.RemoteEndPoint.ToString())
                    .Append(".");
                Logger.Error(sb.ToString(), e);
            }
        }

        /// <summary>
        /// Called when a network error occurs.
        /// </summary>
        /// <param name="endPoint">Remote endpoint.</param>
        /// <param name="socketError">Error.</param>
        private void NetListener_NetworkErrorEvent(IPEndPoint endPoint, SocketError socketError)
        {
            Logger.ErrorFormat("Network error from {0}: {1}.",
                endPoint.ToString(), socketError.ToString());
        }

        /// <summary>
        /// Closes a connection as gracefully as possible.
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
                    .Append(peer.EndPoint.ToString()).Append(".");
                Logger.Error(sb.ToString(), e);
            }
        }
    }
}