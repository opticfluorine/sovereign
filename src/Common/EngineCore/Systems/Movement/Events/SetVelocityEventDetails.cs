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
///     Sets the velocity of an entity. The entity will move under this
///     velocity until the velocity is changed, movement is ended, or the
///     entity cannot proceed to move.
/// </summary>
[MessagePackObject]
public class SetVelocityEventDetails : IEventDetails
{
    /// <summary>
    ///     Entity identifier.
    /// </summary>
    [Key(0)]
    public ulong EntityId { get; set; }

    /// <summary>
    ///     Relative rate of movement along X as a ratio of the entity's base speed.
    /// </summary>
    [Key(1)]
    public float RateX { get; set; }

    /// <summary>
    ///     Relative rate of movement along Y as a ratio of the entity's base speed.
    /// </summary>
    [Key(2)]
    public float RateY { get; set; }
}