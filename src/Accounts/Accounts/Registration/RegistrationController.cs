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
using System.Collections.Generic;
using System.Text;
using MessagePack.Internal;
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
        private readonly IAddAccountQuery query;

        public RegistrationController(PersistenceProviderManager providerManager)
        {
            query = providerManager.PersistenceProvider.AddAccountQuery;
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
            return query.AddAccount(Guid.NewGuid(), username,
                salt, hash, opsLimit, memLimit);
        }

        private byte[] GenerateSalt()
        {
            return PasswordHash.ArgonGenerateSalt();
        }

        private void HashPassword(string password, byte[] salt, out byte[] hash,
            out ulong opsLimit, out ulong memLimit)
        {
            opsLimit = (ulong)PasswordHash.StrengthArgon.Interactive;
            memLimit = (ulong) PasswordHash.StrengthArgon.Interactive;
            hash = PasswordHash.ArgonHashBinary(Encoding.UTF8.GetBytes(password), 
                salt, (long)opsLimit, (int)memLimit);
        }

    }

}
