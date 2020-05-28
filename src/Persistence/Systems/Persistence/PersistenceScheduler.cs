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

using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Timing;
using Sovereign.Persistence.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sovereign.Persistence.Systems.Persistence
{

    /// <summary>
    /// Responsible for scheduling persistence events.
    /// </summary>
    public sealed class PersistenceScheduler
    {
        private readonly IEventSender sender;
        private readonly IPersistenceConfiguration configuration;
        private readonly ISystemTimer systemTimer;

        private const ulong S_TO_US = 1000000;

        public PersistenceScheduler(IEventSender sender,
            IPersistenceConfiguration configuration,
            ISystemTimer systemTimer)
        {
            this.sender = sender;
            this.configuration = configuration;
            this.systemTimer = systemTimer;
        }

        /// <summary>
        /// Schedules the next persistence synchronization.
        /// </summary>
        public void ScheduleSynchronize()
        {
            /* Determine time of next synchronization. */
            var nextTime = systemTimer.GetTime()
                + (ulong)configuration.SyncIntervalSeconds * S_TO_US;

            /* Schedule event. */
            var ev = new Event(EventId.Server_Persistence_Synchronize, 
                nextTime);
            sender.SendEvent(ev);
        }

    }

}
