// Sovereign Engine
// Copyright (c) 2023 opticfluorine
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
using System.Text;
using System.Threading.Tasks;
using Castle.Core.Logging;
using MessagePack;
using Sovereign.ClientCore.Network.Rest;
using Sovereign.ClientCore.Systems.ClientNetwork;
using Sovereign.ClientCore.Systems.ClientState;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Configuration;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Network;
using Sovereign.EngineCore.Network.Rest;
using Sovereign.EngineCore.Systems.Block;
using Sovereign.EngineCore.Systems.WorldManagement;
using Sovereign.EngineCore.World;
using Sovereign.NetworkCore.Network;

namespace Sovereign.ClientCore.Network.Infrastructure;

/// <summary>
///     Client-side world segment loader.
/// </summary>
public sealed class WorldSegmentDataClient
{
    /// <summary>
    ///     Maximum retry count.
    /// </summary>
    public const int MaxRetries = 5;

    /// <summary>
    ///     Delay between consecutive retries, in milliseconds.
    /// </summary>
    public const int RetryDelayMs = 2000;

    /// <summary>
    ///     Maximum response length in bytes.
    /// </summary>
    private const long MaxResponseLength = 128 * 1024;

    private readonly BlockController blockController;
    private readonly IWorldManagementConfiguration config;
    private readonly IEventSender eventSender;
    private readonly WorldSegmentBlockDataLoader loader;
    private readonly ClientNetworkController networkController;
    private readonly RestClient restClient;
    private readonly ClientStateController stateController;
    private readonly WorldSegmentResolver worldSegmentResolver;

    public WorldSegmentDataClient(IEventSender eventSender,
        WorldSegmentResolver worldSegmentResolver,
        IWorldManagementConfiguration config,
        BlockController blockController,
        RestClient restClient,
        ClientNetworkController networkController,
        ClientStateController stateController,
        WorldSegmentBlockDataLoader loader)
    {
        this.eventSender = eventSender;
        this.worldSegmentResolver = worldSegmentResolver;
        this.blockController = blockController;
        this.restClient = restClient;
        this.networkController = networkController;
        this.stateController = stateController;
        this.loader = loader;
        this.config = config;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    /// <summary>
    ///     Loads a world segment asynchronously from the server.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    public void LoadSegment(GridPosition segmentIndex)
    {
        /* Spin up a background task to asynchronously get the segment from the server. */
        Task.Run(() => RetrieveSegmentViaRest(segmentIndex));
    }

    /// <summary>
    ///     Asynchronously retrieves a world segment from the server via REST API.
    /// </summary>
    /// <param name="segmentIndex">Segment index.</param>
    private async void RetrieveSegmentViaRest(GridPosition segmentIndex)
    {
        var retryCount = 0;
        while (retryCount < MaxRetries)
            try
            {
                /* If this is a retry, delay. */
                if (retryCount > 0) await Task.Delay(RetryDelayMs);
                retryCount++;

                /* Attempt to retreive segment from server. */
                var endpoint = new StringBuilder();
                endpoint.Append(RestEndpoints.WorldSegment)
                    .Append("/")
                    .Append(segmentIndex.X)
                    .Append("/")
                    .Append(segmentIndex.Y)
                    .Append("/")
                    .Append(segmentIndex.Z);
                var response = await restClient.Get(endpoint.ToString());
                var contentLen = response.Content.Headers.ContentLength;
                if (contentLen > MaxResponseLength)
                {
                    Logger.ErrorFormat("Response length {0} too long.", contentLen);
                    continue;
                }

                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        var segmentBytes = await response.Content.ReadAsByteArrayAsync();
                        ProcessSegmentBytes(segmentIndex, segmentBytes);
                        return;

                    case HttpStatusCode.ServiceUnavailable:
                        // Server has not yet loaded the segment - delay and try again.
                        Logger.WarnFormat("Server is not ready to serve segment data for {0}.", segmentIndex);
                        break;

                    case HttpStatusCode.Forbidden:
                        // Server reports the player is not currently subscribed - delay and try again.
                        Logger.WarnFormat("Server denied access to segment data for {0}.", segmentIndex);
                        break;

                    default:
                        // Other unknown error - log, try again.
                        Logger.ErrorFormat("Unexpected response code {0} retrieving segment data for {1}.",
                            response.StatusCode, segmentIndex);
                        break;
                }
            }
            catch (NetworkException)
            {
                // If we get this here, then the connection is already known to be lost; abort.
                Logger.DebugFormat("Aborting segment retrieval for {0} due to disconnect.", segmentIndex);
                return;
            }
            catch (Exception e)
            {
                Logger.ErrorFormat(e, "Exception while retrieving segment data for {0}.", segmentIndex);
            }

        /* If we get here, all the retries failed and we hit the limit. */
        Logger.ErrorFormat("Failed to retrieve segment data for {0} after {1} tries; connection assumed lost.",
            segmentIndex, MaxRetries);
        networkController.DeclareConnectionLost(eventSender);
    }

    /// <summary>
    ///     Loads a segment from its compressed representation.
    /// </summary>
    /// <param name="segmentIndex">Segment index.</param>
    /// <param name="segmentBytes">Bytes from server.</param>
    private void ProcessSegmentBytes(GridPosition segmentIndex, byte[] segmentBytes)
    {
        /* Decompress and unpack data. */
        var segmentData = MessagePackSerializer.Deserialize<WorldSegmentBlockData>(segmentBytes,
            MessageConfig.CompressedUntrustedMessagePackOptions);

        loader.Load(segmentIndex, segmentData);
        stateController.WorldSegmentLoaded(eventSender, segmentIndex);
    }
}