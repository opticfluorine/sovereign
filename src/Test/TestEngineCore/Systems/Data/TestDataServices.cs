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

using System;
using System.Numerics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Xunit;

namespace Sovereign.EngineCore.Systems.Data;

/// <summary>
///     Unit tests for typed reads through IDataServices.
/// </summary>
public class TestDataServices
{
    private const ulong TemplateId = 0x7ffe000000000001;
    private const ulong EntityId = 0x7fff000000000001;

    private readonly EntityTable entityTable = new();
    private readonly EntityKeyValueStore entityStore;
    private readonly GlobalKeyValueStore globalStore;
    private readonly Mock<ILogger<DataServices>> logger = new();
    private readonly DataServices services;

    public TestDataServices()
    {
        var mockEventSender = new Mock<IEventSender>();
        var internalController = new DataInternalController(mockEventSender.Object);
        globalStore = new GlobalKeyValueStore(internalController, NullLogger<GlobalKeyValueStore>.Instance);
        entityStore = new EntityKeyValueStore(entityTable, mockEventSender.Object, internalController);
        services = new DataServices(globalStore, entityStore, logger.Object);

        entityTable.Add(TemplateId, 0, false, false, false);
        entityTable.Add(EntityId, TemplateId, false, false, false);
        entityTable.UpdateAllEntities();
    }

    [Fact]
    public void TryGetGlobal_TypedAccessors_ReturnValuesForMatchingTags()
    {
        globalStore.SetGlobal("int", DataValue.FromInt(42));
        globalStore.SetGlobal("bool", DataValue.FromBool(true));
        globalStore.SetGlobal("float", DataValue.FromFloat(1.5));
        globalStore.SetGlobal("vector", DataValue.FromVector3(new Vector3(1, 2, 3)));
        globalStore.SetGlobal("entity", DataValue.FromEntityId(0x7fff000000000042UL));
        globalStore.SetGlobal("string", DataValue.FromString("text"));

        Assert.True(services.TryGetGlobalInt("int", out var intValue));
        Assert.Equal(42, intValue);

        Assert.True(services.TryGetGlobalBool("bool", out var boolValue));
        Assert.True(boolValue);

        Assert.True(services.TryGetGlobalFloat("float", out var floatValue));
        Assert.Equal(1.5f, floatValue);

        Assert.True(services.TryGetGlobalVector3("vector", out var vectorValue));
        Assert.Equal(new Vector3(1, 2, 3), vectorValue);

        Assert.True(services.TryGetGlobalEntityId("entity", out var entityValue));
        Assert.Equal(0x7fff000000000042UL, entityValue);

        Assert.True(services.TryGetGlobal("string", out string? stringValue));
        Assert.Equal("text", stringValue);
    }

    [Fact]
    public void TryGetGlobal_StringGetter_ReturnsCanonicalTextOfTypedValue()
    {
        globalStore.SetGlobal("int", DataValue.FromInt(12345));
        globalStore.SetGlobal("float", DataValue.FromFloat(3.14159));

        Assert.True(services.TryGetGlobal("int", out string? intText));
        Assert.Equal("12345", intText);

        Assert.True(services.TryGetGlobal("float", out string? floatText));
        Assert.Equal("3.14159", floatText);
    }

    [Fact]
    public void TryGetGlobal_TagMismatch_ReturnsFalseAndLogs()
    {
        globalStore.SetGlobal("key", DataValue.FromInt(1));

        Assert.False(services.TryGetGlobalBool("key", out _));
        Assert.False(services.TryGetGlobal("key", DataValueType.Float, out _));

        VerifyErrorLogged(Times.Exactly(2));
    }

    [Fact]
    public void HasGlobal_IsUnaffectedByTypedReads()
    {
        globalStore.SetGlobal("key", DataValue.FromFloat(1.0f));

        Assert.True(services.HasGlobal("key"));
        Assert.False(services.TryGetGlobalInt("key", out _));
        Assert.True(services.HasGlobal("key"));
        Assert.False(services.HasGlobal("missing"));
    }

    [Fact]
    public void TryGetEntityKeyValue_ResolvesTypedValueThroughTemplate()
    {
        entityStore.SetValue(TemplateId, "key", DataValue.FromInt(7));

        Assert.True(services.TryGetEntityKeyValueInt(EntityId, "key", out var value));
        Assert.Equal(7, value);

        // The local lookup must not fall back to the template.
        Assert.False(services.TryGetEntityKeyValueLocal(EntityId, "key", out DataValue _));
    }

    [Fact]
    public void TryGetEntityKeyValue_TagMismatch_ReturnsFalseAndLogs()
    {
        entityStore.SetValue(EntityId, "key", DataValue.FromBool(true));

        Assert.False(services.TryGetEntityKeyValueInt(EntityId, "key", out _));

        VerifyErrorLogged(Times.Once());
    }

    [Fact]
    public void GetEntityData_ReturnsTaggedValues()
    {
        entityStore.SetValue(EntityId, "int", DataValue.FromInt(1));
        entityStore.SetValue(EntityId, "string", DataValue.FromString("value"));

        var data = new System.Collections.Generic.Dictionary<string, DataValue>();
        services.GetEntityData(EntityId, data);

        Assert.Equal(2, data.Count);
        Assert.Equal(DataValueType.Int, data["int"].Type);
        Assert.Equal(DataValueType.String, data["string"].Type);
    }

    /// <summary>
    ///     Verifies the number of error-level log entries.
    /// </summary>
    /// <param name="times">Expected number of entries.</param>
    private void VerifyErrorLogged(Times times)
    {
        logger.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<Microsoft.Extensions.Logging.EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), times);
    }
}
