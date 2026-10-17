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

namespace Sovereign.EngineCore.Systems.Data;

/// <summary>
///     Type tag carried by every key-value pair.
/// </summary>
public enum DataValueType
{
    /// <summary>
    ///     String value.
    /// </summary>
    String = 0,

    /// <summary>
    ///     Signed 64-bit integer value.
    /// </summary>
    Int = 1,

    /// <summary>
    ///     Boolean value.
    /// </summary>
    Bool = 2,

    /// <summary>
    ///     Double-precision floating point value.
    /// </summary>
    Float = 3,

    /// <summary>
    ///     Three-dimensional vector value.
    /// </summary>
    Vector3 = 4,

    /// <summary>
    ///     Unsigned 64-bit entity ID value.
    /// </summary>
    EntityId = 5
}

/// <summary>
///     A typed key-value data value carrying its own type tag.
/// </summary>
public readonly struct DataValue : IEquatable<DataValue>
{
    private readonly bool boolValue;
    private readonly double doubleValue;
    private readonly ulong entityIdValue;
    private readonly long longValue;
    private readonly string? stringValue;
    private readonly Vector3 vectorValue;

    /// <summary>
    ///     Type tag for this value.
    /// </summary>
    public DataValueType Type { get; }

    private DataValue(DataValueType type, string? stringValue, long longValue, bool boolValue, double doubleValue,
        Vector3 vectorValue, ulong entityIdValue)
    {
        Type = type;
        this.stringValue = stringValue;
        this.longValue = longValue;
        this.boolValue = boolValue;
        this.doubleValue = doubleValue;
        this.vectorValue = vectorValue;
        this.entityIdValue = entityIdValue;
    }

    /// <summary>
    ///     Creates a String value.
    /// </summary>
    /// <param name="value">String value.</param>
    /// <returns>Tagged value.</returns>
    public static DataValue FromString(string value)
    {
        return new DataValue(DataValueType.String, value, 0, false, 0.0, Vector3.Zero, 0);
    }

    /// <summary>
    ///     Creates an Int value.
    /// </summary>
    /// <param name="value">Integer value.</param>
    /// <returns>Tagged value.</returns>
    public static DataValue FromInt(long value)
    {
        return new DataValue(DataValueType.Int, null, value, false, 0.0, Vector3.Zero, 0);
    }

    /// <summary>
    ///     Creates a Bool value.
    /// </summary>
    /// <param name="value">Boolean value.</param>
    /// <returns>Tagged value.</returns>
    public static DataValue FromBool(bool value)
    {
        return new DataValue(DataValueType.Bool, null, 0, value, 0.0, Vector3.Zero, 0);
    }

    /// <summary>
    ///     Creates a Float value.
    /// </summary>
    /// <param name="value">Floating point value.</param>
    /// <returns>Tagged value.</returns>
    public static DataValue FromFloat(double value)
    {
        return new DataValue(DataValueType.Float, null, 0, false, value, Vector3.Zero, 0);
    }

    /// <summary>
    ///     Creates a Vector3 value.
    /// </summary>
    /// <param name="value">Vector value.</param>
    /// <returns>Tagged value.</returns>
    public static DataValue FromVector3(Vector3 value)
    {
        return new DataValue(DataValueType.Vector3, null, 0, false, 0.0, value, 0);
    }

    /// <summary>
    ///     Creates an EntityId value.
    /// </summary>
    /// <param name="value">Entity ID value.</param>
    /// <returns>Tagged value.</returns>
    public static DataValue FromEntityId(ulong value)
    {
        return new DataValue(DataValueType.EntityId, null, 0, false, 0.0, Vector3.Zero, value);
    }

    /// <summary>
    ///     Gets the value as a string.
    /// </summary>
    /// <returns>String value.</returns>
    /// <exception cref="InvalidOperationException">Thrown if this value is not a String.</exception>
    public string AsString()
    {
        if (Type != DataValueType.String) throw new InvalidOperationException($"Value is {Type}, not String.");
        return stringValue ?? string.Empty;
    }

    /// <summary>
    ///     Gets the value as a signed 64-bit integer.
    /// </summary>
    /// <returns>Integer value.</returns>
    /// <exception cref="InvalidOperationException">Thrown if this value is not an Int.</exception>
    public long AsInt()
    {
        if (Type != DataValueType.Int) throw new InvalidOperationException($"Value is {Type}, not Int.");
        return longValue;
    }

    /// <summary>
    ///     Gets the value as a boolean.
    /// </summary>
    /// <returns>Boolean value.</returns>
    /// <exception cref="InvalidOperationException">Thrown if this value is not a Bool.</exception>
    public bool AsBool()
    {
        if (Type != DataValueType.Bool) throw new InvalidOperationException($"Value is {Type}, not Bool.");
        return boolValue;
    }

    /// <summary>
    ///     Gets the value as a double-precision floating point number.
    /// </summary>
    /// <returns>Floating point value.</returns>
    /// <exception cref="InvalidOperationException">Thrown if this value is not a Float.</exception>
    public double AsFloat()
    {
        if (Type != DataValueType.Float) throw new InvalidOperationException($"Value is {Type}, not Float.");
        return doubleValue;
    }

    /// <summary>
    ///     Gets the value as a three-dimensional vector.
    /// </summary>
    /// <returns>Vector value.</returns>
    /// <exception cref="InvalidOperationException">Thrown if this value is not a Vector3.</exception>
    public Vector3 AsVector3()
    {
        if (Type != DataValueType.Vector3) throw new InvalidOperationException($"Value is {Type}, not Vector3.");
        return vectorValue;
    }

    /// <summary>
    ///     Gets the value as an entity ID.
    /// </summary>
    /// <returns>Entity ID value.</returns>
    /// <exception cref="InvalidOperationException">Thrown if this value is not an EntityId.</exception>
    public ulong AsEntityId()
    {
        if (Type != DataValueType.EntityId) throw new InvalidOperationException($"Value is {Type}, not EntityId.");
        return entityIdValue;
    }

    /// <summary>
    ///     Returns the canonical text encoding of this value.
    /// </summary>
    /// <returns>Canonical text encoding.</returns>
    public override string ToString()
    {
        return DataValueCodec.Encode(this);
    }

    /// <summary>
    ///     Converts a string to a String value.
    /// </summary>
    /// <param name="value">String value.</param>
    public static implicit operator DataValue(string value)
    {
        return FromString(value);
    }

    /// <summary>
    ///     Converts an integer to an Int value.
    /// </summary>
    /// <param name="value">Integer value.</param>
    public static implicit operator DataValue(int value)
    {
        return FromInt(value);
    }

    /// <summary>
    ///     Converts a long to an Int value.
    /// </summary>
    /// <param name="value">Integer value.</param>
    public static implicit operator DataValue(long value)
    {
        return FromInt(value);
    }

    /// <summary>
    ///     Converts a boolean to a Bool value.
    /// </summary>
    /// <param name="value">Boolean value.</param>
    public static implicit operator DataValue(bool value)
    {
        return FromBool(value);
    }

    /// <summary>
    ///     Converts a float to a Float value.
    /// </summary>
    /// <param name="value">Floating point value.</param>
    public static implicit operator DataValue(float value)
    {
        return FromFloat(value);
    }

    /// <summary>
    ///     Converts a double to a Float value.
    /// </summary>
    /// <param name="value">Floating point value.</param>
    public static implicit operator DataValue(double value)
    {
        return FromFloat(value);
    }

    /// <summary>
    ///     Converts a vector to a Vector3 value.
    /// </summary>
    /// <param name="value">Vector value.</param>
    public static implicit operator DataValue(Vector3 value)
    {
        return FromVector3(value);
    }

    /// <summary>
    ///     Converts an entity ID to an EntityId value.
    /// </summary>
    /// <param name="value">Entity ID value.</param>
    public static implicit operator DataValue(ulong value)
    {
        return FromEntityId(value);
    }

    /// <inheritdoc />
    public bool Equals(DataValue other)
    {
        if (Type != other.Type) return false;
        return Type switch
        {
            DataValueType.String => AsString() == other.AsString(),
            DataValueType.Int => AsInt() == other.AsInt(),
            DataValueType.Bool => AsBool() == other.AsBool(),
            DataValueType.Float => AsFloat().Equals(other.AsFloat()),
            DataValueType.Vector3 => AsVector3().Equals(other.AsVector3()),
            DataValueType.EntityId => AsEntityId() == other.AsEntityId(),
            _ => false
        };
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is DataValue other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Type switch
        {
            DataValueType.String => HashCode.Combine(Type, AsString()),
            DataValueType.Int => HashCode.Combine(Type, AsInt()),
            DataValueType.Bool => HashCode.Combine(Type, AsBool()),
            DataValueType.Float => HashCode.Combine(Type, AsFloat()),
            DataValueType.Vector3 => HashCode.Combine(Type, AsVector3()),
            DataValueType.EntityId => HashCode.Combine(Type, AsEntityId()),
            _ => Type.GetHashCode()
        };
    }
}

