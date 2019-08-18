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

using Microsoft.Data.Sqlite;
using Sovereign.Persistence.Database.Queries;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Sovereign.Persistence.Database.Sqlite.Queries
{

    /// <summary>
    /// IRetrieveAccountWithAuthQuery for the SQLite persistence provider.
    /// </summary>
    public sealed class SqliteRetrieveAccountWithAuthQuery : IRetrieveAccountWithAuthQuery
    {

        private readonly SqliteConnection dbConnection;

        /// <summary>
        /// SQL query.
        /// </summary>
        private const string Query =
            @"SELECT * FROM Account
                INNER JOIN Account_Authentication 
                  ON Account.id = Account_Authentication.id
                WHERE Account.username = @Username";

        public SqliteRetrieveAccountWithAuthQuery(IDbConnection dbConnection)
        {
            this.dbConnection = (SqliteConnection)dbConnection;
        }

        public QueryReader RetrieveAccountWithAuth(string username)
        {
            var cmd = new SqliteCommand(Query, dbConnection);

            var param = new SqliteParameter("Username", username);
            param.SqliteType = SqliteType.Text;
            cmd.Parameters.Add(param);

            return new QueryReader(cmd);
        }

    }
}
