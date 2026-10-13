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

using Microsoft.Data.Sqlite;
using Moq;
using Sovereign.EngineCore.Components.Types;
using Sovereign.Persistence.Database;
using Sovereign.Persistence.Database.Sqlite.Queries;
using Sovereign.Persistence.Systems.Persistence;
using TestPersistence.Database.Sqlite;
using Xunit;

namespace TestPersistence.Systems.Persistence;

/// <summary>
///     Tests for SegmentFlagPersister against the SQLite fixture.
/// </summary>
[Collection("Sqlite")]
public class TestSegmentFlagPersister
{
    private readonly PendingSegmentFlagUpdates pending = new();
    private readonly SegmentFlagPersister persister;
    private readonly Mock<IPersistenceProvider> provider = new();
    private readonly SqliteGetSegmentFlagsQuery getQuery;
    private readonly SqliteTestFixture fixture;

    public TestSegmentFlagPersister(SqliteTestFixture fixture)
    {
        this.fixture = fixture;
        getQuery = new SqliteGetSegmentFlagsQuery(fixture.Connection);
        provider.SetupGet(p => p.GetSegmentFlagsQuery).Returns(getQuery);
        provider.SetupGet(p => p.GetSegmentFlagsInRangeQuery)
            .Returns(new SqliteGetSegmentFlagsInRangeQuery(fixture.Connection));
        provider.SetupGet(p => p.SetSegmentFlagsQuery).Returns(new SqliteSetSegmentFlagsQuery(fixture.Connection));
        persister = new SegmentFlagPersister(pending);
    }

    [Fact]
    public void Set_WritesRow()
    {
        var segment = new GridPosition(700, 800, 900);
        pending.Enqueue(segment, segment, WorldSegmentFlag.Pvp, true);

        Synchronize();

        Assert.Equal(WorldSegmentFlag.Pvp, getQuery.GetFlags(segment));
        Assert.Equal(1, CountRows(segment));
    }

    [Fact]
    public void RepeatedSet_IsIdempotent()
    {
        var segment = new GridPosition(701, 801, 901);
        pending.Enqueue(segment, segment, WorldSegmentFlag.Pvp, true);
        pending.Enqueue(segment, segment, WorldSegmentFlag.Pvp, true);

        Synchronize();

        Assert.Equal(WorldSegmentFlag.Pvp, getQuery.GetFlags(segment));
        Assert.Equal(1, CountRows(segment));
    }

    [Fact]
    public void SetThenClear_DeletesRow()
    {
        var segment = new GridPosition(702, 802, 902);
        pending.Enqueue(segment, segment, WorldSegmentFlag.Pvp, true);
        pending.Enqueue(segment, segment, WorldSegmentFlag.Pvp, false);

        Synchronize();

        Assert.Equal(WorldSegmentFlag.None, getQuery.GetFlags(segment));
        Assert.Equal(0, CountRows(segment));
    }

    [Fact]
    public void ClearMissingSegment_WritesNothing()
    {
        var segment = new GridPosition(703, 803, 903);
        pending.Enqueue(segment, segment, WorldSegmentFlag.Pvp, false);

        Synchronize();

        Assert.Equal(0, CountRows(segment));
    }

    [Fact]
    public void SetOverBox_FlagsEverySegmentInBox()
    {
        var min = new GridPosition(710, 810, 910);
        var max = new GridPosition(711, 810, 910);
        pending.Enqueue(min, max, WorldSegmentFlag.Pvp, true);

        Synchronize();

        Assert.Equal(WorldSegmentFlag.Pvp, getQuery.GetFlags(min));
        Assert.Equal(WorldSegmentFlag.Pvp, getQuery.GetFlags(max));
    }

    /// <summary>
    ///     Drains the pending updates within a transaction.
    /// </summary>
    private void Synchronize()
    {
        using var transaction = fixture.Connection.BeginTransaction();
        persister.SynchronizeSegmentFlags(provider.Object, transaction);
        transaction.Commit();
    }

    /// <summary>
    ///     Counts the stored rows for a segment.
    /// </summary>
    /// <param name="segment">World segment index.</param>
    /// <returns>Number of stored rows.</returns>
    private long CountRows(GridPosition segment)
    {
        const string sql = "SELECT COUNT(*) FROM WorldSegmentFlag WHERE x = @X AND y = @Y AND z = @Z";
        using var cmd = new SqliteCommand(sql, fixture.Connection);
        cmd.Parameters.Add(new SqliteParameter("X", segment.X));
        cmd.Parameters.Add(new SqliteParameter("Y", segment.Y));
        cmd.Parameters.Add(new SqliteParameter("Z", segment.Z));
        return (long)cmd.ExecuteScalar()!;
    }
}
