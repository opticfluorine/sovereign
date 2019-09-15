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

        public void ExecuteOnce()
        {
            while (EventCommunicator.GetIncomingEvent(out var ev))
            {
                ticksSincePurge++;
                if (ticksSincePurge > TICKS_BETWEEN_PURGES)
                {
                    attemptLimiter.PurgeExpiredRecords();
                    sharedSecretManager.PurgeOldSecrets();
                    ticksSincePurge = 0;
                }
            }
        }

        public void Initialize()
        {
        }

    }

}
