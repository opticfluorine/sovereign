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

using Microsoft.Extensions.Options;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Timing;
using Sovereign.ServerCore.Configuration;

namespace Sovereign.Persistence.Systems.Persistence;

/// <summary>
///     Responsible for scheduling persistence events.
/// </summary>
public sealed class PersistenceScheduler
{
    private const ulong SToUs = 1000000;
    private readonly DatabaseOptions configuration;
    private readonly IEventSender sender;
    private readonly ISystemTimer systemTimer;

    public PersistenceScheduler(IEventSender sender,
        IOptions<DatabaseOptions> configuration,
        ISystemTimer systemTimer)
    {
        this.sender = sender;
        this.configuration = configuration.Value;
        this.systemTimer = systemTimer;
    }

    /// <summary>
    ///     Schedules the next persistence synchronization.
    /// </summary>
    public void ScheduleSynchronize()
    {
        /* Determine time of next synchronization. */
        var nextTime = systemTimer.GetTime()
                       + (ulong)configuration.SyncIntervalSeconds * SToUs;

        /* Schedule event. */
        var ev = new Event(EventId.Server_Persistence_Synchronize,
            nextTime);
        sender.SendEvent(ev);
    }
}