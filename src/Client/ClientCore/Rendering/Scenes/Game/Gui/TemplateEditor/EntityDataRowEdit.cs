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
using Sovereign.EngineCore.Systems.Data;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui.TemplateEditor;

/// <summary>
///     Editing state for a single free-form entity data row.
/// </summary>
/// <remarks>
///     The row tracks the text shown to the user separately from the last successfully parsed value.
///     Text that cannot be parsed under the selected type leaves the last good value in place and
///     marks the row invalid so that the editor can surface the problem without writing bad data.
/// </remarks>
public sealed class EntityDataRowEdit
{
    private static readonly string[] Names =
    {
        nameof(DataValueType.String),
        nameof(DataValueType.Int),
        nameof(DataValueType.Float),
        nameof(DataValueType.Bool),
        nameof(DataValueType.Vector3),
        nameof(DataValueType.EntityId)
    };

    private static readonly DataValueType[] Values =
    {
        DataValueType.String,
        DataValueType.Int,
        DataValueType.Float,
        DataValueType.Bool,
        DataValueType.Vector3,
        DataValueType.EntityId
    };

    private EntityDataRowEdit(DataValueType type, string text, DataValue value, bool isValid)
    {
        Type = type;
        Text = text;
        Value = value;
        IsValid = isValid;
    }

    /// <summary>
    ///     Type names in display order, matching <see cref="Types" />.
    /// </summary>
    public static string[] TypeNames => Names;

    /// <summary>
    ///     Types in display order, matching <see cref="TypeNames" />.
    /// </summary>
    public static DataValueType[] Types => Values;

    /// <summary>
    ///     Currently selected type.
    /// </summary>
    public DataValueType Type { get; private set; }

    /// <summary>
    ///     Current editable text.
    /// </summary>
    public string Text { get; private set; }

    /// <summary>
    ///     Last value that parsed successfully.
    /// </summary>
    public DataValue Value { get; private set; }

    /// <summary>
    ///     Whether the current text is valid for the selected type.
    /// </summary>
    public bool IsValid { get; private set; }

    /// <summary>
    ///     Creates a row from an existing value.
    /// </summary>
    /// <param name="value">Value to edit.</param>
    /// <returns>Row state.</returns>
    public static EntityDataRowEdit FromDataValue(DataValue value)
    {
        return new EntityDataRowEdit(value.Type, DataValueCodec.Encode(value), value, true);
    }

    /// <summary>
    ///     Gets the default text for the given type.
    /// </summary>
    /// <param name="type">Type tag.</param>
    /// <returns>Default canonical text.</returns>
    public static string DefaultText(DataValueType type)
    {
        return type switch
        {
            DataValueType.Int => "0",
            DataValueType.Float => "0",
            DataValueType.Bool => "false",
            DataValueType.Vector3 => "0,0,0",
            DataValueType.EntityId => "0",
            _ => string.Empty
        };
    }

    /// <summary>
    ///     Gets the index of a type in the display order.
    /// </summary>
    /// <param name="type">Type tag.</param>
    /// <returns>Display index.</returns>
    public static int IndexOf(DataValueType type)
    {
        for (var i = 0; i < Values.Length; i++)
            if (Values[i] == type)
                return i;

        return 0;
    }

    /// <summary>
    ///     Changes the selected type, reparsing the current text against it.
    /// </summary>
    /// <param name="type">New type tag.</param>
    public void SetType(DataValueType type)
    {
        Type = type;
        Reparse();
    }

    /// <summary>
    ///     Replaces the editable text, reparsing it against the current type.
    /// </summary>
    /// <param name="text">New text.</param>
    public void SetText(string text)
    {
        Text = text;
        Reparse();
    }

    /// <summary>
    ///     Resets the text to the current type's default value.
    /// </summary>
    public void ResetToDefault()
    {
        SetText(DefaultText(Type));
    }

    /// <summary>
    ///     Serializes the last good value as a transport value.
    /// </summary>
    /// <returns>Transport value.</returns>
    public DataValueDto ToDto()
    {
        return DataValueDto.FromDataValue(Value);
    }

    /// <summary>
    ///     Reparses the current text, keeping the last good value on failure.
    /// </summary>
    private void Reparse()
    {
        try
        {
            Value = DataValueCodec.Decode(Type, Text);
            IsValid = true;
        }
        catch (FormatException)
        {
            IsValid = false;
        }
    }
}