/// <summary>
///     Canonical text encoding for <see cref="DataValue" />.
/// </summary>
public static class DataValueCodec
{
    /// <summary>
    ///     Encodes a value into its canonical text form.
    /// </summary>
    /// <param name="value">Value to encode.</param>
    /// <returns>Canonical text encoding.</returns>
    public static string Encode(DataValue value)
    {
        return value.Type switch
        {
            DataValueType.String => value.AsString(),
            DataValueType.Int => value.AsInt().ToString(CultureInfo.InvariantCulture),
            DataValueType.Bool => value.AsBool() ? "true" : "false",
            DataValueType.Float => EncodeFloat(value.AsFloat()),
            DataValueType.Vector3 => EncodeVector3(value.AsVector3()),
            DataValueType.EntityId => value.AsEntityId().ToString("x16", CultureInfo.InvariantCulture),
            _ => throw new InvalidOperationException($"Unknown data value type {value.Type}.")
        };
    }

    /// <summary>
    ///     Decodes canonical text into a value with the given type tag.
    /// </summary>
    /// <param name="type">Type tag.</param>
    /// <param name="text">Canonical text encoding.</param>
    /// <returns>Decoded value.</returns>
    /// <exception cref="FormatException">Thrown if the text is malformed for the given type.</exception>
    public static DataValue Decode(DataValueType type, string text)
    {
        switch (type)
        {
            case DataValueType.String:
                return DataValue.FromString(text);
            case DataValueType.Int:
                if (!long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intValue))
                    throw new FormatException($"Malformed Int value '{text}'.");
                return DataValue.FromInt(intValue);
            case DataValueType.Bool:
                if (!bool.TryParse(text, out var boolValue))
                    throw new FormatException($"Malformed Bool value '{text}'.");
                return DataValue.FromBool(boolValue);
            case DataValueType.Float:
                if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var floatValue))
                    throw new FormatException($"Malformed Float value '{text}'.");
                return DataValue.FromFloat(floatValue);
            case DataValueType.Vector3:
                return DataValue.FromVector3(DecodeVector3(text));
            case DataValueType.EntityId:
                if (!ulong.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var entityIdValue))
                    throw new FormatException($"Malformed EntityId value '{text}'.");
                return DataValue.FromEntityId(entityIdValue);
            default:
                throw new FormatException($"Unknown data value type {type}.");
        }
    }

    /// <summary>
    ///     Encodes a floating point number using a culture-invariant round-trip format.
    /// </summary>
    /// <param name="value">Value to encode.</param>
    /// <returns>Canonical text encoding.</returns>
    private static string EncodeFloat(double value)
    {
        return value.ToString("R", CultureInfo.InvariantCulture);
    }

    /// <summary>
    ///     Encodes a vector as three comma-separated round-trip floats.
    /// </summary>
    /// <param name="value">Vector to encode.</param>
    /// <returns>Canonical text encoding.</returns>
    private static string EncodeVector3(Vector3 value)
    {
        return string.Join(',',
            ((float)value.X).ToString("R", CultureInfo.InvariantCulture),
            ((float)value.Y).ToString("R", CultureInfo.InvariantCulture),
            ((float)value.Z).ToString("R", CultureInfo.InvariantCulture));
    }

    /// <summary>
    ///     Decodes a vector from three comma-separated floats.
    /// </summary>
    /// <param name="text">Canonical text encoding.</param>
    /// <returns>Decoded vector.</returns>
    /// <exception cref="FormatException">Thrown if the text is malformed.</exception>
    private static Vector3 DecodeVector3(string text)
    {
        var parts = text.Split(',');
        if (parts.Length != 3) throw new FormatException($"Malformed Vector3 value '{text}'.");

        if (!float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x) ||
            !float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y) ||
            !float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var z))
            throw new FormatException($"Malformed Vector3 value '{text}'.");

        return new Vector3(x, y, z);
    }
}
