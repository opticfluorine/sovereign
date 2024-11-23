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

using System.Numerics;
using MessagePack;
using Sovereign.EngineUtil.Attributes;

namespace Sovereign.EngineCore.Events.Details;

/// <summary>
///     Reusable event details for events with an entity ID and a Vector3.
/// </summary>
[MessagePackObject]
[Scriptable]
public sealed class EntityVectorEventDetails : IEventDetails
{
    /// <summary>
    ///     Entity ID.
    /// </summary>
    [Key(0)]
    [ScriptableOrder(0)]
    public ulong EntityId { get; set; }

    /// <summary>
    ///     Position.
    /// </summary>
    [Key(1)]
    [ScriptableOrder(1)]
    public Vector3 Vector { get; set; }
}