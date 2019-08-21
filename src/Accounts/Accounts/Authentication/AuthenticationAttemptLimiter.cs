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

using Sovereign.Accounts.Configuration;
using Sovereign.EngineCore.Timing;
using Sovereign.EngineCore.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sovereign.Accounts.Accounts.Authentication
{

    /// <summary>
    /// Responsible for limiting the number of failed login attempts in a given
    /// time period, restricting account login if the maximum rate is exceeded.
    /// </summary>
    public sealed class AuthenticationAttemptLimiter
    {
        private readonly IAccountsConfiguration config;
        private readonly ISystemTimer systemTimer;

        /// <summary>
        /// Attempt records per username.
        /// </summary>
        private readonly IDictionary<string, AttemptRecord> attemptRecords
            = new Dictionary<string, AttemptRecord>();

        public AuthenticationAttemptLimiter(IAccountsConfiguration config,
            ISystemTimer systemTimer)
        {
            this.config = config;
            this.systemTimer = systemTimer;
        }

        /// <summary>
        /// Determines whether the account with the given username is currently
        /// denied login due to exceeding the maximum allowed login attempt rate.
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public bool IsAccountLoginDisabled(string username)
        {
            return attemptRecords.ContainsKey(username) &&
                attemptRecords[username].FailedAttemptCount >= config.MaxFailedLoginAttempts;
        }

        /// <summary>
        /// Registers a failed login attempt for the given username.
        /// </summary>
        /// <param name="username">Username.</param>
        public void RegisterFailedAttempt(string username)
        {
            if (!attemptRecords.ContainsKey(username))
            {
                attemptRecords[username] = new AttemptRecord();
            }

            var record = attemptRecords[username];
            record.FailedAttemptCount++;
            record.LastAttemptSystemTime = systemTimer.GetTime();
        }

        /// <summary>
        /// Purges any attempt records that have expired.
        /// </summary>
        public void PurgeExpiredRecords()
        {
            var currentSystemTime = systemTimer.GetTime();
            foreach (var username in attemptRecords.Keys)
            {
                var record = attemptRecords[username];
                var deltaSec = (currentSystemTime - record.LastAttemptSystemTime)
                    / Units.SystemTime.Second;
                if (deltaSec > (ulong)config.LoginDenialPeriodSeconds)
                {
                    // Login denial period is elapsed, purge record.
                    attemptRecords.Remove(username);
                }
            }
        }

        /// <summary>
        /// Tracking class for login attempts.
        /// </summary>
        private sealed class AttemptRecord
        {

            /// <summary>
            /// Failed attempt count.
            /// </summary>
            public int FailedAttemptCount { get; set; }

            /// <summary>
            /// System time of the last failed login attempt.
            /// </summary>
            public ulong LastAttemptSystemTime { get; set; }

        }

    }

}
