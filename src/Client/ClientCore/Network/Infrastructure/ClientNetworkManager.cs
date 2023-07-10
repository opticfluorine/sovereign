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
    public string Host { get; set; }

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

    /// <summary>
    ///     Latest login response.
    /// </summary>
    private LoginResponse loginResponse;

    public ClientNetworkManager(NetworkConnectionManager connectionManager,
        NetworkSerializer networkSerializer, RestClient restClient,
        AuthenticationClient authClient, IEventSender eventSender,
        ClientNetworkController clientNetworkController)
    {
        this.connectionManager = connectionManager;
        this.networkSerializer = networkSerializer;
        this.restClient = restClient;
        this.authClient = authClient;
        this.eventSender = eventSender;
        this.clientNetworkController = clientNetworkController;

        netListener = new EventBasedNetListener();
        netManager = new NetManager(netListener);

        netListener.PeerDisconnectedEvent += NetListener_PeerDisconnectedEvent;
        netListener.NetworkReceiveEvent += NetListener_NetworkReceiveEvent;
        netListener.NetworkErrorEvent += NetListener_NetworkErrorEvent;
    }

    /// <summary>
    ///     Active connection.
    /// </summary>
    public NetworkConnection Connection { get; private set; }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    /// <summary>
    ///     Current connection parameters.
    /// </summary>
    public ClientConnectionParameters ConnectionParameters { get; private set; }

    /// <summary>
    ///     Client state.
    /// </summary>
    internal NetworkClientState ClientState { get; private set; }
        = NetworkClientState.Disconnected;

    /// <summary>
    ///     Last network error message.
    /// </summary>
    internal string ErrorMessage { get; private set; }

    public event OnNetworkReceive OnNetworkReceive;

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
                evInfo.Connection.SendEvent(evInfo.Event, evInfo.DeliveryMethod, networkSerializer);
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
        if (ClientState != NetworkClientState.Disconnected)
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
                clientNetworkController.LoginFailed(eventSender, "Unhandled exception occurred during login.");
            }
            else
            {
                if (task.Result.HasValue)
                    // Authentication succeeded, proceed with connection.
                    ContinueConnection(task.Result.Value);
                else
                    // Authentication failed.
                    clientNetworkController.LoginFailed(eventSender, "Login failed.");
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

        // Start up the connection to the event server.
        var cmd = new ClientNetworkCommand();
        cmd.CommandType = ClientNetworkCommandType.BeginConnection;
        cmd.Host = ConnectionParameters.Host;
        cmd.Port = ConnectionParameters.Port;

        commandQueue.Enqueue(cmd);
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
            var resolvedHost = Dns.GetHostEntry(host);
            if (resolvedHost.AddressList.Count() == 0) throw new ArgumentException("Host could not be resolved.");
            var addr = resolvedHost.AddressList[0];

            // Grab HMAC key (shared secret) from the login response.
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
        if (ClientState != NetworkClientState.Connecting ||
            ClientState != NetworkClientState.Connected)
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
            OnNetworkReceive(ev, Connection);
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
        clientNetworkController.DeclareConnectionLost(eventSender);
    }
}