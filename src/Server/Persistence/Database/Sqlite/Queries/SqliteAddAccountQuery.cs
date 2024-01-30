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
using System.Data;
using Microsoft.Data.Sqlite;
using Sovereign.Persistence.Database.Queries;

namespace Sovereign.Persistence.Database.Sqlite.Queries;

/// <summary>
///     SQLite implementation of IAddAccountQuery.
/// </summary>
public sealed class SqliteAddAccountQuery : IAddAccountQuery
{
    /// <summary>
    ///     SQLite error code for constraint errors.
    /// </summary>
    private const int SQLITE_ERROR_CONSTRAINT = 19;

    /// <summary>
    ///     SQL query for inserting the username.
    /// </summary>
    private const string sqlAccount = @"INSERT INTO Account(id, username) VALUES(@Id, @Username)";

    /// <summary>
    ///     SQL query for inserting the authentication details.
    /// </summary>
    private const string sqlAuth = @"INSERT INTO Account_Authentication
                                            (id, password_salt, password_hash, opslimit, memlimit)
                                            VALUES (@Id, @Salt, @Hash, @Opslimit, @Memlimit)";

    private readonly SqliteConnection connection;

    public SqliteAddAccountQuery(IDbConnection connection)
    {
        this.connection = (SqliteConnection)connection;
    }

    public bool AddAccount(Guid id, string username, byte[] passwordSalt, byte[] passwordHash, ulong opslimit,
        ulong memlimit)
    {
        // Open a transaction.
        using var transaction = connection.BeginTransaction();
        try
        {
            // Add the username.
            using var cmdAccount = new SqliteCommand(sqlAccount, connection, transaction);

            var pId = new SqliteParameter("Id", id.ToByteArray());
            pId.SqliteType = SqliteType.Blob;
            cmdAccount.Parameters.Add(pId);

            var pUsername = new SqliteParameter("Username", username);
            pUsername.SqliteType = SqliteType.Text;
            cmdAccount.Parameters.Add(pUsername);

            cmdAccount.ExecuteNonQuery();

            // Add the authentication details.
            using var cmdAuth = new SqliteCommand(sqlAuth, connection, transaction);
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

            // Some other error; propagate.
            throw;
        }

        return true;
    }
}