/*
 * Engine8 Dynamic World MMORPG Engine
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

using Castle.Core.Logging;
using Engine8.EngineCore.Timing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine8.EngineCore.Events
{

    /// <summary>
    /// 
    /// </summary>
    public class EventTimedAction : ITimedAction
    {

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        // Engine state advances every 10 ms.
        public ulong Interval { get; } = 10000;

        /// <summary>
        /// Event loop.
        /// </summary>
        private readonly IEventLoop eventLoop;

        public EventTimedAction(IEventLoop eventLoop)
        {
            this.eventLoop = eventLoop;
        }

        public void Invoke(ulong triggerTime)
        {
            Logger.DebugFormat("Begin event tick {0}.", triggerTime / Interval);
            eventLoop.UpdateSystemTime(triggerTime);
        }

    }

}
