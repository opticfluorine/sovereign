/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using Castle.Core.Logging;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Timing;

namespace Sovereign.Performance.Performance;

/// <summary>
///     Responsible for monitoring local event loop latency.
/// </summary>
public sealed class EventLatencyPerformanceMonitor : IPerformanceMonitor
{
    private readonly ISystemTimer systemTimer;

    /// <summary>
    ///     Latency event count since the last report.
    /// </summary>
    private uint latencyEventCount;

    /// <summary>
    ///     Sum of latency since the last report, in microseconds.
    /// </summary>
    private ulong latencySumMicroseconds;

    public EventLatencyPerformanceMonitor(ISystemTimer systemTimer)
    {
        this.systemTimer = systemTimer;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    public void OnPerformanceEvent(IEventDetails eventDetails)
    {
        var timeDetails = (TimeEventDetails)eventDetails;
        latencySumMicroseconds += systemTimer.GetTime() - timeDetails.SystemTime;
        latencyEventCount++;
    }

    public void ReportPerformance()
    {
        var avgLatencyUs = latencySumMicroseconds / (double)latencyEventCount;
        latencySumMicroseconds = 0;
        latencyEventCount = 0;

        Logger.DebugFormat("Average local event latency: {0:F2} us", avgLatencyUs);
    }
}