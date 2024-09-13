﻿/*
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
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Castle.Core.Logging;
using LiteNetLib;
using Sodium;
using Sovereign.ClientCore.Network.Rest;
using Sovereign.ClientCore.Systems.ClientNetwork;
using Sovereign.EngineCore.Events;
using Sovereign.NetworkCore.Network.Infrastructure;
using Sovereign.NetworkCore.Network.Pipeline.Outbound;
using Sovereign.NetworkCore.Network.Rest.Data;

namespace Sovereign.ClientCore.Network.Infrastructure;

internal enum ClientNetworkCommandType
{
    /// <summary>
    ///     Begin the network connection.
    /// </summary>
    BeginConnection,

    /// <summary>
    ///     End the network connection.
    /// </summary>
    EndConnection
}

/// <summary>
///     Asynchronous command to the network manager.
/// </summary>
internal sealed class ClientNetworkCommand
{
    /// <summary>
    ///     Command type.
    /// </summary>
    public ClientNetworkCommandType CommandType { get; set; }

    /// <summary>
    ///     Server host. Only used for BeginConnection.
    /// </summary>
    public string Host { get; set; } = "";

    /// <summary>
    ///     Server port. Only used for EndConnection.
    /// </summary>
    public ushort Port { get; set; }
}

/// <summary>
///     Client-side network manager.
/// </summary>
public sealed class ClientNetworkManager : INetworkManager
{
    private readonly AuthenticationClient authClient;
    private readonly ClientNetworkController clientNetworkController;

    /// <summary>
    ///     Command queue.
    /// </summary>
    private readonly ConcurrentQueue<ClientNetworkCommand> commandQueue = new();

    private readonly NetworkConnectionManager connectionManager;
    private readonly IEventSender eventSender;
    private readonly EventBasedNetListener netListener;
    private readonly NetManager netManager;
    private readonly NetworkSerializer networkSerializer;

    /// <summary>
    ///     Outbound event queue from the outbound pipeline.
    /// </summary>
    private readonly ConcurrentQueue<OutboundEventInfo> outboundEventQueue = new();

    private readonly RestClient restClient;
    private readonly TemplateEntityDataClient templateEntityDataClient;

    /// <summary>
    ///     Latest login response.
    /// </summary>
    private LoginResponse? loginResponse;

    public ClientNetworkManager(NetworkConnectionManager connectionManager,
        NetworkSerializer networkSerializer, RestClient restClient,
        AuthenticationClient authClient, IEventSender eventSender,
        ClientNetworkController clientNetworkController, TemplateEntityDataClient templateEntityDataClient)
    {
        this.connectionManager = connectionManager;
        this.networkSerializer = networkSerializer;
        this.restClient = restClient;
        this.authClient = authClient;
        this.eventSender = eventSender;
        this.clientNetworkController = clientNetworkController;
        this.templateEntityDataClient = templateEntityDataClient;

        netListener = new EventBasedNetListener();
        netManager = new NetManager(netListener);

        netListener.PeerDisconnectedEvent += NetListener_PeerDisconnectedEvent;
        netListener.NetworkReceiveEvent += NetListener_NetworkReceiveEvent;
        netListener.NetworkErrorEvent += NetListener_NetworkErrorEvent;

#if DEBUG
        // For debug builds only, max out the disconnect timeout so that connections survive a breakpoint.
        netManager.DisconnectTimeout = int.MaxValue;
#endif
    }

    /// <summary>
    ///     Active connection.
    /// </summary>
    public NetworkConnection? Connection { get; private set; }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    /// <summary>
    ///     Current connection parameters.
    /// </summary>
    public ClientConnectionParameters? ConnectionParameters { get; private set; }

    /// <summary>
    ///     Client state.
    /// </summary>
    public NetworkClientState ClientState { get; private set; }
        = NetworkClientState.Disconnected;

    /// <summary>
    ///     Last network error message.
    /// </summary>
    public string ErrorMessage { get; private set; } = "";

    public event OnNetworkReceive? OnNetworkReceive;

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

        // Send any events that have been enqueued.
        while (outboundEventQueue.TryDequeue(out var evInfo))
            try
            {
                evInfo.Connection?.SendEvent(evInfo.Event, evInfo.DeliveryMethod, networkSerializer);
            }
            catch (Exception e)
            {
                Logger.Error("Failed to send event to server.", e);
            }

        // Poll the network.
        netManager.PollEvents();
    }

    public void EnqueueEvent(OutboundEventInfo evInfo)
    {
        outboundEventQueue.Enqueue(evInfo);
    }

    public void Disconnect(int connectionId)
    {
        // Not implemented for client.
    }

    /// <summary>
    ///     Asynchronously begins a connection to a server.
    /// </summary>
    /// <param name="connectionParameters">Client connection parameters to use.</param>
    /// <param name="loginParameters">Login parameters to use.</param>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the client is not currently disconnected.
    /// </exception>
    internal void BeginConnection(ClientConnectionParameters connectionParameters, LoginParameters loginParameters)
    {
        if (ClientState != NetworkClientState.Disconnected && ClientState != NetworkClientState.Failed)
            throw new InvalidOperationException("Client is not disconnected.");

        ConnectionParameters = connectionParameters;

        Logger.InfoFormat("Connecting to {0}:{1} [REST: {2}:{3}] as {4}.",
            ConnectionParameters.Host, ConnectionParameters.Port,
            ConnectionParameters.RestHost, ConnectionParameters.RestPort,
            loginParameters.Username);

        // Reconfigure the REST client for the latest connection, then attempt to login.
        ClientState = NetworkClientState.Connecting;
        restClient.SelectServer(ConnectionParameters);
        var task = authClient.LoginAsync(loginParameters).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                // Special failure case.
                Logger.Error("Login failed with unhandled exception; connection stopped.", task.Exception);
                ClientState = NetworkClientState.Failed;
                if (task.Exception != null) ErrorMessage = task.Exception.Message;
                clientNetworkController.LoginFailed(eventSender, "Unhandled exception occurred during login.");
            }
            else
            {
                if (task.Result.HasFirst)
                {
                    // Authentication succeeded, proceed with connection.
                    ContinueConnection(task.Result.First);
                }
                else
                {
                    // Authentication failed.
                    clientNetworkController.LoginFailed(eventSender, "Login failed.");
                    restClient.Disconnect();
                    ClientState = NetworkClientState.Failed;
                    ErrorMessage = task.Result.Second;
                }
            }
        });
    }

    /// <summary>
    ///     Asynchronously ends a connection to a server.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the client is not currently connecting or connected.
    /// </exception>
    internal void EndConnection()
    {
        if (!(ClientState == NetworkClientState.Connecting ||
              ClientState == NetworkClientState.Connected))
            throw new InvalidOperationException("Client is not connecting or connected.");

        // Stop the REST client.
        restClient.Disconnect();

        // Clear the login information.
        loginResponse = null;

        // Stop the event connection.
        var cmd = new ClientNetworkCommand();
        cmd.CommandType = ClientNetworkCommandType.EndConnection;

        commandQueue.Enqueue(cmd);
    }

    /// <summary>
    ///     Resets the error condition on a failed connection.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the client has not failed.
    /// </exception>
    internal void ResetError()
    {
        if (ClientState != NetworkClientState.Failed) throw new InvalidOperationException("Client has not failed.");

        ClientState = NetworkClientState.Disconnected;
        Connection = null;
    }

    /// <summary>
    ///     Continues connection to server following a successful login.
    /// </summary>
    /// <param name="loginResponse">Successful login response.</param>
    private void ContinueConnection(LoginResponse loginResponse)
    {
        // Store the login details.
        this.loginResponse = loginResponse;

        // Configure the REST API to make authenticated calls going forward.
        if (loginResponse.UserId == null || loginResponse.RestApiKey == null)
        {
            Logger.Error("Received incomplete login response from server; aborting connection.");
            return;
        }

        restClient.SetCredentials(Guid.Parse(loginResponse.UserId), loginResponse.RestApiKey);

        // Start up the connection to the event server.
        if (ConnectionParameters == null)
        {
            Logger.Error("No connection parameters set when continuing connection; aborting connection.");
            return;
        }

        var cmd = new ClientNetworkCommand();
        cmd.CommandType = ClientNetworkCommandType.BeginConnection;
        cmd.Host = ConnectionParameters.Host;
        cmd.Port = ConnectionParameters.Port;

        commandQueue.Enqueue(cmd);

        // Load the initial set of template entities from the REST server.
        // Future updates will arrive via the event server connection.
        templateEntityDataClient.LoadTemplateEntities();
    }

    /// <summary>
    ///     Handles any pending commands.
    /// </summary>
    private void HandleCommands()
    {
        while (commandQueue.TryDequeue(out var cmd))
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

    /// <summary>
    ///     Begins a connection.
    /// </summary>
    /// <param name="host">Server host.</param>
    /// <param name="port">Server port.</param>
    private void HandleBeginCommand(string host, ushort port)
    {
        try
        {
            // Resolve host.
            IPAddress? addr;
            if (!IPAddress.TryParse(host, out addr))
            {
                var resolvedHost = Dns.GetHostEntry(host);
                if (resolvedHost.AddressList.Count() == 0) throw new ArgumentException("Host could not be resolved.");
                addr = resolvedHost.AddressList[0];
            }

            // Grab HMAC key (shared secret) from the login response.
            if (loginResponse == null || loginResponse.SharedSecret == null || loginResponse.UserId == null)
            {
                Logger.Error("Login response missing or incomplete; aborting connection.");
                return;
            }

            var hmacKey = Utilities.HexToBinary(loginResponse.SharedSecret);

            // Connect.
            var peer = netManager.Connect(new IPEndPoint(addr, port), loginResponse.UserId);

            // Record the connection.
            Connection = connectionManager.CreateConnection(peer, hmacKey);
            ClientState = NetworkClientState.Connected;
            clientNetworkController.Connected(eventSender);
        }
        catch (Exception e)
        {
            // Connection failed, store error.
            ErrorMessage = e.Message;
            ClientState = NetworkClientState.Failed;

            Logger.Error("Failed to connect to server.", e);
            clientNetworkController.ConnectionAttemptFailed(eventSender, ErrorMessage);
        }
    }

    /// <summary>
    ///     Ends a connection.
    /// </summary>
    private void HandleEndCommand()
    {
        // If already disconnected, silently fail.
        if (ClientState == NetworkClientState.Disconnected ||
            ClientState == NetworkClientState.Failed)
            return;

        // Disconnect.
        try
        {
            if (Connection != null)
            {
                connectionManager.RemoveConnection(Connection);
                Connection = null;
            }

            Logger.Error("Disconnected.");
            ClientState = NetworkClientState.Disconnected;
            Connection = null;
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
    ///     Called when a network error occurs.
    /// </summary>
    /// <param name="endPoint">Remote endpoint.</param>
    /// <param name="socketError">Error.</param>
    private void NetListener_NetworkErrorEvent(IPEndPoint endPoint, SocketError socketError)
    {
        // Disconnect if the connection survived.
        if (Connection != null)
        {
            connectionManager.RemoveConnection(Connection);
            Connection = null;
        }

        // Record error.
        ErrorMessage = socketError.ToString();
        ClientState = NetworkClientState.Failed;

        Logger.ErrorFormat("Network error: {0}", socketError.ToString());
    }

    /// <summary>
    ///     Called when a packet is received.
    /// </summary>
    /// <param name="peer">Remote peer.</param>
    /// <param name="reader">Data.</param>
    /// <param name="channelNumber">Channel number.</param>
    /// <param name="deliveryMethod">Delivery method.</param>
    private void NetListener_NetworkReceiveEvent(NetPeer peer, NetPacketReader reader, byte channelNumber,
        DeliveryMethod deliveryMethod)
    {
        if (Connection == null)
        {
            Logger.Error("Received packet when client not ready.");
            return;
        }

        try
        {
            var ev = networkSerializer.DeserializeEvent(Connection, reader.GetRemainingBytes());
            OnNetworkReceive?.Invoke(ev, Connection);
        }
        catch (Exception e)
        {
            Logger.Error("Error receiving packet.", e);
        }
    }

    /// <summary>
    ///     Called when the remote peer is disconnected.
    /// </summary>
    /// <param name="peer">Remote peer.</param>
    /// <param name="disconnectInfo">Disconnect info.</param>
    private void NetListener_PeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        // Notify any systems that care that the connection has been lost.
        // This will route back to here and gracefully clean up the connection.
        Logger.InfoFormat("Connection lost: {0}", disconnectInfo.Reason);
        if (disconnectInfo.Reason != DisconnectReason.DisconnectPeerCalled)
            clientNetworkController.DeclareConnectionLost(eventSender);
    }
}