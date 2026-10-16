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
using System.Numerics;
using System.Text.Json;
using Sovereign.EngineCore.Network;
using Sovereign.EngineCore.Systems.Data;
using Sovereign.NetworkCore.Network.Rest.Data;
using Xunit;

namespace TestClientCore.Network;

/// <summary>
///     Tests for the entity data transport DTO round-trip.
/// </summary>
public class TestDataValueDto
{
    [Theory]
    [InlineData(DataValueType.String)]
    [InlineData(DataValueType.Int)]
    [InlineData(DataValueType.Float)]
    [InlineData(DataValueType.Bool)]
    [InlineData(DataValueType.Vector3)]
    [InlineData(DataValueType.EntityId)]
    public void RoundTrip_AllTypes(DataValueType type)
    {
        var value = CreateValue(type);

        var dto = DataValueDto.FromDataValue(value);
        Assert.Equal(type.ToString(), dto.Type);

        Assert.True(dto.TryToDataValue(out var decoded));
        Assert.Equal(value, decoded);
    }

    [Theory]
    [InlineData("String", DataValueType.String)]
    [InlineData("int", DataValueType.Int)]
    [InlineData("FLOAT", DataValueType.Float)]
    [InlineData("bool", DataValueType.Bool)]
    public void TryToDataValue_TypeNameIsCaseInsensitive(string type, DataValueType expected)
    {
        var dto = new DataValueDto { Type = type, Value = CreateValue(expected).ToString() };

        Assert.True(dto.TryToDataValue(out var value));
        Assert.Equal(expected, value.Type);
    }

    [Theory]
    [InlineData("Nope")]
    [InlineData("")]
    [InlineData("7")]
    public void TryToDataValue_UnknownTypeNameFails(string type)
    {
        var dto = new DataValueDto { Type = type, Value = "1" };

        Assert.False(dto.TryToDataValue(out _));
    }

    [Fact]
    public void TryToDataValue_MalformedValueFails()
    {
        var dto = new DataValueDto { Type = "Int", Value = "not-a-number" };

        Assert.False(dto.TryToDataValue(out _));
    }

    [Fact]
    public void JsonRoundTrip_UsesLowerCaseFieldNames()
    {
        var response = new EntityDataResponse
        {
            EntityData = new Dictionary<string, DataValueDto>
            {
                ["k"] = DataValueDto.FromDataValue(DataValue.FromInt(7))
            }
        };

        var json = JsonSerializer.Serialize(response, MessageConfig.JsonOptions);
        Assert.Contains("\"type\":\"Int\"", json);
        Assert.Contains("\"value\":\"7\"", json);

        var decoded = JsonSerializer.Deserialize<EntityDataResponse>(json, MessageConfig.JsonOptions);
        Assert.NotNull(decoded);
        Assert.True(decoded!.EntityData["k"].TryToDataValue(out var value));
        Assert.Equal(7, value.AsInt());
    }

    /// <summary>
    ///     Creates a representative value for the given type.
    /// </summary>
    /// <param name="type">Type tag.</param>
    /// <returns>Value.</returns>
    private static DataValue CreateValue(DataValueType type)
    {
        return type switch
        {
            DataValueType.String => DataValue.FromString("hello, world"),
            DataValueType.Int => DataValue.FromInt(-42),
            DataValueType.Float => DataValue.FromFloat(3.5),
            DataValueType.Bool => DataValue.FromBool(true),
            DataValueType.Vector3 => DataValue.FromVector3(new Vector3(1.0f, 2.0f, 3.0f)),
            DataValueType.EntityId => DataValue.FromEntityId(0x1234567890),
            _ => DataValue.FromString(string.Empty)
        };
    }
}
