/*
 * Sovereign Dynamic World MMORPG Engine
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

using Sovereign.EngineCore.Events;
using System.Collections.Generic;

namespace Sovereign.EngineCore.Systems
{

    /// <summary>
    /// Interface implemented by all Systems in the ECS framework.
    /// </summary>
    public interface ISystem
    {

        /// <summary>
        /// Event communicator for the given service.
        /// </summary>
        EventCommunicator EventCommunicator { get; set; }

        /// <summary>
        /// The set of event IDs that this system is listening for.
        /// </summary>
        ISet<EventId> EventIdsOfInterest { get; }

        /// <summary>
        /// Estimated scale of the workload performed by this system.
        /// Larger values indicate greater expected workloads.
        /// This value is used to balance the system threads.
        /// </summary>
        int WorkloadEstimate { get; }

        /// <summary>
        /// Initializes the system. Called from the system thread.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Cleans up the system. Called from the system thread.
        /// </summary>
        void Cleanup();

        /// <summary>
        /// Executes the system once.
        /// </summary>
        void ExecuteOnce();

    }

}
