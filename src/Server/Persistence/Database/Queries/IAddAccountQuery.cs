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

namespace Sovereign.Persistence.Database.Queries
{

    /// <summary>
    /// Query for adding a new account.
    /// </summary>
    public interface IAddAccountQuery
    {

        /// <summary>
        /// Adds a new account to the database.
        /// </summary>
        /// <param name="id">Account ID.</param>
        /// <param name="username">Username.</param>
        /// <param name="passwordSalt">Password salt.</param>
        /// <param name="passwordHash">Password hash.</param>
        /// <param name="opslimit">Password hashing opslimit value.</param>
        /// <param name="memlimit">Password hashing memlimit value.</param>
        /// <returns>true if successful, false otherwise.</returns>
        bool AddAccount(Guid id, string username, byte[] passwordSalt, byte[] passwordHash,
            ulong opslimit, ulong memlimit);

    }

}
