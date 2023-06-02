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

using MessagePack;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Network.Rest;
using Sovereign.ServerCore.Systems.WorldManagement;
using System.Threading.Tasks;
using WatsonWebserver;

namespace Sovereign.ServerNetwork.Network.Rest.WorldSegment
{

    /// <summary>
    /// REST service providing batch transfer of world segment block data from 
    /// server to client.
    /// </summary>
    public sealed class WorldSegmentRestService : IRestService
    {
        private readonly WorldSegmentBlockDataManager blockDataManager;

        public string Path => RestEndpoints.WorldSegment + "/{x}/{y}/{z}";

        public RestPathType PathType => RestPathType.Parameter;

        public HttpMethod RequestType => HttpMethod.GET;

        public WorldSegmentRestService(WorldSegmentBlockDataManager blockDataManager)
        {
            this.blockDataManager = blockDataManager;
        }

        public async Task OnRequest(HttpContext ctx)
        {
            // Parse parameters.
            int x, y, z;
            if (int.TryParse(ctx.Request.Url.Parameters["x"], out x)
                && int.TryParse(ctx.Request.Url.Parameters["y"], out y)
                && int.TryParse(ctx.Request.Url.Parameters["z"], out z))
            {
                // Successful parse, try to fulfill the request.
                // At some point we should also check that the segment is in range for the
                // user that requested it, we don't want to be leaking data for the other
                // side of the world.
                var dataTask = blockDataManager.GetWorldSegmentBlockData(new GridPosition(x, y, z));
                if (dataTask != null)
                {
                    // Get the latest version of the block data and encode it for transfer.
                    var blockData = await dataTask;
                    var options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
                    var encodedData = MessagePackSerializer.Serialize(blockData, options);

                    ctx.Response.ContentType = "application/octet-stream";
                    ctx.Response.ContentLength = encodedData.Length;
                    await ctx.Response.Send(encodedData);
                }
                else
                {
                    // Segment is not loaded by server at this time.
                    ctx.Response.StatusCode = 503;
                    await ctx.Response.Send();
                }
            }
            else
            {
                // Invalid coordinates.
                ctx.Response.StatusCode = 404;
                await ctx.Response.Send();
            }
        }
    }

}

