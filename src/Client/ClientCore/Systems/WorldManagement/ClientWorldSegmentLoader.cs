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
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Network.Rest;
using Sovereign.EngineCore.Systems.Block;
using Sovereign.EngineCore.Systems.Block.Events;
using Sovereign.EngineCore.Systems.WorldManagement;
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

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        /// <summary>
        /// Maximum retry count.
        /// </summary>
        public const int MaxRetries = 3;

        /// <summary>
        /// Delay between consecutive retries, in milliseconds.
        /// </summary>
        public const int RetryDelayMs = 1000;

        public ClientWorldSegmentLoader(IEventSender eventSender,
            WorldSegmentResolver worldSegmentResolver,
            WorldSegmentRegistry worldSegmentRegistry,
            IWorldManagementConfiguration config,
            BlockController blockController,
            RestClient restClient)
        {
            this.eventSender = eventSender;
            this.worldSegmentResolver = worldSegmentResolver;
            this.worldSegmentRegistry = worldSegmentRegistry;
            this.config = config;
            this.blockController = blockController;
            this.restClient = restClient;
        }

        public void LoadSegment(GridPosition segmentIndex)
        {
            /* Fill the segment with a known block pattern for test. */
            blockController.AddBlocks(eventSender,
                (blocks) => CreateBlocks(blocks, segmentIndex));
            worldSegmentRegistry.OnSegmentLoaded(segmentIndex);

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
                            ProcessSegmentBytes(segmentBytes);
                            worldSegmentRegistry.OnSegmentLoaded(segmentIndex);
                            return;

                        case HttpStatusCode.ServiceUnavailable:
                            // Server has not yet loaded the segment - delay and try again.
                            Logger.WarnFormat("Server is not ready to serve segment data for {1}.", segmentIndex);
                            break;

                        default:
                            // Other unknown error - log, try again.
                            Logger.ErrorFormat("Unexpected response code {1} retrieving segment data for {2}.", response.StatusCode, segmentIndex);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Logger.ErrorFormat(e, "Exception while retrieving segment data for {1}.", segmentIndex);
                }

                retryCount++;
            }

            /* If we get here, all the retries failed and we hit the limit. */
            Logger.ErrorFormat("Failed to retrieve segment data for {1} after {2} tries; connection assumed lost.", segmentIndex, MaxRetries);
            // TODO: Send an event indicating presumed connection failure.
        }

        /// <summary>
        /// Loads a segment from its compressed representation.
        /// </summary>
        /// <param name="segmentBytes">Bytes from server.</param>
        /// <exception cref="NotImplementedException"></exception>
        private void ProcessSegmentBytes(byte[] segmentBytes)
        {
            /* Decompress and unpack data. */
            var options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
            var segmentData = MessagePackSerializer.Deserialize<WorldSegmentBlockData>(segmentBytes, options);

            /* Basic top-level validation. */
            if (segmentData.DefaultMaterialsPerPlane.Length != config.SegmentLength)
            {
                throw new RankException("Bad number of default materials in segment data.");
            }

            /* Start by processing depth plaens containing non-default blocks. */
            var processed = new bool[config.SegmentLength];
            foreach (var plane in segmentData.DataPlanes)
            {
                // Validation.
                if (plane.OffsetZ < 0 || plane.OffsetZ >= config.SegmentLength)
                {
                    throw new IndexOutOfRangeException("Bad Z-offset in world segment data plane.");
                }

                processed[plane.OffsetZ] = true;
                ProcessMixedPlane(plane, segmentData.DefaultMaterialsPerPlane[plane.OffsetZ]);
            }

            /* Process the remaining all-default planes. */
            for (int i = 0; i < processed.Length; i++)
            {
                if (!processed[i])
                {
                    ProcessDefaultPlane(i, segmentData.DefaultMaterialsPerPlane[i]);
                }
            }
        }

        /// <summary>
        /// Processes a plane containing only default blocks.
        /// </summary>
        /// <param name="offsetZ">Z offset of the plane.</param>
        /// <param name="defaultBlock">Default block type.</param>"
        private void ProcessDefaultPlane(int offsetZ, BlockMaterialData defaultBlock)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Processes a plane containing non-default blocks.
        /// </summary>
        /// <param name="plane">Depth plane to process.</param>
        /// <param name="defaultBlock">Default block type.</param>
        private void ProcessMixedPlane(WorldSegmentBlockDataPlane plane, BlockMaterialData defaultBlock)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates the test blocks for a world segment.
        /// </summary>
        /// <param name="blocks">List to hold created blocks.</param>
        /// <param name="segmentIndex">World segment index.</param>
        private void CreateBlocks(IList<BlockRecord> blocks,
            GridPosition segmentIndex)
        {
            var origin = (GridPosition)worldSegmentResolver
                .GetRangeForWorldSegment(segmentIndex).Item1;
            for (int i = 0; i < config.SegmentLength; ++i)
            {
                for (int j = 0; j < config.SegmentLength; ++j)
                {
                    var pos = new GridPosition(
                        origin.X + i,
                        origin.Y + j,
                        origin.Z);
                    blocks.Add(new BlockRecord()
                    {
                        Position = pos,
                        Material = 2 + ((i % 2 + j) % 2),
                        MaterialModifier = 0
                    });
                }
            }
        }

    }
}
