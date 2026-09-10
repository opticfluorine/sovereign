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

namespace Sovereign.EngineCore.Events.Details;

/// <summary>
///     Details for using an item as a tool on a target entity.
/// </summary>
[MessagePackObject]
public class UseItemEventDetails : IEventDetails
{
    /// <summary>
    ///     Item entity to be used as a tool.
    /// </summary>
    [Key(0)]
    public ulong ToolEntityId { get; set; }

    /// <summary>
    ///     Entity on which the tool is used.
    /// </summary>
    [Key(1)]
    public ulong TargetEntityId { get; set; }
}
