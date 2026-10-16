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
using Moq;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Main;
using Sovereign.EngineCore.Systems.Data;
using Sovereign.Persistence.Database;
using Sovereign.Persistence.Database.Queries;
using Sovereign.Persistence.Systems.Persistence;
using Sovereign.ServerCore.Systems.WorldManagement;
using Xunit;

namespace TestPersistence.Systems.Persistence;

/// <summary>
///     Tests for SegmentKeyValueRetriever, in particular the fold of pending updates into
///     segment key-value data loaded from a possibly stale database.
/// </summary>
public class TestSegmentKeyValueRetriever
{
    private readonly SegmentKeyValueManager manager = new();
    private readonly PendingSegmentKeyValueUpdates pending = new();
    private readonly Mock<IPersistenceProvider> provider = new();
    private readonly StubGetSegmentKeyValuesQuery query = new();
    private readonly SegmentKeyValueRetriever retriever;

    public TestSegmentKeyValueRetriever()
    {
        provider.SetupGet(p => p.GetSegmentKeyValuesQuery).Returns(query);
        retriever = new SegmentKeyValueRetriever(manager, pending);
    }

    [Fact]
    public void RetrieveKeyValues_NoPending_StoresDatabaseValue()
    {
        var segment = new GridPosition(1, 1, 1);
        query.Rows.Add(Row("Owner", DataValue.FromEntityId(0x1234)));

        retriever.RetrieveKeyValues(provider.Object, segment);

        Assert.True(manager.TryGetValue(segment, "Owner", out var value));
        Assert.Equal(DataValueType.EntityId, value.Type);
        Assert.Equal(0x1234UL, value.AsEntityId());
        Assert.True(manager.IsLoaded(segment));
    }

    [Fact]
    public void RetrieveKeyValues_NoRows_MarksSegmentLoadedWithNoValues()
    {
        var segment = new GridPosition(2, 2, 2);

        retriever.RetrieveKeyValues(provider.Object, segment);

        Assert.True(manager.IsLoaded(segment));
        Assert.False(manager.TryGetValue(segment, "Owner", out _));
    }

    [Fact]
    public void RetrieveKeyValues_PendingSet_FoldsOverStaleDatabaseValue()
    {
        // Load-bearing race: the writer cached a new value and queued a delta, but the database
        // still holds the old value when the segment reloads.
        var segment = new GridPosition(3, 3, 3);
        query.Rows.Add(Row("Owner", DataValue.FromEntityId(0x1)));
        pending.Enqueue(segment, "Owner", DataValue.FromEntityId(0x2));

        retriever.RetrieveKeyValues(provider.Object, segment);

        Assert.True(manager.TryGetValue(segment, "Owner", out var value));
        Assert.Equal(0x2UL, value.AsEntityId());
    }

    [Fact]
    public void RetrieveKeyValues_PendingRemove_FoldsOverStaleDatabaseValue()
    {
        var segment = new GridPosition(4, 4, 4);
        query.Rows.Add(Row("Owner", DataValue.FromEntityId(0x1)));
        pending.Enqueue(segment, "Owner", null);

        retriever.RetrieveKeyValues(provider.Object, segment);

        Assert.False(manager.TryGetValue(segment, "Owner", out _));
    }

    [Fact]
    public void RetrieveKeyValues_PendingUpdateForOtherSegment_IsNotFolded()
    {
        var segment = new GridPosition(5, 5, 5);
        query.Rows.Add(Row("Owner", DataValue.FromEntityId(0x1)));
        pending.Enqueue(new GridPosition(6, 6, 6), "Owner", DataValue.FromEntityId(0x2));

        retriever.RetrieveKeyValues(provider.Object, segment);

        Assert.True(manager.TryGetValue(segment, "Owner", out var value));
        Assert.Equal(0x1UL, value.AsEntityId());
    }

    [Fact]
    public void RetrieveKeyValues_DeactivatedBeforeDrain_StillFoldsPendingDelta()
    {
        // The card's load-bearing race: a write is cached and queued, the segment deactivates
        // (cache dropped) before the persister drains the delta, and the reactivation loads the
        // still-stale database value. The pending delta must survive the deactivation and be
        // folded over the freshly loaded rows.
        var segment = new GridPosition(8, 8, 8);
        query.Rows.Add(Row("Owner", DataValue.FromEntityId(0x1)));

        manager.SetValue(segment, "Owner", DataValue.FromEntityId(0x2));
        pending.Enqueue(segment, "Owner", DataValue.FromEntityId(0x2));

        manager.DropSegment(segment);
        Assert.False(manager.IsLoaded(segment));

        retriever.RetrieveKeyValues(provider.Object, segment);

        Assert.True(manager.TryGetValue(segment, "Owner", out var value));
        Assert.Equal(0x2UL, value.AsEntityId());
    }

    [Fact]
    public void RetrieveKeyValues_MalformedRow_ThrowsFatalError()
    {
        var segment = new GridPosition(7, 7, 7);
        query.Rows.Add(new SegmentKeyValueRow("Owner", "not-an-entity-id", (int)DataValueType.EntityId));

        Assert.Throws<FatalErrorException>(() => retriever.RetrieveKeyValues(provider.Object, segment));
    }

    /// <summary>
    ///     Builds a raw row from a typed value.
    /// </summary>
    /// <param name="key">Key.</param>
    /// <param name="value">Value.</param>
    /// <returns>Raw row.</returns>
    private static SegmentKeyValueRow Row(string key, DataValue value)
    {
        return new SegmentKeyValueRow(key, DataValueCodec.Encode(value), (int)value.Type);
    }

    /// <summary>
    ///     Stub query that returns the configured rows.
    /// </summary>
    private sealed class StubGetSegmentKeyValuesQuery : IGetSegmentKeyValuesQuery
    {
        /// <summary>
        ///     Rows returned by the query.
        /// </summary>
        public List<SegmentKeyValueRow> Rows { get; } = new();

        public IReadOnlyList<SegmentKeyValueRow> GetKeyValues(GridPosition segmentIndex)
        {
            return Rows;
        }
    }
}
