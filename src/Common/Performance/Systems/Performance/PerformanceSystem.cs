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

using System.Collections.Generic;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Systems;
using Sovereign.EngineCore.Timing;
using Sovereign.EngineCore.Util;
using Sovereign.Performance.Performance;

namespace Sovereign.Performance.Systems.Performance
{

    /// <summary>
    /// Responsible for monitoring engine performance.
    /// </summary>
    public class PerformanceSystem : ISystem
    {
        private readonly EventLatencyPerformanceMonitor eventLatency;
        private readonly ISystemTimer systemTimer;

        /// <summary>
        /// System time interval between performance reports.
        /// </summary>
        private const ulong REPORT_INTERVAL_US = Units.SystemTime.Minute;

        /// <summary>
        /// Performance monitors.
        /// </summary>
        private readonly IList<IPerformanceMonitor> monitors = new List<IPerformanceMonitor>();

        /// <summary>
        /// System time of the last performance report.
        /// </summary>
        private ulong lastReportTime;

        public int WorkloadEstimate => 0;
        public EventCommunicator EventCommunicator { get; }
        public ISet<EventId> EventIdsOfInterest { get; } = new HashSet<EventId>()
        {
            EventId.Core_Tick,
            EventId.Core_Performance_EventLatencyTest
        };

        public PerformanceSystem(EventCommunicator eventCommunicator,
            EventLatencyPerformanceMonitor eventLatency,
            ISystemTimer systemTimer,
            IEventLoop eventLoop,
            EventDescriptions eventDescriptions)
        {
            this.systemTimer = systemTimer;
            EventCommunicator = eventCommunicator;

            this.eventLatency = eventLatency;
            monitors.Add(eventLatency);

            eventDescriptions.RegisterEvent<TimeEventDetails>(EventId.Core_Performance_EventLatencyTest);

            eventLoop.RegisterSystem(this);
        }

        public void Initialize()
        {
            lastReportTime = systemTimer.GetTime();
        }

        public void Cleanup()
        {
        }

        public int ExecuteOnce()
        {
            var eventsProcessed = 0;
            while (EventCommunicator.GetIncomingEvent(out var ev))
            {
                switch (ev.EventId)
                {
                    case EventId.Core_Tick:
                        OnTick();
                        break;

                    case EventId.Core_Performance_EventLatencyTest:
                        eventLatency.OnPerformanceEvent(ev.EventDetails);
                        break;
                }
                eventsProcessed++;
            }

            return eventsProcessed;
        }

        /// <summary>
        /// Called when a system tick occurs.
        /// </summary>
        private void OnTick()
        {
            // Check if it's time to publish another performance report.
            var now = systemTimer.GetTime();
            if (now - lastReportTime < REPORT_INTERVAL_US) return;

            // Publish performance report.
            lastReportTime = now;
            foreach (var monitor in monitors)
            {
                monitor.ReportPerformance();
            }
        }
    }
}