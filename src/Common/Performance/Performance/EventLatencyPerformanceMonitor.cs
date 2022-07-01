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

using Castle.Core.Logging;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Timing;
using Sovereign.EngineUtil.Numerics;

namespace Sovereign.Performance.Performance
{

    /// <summary>
    /// Responsible for monitoring local event loop latency.
    /// </summary>
    public sealed class EventLatencyPerformanceMonitor : IPerformanceMonitor
    {
        private readonly ISystemTimer systemTimer;

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        /// <summary>
        /// Sum of latency since the last report, in microseconds.
        /// </summary>
        private ulong latencySumMicroseconds;

        /// <summary>
        /// Latency event count since the last report.
        /// </summary>
        private uint latencyEventCount;

        public EventLatencyPerformanceMonitor(ISystemTimer systemTimer)
        {
            this.systemTimer = systemTimer;
        }

        public void OnPerformanceEvent(IEventDetails eventDetails)
        {
            var timeDetails = (TimeEventDetails)eventDetails;
            latencySumMicroseconds += systemTimer.GetTime() - timeDetails.SystemTime;
            latencyEventCount++;
        }

        public void ReportPerformance()
        {
            var avgLatencyUs = (double)latencySumMicroseconds / (double)latencyEventCount;
            latencySumMicroseconds = 0;
            latencyEventCount = 0;

            Logger.DebugFormat("Average local event latency: {0:F2} us", avgLatencyUs);
        }
    }

}