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

using Sovereign.EngineCore.Entities;
using Xunit;

namespace Sovereign.EngineCore.Components;

public class TestBaseComponentCollection
{
    private readonly BaseComponentCollection<int> collection;

    public TestBaseComponentCollection()
    {
        var entityTable = new EntityTable();
        var entityNotifier = new EntityNotifier();
        var componentManager = new ComponentManager(entityNotifier);
        collection =
            new DummyComponentCollection<int>(entityTable, componentManager, 10, ComponentOperators.IntOperators,
                ComponentType.Material);
    }

    [Fact]
    public void AddComponent_AddsComponentSuccessfully()
    {
        ulong entityId = 1;
        var initialValue = 42;

        collection.AddComponent(entityId, initialValue);
        collection.ApplyComponentUpdates();

        Assert.True(collection.HasComponentForEntity(entityId));
        Assert.Equal(initialValue, collection[entityId]);
    }

    [Fact]
    public void ModifyComponent_MultipliesComponentSuccessfully()
    {
        ulong entityId = 1;
        var initialValue = 42;
        var multiplyValue = 2;
        var expectedValue = initialValue * multiplyValue;

        collection.AddComponent(entityId, initialValue);
        collection.ApplyComponentUpdates();
        collection.ModifyComponent(entityId, ComponentOperation.Multiply, multiplyValue);
        collection.ApplyComponentUpdates();

        Assert.Equal(expectedValue, collection[entityId]);
    }

    [Fact]
    public void ModifyComponent_DividesComponentSuccessfully()
    {
        ulong entityId = 1;
        var initialValue = 42;
        var divideValue = 2;
        var expectedValue = initialValue / divideValue;

        collection.AddComponent(entityId, initialValue);
        collection.ApplyComponentUpdates();
        collection.ModifyComponent(entityId, ComponentOperation.Divide, divideValue);
        collection.ApplyComponentUpdates();

        Assert.Equal(expectedValue, collection[entityId]);
    }

    [Fact]
    public void RemoveComponent_RemovesComponentSuccessfully()
    {
        ulong entityId = 1;
        var initialValue = 42;

        collection.AddComponent(entityId, initialValue);
        collection.ApplyComponentUpdates();
        collection.RemoveComponent(entityId);
        collection.ApplyComponentUpdates();

        Assert.False(collection.HasComponentForEntity(entityId));
    }

    [Fact]
    public void ModifyComponent_SetsComponentSuccessfully()
    {
        ulong entityId = 1;
        var initialValue = 42;
        var newValue = 84;

        collection.AddComponent(entityId, initialValue);
        collection.ApplyComponentUpdates();
        collection.ModifyComponent(entityId, ComponentOperation.Set, newValue);
        collection.ApplyComponentUpdates();

        Assert.Equal(newValue, collection[entityId]);
    }

    [Fact]
    public void ModifyComponent_AddsToComponentSuccessfully()
    {
        ulong entityId = 1;
        var initialValue = 42;
        var addValue = 10;
        var expectedValue = initialValue + addValue;

        collection.AddComponent(entityId, initialValue);
        collection.ApplyComponentUpdates();
        collection.ModifyComponent(entityId, ComponentOperation.Add, addValue);
        collection.ApplyComponentUpdates();

        Assert.Equal(expectedValue, collection[entityId]);
    }

    [Fact]
    public void AddOrUpdateComponent_AddsOrUpdatesComponentSuccessfully()
    {
        ulong entityId = 1;
        var initialValue = 42;
        var newValue = 84;

        collection.AddOrUpdateComponent(entityId, initialValue);
        collection.ApplyComponentUpdates();
        Assert.Equal(initialValue, collection[entityId]);

        collection.AddOrUpdateComponent(entityId, newValue);
        collection.ApplyComponentUpdates();
        Assert.Equal(newValue, collection[entityId]);
    }

