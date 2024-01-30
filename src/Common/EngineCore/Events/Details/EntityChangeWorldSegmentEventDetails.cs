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
using Sovereign.EngineCore.Components.Indexers;

namespace Sovereign.EngineCore.Events.Details;

/// <summary>
///     Event details for an entity moving between world segments.
/// </summary>
[MessagePackObject]
public class EntityChangeWorldSegmentEventDetails : IEventDetails
{
    /// <summary>
    ///     Associated entity ID.
    /// </summary>
    [Key(0)]
    public ulong EntityId { get; set; }

    /// <summary>
    ///     Previous world segment index.
    /// </summary>
    [Key(1)]
    public GridPosition PreviousSegmentIndex { get; set; }

    /// <summary>
    ///     New world segment index.
    /// </summary>
    [Key(2)]
    public GridPosition NewSegmentIndex { get; set; }
}