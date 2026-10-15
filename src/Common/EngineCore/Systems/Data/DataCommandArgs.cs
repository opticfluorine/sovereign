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

namespace Sovereign.EngineCore.Systems.Data;

/// <summary>
///     Parses the value and optional type arguments of admin data commands.
/// </summary>
public static class DataCommandArgs
{
    /// <summary>
    ///     Parses an optional type tag name.
    /// </summary>
    /// <param name="text">Type tag name.</param>
    /// <param name="type">Parsed type tag. Only valid if the method returns true.</param>
    /// <returns>true if the name is a defined type tag, false otherwise.</returns>
    public static bool TryParseType(string text, out DataValueType type)
    {
        return Enum.TryParse(text, true, out type) && Enum.IsDefined(type);
    }

    /// <summary>
    ///     Decodes canonical text against the given type tag.
    /// </summary>
    /// <param name="type">Type tag.</param>
    /// <param name="text">Canonical text.</param>
    /// <param name="value">Decoded value. Only valid if the method returns true.</param>
    /// <returns>true if the text is valid for the type, false otherwise.</returns>
    public static bool TryDecode(DataValueType type, string text, out DataValue value)
    {
        try
        {
            value = DataValueCodec.Decode(type, text);
            return true;
        }
        catch (FormatException)
        {
            value = default;
            return false;
        }
    }

    /// <summary>
    ///     Parses the value and optional type arguments of a set command.
    /// </summary>
    /// <param name="tokens">Command tokens.</param>
    /// <param name="startIndex">Index of the value token.</param>
    /// <param name="value">Parsed value. Only valid if the method returns true.</param>
    /// <returns>true if a single value and an optional defined type were parsed, false otherwise.</returns>
    public static bool TryParseSetValue(string[] tokens, int startIndex, out DataValue value)
    {
        value = default;

        var count = tokens.Length - startIndex;
        if (count is < 1 or > 2) return false;

        var type = DataValueType.String;
        if (count == 2 && !TryParseType(tokens[startIndex + 1], out type)) return false;

        return TryDecode(type, tokens[startIndex], out value);
    }
}
