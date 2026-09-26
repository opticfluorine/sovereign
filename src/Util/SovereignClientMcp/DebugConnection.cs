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

using System.Collections.Concurrent;
using LiteNetLib;
using Microsoft.Extensions.Logging;
using Sovereign.ClientCore.Systems.DebugInterface.Protocol;
using Sovereign.EngineCore.Network;

namespace SovereignClientMcp;

/// <summary>
///     Manages the connection to the client debug interface, mapping requests to
///     their responses. All NetManager interaction occurs on a dedicated background
///     poll thread.
/// </summary>
public sealed class DebugConnection : IDisposable
{
    /// <summary>Interval between poll thread iterations in milliseconds.</summary>
    private const int PollIntervalMs = 10;

    /// <summary>Maximum time to wait for a connection to be established.</summary>
    private static readonly TimeSpan ConnectTimeout = TimeSpan.FromSeconds(10.0);

    /// <summary>Maximum time to wait for a request response.</summary>
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(30.0);

    private readonly string host;
    private readonly ushort port;
    private readonly EventBasedNetListener netListener;
    private readonly NetManager netManager;
    private readonly ConcurrentQueue<Action> queuedOperations = new();
    private readonly ConcurrentDictionary<uint, TaskCompletionSource<DebugResponse>> pendingRequests = new();
    private readonly ILogger<DebugConnection> logger;
    private readonly Thread pollThread;
    private uint nextRequestId;
    private volatile bool running = true;

    /// <summary>
    ///     The connected peer, or null if not connected. Only accessed on the poll thread.
    /// </summary>
    private NetPeer? peer;

    /// <param name="options">MCP server runtime options.</param>
    /// <param name="logger">Logger.</param>
    public DebugConnection(ClientMcpOptions options, ILogger<DebugConnection> logger)
    {
        host = options.Host;
        port = options.Port;
        this.logger = logger;

        netListener = new EventBasedNetListener();
        netListener.NetworkReceiveEvent += NetListener_OnNetworkReceive;
        netListener.PeerDisconnectedEvent += NetListener_OnPeerDisconnected;

        netManager = new NetManager(netListener);
        if (!netManager.Start())
            throw new DebugClientException("Failed to start the client debug interface network client.");

        pollThread = new Thread(PollLoop)
        {
            IsBackground = true,
            Name = "SovereignClientMcp.DebugConnection"
        };
        pollThread.Start();
    }

    /// <summary>
    ///     Sends a request to the client debug interface and waits for the matching response.
    /// </summary>
    /// <param name="request">Request to send. The request ID is assigned by this method.</param>
    /// <returns>The response to the request.</returns>
    /// <exception cref="DebugClientException">
    ///     If the request could not be sent or no response arrived in time.
    /// </exception>
    public async Task<DebugResponse> SendRequestAsync(DebugRequest request)
    {
        await EnsureConnectedAsync();
        var requestId = ++nextRequestId;
        request.RequestId = requestId;
        var completion = new TaskCompletionSource<DebugResponse>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        pendingRequests[requestId] = completion;

        var bytes = MessageConfig.SerializeMsgPack(request);
        queuedOperations.Enqueue(() => SendOnPollThread(requestId, bytes, completion));

        try
        {
            return await completion.Task.WaitAsync(RequestTimeout);
        }
        catch (TimeoutException)
        {
            throw new DebugClientException(
                $"Timed out waiting for response {requestId} from the client debug interface.");
        }
        finally
        {
            pendingRequests.TryRemove(requestId, out _);
        }
    }

    /// <summary>
    ///     Stops the connection and fails any pending requests.
    /// </summary>
    public void Dispose()
    {
        if (!running) return;
        running = false;
        pollThread.Join((int)TimeSpan.FromSeconds(1.0).TotalMilliseconds);
        FailAllPending("The connection to the client debug interface was closed.");
        netManager.Stop();
    }

