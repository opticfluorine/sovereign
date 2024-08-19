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

[MessagePackObject]
public class GlobalChatEventDetails : IEventDetails
{
    /// <summary>
    ///     Name of the player who sent the message. This is a string rather than an entity ID for global
    ///     chat since the player entity may not be known to all clients.
    /// </summary>
    [Key(0)]
    public string SenderName { get; set; } = "";

    /// <summary>
    ///     Chat message.
    /// </summary>
    [Key(1)]
    public string Message { get; set; } = "";
}