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

using Sovereign.EngineCore.Events;
using Sovereign.NetworkCore.Network.Service;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sovereign.NetworkCore.Systems.Network
{

    /// <summary>
    /// Event adapter that injects events received and processed from the network.
    /// </summary>
    public sealed class NetworkEventAdapter : IEventAdapter
    {
        private readonly ReceivedEventQueue eventQueue;

        public NetworkEventAdapter(ReceivedEventQueue eventQueue,
            EventAdapterManager adapterManager)
        {
            this.eventQueue = eventQueue;

            adapterManager.RegisterEventAdapter(this);
        }

        public bool PollEvent(out Event ev)
        {
            return eventQueue.ReceivedEvents.TryDequeue(out ev);
        }

        public void PrepareEvents()
        {
            /* no action */
        }

    }

}
