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

using Sovereign.Accounts.Accounts.Authentication;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sovereign.Accounts.Systems.Accounts
{

    /// <summary>
    /// 
    /// </summary>
    public sealed class AccountsSystem : ISystem
    {

        /// <summary>
        /// Number of ticks between subsequent limiter purges.
        /// </summary>
        const int TICKS_BETWEEN_PURGES = 30;

        /// <summary>
        /// Number of ticks since the last limiter purge.
        /// </summary>
        private int ticksSincePurge = 0;

        private readonly ISet<EventId> eventIdsOfInterest = new HashSet<EventId>()
        {
            EventId.Core_Tick
        };
        private readonly AuthenticationAttemptLimiter attemptLimiter;
        private readonly SharedSecretManager sharedSecretManager;

        public EventCommunicator EventCommunicator { get; private set; }

        public ISet<EventId> EventIdsOfInterest { get => eventIdsOfInterest; }

        public int WorkloadEstimate => 10;

        public AccountsSystem(IEventLoop eventLoop,
            EventCommunicator eventCommunicator,
            AuthenticationAttemptLimiter attemptLimiter,
            SharedSecretManager sharedSecretManager)
        {
            this.attemptLimiter = attemptLimiter;
            this.sharedSecretManager = sharedSecretManager;

            EventCommunicator = eventCommunicator;
            eventLoop.RegisterSystem(this);
        }

        public void Cleanup()
        {
        }

        public int ExecuteOnce()
        {
            var eventsProcessed = 0;
            while (EventCommunicator.GetIncomingEvent(out var ev))
            {
                ticksSincePurge++;
                if (ticksSincePurge > TICKS_BETWEEN_PURGES)
                {
                    attemptLimiter.PurgeExpiredRecords();
                    sharedSecretManager.PurgeOldSecrets();
                    ticksSincePurge = 0;
                }

                eventsProcessed++;
            }

            return eventsProcessed;
        }

        public void Initialize()
        {
        }

    }

}
