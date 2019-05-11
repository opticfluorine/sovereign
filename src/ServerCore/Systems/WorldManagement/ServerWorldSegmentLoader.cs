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
using Sovereign.EngineCore.Events;
using Sovereign.ServerCore.Systems.Persistence;
using Sovereign.WorldManagement.Systems.WorldManagement;
using Sovereign.WorldManagement.WorldSegments;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sovereign.ServerCore.Systems.WorldManagement
{

    /// <summary>
    /// Server-side world segment loader.
    /// </summary>
    public class ServerWorldSegmentLoader : IWorldSegmentLoader
    {
        private readonly PersistenceController persistenceController;
        private readonly WorldSegmentRegistry worldSegmentRegistry;
        private readonly WorldSegmentResolver worldSegmentResolver;
        private readonly IEventSender eventSender;

        public ServerWorldSegmentLoader(PersistenceController persistenceController,
            WorldSegmentRegistry worldSegmentRegistry,
            WorldSegmentResolver worldSegmentResolver,
            IEventSender eventSender)
        {
            this.persistenceController = persistenceController;
            this.worldSegmentRegistry = worldSegmentRegistry;
            this.worldSegmentResolver = worldSegmentResolver;
            this.eventSender = eventSender;
        }

        public void LoadSegment(GridPosition segmentIndex)
        {
            /* Skip loading if already loaded. */
            if (worldSegmentRegistry.IsLoaded(segmentIndex)) return;

            /* Load. */
            (var minPos, var maxPos) = worldSegmentResolver.GetRangeForWorldSegment(segmentIndex);
            persistenceController.RetrieveEntitiesInRange(eventSender, minPos, maxPos);
            worldSegmentRegistry.OnSegmentLoaded(segmentIndex);
        }

    }
}
