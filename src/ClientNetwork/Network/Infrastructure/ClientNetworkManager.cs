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
using Sovereign.ClientCore.Network;
using Sovereign.NetworkCore.Network.Infrastructure;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientNetwork.Network.Infrastructure
{

    internal enum ClientNetworkCommandType
    {
        /// <summary>
        /// Begin the network connection.
        /// </summary>
        BeginConnection,

        /// <summary>
        /// End the network connection.
        /// </summary>
        EndConnection
    }

    /// <summary>
    /// Asynchronous command to the network manager.
    /// </summary>
    internal sealed class ClientNetworkCommand
    {

        /// <summary>
        /// Command type.
        /// </summary>
        public ClientNetworkCommandType CommandType { get; set; }

        /// <summary>
        /// Server host. Only used for BeginConnection.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Server port. Only used for EndConnection.
        /// </summary>
        public ushort Port { get; set; }

    }

    /// <summary>
    /// Client-side network manager.
    /// </summary>
    public sealed class ClientNetworkManager : INetworkManager
    {
        private readonly NetworkConnectionManager connectionManager;
        private readonly NetworkSerializer networkSerializer;
        private readonly EventBasedNetListener netListener;
        private readonly NetManager netManager;

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        /// <summary>
        /// Command queue.
        /// </summary>
        private readonly ConcurrentQueue<ClientNetworkCommand> commandQueue
            = new ConcurrentQueue<ClientNetworkCommand>();

        public event OnNetworkReceive OnNetworkReceive;

        /// <summary>
        /// Active connection.
        /// </summary>
        private NetworkConnection conn;

        /// <summary>
        /// Client state.
        /// </summary>
        internal NetworkClientState ClientState { get; private set; } 
            = NetworkClientState.Disconnected;

        /// <summary>
        /// Last network error message.
        /// </summary>
        internal string ErrorMessage { get; private set; }

        public ClientNetworkManager(NetworkConnectionManager connectionManager,
            NetworkSerializer networkSerializer)
        {
            this.connectionManager = connectionManager;
            this.networkSerializer = networkSerializer;

            netListener = new EventBasedNetListener();
            netManager = new NetManager(netListener);

            netListener.PeerDisconnectedEvent += NetListener_PeerDisconnectedEvent;
            netListener.NetworkReceiveEvent += NetListener_NetworkReceiveEvent;
            netListener.NetworkErrorEvent += NetListener_NetworkErrorEvent;
        }

        public void Initialize()
        {
            netManager.Start();
        }

        public void Dispose()
        {
            netManager.Stop();
        }

        public void Poll()
        {
            // Handle any inbound commands.
            HandleCommands();

            // Poll the network.
            netManager.PollEvents();
        }

        /// <summary>
        /// Asynchronously begins a connection to a server.
        /// </summary>
        /// <param name="host">Server host.</param>
        /// <param name="port">Server port.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the client is not currently disconnected.
        /// </exception>
        internal void BeginConnection(string host, ushort port)
        {
            if (ClientState != NetworkClientState.Disconnected)
            {
                throw new InvalidOperationException("Client is not disconnected.");
            }

            var cmd = new ClientNetworkCommand();
            cmd.CommandType = ClientNetworkCommandType.BeginConnection;
            cmd.Host = host;
            cmd.Port = port;

            commandQueue.Enqueue(cmd);
        }

        /// <summary>
        /// Asynchronously ends a connection to a server.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the client is not currently connecting or connected.
        /// </exception>
        internal void EndConnection()
        {
            if (ClientState != NetworkClientState.Connecting || 
                ClientState != NetworkClientState.Connected)
            {
                throw new InvalidOperationException("Client is not connecting or connected.");
            }

            var cmd = new ClientNetworkCommand();
            cmd.CommandType = ClientNetworkCommandType.EndConnection;

            commandQueue.Enqueue(cmd);
        }

        /// <summary>
        /// Resets the error condition on a failed connection.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the client has not failed.
        /// </exception>
        internal void ResetError()
        {
            if (ClientState != NetworkClientState.Failed)
            {
                throw new InvalidOperationException("Client has not failed.");
            }

            ClientState = NetworkClientState.Disconnected;
        }

        /// <summary>
        /// Handles any pending commands.
        /// </summary>
        private void HandleCommands()
        {
            while (commandQueue.TryDequeue(out var cmd))
            {
                switch (cmd.CommandType)
                {
                    case ClientNetworkCommandType.BeginConnection:
                        HandleBeginCommand(cmd.Host, cmd.Port);
                        break;

                    case ClientNetworkCommandType.EndConnection:
                        HandleEndCommand();
                        break;

                    default:
                        Logger.Error("Unrecognized client network command.");
                        break;
                }
            }
        }

        /// <summary>
        /// Begins a connection.
        /// </summary>
        /// <param name="host">Server host.</param>
        /// <param name="port">Server port.</param>
        private void HandleBeginCommand(string host, ushort port)
        {
            // Sanity check.
            if (ClientState != NetworkClientState.Disconnected)
            {
                Logger.Error("Client is not ready.");
                return;
            }

            // Connect.
            ClientState = NetworkClientState.Connecting;
            try
            {
                // Resolve host.
                var resolvedHost = Dns.GetHostEntry(host);
                if (resolvedHost.AddressList.Count() == 0)
                {
                    throw new ArgumentException("Host could not be resolved.");
                }
                var addr = resolvedHost.AddressList[0];

                // TODO: Authentication.
                var hmacKey = new byte[64];
                Array.Clear(hmacKey, 0, hmacKey.Length);

                // Connect.
                var peer = netManager.Connect(new IPEndPoint(addr, port), "");
                conn = connectionManager.CreateConnection(peer, hmacKey);
                ClientState = NetworkClientState.Connected;
            }
            catch (Exception e)
            {
                // Connection failed, store error.
                ErrorMessage = e.Message;
                ClientState = NetworkClientState.Failed;

                Logger.Error("Failed to connect to server.", e);
            }
        }

        /// <summary>
        /// Ends a connection.
        /// </summary>
        private void HandleEndCommand()
        {
            // Sanity check.
            if (ClientState != NetworkClientState.Connecting ||
                ClientState != NetworkClientState.Connected)
            {
                Logger.Error("Client is not ready.");
                return;
            }

            // Disconnect.
            try
            {
                if (conn != null)
                {
                    connectionManager.RemoveConnection(conn);
                    conn = null;
                }
                Logger.Error("Disconnected.");
                ClientState = NetworkClientState.Disconnected;
            }
            catch (Exception e)
            {
                // Failed.
                ErrorMessage = e.Message;
                ClientState = NetworkClientState.Failed;

                Logger.Error("Failed to end connection.", e);
            }
        }

        /// <summary>
        /// Called when a network error occurs.
        /// </summary>
        /// <param name="endPoint">Remote endpoint.</param>
        /// <param name="socketError">Error.</param>
        private void NetListener_NetworkErrorEvent(IPEndPoint endPoint, SocketError socketError)
        {
            // Disconnect if the connection survived.
            if (conn != null)
            {
                connectionManager.RemoveConnection(conn);
                conn = null;
            }

            // Record error.
            ErrorMessage = socketError.ToString();
            ClientState = NetworkClientState.Failed;

            Logger.ErrorFormat("Network error: {0}", socketError.ToString());
        }

        /// <summary>
        /// Called when a packet is received.
        /// </summary>
        /// <param name="peer">Remote peer.</param>
        /// <param name="reader">Data.</param>
        /// <param name="deliveryMethod">Delivery method.</param>
        private void NetListener_NetworkReceiveEvent(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            if (conn == null)
            {
                Logger.Error("Received packet when client not ready.");
                return;
            }

            try
            {
                var ev = networkSerializer.DeserializeEvent(conn, reader.GetRemainingBytes());
                OnNetworkReceive(ev, conn);
            }
            catch (Exception e)
            {
                Logger.Error("Error receiving packet.", e);
            }
        }

        /// <summary>
        /// Called when the remote peer is disconnected.
        /// </summary>
        /// <param name="peer">Remote peer.</param>
        /// <param name="disconnectInfo">Disconnect info.</param>
        private void NetListener_PeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            if (conn != null)
            {
                Logger.InfoFormat("Disconnected by remote peer. Reason: {0}", disconnectInfo.Reason.ToString());
                connectionManager.RemoveConnection(conn);
                conn = null;
                ClientState = NetworkClientState.Failed;
                ErrorMessage = disconnectInfo.Reason.ToString();
            }
        }

    }

}
