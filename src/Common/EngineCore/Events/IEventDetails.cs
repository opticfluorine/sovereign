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
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Systems.Movement.Events;

namespace Sovereign.EngineCore.Events;

/// <summary>
///     Interface implemented by event detail classes.
/// </summary>
/// <remarks>
///     Ensure that derived types are linked with a ProtoInclude here.
/// </remarks>
[Union(0, typeof(SetVelocityEventDetails))]
[Union(1, typeof(MoveOnceEventDetails))]
[Union(2, typeof(EntityEventDetails))]
[Union(3, typeof(EntityVectorEventDetails))]
[Union(4, typeof(WorldSegmentSubscriptionEventDetails))]
[Union(5, typeof(EntityDefinitionEventDetails))]
public interface IEventDetails
{
}