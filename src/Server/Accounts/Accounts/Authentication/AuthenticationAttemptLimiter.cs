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

using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Sovereign.EngineCore.Timing;
using Sovereign.EngineCore.Util;
using Sovereign.ServerCore.Configuration;

namespace Sovereign.Accounts.Accounts.Authentication;

/// <summary>
///     Responsible for limiting the number of failed login attempts in a given
///     time period, restricting account login if the maximum rate is exceeded.
/// </summary>
public sealed class AuthenticationAttemptLimiter
{
    /// <summary>
    ///     Attempt records per username.
    /// </summary>
    private readonly IDictionary<string, AttemptRecord> attemptRecords
        = new Dictionary<string, AttemptRecord>();

    private readonly AccountsOptions config;

    private readonly ISystemTimer systemTimer;

    public AuthenticationAttemptLimiter(IOptions<AccountsOptions> config,
        ISystemTimer systemTimer)
    {
        this.config = config.Value;
        this.systemTimer = systemTimer;
    }

    /// <summary>
    ///     Determines whether the account with the given username is currently
    ///     denied login due to exceeding the maximum allowed login attempt rate.
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    public bool IsAccountLoginDisabled(string username)
    {
        return attemptRecords.ContainsKey(username) &&
               attemptRecords[username].FailedAttemptCount >= config.MaxFailedLoginAttempts;
    }

    /// <summary>
    ///     Registers a failed login attempt for the given username.
    /// </summary>
    /// <param name="username">Username.</param>
    public void RegisterFailedAttempt(string username)
    {
        if (!attemptRecords.ContainsKey(username)) attemptRecords[username] = new AttemptRecord();

        var record = attemptRecords[username];
        record.FailedAttemptCount++;
        record.LastAttemptSystemTime = systemTimer.GetTime();
    }

    /// <summary>
    ///     Purges any attempt records that have expired.
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
                // Login denial period is elapsed, purge record.
                attemptRecords.Remove(username);
        }
    }

    /// <summary>
    ///     Tracking class for login attempts.
    /// </summary>
    private sealed class AttemptRecord
    {
        /// <summary>
        ///     Failed attempt count.
        /// </summary>
        public int FailedAttemptCount { get; set; }

        /// <summary>
        ///     System time of the last failed login attempt.
        /// </summary>
        public ulong LastAttemptSystemTime { get; set; }
    }
}