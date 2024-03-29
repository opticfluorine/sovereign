﻿/*
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

namespace Sovereign.Persistence.Accounts;

/// <summary>
///     Data class for a user account.
/// </summary>
/// <remarks>
///     Any changes to the persisted user account schema must be documented in
///     the "Account Privacy" section of docs/accounts.md.
/// </remarks>
public sealed class Account
{
    /// <summary>
    ///     Creates an account record with the given fields.
    /// </summary>
    /// <param name="id">Account ID.</param>
    /// <param name="username">Username.</param>
    internal Account(Guid id, string username)
    {
        Id = id;
        Username = username;

        AuthDetailsPresent = false;
    }

    /// <summary>
    ///     Creates an account record with the given fields.
    /// </summary>
    /// <param name="id">Account ID.</param>
    /// <param name="username">Username.</param>
    /// <param name="salt">Password salt.</param>
    /// <param name="hash">Password hash.</param>
    /// <param name="opslimit">Argon2 opslimit.</param>
    /// <param name="memlimit">Argon2 memlimit.</param>
    internal Account(Guid id, string username, byte[] salt, byte[] hash,
        ulong opslimit, ulong memlimit)
    {
        Id = id;
        Username = username;

        Salt = new byte[salt.Length];
        Array.Copy(salt, Salt, salt.Length);

        Hash = new byte[hash.Length];
        Array.Copy(hash, Hash, hash.Length);

        Opslimit = opslimit;
        Memlimit = memlimit;

        AuthDetailsPresent = true;
    }

    /// <summary>
    ///     Account ID.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    ///     Account username.
    /// </summary>
    public string Username { get; private set; }

    /// <summary>
    ///     Whether authentication details are present in this record.
    /// </summary>
    public bool AuthDetailsPresent { get; private set; }

    /// <summary>
    ///     Password salt.
    /// </summary>
    public byte[] Salt { get; } = Array.Empty<byte>();

    /// <summary>
    ///     Password hash.
    /// </summary>
    public byte[] Hash { get; } = Array.Empty<byte>();

    /// <summary>
    ///     Argon2 operations limit for password hashing.
    /// </summary>
    public ulong Opslimit { get; private set; }

    /// <summary>
    ///     Argon2 memory limit for password hashing.
    /// </summary>
    public ulong Memlimit { get; private set; }
}