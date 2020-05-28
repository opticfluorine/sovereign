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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Sodium;
using Sovereign.Accounts.Configuration;
using Sovereign.EngineCore.Timing;
using Sovereign.EngineCore.Util;
using Sovereign.EngineUtil.Numerics;

namespace Sovereign.Accounts.Accounts.Authentication
{

    sealed class PendingSecret
    {

        public ulong GenerationTime { get; set; }

        public byte[] SharedSecret { get; set; }

    }

    /// <summary>
    /// Responsible for managing shared secrets during the
    /// connection handoff.
    /// </summary>
    public sealed class SharedSecretManager
    {

        /// <summary>
        /// Length of the shared secret, in bytes.
        /// </summary>
        private const int SECRET_LENGTH = 16;

        /// <summary>
        /// Pending secrets by user ID.
        /// </summary>
        private readonly IDictionary<Guid, PendingSecret> pendingSecrets
            = new ConcurrentDictionary<Guid, PendingSecret>();

        private readonly ISystemTimer timer;
        private readonly IAccountsConfiguration accountsConfiguration;

        public SharedSecretManager(ISystemTimer timer, 
            IAccountsConfiguration accountsConfiguration)
        {
            this.timer = timer;
            this.accountsConfiguration = accountsConfiguration;
        }

        /// <summary>
        /// Adds a secret for the given user ID.
        /// </summary>
        /// <param name="userId">User ID.</param>
        /// <returns>Hexadecimal-encoded shared secret.</returns>
        /// <remarks>
        /// If a shared secret is already pending for the given user ID,
        /// calling AddSecret will overwrite the existing secret.
        /// </remarks>
        public string AddSecret(Guid userId)
        {
            var pendingSecret = new PendingSecret()
            {
                GenerationTime = timer.GetTime(),
                SharedSecret = CreateSecret()
            };
            pendingSecrets[userId] = pendingSecret;
            return Utilities.BinaryToHex(pendingSecret.SharedSecret);
        }

        /// <summary>
        /// Takes the secret for the given user ID if one is pending.
        /// </summary>
        /// <param name="userId">User ID.</param>
        /// <param name="sharedSecret"></param>
        /// <returns>true if a secret was pending, false otherwise.</returns>
        public bool TakeSecret(Guid userId, out byte[] sharedSecret)
        {
            sharedSecret = null;
            var hasSecret = pendingSecrets.ContainsKey(userId);
            if (hasSecret)
            {
                var pendingSecret = pendingSecrets[userId];
                pendingSecrets.Remove(userId);

                // Ensure that the secret hasn't expired.
                if (!HasSecretExpired(pendingSecret, timer.GetTime()))
                {
                    sharedSecret = pendingSecret.SharedSecret;
                }
                else
                {
                    // Secret expired, go ahead and remove it.
                    pendingSecrets.Remove(userId);
                }
            }

            return sharedSecret != null;
        }

        /// <summary>
        /// Purges old expired pending secrets from memory.
        /// </summary>
        public void PurgeOldSecrets()
        {
            var now = timer.GetTime();
            foreach (var userId in pendingSecrets.Keys)
            {
                var pendingSecret = pendingSecrets[userId];
                if (HasSecretExpired(pendingSecret, now))
                {
                    // Secret has expired, remove.
                    pendingSecrets.Remove(userId);
                }
            }
        }

        /// <summary>
        /// Generates a new shared secret.
        /// </summary>
        /// <returns>Shared secret.</returns>
        private byte[] CreateSecret()
        {
            return SodiumCore.GetRandomBytes(SECRET_LENGTH);
        }

        /// <summary>
        /// Checks whether the given pending secret has expired.
        /// </summary>
        /// <param name="pendingSecret">Pending secret.</param>
        /// <param name="systemTime">Current system time.</param>
        /// <returns>true if expired, false otherwise.</returns>
        private bool HasSecretExpired(PendingSecret pendingSecret, ulong systemTime)
        {
            var delta = (systemTime - pendingSecret.GenerationTime) * UnitConversions.UsToS;
            return delta > accountsConfiguration.HandoffPeriodSeconds;
        }

    }

}
