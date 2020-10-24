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
