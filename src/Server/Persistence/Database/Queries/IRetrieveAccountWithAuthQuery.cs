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
    /// Interface for retrieving account details including authentication
    /// fields (password hash, salt).
    /// </summary>
    /// <remarks>
    /// This should only be used when attempting to authenticate a client.
    /// If only the account ID associated with a username is known,
    /// IRetrieveAccountQuery should be used instead.
    /// </remarks>
    public interface IRetrieveAccountWithAuthQuery
    {

        /// <summary>
        /// Retrieves an account and its authentication details.
        /// </summary>
        /// <param name="username">Username.</param>
        /// <returns>QueryReader to retrieve the data.</returns>
        QueryReader RetrieveAccountWithAuth(string username);

    }

}
