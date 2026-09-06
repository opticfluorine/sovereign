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
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Xunit;

namespace Sovereign.EngineCore.Systems.Data;

/// <summary>
///     Unit tests for EntityKeyValueStore.
/// </summary>
public class TestEntityKeyValueStore
{
    private const ulong TemplateId = 0x7ffe000000000001;
    private const ulong EntityId = 0x7fff000000000001;

    private readonly EntityTable entityTable = new();
    private readonly EntityKeyValueStore store;

    public TestEntityKeyValueStore()
    {
        var mockEventSender = new Mock<IEventSender>();
        var internalController = new DataInternalController(mockEventSender.Object);
        store = new EntityKeyValueStore(entityTable, mockEventSender.Object, internalController);

        // Set up a template entity with one instance.
        entityTable.Add(TemplateId, 0, false, false, false);
        entityTable.Add(EntityId, TemplateId, false, false, false);
        entityTable.UpdateAllEntities();
    }

    [Fact]
    public void TryGetValueLocal_ReturnsValueForEntity()
    {
        store.SetValue(EntityId, "MyKey", "MyValue");

        Assert.True(store.TryGetValueLocal(EntityId, "MyKey", out var value));
        Assert.Equal("MyValue", value);
    }

    [Fact]
    public void TryGetValueLocal_ReturnsFalseForMissingKey()
    {
        Assert.False(store.TryGetValueLocal(EntityId, "MyKey", out var value));
        Assert.Null(value);
    }

    [Fact]
    public void TryGetValueLocal_DoesNotFallBackToTemplate()
    {
        store.SetValue(TemplateId, "MyKey", "TemplateValue");

        Assert.False(store.TryGetValueLocal(EntityId, "MyKey", out _));

        // The inheriting lookup still falls back to the template.
        Assert.True(store.TryGetValue(EntityId, "MyKey", out var value));
        Assert.Equal("TemplateValue", value);
    }
}
