// Sovereign Engine
// Copyright (c) 2025 opticfluorine
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using Microsoft.Data.Sqlite;
using Sovereign.Persistence.Database.Sqlite.Queries;
using Xunit;

namespace TestPersistence.Database.Sqlite.Queries;

/// <summary>
///     Verifies that the admin-role Sqlite queries match player names
///     case-insensitively (COLLATE NOCASE).
/// </summary>
[Collection("Sqlite")]
public class TestSqliteAdminRoleQueries_CaseInsensitive
{
    private readonly SqliteTestFixture fixture;

    public TestSqliteAdminRoleQueries_CaseInsensitive(SqliteTestFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void TryAddAdminRole_MatchesCaseInsensitively()
    {
        const ulong entityId = 99011;
        fixture.AddEntity(entityId);
        fixture.AddPlayerCharacter(entityId);
        fixture.AddName(entityId, "Alice");

        var addQuery = new SqliteAddAdminRoleQuery(fixture.Connection);
        Assert.True(addQuery.TryAddAdminRole("alice"));
        Assert.True(IsAdmin(entityId));
    }

    [Fact]
    public void TryAddAdminRole_NoMatch_ReturnsFalse()
    {
        const ulong entityId = 99012;
        fixture.AddEntity(entityId);
        fixture.AddPlayerCharacter(entityId);
        fixture.AddName(entityId, "Alice");

        var addQuery = new SqliteAddAdminRoleQuery(fixture.Connection);
        Assert.False(addQuery.TryAddAdminRole("nobody"));
    }

    [Fact]
    public void RemoveAdminRole_MatchesCaseInsensitively()
    {
        const ulong entityId = 99013;
        fixture.AddEntity(entityId);
        fixture.AddPlayerCharacter(entityId);
        fixture.AddName(entityId, "Alice");

        var addQuery = new SqliteAddAdminRoleQuery(fixture.Connection);
        Assert.True(addQuery.TryAddAdminRole("Alice"));
        Assert.True(IsAdmin(entityId));

        var removeQuery = new SqliteRemoveAdminRoleQuery(fixture.Connection);
        removeQuery.RemoveAdminRole("ALICE");
        Assert.False(IsAdmin(entityId));
    }

    /// <summary>
    ///     Directly queries the admin column for the given entity.
    /// </summary>
    private bool IsAdmin(ulong entityId)
    {
        const string sql = "SELECT admin FROM Entity WHERE id = @Id";
        using var cmd = new SqliteCommand(sql, fixture.Connection);
        var pId = new SqliteParameter("Id", entityId);
        pId.SqliteType = SqliteType.Integer;
        cmd.Parameters.Add(pId);

        var result = cmd.ExecuteScalar();
        return result != null && result != DBNull.Value && (long)result != 0;
    }
}
