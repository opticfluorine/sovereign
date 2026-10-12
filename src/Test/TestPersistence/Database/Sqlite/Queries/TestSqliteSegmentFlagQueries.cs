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
using Microsoft.Data.Sqlite;
using Sovereign.EngineCore.Components.Types;
using Sovereign.Persistence.Database.Sqlite.Queries;
using Xunit;

namespace TestPersistence.Database.Sqlite.Queries;

/// <summary>
///     Tests for the world segment flag Sqlite queries.
/// </summary>
[Collection("Sqlite")]
public class TestSqliteSegmentFlagQueries
{
    private readonly SqliteTestFixture fixture;

    public TestSqliteSegmentFlagQueries(SqliteTestFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void SetFlags_ThenGetFlags_RoundTrip()
    {
        var segment = new GridPosition(100, 200, 300);

        SetFlags(segment, WorldSegmentFlag.Pvp);

        Assert.Equal(WorldSegmentFlag.Pvp, GetFlags(segment));
    }

    [Fact]
    public void SetFlags_UpsertOverwritesExistingRow()
    {
        var segment = new GridPosition(101, 201, 301);

        SetFlags(segment, WorldSegmentFlag.Pvp);
        SetFlags(segment, WorldSegmentFlag.None);
        SetFlags(segment, WorldSegmentFlag.Pvp);

        Assert.Equal(WorldSegmentFlag.Pvp, GetFlags(segment));
        Assert.Equal(1, CountRows(segment));
    }

    [Fact]
    public void SetFlags_ZeroDeletesRow()
    {
        var segment = new GridPosition(102, 202, 302);

        SetFlags(segment, WorldSegmentFlag.Pvp);
        SetFlags(segment, WorldSegmentFlag.None);

        Assert.Equal(WorldSegmentFlag.None, GetFlags(segment));
        Assert.Equal(0, CountRows(segment));
    }

    [Fact]
    public void GetFlags_MissingSegment_ReturnsNone()
    {
        Assert.Equal(WorldSegmentFlag.None, GetFlags(new GridPosition(103, 203, 303)));
    }

    [Fact]
    public void GetFlagsInRange_IsInclusiveOnAllFaces()
    {
        var min = new GridPosition(400, 500, 600);
        var max = new GridPosition(402, 502, 602);

        // Inside points touching each face.
        var inside = new List<GridPosition>
        {
            new(400, 501, 601),
            new(402, 501, 601),
            new(401, 500, 601),
            new(401, 502, 601),
            new(401, 501, 600),
            new(401, 501, 602)
        };

        // Points just outside each face.
        var outside = new List<GridPosition>
        {
            new(399, 501, 601),
            new(403, 501, 601),
            new(401, 499, 601),
            new(401, 503, 601),
            new(401, 501, 599),
            new(401, 501, 603)
        };

        foreach (var segment in inside) SetFlags(segment, WorldSegmentFlag.Pvp);
        foreach (var segment in outside) SetFlags(segment, WorldSegmentFlag.Pvp);

        var result = GetFlagsInRange(min, max);

        foreach (var segment in inside) Assert.Contains(segment, result.Keys);
        foreach (var segment in outside) Assert.DoesNotContain(segment, result.Keys);
    }

    [Fact]
    public void GetFlagsInRange_EmptyBox_ReturnsEmpty()
    {
        var result = GetFlagsInRange(new GridPosition(900, 900, 900), new GridPosition(901, 901, 901));
        Assert.Empty(result);
    }

    /// <summary>
    ///     Sets the flags for a segment within its own transaction.
    /// </summary>
    /// <param name="segment">World segment index.</param>
    /// <param name="flags">Flags to store.</param>
    private void SetFlags(GridPosition segment, WorldSegmentFlag flags)
    {
        var query = new SqliteSetSegmentFlagsQuery(fixture.Connection);
        using var transaction = fixture.Connection.BeginTransaction();
        query.SetFlags(segment, flags, transaction);
        transaction.Commit();
    }

    /// <summary>
    ///     Gets the stored flags for a segment.
    /// </summary>
    /// <param name="segment">World segment index.</param>
    /// <returns>Stored flags.</returns>
    private WorldSegmentFlag GetFlags(GridPosition segment)
    {
        var query = new SqliteGetSegmentFlagsQuery(fixture.Connection);
        return query.GetFlags(segment);
    }

    /// <summary>
    ///     Gets the stored flags over a box.
    /// </summary>
    /// <param name="min">Minimum (inclusive) corner.</param>
    /// <param name="max">Maximum (inclusive) corner.</param>
    /// <returns>Stored flags keyed by segment.</returns>
    private IDictionary<GridPosition, WorldSegmentFlag> GetFlagsInRange(GridPosition min, GridPosition max)
    {
        var query = new SqliteGetSegmentFlagsInRangeQuery(fixture.Connection);
        return query.GetFlagsInRange(min, max);
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
