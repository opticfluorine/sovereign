// Sovereign Engine
// Copyright (c) 2025 opticfluorine
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sovereign.EngineCore.Configuration;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems.Data;
using Sovereign.EngineCore.Timing;

namespace Sovereign.EngineCore.Systems.Time;

/// <summary>
///     In-game clock synchronized between client and server.
/// </summary>
internal interface IGameClock
{
    /// <summary>
    ///     Current game time. Get to read time, set to synchronize with a reference clock.
    /// </summary>
    uint Time { get; set; }

    /// <summary>
    ///     Updates the game clock per tick.
    /// </summary>
    void OnTick();
}

/// <summary>
///     Implementation of IGameClock.
/// </summary>
internal class GameClock(
    ISystemTimer timer,
    TimeInternalController controller,
    IOptions<TimeOptions> options,
    IEventSender eventSender,
    IDataController dataController,
    IDataServices dataServices,
    ILogger<GameClock> logger) : IGameClock
{
    private const ulong SecondsToMicroseconds = 1000000;
    private bool initialized;

    // We start the update timer advanced to the nearest whole second on the wall clock.
    // This allows us to better synchronize the client and server clocks assuming that
    // both wall clocks are synchronized to an NTP server: the error is roughly the sum
    // of the NTP error (~10-100 ms) and the tick interval. Altogether this gives an ideal
    // game clock accuracy of ~ tens of milliseconds.
    private ulong lastUpdateTimeUs = timer.GetTime() + SecondsToMicroseconds - (ulong)DateTimeOffset.UtcNow.Microsecond;
    private ulong updateCounter;

    /// <summary>
    ///     Current game time. Get to read time, set to synchronize with a reference clock.
    /// </summary>
    public uint Time { get; set; }

    /// <summary>
    ///     Updates the game clock per tick.
    /// </summary>
    public void OnTick()
    {
        if (!initialized)
        {
            // Synchronize from a cached value if available.
            if (dataServices.TryGetGlobalUlong(TimeConstants.ClockGlobalKey, out var time))
            {
                logger.LogDebug("Loaded clock time: {Time}", time);
                Time = (uint)time;
            }

            initialized = true;
        }

        // Advance game clock to follow wall clock.
        var systemTime = timer.GetTime();
        if (lastUpdateTimeUs > systemTime) return; // catching up at startup...
        var delta = (systemTime - lastUpdateTimeUs) / SecondsToMicroseconds;
        Time += (uint)delta;
        if (delta > 0)
        {
            lastUpdateTimeUs += delta * SecondsToMicroseconds;
            dataController.SetGlobal(eventSender, TimeConstants.ClockGlobalKey, Time);
        }

        // Send a sync event if enough ticks have passed.
        updateCounter += delta;
        if (updateCounter >= options.Value.SyncIntervalSeconds)
        {
            updateCounter %= options.Value.SyncIntervalSeconds;
            controller.SendClock(Time);
        }
    }
}