﻿/*
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

using Sovereign.EngineCore.Systems;
using System.Collections.Concurrent;

namespace Sovereign.EngineCore.Events
{

    /// <summary>
    /// Interface for event loops, which are implemented as top-level dispatchers.
    /// </summary>
    public interface IEventLoop
    {

        /// <summary>
        /// Advances the event loop time to the given system time value.
        /// This value should be a multiple of the tick interval.
        /// </summary>
        /// <param name="systemTime">System time of the current tick.</param>
        void UpdateSystemTime(ulong systemTime);

        /// <summary>
        /// Pumps the event loop.
        /// </summary>
        /// <returns>
        /// Number of events processed by this call.
        /// </returns>
        int PumpEventLoop();

        /// <summary>
        /// Whether the event loop has terminated.
        /// </summary>
        bool Terminated { get; }

        /// <summary>
        /// Register an event sender.
        /// </summary>
        /// <param name="eventSender">Event sender to register.</param>
        void RegisterEventSender(IEventSender eventSender);

        /// <summary>
        /// Unregisters an event sender.
        /// </summary>
        /// <param name="eventSender">Event sender to unregister.</param>
        void UnregisterEventSender(IEventSender eventSender);

        /// <summary>
        /// Registers a system.
        /// </summary>
        /// <param name="system">System to register.</param>
        void RegisterSystem(ISystem system);

        /// <summary>
        /// Unregisters a system.
        /// </summary>
        /// <param name="system">System to unregister.</param>
        void UnregisterSystem(ISystem system);

    }

}
