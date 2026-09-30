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
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Entities;
using Xunit;

namespace Sovereign.EngineCore.Components.Indexers;

/// <summary>
///     Unit tests for TemplateNameComponentIndexer, covering case-insensitive lookup,
///     enumeration of all indexed templates, duplicate names, and rename/remove
///     semantics.
/// </summary>
public class TestTemplateNameComponentIndexer
{
    private static readonly ulong BaseId = EntityConstants.FirstTemplateEntityId;

    private readonly NameComponentCollection names;
    private readonly TemplateNameComponentIndexer indexer;

    public TestTemplateNameComponentIndexer()
    {
        var entityTable = new EntityTable();
        var notifier = new EntityNotifier();
        var componentManager = new ComponentManager(notifier);
        names = new NameComponentCollection(entityTable, componentManager);
        var filter = new TemplateNameComponentFilter(names);
        indexer = new TemplateNameComponentIndexer(names, filter);
    }

    private void AddName(ulong entityId, string name)
    {
        names.AddComponent(entityId, name);
        names.ApplyComponentUpdates();
    }

    private void RemoveEntity(ulong entityId)
    {
        names.RemoveComponent(entityId);
        names.ApplyComponentUpdates();
    }

    [Fact]
    public void TryGetByName_MatchesCaseInsensitively()
    {
        AddName(BaseId, "Oak Tree");

        Assert.True(indexer.TryGetByName("Oak Tree", out var id1));
        Assert.Equal(BaseId, id1);

        Assert.True(indexer.TryGetByName("oak tree", out var id2));
        Assert.Equal(BaseId, id2);

        Assert.True(indexer.TryGetByName("OAK TREE", out var id3));
        Assert.Equal(BaseId, id3);
    }

    [Fact]
    public void TryGetByName_ReturnsFalseForUnknown()
    {
        Assert.False(indexer.TryGetByName("Nothing", out _));
    }

    [Fact]
    public void Index_IgnoresNonTemplateEntities()
    {
        AddName(BaseId, "Shale");
        AddName(EntityConstants.FirstPersistedEntityId, "NotATemplate");

        Assert.True(indexer.TryGetByName("Shale", out _));
        Assert.False(indexer.TryGetByName("NotATemplate", out _));
        Assert.Single(indexer.EntitiesByName);
    }

    [Fact]
    public void EntitiesByName_EnumeratesBlockAndNonBlockTemplates()
    {
        AddName(BaseId, "Shale");
        AddName(BaseId + 1, "Oak Tree");

        var templates = indexer.EntitiesByName;
        Assert.Equal(2, templates.Count);
        Assert.Equal(BaseId, templates["Shale"]);
        Assert.Equal(BaseId + 1, templates["Oak Tree"]);
    }

    [Fact]
    public void Add_DuplicateName_MapsToLatestEntity()
    {
        AddName(BaseId, "Sword");
        AddName(BaseId + 1, "Sword");

        Assert.Single(indexer.EntitiesByName);
        Assert.True(indexer.TryGetByName("Sword", out var id));
        Assert.Equal(BaseId + 1, id);
    }

    [Fact]
    public void ModifyComponent_RenamesEntry()
    {
        AddName(BaseId, "Shale");

        AddName(BaseId, "Granite");

        Assert.False(indexer.TryGetByName("Shale", out _));
        Assert.True(indexer.TryGetByName("Granite", out var id));
        Assert.Equal(BaseId, id);
    }

    [Fact]
    public void RemoveComponent_RemovesIndexedName()
    {
        AddName(BaseId, "Shale");
        AddName(BaseId + 1, "Granite");

        RemoveEntity(BaseId);

        Assert.False(indexer.TryGetByName("Shale", out _));
        Assert.True(indexer.TryGetByName("Granite", out _));
        Assert.Single(indexer.EntitiesByName);
    }
}
