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

using System.Collections.Generic;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Entities;
using Xunit;

namespace Sovereign.EngineCore.Components.Indexers;

/// <summary>
///     Unit tests for PlayerNameComponentIndexer, covering case-insensitive
///     exact lookup and fuzzy lookup.
/// </summary>
public class TestPlayerNameComponentIndexer
{
    private readonly EntityTable entityTable;
    private readonly PlayerCharacterTagCollection playerTags;
    private readonly NameComponentCollection names;
    private readonly PlayerNameComponentIndexer indexer;

    public TestPlayerNameComponentIndexer()
    {
        entityTable = new EntityTable();
        var notifier = new EntityNotifier();
        var componentManager = new ComponentManager(notifier);
        playerTags = new PlayerCharacterTagCollection(entityTable, componentManager);
        names = new NameComponentCollection(entityTable, componentManager);
        var filter = new PlayerNameEventFilter(playerTags, names);
        indexer = new PlayerNameComponentIndexer(names, filter);
    }

    private void AddPlayer(ulong entityId, string name)
    {
        playerTags.TagEntity(entityId);
        playerTags.ApplyComponentUpdates();
        names.AddComponent(entityId, name);
        names.ApplyComponentUpdates();
    }

    private void RemovePlayer(ulong entityId)
    {
        names.RemoveComponent(entityId);
        names.ApplyComponentUpdates();
    }

    [Fact]
    public void TryGetPlayerByName_MatchesCaseInsensitively()
    {
        const ulong entityId = 1;
        AddPlayer(entityId, "Alice");

        Assert.True(indexer.TryGetPlayerByName("Alice", out var id1));
        Assert.Equal(entityId, id1);
        Assert.True(indexer.TryGetPlayerByName("alice", out var id2));
        Assert.Equal(entityId, id2);
        Assert.True(indexer.TryGetPlayerByName("ALICE", out var id3));
        Assert.Equal(entityId, id3);
    }

    [Fact]
    public void TryGetPlayerByName_ReturnsFalseForUnknown()
    {
        Assert.False(indexer.TryGetPlayerByName("Bob", out _));
    }

    [Fact]
    public void RemoveComponent_RemovesIndexEntry()
    {
        const ulong entityId = 2;
        AddPlayer(entityId, "Alice");
        Assert.True(indexer.TryGetPlayerByName("alice", out _));

        RemovePlayer(entityId);
        Assert.False(indexer.TryGetPlayerByName("alice", out _));
    }

    [Fact]
    public void AppendFuzzyMatches_AppendsBestFirst_PreservesExisting()
    {
        const ulong aliceId = 10;
        const ulong aliciaId = 12;
        const ulong bobId = 11;
        AddPlayer(aliceId, "Alice");
        AddPlayer(aliciaId, "Alicia");
        AddPlayer(bobId, "Bob");

        var results = new List<(string Name, float Score)> { ("preexisting", 1.0f) };
        indexer.AppendFuzzyMatches("Alise", 5, results);

        // Pre-existing entry preserved; Bob has zero similarity and is excluded.
        Assert.Equal(3, results.Count);
        Assert.Equal("preexisting", results[0].Name);

        // Best match first ("Alice", high score).
        Assert.Equal("Alice", results[1].Name);
        Assert.True(results[1].Score > 0.7f);

        // Remaining entries are best-first by descending score.
        Assert.Equal("Alicia", results[2].Name);
        Assert.True(results[1].Score >= results[2].Score);
    }

    [Fact]
    public void AppendFuzzyMatches_NoCandidates_AppendsNothingButPreservesExisting()
    {
        var results = new List<(string Name, float Score)> { ("preexisting", 1.0f) };
        indexer.AppendFuzzyMatches("nobody", 5, results);

        var single = Assert.Single(results);
        Assert.Equal("preexisting", single.Name);
    }
}
