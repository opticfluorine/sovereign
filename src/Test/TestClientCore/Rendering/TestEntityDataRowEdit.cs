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
using Sovereign.ClientCore.Rendering.Scenes.Game.Gui.TemplateEditor;
using Sovereign.EngineCore.Systems.Data;
using Xunit;

namespace TestClientCore.Rendering;

/// <summary>
///     Tests for the headless free-form entity data row edit helper.
/// </summary>
public class TestEntityDataRowEdit
{
    [Fact]
    public void FromDataValue_InitializesValidRow()
    {
        var row = EntityDataRowEdit.FromDataValue(DataValue.FromInt(15));

        Assert.Equal(DataValueType.Int, row.Type);
        Assert.Equal("15", row.Text);
        Assert.True(row.IsValid);
        Assert.Equal(DataValue.FromInt(15), row.Value);
    }

    [Fact]
    public void SetType_ReparsesCurrentText()
    {
        var row = EntityDataRowEdit.FromDataValue(DataValue.FromString("42"));

        row.SetType(DataValueType.Int);

        Assert.True(row.IsValid);
        Assert.Equal(DataValue.FromInt(42), row.Value);
    }

    [Fact]
    public void SetType_UnparsableText_KeepsLastGoodValueAndMarksInvalid()
    {
        var row = EntityDataRowEdit.FromDataValue(DataValue.FromString("hello"));

        row.SetType(DataValueType.Int);

        Assert.False(row.IsValid);
        Assert.Equal(DataValueType.String, row.Value.Type);
        Assert.Equal("hello", row.Value.AsString());
    }

    [Fact]
    public void SetText_ValidText_UpdatesValue()
    {
        var row = EntityDataRowEdit.FromDataValue(DataValue.FromString("hello"));
        row.SetType(DataValueType.Int);

        row.SetText("42");

        Assert.True(row.IsValid);
        Assert.Equal(DataValue.FromInt(42), row.Value);
    }

    [Fact]
    public void SetText_InvalidText_KeepsLastGoodValue()
    {
        var row = EntityDataRowEdit.FromDataValue(DataValue.FromInt(42));

        row.SetText("not-a-number");

        Assert.False(row.IsValid);
        Assert.Equal(DataValue.FromInt(42), row.Value);
    }

    [Fact]
    public void ToDto_SerializesLastGoodValue()
    {
        var row = EntityDataRowEdit.FromDataValue(DataValue.FromInt(42));
        row.SetText("garbage");

        var dto = row.ToDto();

        Assert.Equal("Int", dto.Type);
        Assert.Equal("42", dto.Value);
    }

    [Fact]
    public void SetType_Vector3_ReparsesCommaSeparatedText()
    {
        var row = EntityDataRowEdit.FromDataValue(DataValue.FromString("1,2,3"));

        row.SetType(DataValueType.Vector3);

        Assert.True(row.IsValid);
        Assert.Equal(new Vector3(1.0f, 2.0f, 3.0f), row.Value.AsVector3());
    }

    [Theory]
    [InlineData(DataValueType.String, 0)]
    [InlineData(DataValueType.Int, 1)]
    [InlineData(DataValueType.Float, 2)]
    [InlineData(DataValueType.Bool, 3)]
    [InlineData(DataValueType.Vector3, 4)]
    [InlineData(DataValueType.EntityId, 5)]
    public void IndexOf_MapsTypesToDisplayOrder(DataValueType type, int expected)
    {
        Assert.Equal(expected, EntityDataRowEdit.IndexOf(type));
        Assert.Equal(type, EntityDataRowEdit.Types[expected]);
    }

    [Theory]
    [InlineData(DataValueType.String, "")]
    [InlineData(DataValueType.Int, "0")]
    [InlineData(DataValueType.Float, "0")]
    [InlineData(DataValueType.Bool, "false")]
    [InlineData(DataValueType.Vector3, "0,0,0")]
    [InlineData(DataValueType.EntityId, "0")]
    public void DefaultText_ReturnsParsableDefaults(DataValueType type, string expected)
    {
        Assert.Equal(expected, EntityDataRowEdit.DefaultText(type));
    }
}
