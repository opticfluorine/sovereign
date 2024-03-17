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
///     Details for a system message sent via chat event.
/// </summary>
[MessagePackObject]
public class SystemChatEventDetails : IEventDetails
{
    /// <summary>
    ///     Player entity ID of recipient. Only meaningful on server side.
    /// </summary>
    [IgnoreMember]
    public ulong TargetEntityId { get; set; }

    /// <summary>
    ///     Chat message.
    /// </summary>
    [Key(0)]
    public string Message { get; set; } = "";
}