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
using System.Collections.Generic;
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
using Sovereign.EngineCore.Systems.Block.Events;
using Sovereign.EngineCore.Systems.WorldManagement;
using Sovereign.EngineCore.World;
using Sovereign.NetworkCore.Network;
using EntityConstants = Sovereign.EngineCore.Entities.EntityConstants;

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
        ClientStateController stateController)
    {
        this.eventSender = eventSender;
        this.worldSegmentResolver = worldSegmentResolver;
        this.blockController = blockController;
        this.restClient = restClient;
        this.networkController = networkController;
        this.stateController = stateController;
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

        /* Basic top-level validation. */
        if (segmentData.DefaultsPerPlane.Length != config.SegmentLength)
            throw new RankException("Bad number of default materials in segment data.");

        /* Submit all the blocks we've added. */
        blockController.AddBlocks(eventSender,
            blocksToAdd => CreateBlocksToAdd(segmentIndex, segmentData, blocksToAdd));
        stateController.WorldSegmentLoaded(eventSender, segmentIndex);
    }

    /// <summary>
    ///     Creates block records from segment data and adds them to the given list.
    /// </summary>
    /// <param name="segmentIndex">Segment index.</param>
    /// <param name="segmentData">Segment data.</param>
    /// <param name="blocksToAdd">Running list of blocks to add.</param>
    /// <exception cref="IndexOutOfRangeException">Thrown if a depth plane has a bad Z offset.</exception>
    private void CreateBlocksToAdd(GridPosition segmentIndex, WorldSegmentBlockData segmentData,
        IList<BlockRecord> blocksToAdd)
    {
        /* Start by processing depth plaens containing non-default blocks. */
        var processed = new bool[config.SegmentLength];
        foreach (var plane in segmentData.DataPlanes)
        {
            // Validation.
            if (plane.OffsetZ >= config.SegmentLength)
                throw new IndexOutOfRangeException("Bad Z-offset in world segment data plane.");

            processed[plane.OffsetZ] = true;
            ProcessMixedPlane(segmentIndex, plane, segmentData.DefaultsPerPlane[plane.OffsetZ], blocksToAdd);
        }

        /* Process the remaining all-default planes. */
        for (var i = 0; i < processed.Length; i++)
            if (!processed[i] && segmentData.DefaultsPerPlane[i].BlockType != BlockDataType.Air)
                ProcessDefaultPlane(segmentIndex, i, segmentData.DefaultsPerPlane[i], blocksToAdd);

        if (blocksToAdd.Count > 0)
            Logger.DebugFormat("Adding {0} blocks for world segment {1}.", blocksToAdd.Count, segmentIndex);
    }

    /// <summary>
    ///     Processes a plane containing only default blocks.
    /// </summary>
    /// <param name="segmentIndex">Segment index.</param>
    /// <param name="offsetZ">Z offset of the plane.</param>
    /// <param name="defaultBlock">Default block type.</param>
    /// <param name="blocksToAdd">List of blocks to add.</param>
    private void ProcessDefaultPlane(GridPosition segmentIndex, int offsetZ, BlockData defaultBlock,
        IList<BlockRecord> blocksToAdd)
    {
        var basePosition = worldSegmentResolver.GetRangeForWorldSegment(segmentIndex).Item1;
        var baseX = (int)basePosition.X;
        var baseY = (int)basePosition.Y;
        var baseZ = (int)basePosition.Z;

        for (var x = 0; x < config.SegmentLength; x++)
        for (var y = 0; y < config.SegmentLength; y++)
        {
            var block = new BlockRecord
            {
                Position = new GridPosition(baseX + x, baseY + y, baseZ + offsetZ),
                TemplateEntityId = defaultBlock.TemplateIdOffset + EntityConstants.FirstTemplateEntityId
            };
            blocksToAdd.Add(block);
        }
    }

    /// <summary>
    ///     Processes a plane containing non-default blocks.
    /// </summary>
    /// <param name="segmentIndex">Segment index.</param>
    /// <param name="plane">Depth plane to process.</param>
    /// <param name="defaultBlock">Default block type.</param>
    /// <param name="blocksToAdd">List of blocks to add.</param>
    private void ProcessMixedPlane(GridPosition segmentIndex, WorldSegmentBlockDataPlane plane,
        BlockData defaultBlock, IList<BlockRecord> blocksToAdd)
    {
        var basePosition = worldSegmentResolver.GetRangeForWorldSegment(segmentIndex).Item1;
        var baseX = (int)basePosition.X;
        var baseY = (int)basePosition.Y;
        var baseZ = (int)basePosition.Z;

        /* Process the non-default blocks. */
        var nonDefaults = new bool[config.SegmentLength, config.SegmentLength];
        foreach (var line in plane.Lines)
        {
            /* Validate. */
            if (line.OffsetY >= config.SegmentLength)
                throw new IndexOutOfRangeException("Bad Y-offset in world segment data line.");

            foreach (var block in line.BlockData)
            {
                /* Validate. */
                if (block.OffsetX >= config.SegmentLength)
                    throw new IndexOutOfRangeException("Bad X-offset in world segment data block.");

                var x = baseX + block.OffsetX;
                var y = baseY + line.OffsetY;
                var z = baseZ + plane.OffsetZ;

                /* Add the block. */
                var blockRecord = new BlockRecord
                {
                    Position = new GridPosition(x, y, z),
                    TemplateEntityId = block.Data.TemplateIdOffset + EntityConstants.FirstTemplateEntityId
                };
                blocksToAdd.Add(blockRecord);

                nonDefaults[block.OffsetX, line.OffsetY] = true;
            }
        }

        /* Fill in the default blocks (unless they are air). */
        if (defaultBlock.BlockType != BlockDataType.Air)
            for (var i = 0; i < config.SegmentLength; ++i)
            for (var j = 0; j < config.SegmentLength; ++j)
                if (!nonDefaults[i, j])
                {
                    var blockRecord = new BlockRecord
                    {
                        Position = new GridPosition(baseX + i, baseY + j, baseZ + plane.OffsetZ),
                        TemplateEntityId = defaultBlock.TemplateIdOffset + EntityConstants.FirstTemplateEntityId
                    };
                    blocksToAdd.Add(blockRecord);
                }
    }
}