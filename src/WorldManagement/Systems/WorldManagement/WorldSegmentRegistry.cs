﻿/*
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
using System;
using System.Collections.Generic;
using System.Text;

namespace Sovereign.WorldManagement.Systems.WorldManagement
{

    /// <summary>
    /// Responsible for tracking which world segments are currently loaded.
    /// </summary>
    public sealed class WorldSegmentRegistry
    {
        /// <summary>
        /// Set of currently loaded world segment indices.
        /// </summary>
        private readonly ISet<GridPosition> loadedSegments = new HashSet<GridPosition>();

        public bool IsLoaded(GridPosition segmentIndex)
        {
            return loadedSegments.Contains(segmentIndex);
        }

        public void OnSegmentLoaded(GridPosition segmentIndex)
        {
            loadedSegments.Add(segmentIndex);
        }

        public void OnSegmentUnloaded(GridPosition segmentIndex)
        {
            loadedSegments.Remove(segmentIndex);
        }

    }

}
