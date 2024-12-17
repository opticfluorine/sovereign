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

using MessagePack;

namespace Sovereign.EngineCore.Events.Details;

/// <summary>
///     Denotes the type of target for a generic chat message.
/// </summary>
public enum GenericChatTarget
{
    /// <summary>
    ///     Targets a single player. TargetId is player entity ID.
    /// </summary>
    Player,

    /// <summary>
    ///     Targets all players. TargetId is not used.
    /// </summary>
    Global
}

/// <summary>
///     Details for a generic message sent via chat event.
/// </summary>
[MessagePackObject]
public class GenericChatEventDetails : IEventDetails
{
    /// <summary>
    ///     Target type. Only meaningful on server side.
    /// </summary>
    [IgnoreMember]
    public GenericChatTarget Target { get; set; }

    /// <summary>
    ///     Target object ID.
    ///     Meaning depends on Target value.
    ///     Only meaningful on server side.
    /// </summary>
    [IgnoreMember]
    public ulong TargetId { get; set; }

    /// <summary>
    ///     Chat message.
    /// </summary>
    [Key(0)]
    public string Message { get; set; } = "";

    /// <summary>
    ///     Text color.
    /// </summary>
    [Key(1)]
    public uint Color { get; set; }
}