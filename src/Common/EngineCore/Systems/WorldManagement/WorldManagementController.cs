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
using Sovereign.EngineCore.Events.Details;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sovereign.EngineCore.Systems.WorldManagement
{

    /// <summary>
    /// Event controller for the world management system.
    /// </summary>
    public sealed class WorldManagementController
    {

        /// <summary>
        /// Loads the given world segment if it is not already loaded.
        /// </summary>
        /// <param name="eventSender">Event sender.</param>
        /// <param name="segmentIndex">World segment index.</param>
        public void LoadSegment(IEventSender eventSender, GridPosition segmentIndex)
        {
            var details = new WorldSegmentEventDetails()
            {
                SegmentIndex = segmentIndex
            };
            var ev = new Event(EventId.Core_WorldManagement_LoadSegment, details);
            eventSender.SendEvent(ev);
        }

        /// <summary>
        /// Unloads the given world segment if it is already loaded.
        /// </summary>
        /// <param name="eventSender">Event sender.</param>
        /// <param name="segmentIndex">World segment index.</param>
        public void UnloadSegment(IEventSender eventSender, GridPosition segmentIndex)
        {
            var details = new WorldSegmentEventDetails()
            {
                SegmentIndex = segmentIndex
            };
            var ev = new Event(EventId.Core_WorldManagement_UnloadSegment, details);
            eventSender.SendEvent(ev);
        }

    }

}
