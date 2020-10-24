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

namespace Sovereign.Persistence.Accounts
{

    /// <summary>
    /// Data class for a user account.
    /// </summary>
    /// <remarks>
    /// Any changes to the persisted user account schema must be documented in
    /// the "Account Privacy" section of docs/accounts.md.
    /// </remarks>
    public sealed class Account
    {

        /// <summary>
        /// Account ID.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Account username.
        /// </summary>
        public string Username { get; private set; }

        /// <summary>
        /// Whether authentication details are present in this record.
        /// </summary>
        public bool AuthDetailsPresent { get; private set; }

        /// <summary>
        /// Password salt.
        /// </summary>
        public byte[] Salt { get; private set; }

        /// <summary>
        /// Password hash.
        /// </summary>
        public byte[] Hash { get; private set; }

        /// <summary>
        /// Argon2 operations limit for password hashing.
        /// </summary>
        public ulong Opslimit { get; private set; }

        /// <summary>
        /// Argon2 memory limit for password hashing.
        /// </summary>
        public ulong Memlimit { get; private set; }

        /// <summary>
        /// Creates an account record with the given fields.
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
        /// Creates an account record with the given fields.
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

    }

}
