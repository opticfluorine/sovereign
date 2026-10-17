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
using System.Globalization;
using System.Numerics;
using Xunit;

namespace Sovereign.EngineCore.Systems.Data;

/// <summary>
///     Unit tests for DataValue and the canonical text codec.
/// </summary>
public class TestDataValueCodec
{
    [Fact]
    public void RoundTrip_String()
    {
        var value = DataValue.FromString("hello, world");

        Assert.Equal(DataValueType.String, value.Type);
        Assert.Equal(value, DataValueCodec.Decode(value.Type, DataValueCodec.Encode(value)));
    }

    [Fact]
    public void RoundTrip_Int()
    {
        var value = DataValue.FromInt(-9000000000L);

        Assert.Equal(DataValueType.Int, value.Type);
        Assert.Equal(value, DataValueCodec.Decode(value.Type, DataValueCodec.Encode(value)));
    }

    [Fact]
    public void RoundTrip_Bool()
    {
        var value = DataValue.FromBool(true);

        Assert.Equal(DataValueType.Bool, value.Type);
        Assert.Equal("true", DataValueCodec.Encode(value));
        Assert.Equal(value, DataValueCodec.Decode(value.Type, DataValueCodec.Encode(value)));
    }

    [Fact]
    public void RoundTrip_Float()
    {
        var value = DataValue.FromFloat(3.14159);

        Assert.Equal(DataValueType.Float, value.Type);
        Assert.Equal(value, DataValueCodec.Decode(value.Type, DataValueCodec.Encode(value)));
    }

    [Fact]
    public void RoundTrip_Vector3()
    {
        var value = DataValue.FromVector3(new Vector3(1.5f, -2.25f, 0.125f));

        Assert.Equal(DataValueType.Vector3, value.Type);
        Assert.Equal(value, DataValueCodec.Decode(value.Type, DataValueCodec.Encode(value)));
    }

    [Fact]
    public void RoundTrip_EntityId()
    {
        var value = DataValue.FromEntityId(0x7FFE000000000001UL);

        Assert.Equal(DataValueType.EntityId, value.Type);
        Assert.Equal("7ffe000000000001", DataValueCodec.Encode(value));
        Assert.Equal(value, DataValueCodec.Decode(value.Type, DataValueCodec.Encode(value)));
    }

    [Fact]
    public void Decode_EntityId_ParsesHex()
    {
        var value = DataValueCodec.Decode(DataValueType.EntityId, "7fff000000000000");

        Assert.Equal(DataValue.FromEntityId(0x7FFF000000000000UL), value);
    }

    [Fact]
    public void Encode_Float_IsCultureInvariant()
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");

            var value = DataValue.FromFloat(3.14159);
            var encoded = DataValueCodec.Encode(value);

            Assert.Equal("3.14159", encoded);
            Assert.DoesNotContain(',', encoded);
            Assert.Equal(value, DataValueCodec.Decode(DataValueType.Float, encoded));
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public void Encode_Vector3_IsCultureInvariant()
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");

            var value = DataValue.FromVector3(new Vector3(1.5f, 2.5f, 3.5f));
            var encoded = DataValueCodec.Encode(value);

            Assert.Equal("1.5,2.5,3.5", encoded);
            Assert.Equal(value, DataValueCodec.Decode(DataValueType.Vector3, encoded));
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Theory]
    [InlineData(DataValueType.Int, "not-an-int")]
    [InlineData(DataValueType.Int, "1.5")]
    [InlineData(DataValueType.Bool, "yes")]
    [InlineData(DataValueType.Float, "not-a-float")]
    [InlineData(DataValueType.Vector3, "1,2")]
    [InlineData(DataValueType.Vector3, "1,2,x")]
    [InlineData(DataValueType.EntityId, "-1")]
    [InlineData((DataValueType)99, "x")]
    public void Decode_Malformed_Throws(DataValueType type, string text)
    {
        Assert.Throws<FormatException>(() => DataValueCodec.Decode(type, text));
    }
}
