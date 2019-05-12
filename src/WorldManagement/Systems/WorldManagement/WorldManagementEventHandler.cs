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

using System;
using System.Collections.Generic;
using System.Text;
using Castle.Core.Logging;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;

namespace Sovereign.WorldManagement.Systems.WorldManagement
{

    /// <summary>
    /// Event handler for WorldManagementSystem.
    /// </summary>
    public sealed class WorldManagementEventHandler
    {
        private readonly IWorldSegmentLoader loader;
        private readonly IWorldSegmentUnloader unloader;

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        public WorldManagementEventHandler(IWorldSegmentLoader loader, 
            IWorldSegmentUnloader unloader)
        {
            this.loader = loader;
            this.unloader = unloader;
        }

        /// <summary>
        /// Handles an incoming event.
        /// </summary>
        /// <param name="ev">Event.</param>
        public void HandleEvent(Event ev)
        {
            switch (ev.EventId)
            {
                case EventId.Core_WorldManagement_LoadSegment:
                    {
                        var details = (WorldSegmentEventDetails)ev.EventDetails;
                        OnLoadSegment(details.SegmentIndex);
                    }
                    break;

                case EventId.Core_WorldManagement_UnloadSegment:
                    {
                        var details = (WorldSegmentEventDetails)ev.EventDetails;
                        OnUnloadSegment(details.SegmentIndex);
                    }
                    break;

                default:
                    Logger.ErrorFormat("Unhandled event ID {0}.", ev.EventId);
                    break;
            }
        }

        /// <summary>
        /// Handles a load segment request.
        /// </summary>
        /// <param name="segmentIndex">Segment index.</param>
        private void OnLoadSegment(GridPosition segmentIndex)
        {
            loader.LoadSegment(segmentIndex);
        }

        /// <summary>
        /// Handles an unload segment request.
        /// </summary>
        /// <param name="segmentIndex">Segment to unload.</param>
        private void OnUnloadSegment(GridPosition segmentIndex)
        {
            unloader.UnloadSegment(segmentIndex);
        }

    }

}
