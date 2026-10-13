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

using Moq;
using Sovereign.EngineCore.Components.Types;
using Sovereign.Persistence.Database;
using Sovereign.Persistence.Database.Queries;
using Sovereign.Persistence.Systems.Persistence;
using Sovereign.ServerCore.Systems.WorldManagement;
using Xunit;

namespace TestPersistence.Systems.Persistence;

/// <summary>
///     Tests for SegmentFlagRetriever, in particular the fold of pending updates into
///     segment flags loaded from a possibly stale database.
/// </summary>
public class TestSegmentFlagRetriever
{
    private readonly SegmentFlagManager manager = new();
    private readonly PendingSegmentFlagUpdates pending = new();
    private readonly StubGetSegmentFlagsQuery query = new();
    private readonly SegmentFlagRetriever retriever;
    private readonly Mock<IPersistenceProvider> provider = new();

    public TestSegmentFlagRetriever()
    {
        provider.SetupGet(p => p.GetSegmentFlagsQuery).Returns(query);
        retriever = new SegmentFlagRetriever(manager, pending);
    }

    [Fact]
    public void RetrieveFlags_NoPending_StoresDatabaseValue()
    {
        var segment = new GridPosition(1, 1, 1);
        query.Flags = WorldSegmentFlag.Pvp;

        retriever.RetrieveFlags(provider.Object, segment);

        Assert.Equal(WorldSegmentFlag.Pvp, manager.GetFlags(segment));
    }

    [Fact]
    public void RetrieveFlags_NoPending_MissingRow_StoresNone()
    {
        var segment = new GridPosition(2, 2, 2);
        query.Flags = WorldSegmentFlag.None;

        retriever.RetrieveFlags(provider.Object, segment);

        Assert.Equal(WorldSegmentFlag.None, manager.GetFlags(segment));
    }

    [Fact]
    public void RetrieveFlags_PendingSet_FoldsOverStaleDatabaseValue()
    {
        var segment = new GridPosition(3, 3, 3);
        query.Flags = WorldSegmentFlag.None;

        // Event path: cache flagged and a pending delta queued before the database write.
        manager.SetFlagsInCache(segment, WorldSegmentFlag.Pvp);
        pending.Enqueue(segment, segment, WorldSegmentFlag.Pvp, true);

        retriever.RetrieveFlags(provider.Object, segment);

        Assert.Equal(WorldSegmentFlag.Pvp, manager.GetFlags(segment));
    }

    [Fact]
    public void RetrieveFlags_PendingClear_FoldsOverStaleDatabaseValue()
    {
        var segment = new GridPosition(4, 4, 4);
        query.Flags = WorldSegmentFlag.Pvp;

        pending.Enqueue(segment, segment, WorldSegmentFlag.Pvp, false);

        retriever.RetrieveFlags(provider.Object, segment);

        Assert.Equal(WorldSegmentFlag.None, manager.GetFlags(segment));
    }

    [Fact]
    public void RetrieveFlags_PendingUpdateForOtherSegment_IsNotFolded()
    {
        var segment = new GridPosition(5, 5, 5);
        query.Flags = WorldSegmentFlag.None;
        pending.Enqueue(new GridPosition(6, 6, 6), new GridPosition(6, 6, 6), WorldSegmentFlag.Pvp, true);

        retriever.RetrieveFlags(provider.Object, segment);

        Assert.Equal(WorldSegmentFlag.None, manager.GetFlags(segment));
    }

    /// <summary>
    ///     Stub query that always reports the configured flags.
    /// </summary>
    private sealed class StubGetSegmentFlagsQuery : IGetSegmentFlagsQuery
    {
        /// <summary>
        ///     Flags reported by the query.
        /// </summary>
        public WorldSegmentFlag Flags;

        public WorldSegmentFlag GetFlags(GridPosition segmentIndex)
        {
            return Flags;
        }
    }
}
