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
    /// SQLite implementation of IAddAccountQuery.
    /// </summary>
    public sealed class SqliteAddAccountQuery : IAddAccountQuery
    {
        private readonly SqliteConnection connection;

        /// <summary>
        /// SQLite error code for constraint errors.
        /// </summary>
        private const int SQLITE_ERROR_CONSTRAINT = 19;

        /// <summary>
        /// SQL query for inserting the username.
        /// </summary>
        private const string sqlAccount = @"INSERT INTO Account(id, username) VALUES(@Id, @Username)";

        /// <summary>
        /// SQL query for inserting the authentication details.
        /// </summary>
        private const string sqlAuth = @"INSERT INTO Account_Authentication
                                            (id, password_salt, password_hash, opslimit, memlimit)
                                            VALUES (@Id, @Salt, @Hash, @Opslimit, @Memlimit)";

        public SqliteAddAccountQuery(IDbConnection connection)
        {
            this.connection = (SqliteConnection)connection;
        }

        public bool AddAccount(Guid id, string username, byte[] passwordSalt, byte[] passwordHash, ulong opslimit, ulong memlimit)
        {
            // Open a transaction.
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    // Add the username.
                    var cmdAccount = new SqliteCommand(sqlAccount, connection, transaction);

                    var pId = new SqliteParameter("Id", id.ToByteArray());
                    pId.SqliteType = SqliteType.Blob;
                    cmdAccount.Parameters.Add(pId);

                    var pUsername = new SqliteParameter("Username", username);
                    pUsername.SqliteType = SqliteType.Text;
                    cmdAccount.Parameters.Add(pUsername);

                    cmdAccount.ExecuteNonQuery();

                    // Add the authentication details.
                    var cmdAuth = new SqliteCommand(sqlAuth, connection, transaction);
                    cmdAuth.Parameters.Add(pId);

                    var pSalt = new SqliteParameter("Salt", passwordSalt);
                    pSalt.SqliteType = SqliteType.Blob;
                    cmdAuth.Parameters.Add(pSalt);

                    var pHash = new SqliteParameter("Hash", passwordHash);
                    pHash.SqliteType = SqliteType.Blob;
                    cmdAuth.Parameters.Add(pHash);

                    var pOpslimit = new SqliteParameter("Opslimit", opslimit);
                    pOpslimit.SqliteType = SqliteType.Integer;
                    cmdAuth.Parameters.Add(pOpslimit);

                    var pMemlimit = new SqliteParameter("Memlimit", memlimit);
                    pMemlimit.SqliteType = SqliteType.Integer;
                    cmdAuth.Parameters.Add(pMemlimit);

                    cmdAuth.ExecuteNonQuery();

                    transaction.Commit();
                }
                catch (SqliteException e)
                {
                    if (e.SqliteErrorCode == SQLITE_ERROR_CONSTRAINT)
                    {
                        // Constraint violation - ID or username already taken.
                        transaction.Rollback();
                        return false;
                    }
                    else
                    {
                        // Some other error; propagate.
                        throw;
                    }
                }
            }

            return true;
        }

    }

}
