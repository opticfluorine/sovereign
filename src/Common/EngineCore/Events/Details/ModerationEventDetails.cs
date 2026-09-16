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

using MessagePack;
using Sovereign.EngineUtil.Attributes;

namespace Sovereign.EngineCore.Events.Details;

/// <summary>
///     Denotes the scope of a chat mute.
/// </summary>
[Scriptable]
[ScriptableEnum]
public enum ChatMuteScope
{
    /// <summary>
    ///     Muted from global chat only.
    /// </summary>
    Global = 0,

    /// <summary>
    ///     Muted from all chat, including local chat and global chat.
    /// </summary>
    All = 1
}

/// <summary>
///     Event details describing a change to a player's chat mute state.
/// </summary>
[MessagePackObject]
[Scriptable]
public class ModerationEventDetails : IEventDetails
{
    /// <summary>
    ///     Entity ID of the affected player.
    /// </summary>
    [Key(0)]
    [ScriptableField]
    public ulong EntityId { get; set; }

    /// <summary>
    ///     Scope of the mute.
    /// </summary>
    [Key(1)]
    [ScriptableField]
    public ChatMuteScope Scope { get; set; }

    /// <summary>
    ///     Absolute system time in microseconds at which the mute expires,
    ///     or 0 if the mute was removed before expiring.
    /// </summary>
    [Key(2)]
    [ScriptableField]
    public ulong ExpirySystemTime { get; set; }
}