    [Fact]
    public void GetComponentForEntity_ReturnsComponentIfExists()
    {
        ulong entityId = 1;
        var initialValue = 42;

        collection.AddComponent(entityId, initialValue);
        collection.ApplyComponentUpdates();

        var maybeComponent = collection.GetComponentForEntity(entityId);
        Assert.True(maybeComponent.HasValue);
        Assert.Equal(initialValue, maybeComponent.Value);
    }

    [Fact]
    public void GetComponentForEntity_ReturnsNoneIfComponentDoesNotExist()
    {
        ulong entityId = 1;

        var maybeComponent = collection.GetComponentForEntity(entityId);
        Assert.False(maybeComponent.HasValue);
    }

    [Fact]
    public void TryGetValue_ReturnsTrueIfComponentExists()
    {
        ulong entityId = 1;
        var initialValue = 42;

        collection.AddComponent(entityId, initialValue);
        collection.ApplyComponentUpdates();

        var result = collection.TryGetValue(entityId, out var value);
        Assert.True(result);
        Assert.Equal(initialValue, value);
    }

    [Fact]
    public void TryGetValue_ReturnsFalseIfComponentDoesNotExist()
    {
        ulong entityId = 1;

        var result = collection.TryGetValue(entityId, out var value);
        Assert.False(result);
        Assert.Equal(0, value);
    }

    [Fact]
    public void GetComponentWithLookback_ReturnsComponentIfExists()
    {
        ulong entityId = 1;
        var initialValue = 42;

        collection.AddComponent(entityId, initialValue);
        collection.ApplyComponentUpdates();

        var component = collection.GetComponentWithLookback(entityId);
        Assert.Equal(initialValue, component);
    }

    [Fact]
    public void GetComponentWithLookback_ThrowsIfComponentDoesNotExist()
    {
        ulong entityId = 1;

        Assert.Throws<KeyNotFoundException>(() => collection.GetComponentWithLookback(entityId));
    }

    [Fact]
    public void GetComponentWithLookback_ReturnsComponentAfterRemoveAndSingleUpdate()
    {
        ulong entityId = 1;
        var initialValue = 42;

        collection.AddComponent(entityId, initialValue);
        collection.ApplyComponentUpdates();
        collection.RemoveComponent(entityId);
        collection.ApplyComponentUpdates();

        var component = collection.GetComponentWithLookback(entityId);
        Assert.Equal(initialValue, component);
    }

    [Fact]
    public void GetComponentWithLookback_ThrowsAfterRemoveAndDoubleUpdate()
    {
        ulong entityId = 1;
        var initialValue = 42;

        collection.AddComponent(entityId, initialValue);
        collection.ApplyComponentUpdates();
        collection.RemoveComponent(entityId);
        collection.ApplyComponentUpdates();
        collection.ApplyComponentUpdates();

        Assert.Throws<KeyNotFoundException>(() => collection.GetComponentWithLookback(entityId));
    }

    [Fact]
    public void GetAllComponents_ReturnsAllComponents()
    {
        ulong entityId1 = 1;
        ulong entityId2 = 2;
        var initialValue1 = 42;
        var initialValue2 = 84;

        collection.AddComponent(entityId1, initialValue1);
        collection.AddComponent(entityId2, initialValue2);
        collection.ApplyComponentUpdates();

        var allComponents = collection.GetAllComponents();

        Assert.Equal(2, allComponents.Count);
        Assert.Equal(initialValue1, allComponents[entityId1]);
        Assert.Equal(initialValue2, allComponents[entityId2]);
    }

    [Fact]
    public void TryFindNearest_ReturnsChildComponentIfNearest()
    {
        ulong parentEntityId = 1;
        ulong childEntityId = 2;
        var parentValue = 84;
        var childValue = 42;

        collection.AddComponent(parentEntityId, parentValue);
        collection.AddComponent(childEntityId, childValue);
        collection.ApplyComponentUpdates();

        var parentCollection = new DummyComponentCollection<ulong>(new EntityTable(),
            new ComponentManager(new EntityNotifier()), 10, ComponentOperators.UlongOperators, ComponentType.Material);
        parentCollection.AddComponent(childEntityId, parentEntityId);
        parentCollection.ApplyComponentUpdates();

        var result = collection.TryFindNearest(childEntityId, parentCollection, out var nearestValue,
            out var owningEntityId);

        Assert.True(result);
        Assert.Equal(childValue, nearestValue);
        Assert.Equal(childEntityId, owningEntityId);
    }
    
