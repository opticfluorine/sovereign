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

namespace Sovereign.EngineCore.Events;

/// <summary>
///     Interface implemented by event detail classes.
/// </summary>
[Union(0, typeof(EntityEventDetails))]
[Union(1, typeof(EntityVectorEventDetails))]
[Union(2, typeof(WorldSegmentSubscriptionEventDetails))]
[Union(3, typeof(EntityDefinitionEventDetails))]
[Union(4, typeof(MoveEventDetails))]
[Union(5, typeof(RequestMoveEventDetails))]
[Union(6, typeof(EntityChangeWorldSegmentEventDetails))]
[Union(7, typeof(EntityDesyncEventDetails))]
[Union(8, typeof(ChatEventDetails))]
[Union(9, typeof(LocalChatEventDetails))]
[Union(10, typeof(GlobalChatEventDetails))]
[Union(11, typeof(SystemChatEventDetails))]
[Union(12, typeof(TemplateEntityDefinitionEventDetails))]
[Union(13, typeof(GridPositionEventDetails))]
[Union(14, typeof(BlockAddEventDetails))]
public interface IEventDetails
{
}