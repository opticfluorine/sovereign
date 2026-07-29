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
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;
using Xunit;

namespace Sovereign.EngineCore.Components.Indexers;

/// <summary>
///     Unit tests for ItemTemplateNameComponentIndexer, covering
///     case-insensitive exact lookup, duplicate-name support, rename/remove
///     semantics, outer-entry cleanup, and fuzzy lookup.
/// </summary>
public class TestItemTemplateNameComponentIndexer
{
    private static readonly ulong BaseId = EntityConstants.FirstTemplateEntityId;

    private readonly EntityTable entityTable;
    private readonly EntityTypeComponentCollection entityTypes;
    private readonly NameComponentCollection names;
    private readonly ItemTemplateNameComponentIndexer indexer;

    public TestItemTemplateNameComponentIndexer()
    {
        entityTable = new EntityTable();
        var notifier = new EntityNotifier();
        var componentManager = new ComponentManager(notifier);
        entityTypes = new EntityTypeComponentCollection(entityTable, componentManager);
        names = new NameComponentCollection(entityTable, componentManager);
        var filter = new ItemTemplateNameComponentFilter(entityTypes, names);
        indexer = new ItemTemplateNameComponentIndexer(names, filter);
    }

    private void AddItem(ulong entityId, string name)
    {
        entityTypes.AddComponent(entityId, EntityType.Item);
        entityTypes.ApplyComponentUpdates();
        names.AddComponent(entityId, name);
        names.ApplyComponentUpdates();
    }

    private void RenameItem(ulong entityId, string newName)
    {
        names.AddComponent(entityId, newName);
        names.ApplyComponentUpdates();
    }

    private void RemoveItem(ulong entityId)
    {
        names.RemoveComponent(entityId);
        names.ApplyComponentUpdates();
    }

    [Fact]
    public void TryGetByName_MatchesCaseInsensitively()
    {
        var entityId = BaseId;
        AddItem(entityId, "Sword");

        Assert.True(indexer.TryGetByName("Sword", out var ids1));
        var single1 = Assert.Single(ids1);
        Assert.Equal(entityId, single1);

        Assert.True(indexer.TryGetByName("sword", out var ids2));
        Assert.Equal(entityId, Assert.Single(ids2));

        Assert.True(indexer.TryGetByName("SWORD", out var ids3));
        Assert.Equal(entityId, Assert.Single(ids3));
    }

    [Fact]
    public void TryGetByName_ReturnsFalseForUnknown()
    {
        Assert.False(indexer.TryGetByName("Nothing", out var ids));
        Assert.Empty(ids);
    }

    [Fact]
    public void TryGetByName_ReturnsAllEntitiesForDuplicateNames()
    {
        var id1 = BaseId;
        var id2 = BaseId + 1;
        AddItem(id1, "Sword");
        AddItem(id2, "Sword");

        Assert.True(indexer.TryGetByName("sword", out var ids));
        var set = new HashSet<ulong>(ids);
        Assert.Equal(2, set.Count);
        Assert.Contains(id1, set);
        Assert.Contains(id2, set);
    }

    [Fact]
    public void RemoveComponent_RemovesEntity_KeepsOthersForSameName()
    {
        var id1 = BaseId;
        var id2 = BaseId + 1;
        AddItem(id1, "Sword");
        AddItem(id2, "Sword");

        RemoveItem(id1);

        Assert.True(indexer.TryGetByName("sword", out var after1));
        Assert.Equal(id2, Assert.Single(after1));

        RemoveItem(id2);

        // Outer entry should be cleaned up once no entities remain.
        Assert.False(indexer.TryGetByName("sword", out var after2));
        Assert.Empty(after2);
    }

    [Fact]
    public void ModifyComponent_RenamesEntry()
    {
        var id = BaseId;
        AddItem(id, "Sword");

        RenameItem(id, "Axe");

        Assert.False(indexer.TryGetByName("sword", out _));
        Assert.True(indexer.TryGetByName("Axe", out var ids));
        Assert.Equal(id, Assert.Single(ids));
    }

    [Fact]
    public void ModifyComponent_FromDuplicateName_LeavesOtherIntact()
    {
        var id1 = BaseId;
        var id2 = BaseId + 1;
        AddItem(id1, "Sword");
        AddItem(id2, "Sword");

        RenameItem(id1, "Axe");

        Assert.True(indexer.TryGetByName("sword", out var swordIds));
        Assert.Equal(id2, Assert.Single(swordIds));

        Assert.True(indexer.TryGetByName("Axe", out var axeIds));
        Assert.Equal(id1, Assert.Single(axeIds));
    }

    [Fact]
    public void AppendFuzzyMatches_AppendsBestFirst_PreservesExisting()
    {
        AddItem(BaseId, "Sword");
        AddItem(BaseId + 1, "Swords");
        AddItem(BaseId + 2, "Axe");

        var results = new List<(string Name, float Score)> { ("preexisting", 1.0f) };
        indexer.AppendFuzzyMatches("Sowrd", 5, results);

        // Pre-existing entry preserved; "Axe" shares no characters with the
        // query and thus has zero similarity, so it is excluded.
        Assert.Equal(3, results.Count);
        Assert.Equal("preexisting", results[0].Name);
        Assert.Equal("Sword", results[1].Name);
        Assert.True(results[1].Score > 0.7f);
        Assert.Equal("Swords", results[2].Name);
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

    [Fact]
    public void AppendFuzzyMatches_DuplicateNamesProduceSingleNameEntry()
    {
        AddItem(BaseId, "Sword");
        AddItem(BaseId + 1, "Sword");

        var results = new List<(string Name, float Score)>();
        indexer.AppendFuzzyMatches("Sword", 5, results);

        // One entry per distinct name, not per entity.
        var single = Assert.Single(results);
        Assert.Equal("Sword", single.Name);
        Assert.Equal(1.0f, single.Score);
    }
}
