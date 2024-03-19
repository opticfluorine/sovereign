// Sovereign Engine
// Copyright (c) 2024 opticfluorine
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

namespace Sovereign.ClientCore.Systems.ClientChat;

/// <summary>
///     Constants for client-side chat.
/// </summary>
public static class ClientChatConstants
{
    /// <summary>
    ///     Default entity name to use if the source entity name is unknown.
    /// </summary>
    public const string UnknownName = "???";

    /// <summary>
    ///     Default text colors for each type of chat.
    /// </summary>
    public static readonly Dictionary<ChatType, Vector4> ChatTextColors = new()
    {
        { ChatType.Local, new Vector4(0.7f, 0.7f, 0.7f, 1.0f) },
        { ChatType.Global, new Vector4(1.0f) },
        { ChatType.System, new Vector4(0.5f, 0.5f, 0.5f, 1.0f) }
    };

    /// <summary>
    ///     Default text color if the chat type is not known.
    /// </summary>
    public static readonly Vector4 DefaultTextColor = new(1.0f);
}