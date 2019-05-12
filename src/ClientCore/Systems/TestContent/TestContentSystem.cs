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

using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems;
using Sovereign.EngineCore.Systems.WorldManagement;
using System;
using System.Collections.Generic;

namespace Sovereign.ClientCore.Systems.TestContent
{

    /// <summary>
    /// System responsible for supplying content for early testing.
    /// </summary>
    /// <remarks>
    /// This will be removed in the future.
    /// </remarks>
    public sealed class TestContentSystem : ISystem, IDisposable
    {
        private readonly IEventLoop eventLoop;

        public EventCommunicator EventCommunicator { get; private set; }

        private readonly IEventSender eventSender;
        private readonly WorldManagementController worldManagementController;

        public ISet<EventId> EventIdsOfInterest => new HashSet<EventId>()
        {
            /* no events */
        };

        public int WorkloadEstimate => 0;

        public TestContentSystem(IEventLoop eventLoop,
            EventCommunicator eventCommunicator,
            IEventSender eventSender,
            WorldManagementController worldManagementController)
        {
            this.eventLoop = eventLoop;
            EventCommunicator = eventCommunicator;
            this.eventSender = eventSender;
            this.worldManagementController = worldManagementController;

            eventLoop.RegisterSystem(this);
        }

        public void Initialize()
        {
            /* Load some test world segments. */
            for (int i = -1; i < 2; ++i)
            {
                for (int j = -1; j < 2; ++j)
                {
                    worldManagementController.LoadSegment(eventSender,
                        new GridPosition(i, j, 0));
                }
            }
        }

        public void Cleanup()
        {

        }

        public void ExecuteOnce()
        {
            /* No action. */
        }

        public void Dispose()
        {
            eventLoop.UnregisterSystem(this);
        }

    }

}
