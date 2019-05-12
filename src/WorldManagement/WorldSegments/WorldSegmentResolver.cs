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
