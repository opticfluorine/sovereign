/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
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

using Sovereign.ClientCore.Configuration;
using Sovereign.ClientCore.Rendering.Components.Indexers;
using Sovereign.ClientCore.Rendering.Configuration;
using Sovereign.ClientCore.Systems.Camera;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineUtil.Numerics;
using System.Collections.Generic;
using System.Numerics;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.World
{

    /// <summary>
    /// Retrieves drawable entities for world rendering.
    /// </summary>
    public sealed class WorldEntityRetriever
    {
        private readonly DrawablePositionComponentIndexer drawableIndexer;
        private readonly CameraManager camera;
        private readonly DisplayViewport viewport;
        private readonly IClientConfiguration clientConfiguration;

        /// <summary>
        /// Half of the viewport width as a multiple of the tile width.
        /// </summary>
        private readonly float halfX;

        /// <summary>
        /// Half of the viewport height as a multiple of the tile height.
        /// </summary>
        private readonly float halfY;

        public WorldEntityRetriever(DrawablePositionComponentIndexer drawableIndexer,
            CameraManager camera, DisplayViewport viewport, IClientConfiguration clientConfiguration)
        {
            this.drawableIndexer = drawableIndexer;
            this.camera = camera;
            this.viewport = viewport;
            this.clientConfiguration = clientConfiguration;

            halfX = viewport.WidthInTiles * 0.5f;
            halfY = viewport.HeightInTiles * 0.5f;
        }

        /// <summary>
        /// Retrieves the drawable entities for world rendering.
        /// </summary>
        /// <param name="entityBuffer">Drawable entity buffer.</param>
        /// <param name="timeSinceTick">Time since the last tick, in seconds.</param>
        public void RetrieveEntities(IList<PositionedEntity> entityBuffer, float timeSinceTick)
        {
            DetermineExtents(out var minExtent, out var maxExtent, timeSinceTick);
            using (var indexLock = drawableIndexer.AcquireLock())
            {
                drawableIndexer.GetEntitiesInRange(indexLock, minExtent, maxExtent, entityBuffer);
            }
        }

        /// <summary>
        /// Determines the search extents.
        /// </summary>
        /// <param name="minExtent">Minimum extent.</param>
        /// <param name="maxExtent">Maximum extent.</param>
        /// <param name="timeSinceTick">Time since the last tick, in seconds.</param>
        private void DetermineExtents(out Vector3 minExtent, out Vector3 maxExtent,
            float timeSinceTick)
        {
            /* Interpolate the camera position */
            var centerPos = camera.Position.InterpolateByTime(camera.Velocity, timeSinceTick);

            var minX = centerPos.X - halfX - clientConfiguration.RenderSearchSpacerX;
            var maxX = centerPos.X + halfX + clientConfiguration.RenderSearchSpacerX;

            var minY = centerPos.Y - halfY - clientConfiguration.RenderSearchSpacerY;
            var maxY = centerPos.Y + halfY + clientConfiguration.RenderSearchSpacerY;

            /* z uses same spans as y */
            var minZ = centerPos.Z - halfY - clientConfiguration.RenderSearchSpacerY;
            var maxZ = centerPos.Z + halfY + clientConfiguration.RenderSearchSpacerY;

            minExtent = new Vector3(minX, minY, minZ);
            maxExtent = new Vector3(maxX, maxY, maxZ);
        }

    }

}
