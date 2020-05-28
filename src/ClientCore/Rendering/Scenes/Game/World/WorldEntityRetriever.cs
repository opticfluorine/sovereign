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
