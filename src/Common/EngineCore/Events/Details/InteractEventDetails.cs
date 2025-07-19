// Sovereign Engine
// Copyright (c) 2025 opticfluorine
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
public class InteractEventDetails : IEventDetails
{
    /// <summary>
    ///     Entity that triggered the interaction.
    /// </summary>
    [Key(0)]
    public ulong SourceEntityId { get; set; }

    /// <summary>
    ///     Entity that is being interacted with.
    /// </summary>
    [Key(1)]
    public ulong TargetEntityId { get; set; }

    /// <summary>
    ///     Item entity that is being used to perform the interaction. If equal to SourceEntityId,
    ///     indicates that no tool is being used.
    /// </summary>
    [Key(2)]
    public ulong ToolEntityId { get; set; }
}