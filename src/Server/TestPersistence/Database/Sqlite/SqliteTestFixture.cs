﻿/*
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
using System;
using System.IO;
using System.Numerics;
using Xunit;

namespace TestPersistence.Database.Sqlite
{

    /// <summary>
    /// Reusable test fixture for testing the Sqlite persistence
    /// implementation.
    /// </summary>
    public class SqliteTestFixture : IDisposable
    {

        /// <summary>
        /// Path to the full SQL setup file.
        /// </summary>
        private const string SetupFile = @"../../../../Persistence/Migrations/Full/Full_sqlite.sql";

        /// <summary>
        /// In-memory database connection.
        /// </summary>
        public SqliteConnection Connection { get; private set; }

        public SqliteTestFixture()
        {
            Connection = CreateConnection();
            Connection.Open();
            ConfigureDatabase();
        }

        public void Dispose()
        {
            Connection.Dispose();
        }

        /// <summary>
        /// Adds an entity to the database.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        public void AddEntity(ulong entityId)
        {
            const string sql = "INSERT INTO Entity (id) VALUES (@Id)";
            var cmd = new SqliteCommand(sql, Connection);

            var param = new SqliteParameter("Id", entityId);
            param.SqliteType = SqliteType.Integer;
            cmd.Parameters.Add(param);

            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Positions an existing entity in the database.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        /// <param name="position">Position.</param>
        public void PositionEntity(ulong entityId, Vector3 position)
        {
            const string sql = @"INSERT INTO Position (id, x, y, z) 
                                 VALUES (@Id, @X, @Y, @Z)";
            var cmd = new SqliteCommand(sql, Connection);

            var pId = new SqliteParameter("Id", entityId);
            pId.SqliteType = SqliteType.Integer;
            cmd.Parameters.Add(pId);

            var pX = new SqliteParameter("X", position.X);
            pX.SqliteType = SqliteType.Real;
            cmd.Parameters.Add(pX);

            var pY = new SqliteParameter("Y", position.Y);
            pY.SqliteType = SqliteType.Real;
            cmd.Parameters.Add(pY);

            var pZ = new SqliteParameter("Z", position.Z);
            pZ.SqliteType = SqliteType.Real;
            cmd.Parameters.Add(pZ);

            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Adds a material component to the given entity.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        /// <param name="material">Material.</param>
        public void AddMaterial(ulong entityId, int material)
        {
            const string sql = @"INSERT INTO Material (id, material) VALUES (@Id, @Val)";
            using (var cmd = new SqliteCommand(sql, Connection))
            {
                var pId = new SqliteParameter("Id", entityId);
                pId.SqliteType = SqliteType.Integer;
                cmd.Parameters.Add(pId);

                var pVal = new SqliteParameter("Val", material);
                pVal.SqliteType = SqliteType.Integer;
                cmd.Parameters.Add(pVal);

                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Adds the given migration level to the database.
        /// </summary>
        /// <param name="level">Migration level.</param>
        public void AddMigrationLevel(int level)
        {
            const string sql = @"INSERT INTO MigrationLog (id, name) VALUES (@Id, ""test"")";
            var cmd = new SqliteCommand(sql, Connection);

            var pId = new SqliteParameter("Id", level);
            pId.SqliteType = SqliteType.Integer;
            cmd.Parameters.Add(pId);

            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Adds an account with the given username.
        /// </summary>
        /// <param name="id">Account ID.</param>
        /// <param name="username">Username.</param>
        /// <param name="salt">Password salt.</param>
        /// <param name="hash">Password hash.</param>
        public void AddAccount(Guid id, string username, byte[] salt, byte[] hash)
        {
            /* Add username info. */
            const string sql = @"INSERT INTO Account (id, username) VALUES (@Id, @Username)";
            var cmd = new SqliteCommand(sql, Connection);

            var pId = new SqliteParameter("Id", id.ToByteArray());
            pId.SqliteType = SqliteType.Blob;
            cmd.Parameters.Add(pId);

            var pUsername = new SqliteParameter("Username", username);
            pUsername.SqliteType = SqliteType.Text;
            cmd.Parameters.Add(pUsername);

            cmd.ExecuteNonQuery();

            /* Add authentication details. */
            const string sql2 = @"INSERT INTO Account_Authentication 
                                    (id, password_salt, password_hash, opslimit, memlimit) 
                                    VALUES (@Id, @Salt, @Hash, 0, 0)";
            var cmd2 = new SqliteCommand(sql2, Connection);
            cmd2.Parameters.Add(pId);

            var pSalt = new SqliteParameter("Salt", salt);
            pSalt.SqliteType = SqliteType.Blob;
            cmd2.Parameters.Add(pSalt);

            var pHash = new SqliteParameter("Hash", hash);
            pHash.SqliteType = SqliteType.Blob;
            cmd2.Parameters.Add(pHash);

            cmd2.ExecuteNonQuery();
        }

        /// <summary>
        /// Creates an in-memory database.
        /// </summary>
        /// <returns>In-memory database.</returns>
        private SqliteConnection CreateConnection()
        {
            var builder = new SqliteConnectionStringBuilder();
            builder.Mode = SqliteOpenMode.Memory;
            return new SqliteConnection(builder.ConnectionString);
        }

        private void ConfigureDatabase()
        {
            /* Read in the setup script. */
            string script;
            using (var reader = new StreamReader(SetupFile))
            {
                script = reader.ReadToEnd();
            }

            /* Prepare an SQL command. */
            var command = new SqliteCommand(script, Connection);

            /* Execute the command. */
            command.ExecuteNonQuery();
        }

    }

    /// <summary>
    /// Parent of the Sqlite persistence test collection.
    /// </summary>
    [CollectionDefinition("Sqlite")]
    public class SqliteTestCollection : ICollectionFixture<SqliteTestFixture>
    {

    }

}