    /// <summary>
    ///     Ensures that a connection to the client debug interface is established,
    ///     connecting lazily on first use.
    /// </summary>
    /// <exception cref="DebugClientException">If the connection could not be established.</exception>
    private async Task EnsureConnectedAsync()
    {
        if (peer is { ConnectionState: ConnectionState.Connected }) return;

        var completion = new TaskCompletionSource<NetPeer>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        queuedOperations.Enqueue(() =>
        {
            if (peer is { ConnectionState: ConnectionState.Connected } connectedPeer)
            {
                completion.TrySetResult(connectedPeer);
                return;
            }

            var newPeer = netManager.Connect(host, port, string.Empty);
            if (newPeer == null)
            {
                completion.TrySetException(new DebugClientException(
                    $"Failed to begin connecting to the client debug interface at {host}:{port}."));
                return;
            }

            peer = newPeer;
            completion.TrySetResult(newPeer);
        });

        var connectPeer = await completion.Task;
        var deadline = Environment.TickCount64 + (long)ConnectTimeout.TotalMilliseconds;
        while (connectPeer.ConnectionState != ConnectionState.Connected)
        {
            if (connectPeer.ConnectionState is ConnectionState.Disconnected
                or ConnectionState.ShutdownRequested)
            {
                throw new DebugClientException(
                    $"The client debug interface at {host}:{port} refused the connection.");
            }

            if (Environment.TickCount64 >= deadline)
                throw new DebugClientException(
                    $"Timed out connecting to the client debug interface at {host}:{port}.");

            await Task.Delay(PollIntervalMs);
        }

        logger.LogInformation("Connected to the client debug interface at {Host}:{Port}.",
            host, port);
    }

    /// <summary>
    ///     Sends a serialized request to the connected peer. Only called on the poll thread.
    /// </summary>
    /// <param name="requestId">Request ID.</param>
    /// <param name="bytes">Serialized request.</param>
    /// <param name="completion">Completion source to signal if the send fails.</param>
    private void SendOnPollThread(uint requestId, byte[] bytes,
        TaskCompletionSource<DebugResponse> completion)
    {
        if (peer is not { ConnectionState: ConnectionState.Connected } sendPeer)
        {
            pendingRequests.TryRemove(requestId, out _);
            completion.TrySetException(new DebugClientException(
                "The client debug interface is not connected."));
            return;
        }

        try
        {
            sendPeer.Send(bytes, DeliveryMethod.ReliableOrdered);
        }
        catch (Exception e)
        {
            pendingRequests.TryRemove(requestId, out _);
            completion.TrySetException(new DebugClientException(
                $"Failed to send request {requestId} to the client debug interface: {e.Message}"));
        }
    }

    /// <summary>
    ///     Fails all pending requests with the given message.
    /// </summary>
    /// <param name="message">Error message.</param>
    private void FailAllPending(string message)
    {
        foreach (var requestId in pendingRequests.Keys)
        {
            if (pendingRequests.TryRemove(requestId, out var completion))
                completion.TrySetException(new DebugClientException(message));
        }
    }

    /// <summary>
    ///     Background poll loop. Drains queued operations and polls network events so that
    ///     responses are received while scripts block on pending requests.
    /// </summary>
    private void PollLoop()
    {
        while (running)
        {
            while (queuedOperations.TryDequeue(out var operation))
            {
                try
                {
                    operation();
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Failed to process a queued connection operation.");
                }
            }

            netManager.PollEvents();
            Thread.Sleep(PollIntervalMs);
        }
    }

    /// <summary>
    ///     Called when a response packet is received from the client.
    /// </summary>
    /// <param name="peer">Source peer.</param>
    /// <param name="reader">Packet reader positioned at the response payload.</param>
    /// <param name="channelNumber">Channel number.</param>
    /// <param name="deliveryMethod">Delivery method.</param>
    private void NetListener_OnNetworkReceive(NetPeer peer, NetPacketReader reader,
        byte channelNumber, DeliveryMethod deliveryMethod)
    {
        try
        {
            var response = MessageConfig.DeserializeMsgPack<DebugResponse>(
                reader.GetRemainingBytes());
            if (pendingRequests.TryRemove(response.RequestId, out var completion))
                completion.TrySetResult(response);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Malformed response from the client debug interface.");
        }
        finally
        {
            reader.Recycle();
        }
    }

    /// <summary>
    ///     Called when the peer disconnects; fails all pending requests.
    /// </summary>
    /// <param name="peer">Disconnected peer.</param>
    /// <param name="disconnectInfo">Disconnect details.</param>
    private void NetListener_OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        if (this.peer == peer) this.peer = null;
        FailAllPending("The connection to the client debug interface was lost.");
    }
}