    [Fact]
    public void TryFindNearest_ReturnsParentComponentIfNearest()
    {
        ulong parentEntityId = 1;
        ulong childEntityId = 2;
        var parentValue = 42;

        collection.AddComponent(parentEntityId, parentValue);
        collection.ApplyComponentUpdates();

        var parentCollection = new DummyComponentCollection<ulong>(new EntityTable(),
            new ComponentManager(new EntityNotifier()), 10, ComponentOperators.UlongOperators, ComponentType.Material);
        parentCollection.AddComponent(childEntityId, parentEntityId);
        parentCollection.ApplyComponentUpdates();

        var result = collection.TryFindNearest(childEntityId, parentCollection, out var nearestValue,
            out var owningEntityId);

        Assert.True(result);
        Assert.Equal(parentValue, nearestValue);
        Assert.Equal(parentEntityId, owningEntityId);
    }

    [Fact]
    public void TryFindNearest_ReturnsGrandparentComponentIfNearest()
    {
        ulong grandparentEntityId = 1;
        ulong parentEntityId = 2;
        ulong childEntityId = 3;
        var grandparentValue = 42;

        collection.AddComponent(grandparentEntityId, grandparentValue);
        collection.ApplyComponentUpdates();

        var parentCollection = new DummyComponentCollection<ulong>(new EntityTable(),
            new ComponentManager(new EntityNotifier()), 10, ComponentOperators.UlongOperators, ComponentType.Material);
        parentCollection.AddComponent(parentEntityId, grandparentEntityId);
        parentCollection.AddComponent(childEntityId, parentEntityId);
        parentCollection.ApplyComponentUpdates();

        var result = collection.TryFindNearest(childEntityId, parentCollection, out var nearestValue,
            out var owningEntityId);

        Assert.True(result);
        Assert.Equal(grandparentValue, nearestValue);
        Assert.Equal(grandparentEntityId, owningEntityId);
    }

    [Fact]
    public void TryFindNearest_ReturnsFalseIfNoComponentExists()
    {
        ulong parentEntityId = 1;
        ulong childEntityId = 2;

        var parentCollection = new DummyComponentCollection<ulong>(new EntityTable(),
            new ComponentManager(new EntityNotifier()), 10, ComponentOperators.UlongOperators, ComponentType.Material);
        parentCollection.AddComponent(childEntityId, parentEntityId);
        parentCollection.ApplyComponentUpdates();

        var result = collection.TryFindNearest(childEntityId, parentCollection, out var nearestValue,
            out var owningEntityId);

        Assert.False(result);
        Assert.Equal(default, nearestValue);
        Assert.Equal(0UL, owningEntityId);
    }

    [Fact]
    public void Clear_RemovesAllComponents()
    {
        ulong entityId1 = 1;
        ulong entityId2 = 2;
        var initialValue = 42;

        collection.AddComponent(entityId1, initialValue);
        collection.AddComponent(entityId2, initialValue);
        collection.ApplyComponentUpdates();

        collection.Clear();

        Assert.False(collection.HasComponentForEntity(entityId1));
        Assert.False(collection.HasComponentForEntity(entityId2));
    }

    private class DummyComponentCollection<T> : BaseComponentCollection<T> where T : notnull
    {
        public DummyComponentCollection(EntityTable entityTable, ComponentManager componentManager, int initialCapacity,
            Dictionary<ComponentOperation, Func<T, T, T>> operators, ComponentType componentType) : base(entityTable,
            componentManager, initialCapacity, operators, componentType)
        {
        }
    }
}