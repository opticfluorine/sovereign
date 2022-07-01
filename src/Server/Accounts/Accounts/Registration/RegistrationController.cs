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
using System.Text;
using Sodium;
using Sovereign.Persistence.Database;
using Sovereign.Persistence.Database.Queries;

namespace Sovereign.Accounts.Accounts.Registration
{

    /// <summary>
    /// Responsible for performing registrations.
    /// </summary>
    public sealed class RegistrationController
    {

        /// <summary>
        /// Argon2 opslimit.
        /// </summary>
        /// <remarks>
        /// Ideally the "Interactive" constant would be used,
        /// but the design of the Sodium.Core bindings unfortunately precludes
        /// this - it is possible to use these constants, but not to retrieve
        /// the values and store them alongside the hash in the database.
        /// </remarks>
        private const long OPSLIMIT = 4;

        /// <summary>
        /// Argon2 memlimit.
        /// </summary>
        /// <remarks>
        /// See remarks on OPSLIMIT.
        /// </remarks>
        private const int MEMLIMIT = 33554432;

        private readonly PersistenceProviderManager providerManager;

        public RegistrationController(PersistenceProviderManager providerManager)
        {
            this.providerManager = providerManager;
        }

        /// <summary>
        /// Registers the given username and password.
        /// </summary>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        /// <returns>true if successful, false otherwise.</returns>
        public bool Register(string username, string password)
        {
            /* Generate authentication details. */
            var salt = GenerateSalt();
            HashPassword(password, salt, out var hash,
                out var opsLimit, out var memLimit);

            /* Create account. */
            var query = providerManager.PersistenceProvider.AddAccountQuery;
            return query.AddAccount(Guid.NewGuid(), username,
                salt, hash, opsLimit, memLimit);
        }

        /// <summary>
        /// Generates a salt for a password hash.
        /// </summary>
        /// <returns>Salt.</returns>
        private byte[] GenerateSalt()
        {
            return PasswordHash.ArgonGenerateSalt();
        }

        /// <summary>
        /// Hashes a password.
        /// </summary>
        /// <param name="password">Password to be hashed.</param>
        /// <param name="salt">Password salt.</param>
        /// <param name="hash">Password hash.</param>
        /// <param name="opsLimit">Opslimit parameter.</param>
        /// <param name="memLimit">Memlimit parameter.</param>
        private void HashPassword(string password, byte[] salt, out byte[] hash,
            out ulong opsLimit, out ulong memLimit)
        {
            opsLimit = (ulong)OPSLIMIT;
            memLimit = (ulong)MEMLIMIT;
            hash = PasswordHash.ArgonHashBinary(Encoding.UTF8.GetBytes(password),
                salt, OPSLIMIT, MEMLIMIT);
        }

    }

}
