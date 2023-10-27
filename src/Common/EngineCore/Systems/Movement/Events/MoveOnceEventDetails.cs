/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using MessagePack;
using Sovereign.EngineCore.Events;

namespace Sovereign.EngineCore.Systems.Movement.Events;

/// <summary>
///     Event details associated with the Core_Movement event, which
///     describes an attempt to move an entity relative to its current
///     position.
///     If the entity is able to move to the new position (i.e. the entity
///     is not fixed and is not somehow forbidden from the new position),
///     the movement is successful. Otherwise no movement occurs.
/// </summary>
[MessagePackObject]
public class MoveOnceEventDetails : IEventDetails
{
    /// <summary>
    ///     Identifier of the entity that is moving.
    /// </summary>
    [Key(0)]
    public ulong EntityId { get; set; }

    /// <summary>
    ///     Unique movement phase used to filter expired events.
    /// </summary>
    [Key(1)]
    public uint MovementPhase { get; set; }
}