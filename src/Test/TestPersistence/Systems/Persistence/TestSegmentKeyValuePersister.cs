// Sovereign Engine
// Copyright (c) 2026 opticfluorine
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

using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Sqlite;
using Moq;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Systems.Data;
using Sovereign.Persistence.Database;
using Sovereign.Persistence.Database.Sqlite.Queries;
using Sovereign.Persistence.Systems.Persistence;
using Sovereign.ServerCore.Systems.WorldManagement;
using TestPersistence.Database.Sqlite;
using Xunit;

namespace TestPersistence.Systems.Persistence;

/// <summary>
///     Tests for SegmentKeyValuePersister against the SQLite fixture.
/// </summary>
[Collection("Sqlite")]
public class TestSegmentKeyValuePersister
{
    private readonly SqliteTestFixture fixture;
    private readonly SqliteGetSegmentKeyValuesQuery getQuery;
    private readonly PendingSegmentKeyValueUpdates pending = new();
    private readonly SegmentKeyValuePersister persister;
    private readonly Mock<IPersistenceProvider> provider = new();

    public TestSegmentKeyValuePersister(SqliteTestFixture fixture)
    {
        this.fixture = fixture;
        getQuery = new SqliteGetSegmentKeyValuesQuery(fixture.Connection);
        provider.SetupGet(p => p.GetSegmentKeyValuesQuery).Returns(getQuery);
        provider.SetupGet(p => p.SetSegmentKeyValueQuery).Returns(new SqliteSetSegmentKeyValueQuery(fixture.Connection));
        provider.SetupGet(p => p.RemoveSegmentKeyValueQuery)
            .Returns(new SqliteRemoveSegmentKeyValueQuery(fixture.Connection));
        persister = new SegmentKeyValuePersister(pending);
    }

    [Fact]
    public void Set_WritesTypedRow()
    {
        var segment = new GridPosition(700, 800, 900);
        pending.Enqueue(segment, "Owner", DataValue.FromEntityId(0x1234));

        Synchronize();

        var rows = ReadRows(segment);
        var row = Assert.Single(rows);
        Assert.Equal("Owner", row.Key);
        Assert.Equal((int)DataValueType.EntityId, row.Type);
        Assert.Equal(DataValueCodec.Encode(DataValue.FromEntityId(0x1234)), row.Value);
    }

    [Fact]
    public void SetThenRemove_DeletesRow()
    {
        var segment = new GridPosition(701, 801, 901);
        pending.Enqueue(segment, "Owner", DataValue.FromEntityId(0x1));
        pending.Enqueue(segment, "Owner", null);

        Synchronize();

        Assert.Empty(ReadRows(segment));
        Assert.Equal(0, CountRows(segment));
    }

    [Fact]
    public void RemoveMissing_IsNoOp()
    {
        var segment = new GridPosition(702, 802, 902);
        pending.Enqueue(segment, "Owner", null);

        Synchronize();

        Assert.Equal(0, CountRows(segment));
    }

    [Fact]
    public void RepeatedDrain_IsNoOp()
    {
        var segment = new GridPosition(703, 803, 903);
        pending.Enqueue(segment, "Owner", DataValue.FromString("Alice"));

        Synchronize();
        Synchronize();

        Assert.Equal(1, CountRows(segment));
    }

    /// <summary>
    ///     Drains the pending updates within a transaction.
    /// </summary>
    private void Synchronize()
    {
        using var transaction = fixture.Connection.BeginTransaction();
        persister.SynchronizeSegmentKeyValues(provider.Object, transaction);
        transaction.Commit();
    }

    /// <summary>
    ///     Reads the stored rows for a segment.
    /// </summary>
    /// <param name="segment">World segment index.</param>
    /// <returns>Stored rows.</returns>
    private IReadOnlyList<Sovereign.Persistence.Database.Queries.SegmentKeyValueRow> ReadRows(GridPosition segment)
    {
        return getQuery.GetKeyValues(segment);
    }

    /// <summary>
    ///     Counts the stored rows for a segment.
    /// </summary>
    /// <param name="segment">World segment index.</param>
    /// <returns>Number of stored rows.</returns>
    private long CountRows(GridPosition segment)
    {
        const string sql = "SELECT COUNT(*) FROM WorldSegmentKeyValue WHERE x = @X AND y = @Y AND z = @Z";
        using var cmd = new SqliteCommand(sql, fixture.Connection);
        cmd.Parameters.Add(new SqliteParameter("X", segment.X));
        cmd.Parameters.Add(new SqliteParameter("Y", segment.Y));
        cmd.Parameters.Add(new SqliteParameter("Z", segment.Z));
        return (long)cmd.ExecuteScalar()!;
    }
}
