/*
 * Sovereign Engine
 * Copyright (c) 2020 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
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