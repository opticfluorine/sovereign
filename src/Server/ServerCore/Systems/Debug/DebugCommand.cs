// Sovereign Engine
// Copyright (c) 2023 opticfluorine
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

using System.Text.Json.Serialization;

namespace Sovereign.ServerCore.Systems.Debug;

public enum DebugCommandType
{
    /// <summary>
    ///     Unknown command type.
    /// </summary>
    Unknown,

    /// <summary>
    ///     Debug command to generate test world data.
    /// </summary>
    GenerateWorldData
}

/// <summary>
///     JSON-serializable debug command structure.
/// </summary>
public sealed class DebugCommand
{
    /// <summary>
    ///     Debug command type.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DebugCommandType? Type { get; set; }

    /// <summary>
    ///     Checks whether the record is valid.
    /// </summary>
    public bool IsValid => Type != DebugCommandType.Unknown;
}