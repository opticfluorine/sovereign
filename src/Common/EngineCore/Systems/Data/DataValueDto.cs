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
using MessagePack;

namespace Sovereign.EngineCore.Systems.Data;

/// <summary>
///     Transport data type for a single tagged key-value data value.
/// </summary>
[MessagePackObject]
public class DataValueDto
{
    /// <summary>
    ///     Type tag name, matching a <see cref="DataValueType" /> name.
    /// </summary>
    [Key(0)]
    public string Type { get; set; } = DataValueType.String.ToString();

    /// <summary>
    ///     Canonical text encoding of the value.
    /// </summary>
    [Key(1)]
    public string Value { get; set; } = string.Empty;

    /// <summary>
    ///     Creates a transport value from a tagged data value.
    /// </summary>
    /// <param name="value">Tagged data value.</param>
    /// <returns>Transport value.</returns>
    public static DataValueDto FromDataValue(DataValue value)
    {
        return new DataValueDto
        {
            Type = value.Type.ToString(),
            Value = DataValueCodec.Encode(value)
        };
    }

    /// <summary>
    ///     Attempts to decode this transport value into a tagged data value.
    /// </summary>
    /// <param name="value">Decoded value. Only valid if the method returns true.</param>
    /// <returns>true if the tag name and text encoding are valid, false otherwise.</returns>
    public bool TryToDataValue(out DataValue value)
    {
        value = default;

        if (!Enum.TryParse<DataValueType>(Type, true, out var type) || !Enum.IsDefined(type)) return false;

        try
        {
            value = DataValueCodec.Decode(type, Value);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
