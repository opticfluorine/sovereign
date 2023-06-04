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
using MessagePack;
using Sovereign.ClientCore.Network.Rest;
using Sovereign.ClientCore.Systems.ClientNetwork;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Network;
using Sovereign.EngineCore.Network.Rest;
using Sovereign.EngineCore.Systems.Block;
using Sovereign.EngineCore.Systems.Block.Events;
using Sovereign.EngineCore.Systems.WorldManagement;
using Sovereign.EngineCore.World.Materials;
using Sovereign.WorldManagement.Configuration;
using Sovereign.WorldManagement.Systems.WorldManagement;
using Sovereign.WorldManagement.WorldSegments;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Systems.WorldManagement
{

    /// <summary>
    /// Client-side world segment loader.
    /// </summary>
    public sealed class ClientWorldSegmentLoader : IWorldSegmentLoader
    {
        private readonly IEventSender eventSender;
        private readonly WorldSegmentResolver worldSegmentResolver;
        private readonly WorldSegmentRegistry worldSegmentRegistry;
        private readonly IWorldManagementConfiguration config;
        private readonly BlockController blockController;
        private readonly RestClient restClient;
        private readonly ClientNetworkController networkController;

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        /// <summary>
        /// Maximum retry count.
        /// </summary>
        public const int MaxRetries = 5;

        /// <summary>
        /// Delay between consecutive retries, in milliseconds.
        /// </summary>
        public const int RetryDelayMs = 2000;

        public ClientWorldSegmentLoader(IEventSender eventSender,
            WorldSegmentResolver worldSegmentResolver,
            WorldSegmentRegistry worldSegmentRegistry,
            IWorldManagementConfiguration config,
            BlockController blockController,
            RestClient restClient,
            ClientNetworkController networkController)
        {
            this.eventSender = eventSender;
            this.worldSegmentResolver = worldSegmentResolver;
            this.worldSegmentRegistry = worldSegmentRegistry;
            this.config = config;
            this.blockController = blockController;
            this.restClient = restClient;
            this.networkController = networkController;
        }

        public void LoadSegment(GridPosition segmentIndex)
        {
            /* Spin up a background task to asynchronously get the segment from the server. */
            Task.Run(() => RetrieveSegmentViaRest(segmentIndex));
        }

        /// <summary>
        /// Asynchronously retrieves a world segment from the server via REST API.
        /// </summary>
        /// <param name="segmentIndex">Segment index.</param>
        private async void RetrieveSegmentViaRest(GridPosition segmentIndex)
        {
            var retryCount = 0;
            while (retryCount < MaxRetries)
            {
                try
                {
                    /* If this is a retry, delay. */
                    if (retryCount > 0)
                    {
                        await Task.Delay(RetryDelayMs);
                    }

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
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            var segmentBytes = await response.Content.ReadAsByteArrayAsync();
                            ProcessSegmentBytes(segmentIndex, segmentBytes);
                            worldSegmentRegistry.OnSegmentLoaded(segmentIndex);
                            return;

                        case HttpStatusCode.ServiceUnavailable:
                            // Server has not yet loaded the segment - delay and try again.
                            Logger.WarnFormat("Server is not ready to serve segment data for {0}.", segmentIndex);
                            break;

                        default:
                            // Other unknown error - log, try again.
                            Logger.ErrorFormat("Unexpected response code {0} retrieving segment data for {1}.", response.StatusCode, segmentIndex);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Logger.ErrorFormat(e, "Exception while retrieving segment data for {0}.", segmentIndex);
                }

                retryCount++;
            }

            /* If we get here, all the retries failed and we hit the limit. */
            Logger.ErrorFormat("Failed to retrieve segment data for {0} after {1} tries; connection assumed lost.", segmentIndex, MaxRetries);
            networkController.DeclareConnectionLost(eventSender);
        }

        /// <summary>
        /// Loads a segment from its compressed representation.
        /// </summary>
        /// <param name="segmentIndex">Segment index.</param>
        /// <param name="segmentBytes">Bytes from server.</param>
        private void ProcessSegmentBytes(GridPosition segmentIndex, byte[] segmentBytes)
        {
            /* Decompress and unpack data. */
            var segmentData = MessagePackSerializer.Deserialize<WorldSegmentBlockData>(segmentBytes, 
                MessageConfig.CompressedUntrustedMessagePackOptions);

            /* Basic top-level validation. */
            if (segmentData.DefaultMaterialsPerPlane.Length != config.SegmentLength)
            {
                throw new RankException("Bad number of default materials in segment data.");
            }

            /* Submit all the blocks we've added. */
            var controller = new BlockController();
            controller.AddBlocks(eventSender, (blocksToAdd) => CreateBlocksToAdd(segmentIndex, segmentData, blocksToAdd));
        }

        /// <summary>
        /// Creates block records from segment data and adds them to the given list.
        /// </summary>
        /// <param name="segmentIndex">Segment index.</param>
        /// <param name="segmentData">Segment data.</param>
        /// <param name="blocksToAdd">Running list of blocks to add.</param>
        /// <exception cref="IndexOutOfRangeException">Thrown if a depth plane has a bad Z offset.</exception>
        private void CreateBlocksToAdd(GridPosition segmentIndex, WorldSegmentBlockData segmentData, IList<BlockRecord> blocksToAdd)
        {
            /* Start by processing depth plaens containing non-default blocks. */
            var processed = new bool[config.SegmentLength];
            var toAdd = new List<BlockRecord>();
            foreach (var plane in segmentData.DataPlanes)
            {
                // Validation.
                if (plane.OffsetZ < 0 || plane.OffsetZ >= config.SegmentLength)
                {
                    throw new IndexOutOfRangeException("Bad Z-offset in world segment data plane.");
                }

                processed[plane.OffsetZ] = true;
                ProcessMixedPlane(segmentIndex, plane, segmentData.DefaultMaterialsPerPlane[plane.OffsetZ], blocksToAdd);
            }

            /* Process the remaining all-default planes. */
            for (int i = 0; i < processed.Length; i++)
            {
                if (!processed[i] && segmentData.DefaultMaterialsPerPlane[i].MaterialId != Material.Air)
                {
                    ProcessDefaultPlane(segmentIndex, i, segmentData.DefaultMaterialsPerPlane[i], blocksToAdd);
                }
            }
        }

        /// <summary>
        /// Processes a plane containing only default blocks.
        /// </summary>
        /// <param name="segmentIndex">Segment index.</param>
        /// <param name="offsetZ">Z offset of the plane.</param>
        /// <param name="defaultBlock">Default block type.</param>
        /// <param name="blocksToAdd">List of blocks to add.</param>
        private void ProcessDefaultPlane(GridPosition segmentIndex, int offsetZ, BlockMaterialData defaultBlock, IList<BlockRecord> blocksToAdd)
        {
            var basePosition = worldSegmentResolver.GetRangeForWorldSegment(segmentIndex).Item1;
            var baseX = (int)basePosition.X;
            var baseY = (int)basePosition.Y;
            var baseZ = (int)basePosition.Z;

            for (int x = 0; x < config.SegmentLength; x++)
            {
                for (int y = 0; y < config.SegmentLength; y++)
                {
                    var block = new BlockRecord()
                    {
                        Position = new GridPosition(baseX + x, baseY + y, baseZ + offsetZ),
                        Material = defaultBlock.MaterialId,
                        MaterialModifier = defaultBlock.ModifierId
                    };
                    blocksToAdd.Add(block);
                }
            }
        }

        /// <summary>
        /// Processes a plane containing non-default blocks.
        /// </summary>
        /// <param name="segmentIndex">Segment index.</param>
        /// <param name="plane">Depth plane to process.</param>
        /// <param name="defaultBlock">Default block type.</param>
        /// <param name="blocksToAdd">List of blocks to add.</param>
        private void ProcessMixedPlane(GridPosition segmentIndex, WorldSegmentBlockDataPlane plane, BlockMaterialData defaultBlock, IList<BlockRecord> blocksToAdd)
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
                if (line.OffsetY < 0 || line.OffsetY >= config.SegmentLength)
                {
                    throw new IndexOutOfRangeException("Bad Y-offset in world segment data line.");
                }

                foreach (var block in line.BlockData)
                {
                    /* Validate. */
                    if (block.OffsetX < 0 || block.OffsetX >= config.SegmentLength)
                    {
                        throw new IndexOutOfRangeException("Bad X-offset in world segment data block.");
                    }

                    var x = baseX + block.OffsetX;
                    var y = baseY + line.OffsetY;
                    var z = baseZ + plane.OffsetZ;

                    /* Add the block. */
                    var blockRecord = new BlockRecord()
                    {
                        Position = new GridPosition(x, y, z),
                        Material = block.MaterialData.MaterialId,
                        MaterialModifier = block.MaterialData.ModifierId
                    };
                    blocksToAdd.Add(blockRecord);

                    nonDefaults[block.OffsetX, line.OffsetY] = true;
                }
            }

            /* Fill in the default blocks (unless they are air). */
            if (defaultBlock.MaterialId != Material.Air)
            {
                for (var i = 0; i < config.SegmentLength; ++i)
                {
                    for (var j = 0; j < config.SegmentLength; ++j)
                    {
                        if (!nonDefaults[i, j])
                        {
                            var blockRecord = new BlockRecord()
                            {
                                Position = new GridPosition(baseX + i, baseY + j, baseZ + plane.OffsetZ),
                                Material = defaultBlock.MaterialId,
                                MaterialModifier = defaultBlock.ModifierId
                            };
                            blocksToAdd.Add(blockRecord);
                        }
                    }
                }
            }
        }

    }
}
