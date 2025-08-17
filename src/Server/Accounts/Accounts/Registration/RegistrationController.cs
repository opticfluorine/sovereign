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

using System;
using System.Text;
using Sodium;
using Sovereign.Persistence.Database;

namespace Sovereign.Accounts.Accounts.Registration;

/// <summary>
///     Responsible for performing registrations.
/// </summary>
public sealed class RegistrationController(PersistenceProviderManager providerManager)
{
    /// <summary>
    ///     Argon2 opslimit.
    /// </summary>
    /// <remarks>
    ///     Ideally the "Interactive" constant would be used,
    ///     but the design of the Sodium.Core bindings unfortunately precludes
    ///     this - it is possible to use these constants, but not to retrieve
    ///     the values and store them alongside the hash in the database.
    /// </remarks>
    private const long OPSLIMIT = 4;

    /// <summary>
    ///     Argon2 memlimit.
    /// </summary>
    /// <remarks>
    ///     See remarks on OPSLIMIT.
    /// </remarks>
    private const int MEMLIMIT = 33554432;

    /// <summary>
    ///     Registers the given username and password.
    /// </summary>
    /// <param name="username">Username.</param>
    /// <param name="password">Password.</param>
    /// <returns>true if successful, false otherwise.</returns>
    public bool RegisterSync(string username, string password)
    {
        /* Generate authentication details. */
        var salt = GenerateSalt();
        HashPassword(password, salt, out var hash,
            out var opsLimit, out var memLimit);

        /* Create account. */
        providerManager.PersistenceProvider.TransactionLock.Acquire();
        try
        {
            var query = providerManager.PersistenceProvider.AddAccountQuery;
            return query.AddAccount(Guid.NewGuid(), username,
                salt, hash, opsLimit, memLimit);
        }
        finally
        {
            providerManager.PersistenceProvider.TransactionLock.Release();
        }
    }

    /// <summary>
    ///     Generates a salt for a password hash.
    /// </summary>
    /// <returns>Salt.</returns>
    private byte[] GenerateSalt()
    {
        return PasswordHash.ArgonGenerateSalt();
    }

    /// <summary>
    ///     Hashes a password.
    /// </summary>
    /// <param name="password">Password to be hashed.</param>
    /// <param name="salt">Password salt.</param>
    /// <param name="hash">Password hash.</param>
    /// <param name="opsLimit">Opslimit parameter.</param>
    /// <param name="memLimit">Memlimit parameter.</param>
    private void HashPassword(string password, byte[] salt, out byte[] hash,
        out ulong opsLimit, out ulong memLimit)
    {
        opsLimit = OPSLIMIT;
        memLimit = MEMLIMIT;
        hash = PasswordHash.ArgonHashBinary(Encoding.UTF8.GetBytes(password),
            salt, OPSLIMIT, MEMLIMIT);
    }
}