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
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.Numerics;
using MessagePack;

namespace Sovereign.EngineCore.Events.Details;

/// <summary>
///     Event details for adding an NPC at a position with a template.
/// </summary>
[MessagePackObject]
public class NpcAddEventDetails : IEventDetails
{
    /// <summary>
    ///     NPC position.
    /// </summary>
    [Key(0)]
    public Vector3 Position { get; set; }

    /// <summary>
    ///     NPC template ID.
    /// </summary>
    [Key(1)]
    public ulong NpcTemplateId { get; set; }
}

/// <summary>
///     Event details for removing a non-block entity by entity ID.
/// </summary>
[MessagePackObject]
public class NonBlockRemoveEventDetails : IEventDetails
{
    /// <summary>
    ///     NPC entity ID.
    /// </summary>
    [Key(0)]
    public ulong EntityId { get; set; }
}