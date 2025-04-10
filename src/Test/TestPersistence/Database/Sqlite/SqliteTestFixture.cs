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
using System.IO;
using System.Numerics;
using Microsoft.Data.Sqlite;
using Xunit;

namespace TestPersistence.Database.Sqlite;

/// <summary>
///     Reusable test fixture for testing the Sqlite persistence
///     implementation.
/// </summary>
public class SqliteTestFixture : IDisposable
{
    /// <summary>
    ///     Path to the full SQL setup file.
    /// </summary>
    private const string SetupFile = @"../../../../../Server/Persistence/Migrations/Full/Full_sqlite.sql";

    public SqliteTestFixture()
    {
        Connection = CreateConnection();
        Connection.Open();
        ConfigureDatabase();
    }

    /// <summary>
    ///     In-memory database connection.
    /// </summary>
    public SqliteConnection Connection { get; }

    public void Dispose()
    {
        Connection.Dispose();
    }

    /// <summary>
    ///     Adds an entity to the database.
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
    ///     Positions an existing entity in the database.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="position">Position.</param>
    public void PositionEntity(ulong entityId, Vector3 position)
    {
        const string sql = @"UPDATE Entity SET pos_x = @X, pos_y = @Y, pos_z = @Z WHERE id = @Id";
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
    ///     Adds a material component to the given entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="material">Material.</param>
    public void AddMaterial(ulong entityId, int material)
    {
        const string sql = @"UPDATE Entity SET material = @Val WHERE id = @Id";
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
    ///     Adds a name component to the given entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="name">Name.</param>
    public void AddName(ulong entityId, string name)
    {
        const string sql = @"UPDATE Entity SET name = @Name WHERE id = @Id";
        using (var cmd = new SqliteCommand(sql, Connection))
        {
            var pId = new SqliteParameter("Id", entityId);
            pId.SqliteType = SqliteType.Integer;
            cmd.Parameters.Add(pId);

            var pName = new SqliteParameter("Name", name);
            pName.SqliteType = SqliteType.Text;
            cmd.Parameters.Add(pName);

            cmd.ExecuteNonQuery();
        }
    }

    /// <summary>
    ///     Adds a player character tag to the given entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    public void AddPlayerCharacter(ulong entityId)
    {
        const string sql = @"UPDATE Entity SET player_char = TRUE WHERE id = @Id";
        using (var cmd = new SqliteCommand(sql, Connection))
        {
            var pId = new SqliteParameter("Id", entityId);
            pId.SqliteType = SqliteType.Integer;
            cmd.Parameters.Add(pId);

            cmd.ExecuteNonQuery();
        }
    }

    /// <summary>
    ///     Adds an animated sprite component to the given entity..
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="animatedSpriteId">Animated sprite ID.</param>
    public void AddAnimatedSprite(ulong entityId, int animatedSpriteId)
    {
        const string sql = @"UPDATE Entity SET animated_sprite = @SpriteId WHERE id = @Id";
        using var cmd = new SqliteCommand(sql, Connection);

        var pId = new SqliteParameter("Id", entityId);
        pId.SqliteType = SqliteType.Integer;
        cmd.Parameters.Add(pId);

        var pSpriteId = new SqliteParameter("SpriteId", animatedSpriteId);
        pId.SqliteType = SqliteType.Integer;
        cmd.Parameters.Add(pSpriteId);

        cmd.ExecuteNonQuery();
    }

    /// <summary>
    ///     Adds an account component to the given entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="accountId">Linked account ID.</param>
    public void AddAccountComponent(ulong entityId, Guid accountId)
    {
        const string sql = @"UPDATE Entity SET account_id = @AccountId WHERE id = @Id";
        using (var cmd = new SqliteCommand(sql, Connection))
        {
            var pId = new SqliteParameter("Id", entityId);
            pId.SqliteType = SqliteType.Integer;
            cmd.Parameters.Add(pId);

            var pAccountId = new SqliteParameter("AccountId", accountId.ToByteArray());
            pAccountId.SqliteType = SqliteType.Blob;
            cmd.Parameters.Add(pAccountId);

            cmd.ExecuteNonQuery();
        }
    }

    /// <summary>
    ///     Adds the given migration level to the database.
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
    ///     Adds an account with the given username.
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
    ///     Creates an in-memory database.
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
///     Parent of the Sqlite persistence test collection.
/// </summary>
[CollectionDefinition("Sqlite")]
public class SqliteTestCollection : ICollectionFixture<SqliteTestFixture>
{
}