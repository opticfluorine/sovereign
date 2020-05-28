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

using Sovereign.EngineCore.Components.Indexers;
using Sovereign.WorldManagement.Configuration;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Sovereign.WorldManagement.WorldSegments
{

    /// <summary>
    /// Resolves world segments to position ranges.
    /// </summary>
    public sealed class WorldSegmentResolver
    {
        private readonly IWorldManagementConfiguration config;
        private readonly Vector3 step;

        public WorldSegmentResolver(IWorldManagementConfiguration config)
        {
            this.config = config;
            step = new Vector3(config.SegmentLength);
        }

        /// <summary>
        /// Gets the position range associated with the given world segment.
        /// </summary>
        /// <param name="segmentIndex">World segment index.</param>
        /// <returns></returns>
        public Tuple<Vector3, Vector3> GetRangeForWorldSegment(GridPosition segmentIndex)
        {
            var min = new Vector3(segmentIndex.X, segmentIndex.Y, segmentIndex.Z) *
                config.SegmentLength;
            var max = min + step;
            return new Tuple<Vector3, Vector3>(min, max);
        }

        /// <summary>
        /// Gets the world segment index to which the given position belongs.
        /// </summary>
        /// <param name="position">Position.</param>
        /// <returns>World segment index.</returns>
        public GridPosition GetWorldSegmentForPosition(Vector3 position)
        {
            return new GridPosition()
            {
                X = (int)position.X / (int)config.SegmentLength,
                Y = (int)position.Y / (int)config.SegmentLength,
                Z = (int)position.Z / (int)config.SegmentLength
            };
        }

    }

}
