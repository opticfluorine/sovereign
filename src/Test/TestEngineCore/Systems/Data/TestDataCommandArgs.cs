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

using System.Numerics;
using Xunit;

namespace Sovereign.EngineCore.Systems.Data;

/// <summary>
///     Unit tests for the admin data command argument parser.
/// </summary>
public class TestDataCommandArgs
{
    [Theory]
    [InlineData("String", DataValueType.String)]
    [InlineData("Int", DataValueType.Int)]
    [InlineData("Float", DataValueType.Float)]
    [InlineData("Bool", DataValueType.Bool)]
    [InlineData("Vector3", DataValueType.Vector3)]
    [InlineData("EntityId", DataValueType.EntityId)]
    [InlineData("string", DataValueType.String)]
    [InlineData("INT", DataValueType.Int)]
    public void TryParseType_DefinedNames(string text, DataValueType expected)
    {
        Assert.True(DataCommandArgs.TryParseType(text, out var type));
        Assert.Equal(expected, type);
    }

    [Theory]
    [InlineData("Nope")]
    [InlineData("")]
    [InlineData("7")]
    [InlineData("NotAType")]
    public void TryParseType_UndefinedNames(string text)
    {
        Assert.False(DataCommandArgs.TryParseType(text, out _));
    }

    [Fact]
    public void TryParseSetValue_MissingTypeDefaultsToString()
    {
        Assert.True(DataCommandArgs.TryParseSetValue(new[] { "key", "hello" }, 1, out var value));
        Assert.Equal(DataValueType.String, value.Type);
        Assert.Equal("hello", value.AsString());
    }

    [Fact]
    public void TryParseSetValue_ExplicitType()
    {
        Assert.True(DataCommandArgs.TryParseSetValue(new[] { "key", "42", "Int" }, 1, out var value));
        Assert.Equal(DataValueType.Int, value.Type);
        Assert.Equal(42, value.AsInt());
    }

    [Fact]
    public void TryParseSetValue_Vector3Type()
    {
        Assert.True(DataCommandArgs.TryParseSetValue(new[] { "key", "1,2,3", "Vector3" }, 1, out var value));
        Assert.Equal(DataValueType.Vector3, value.Type);
        Assert.Equal(new Vector3(1.0f, 2.0f, 3.0f), value.AsVector3());
    }

    [Fact]
    public void TryParseSetValue_EntityIdType()
    {
        // EntityId values parse as hexadecimal ("ff" = 255).
        Assert.True(DataCommandArgs.TryParseSetValue(new[] { "key", "ff", "EntityId" }, 1, out var value));
        Assert.Equal(DataValueType.EntityId, value.Type);
        Assert.Equal(255UL, value.AsEntityId());
    }

    [Fact]
    public void TryParseSetValue_BadTypeName()
    {
        Assert.False(DataCommandArgs.TryParseSetValue(new[] { "key", "42", "Nope" }, 1, out _));
    }

    [Fact]
    public void TryParseSetValue_ValueInvalidForType()
    {
        Assert.False(DataCommandArgs.TryParseSetValue(new[] { "key", "hello", "Int" }, 1, out _));
    }

    [Fact]
    public void TryParseSetValue_TooManyArguments()
    {
        Assert.False(DataCommandArgs.TryParseSetValue(new[] { "key", "a", "b", "c" }, 1, out _));
    }

    [Fact]
    public void TryParseSetValue_MissingValue()
    {
        Assert.False(DataCommandArgs.TryParseSetValue(new[] { "key" }, 1, out _));
    }

    [Fact]
    public void TryParseSetValue_RespectsStartIndex()
    {
        Assert.True(DataCommandArgs.TryParseSetValue(new[] { "entity", "key", "true", "Bool" }, 2, out var value));
        Assert.Equal(DataValueType.Bool, value.Type);
        Assert.True(value.AsBool());
    }
}
