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

using System.Text.Json.Serialization;
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
[Union(15, typeof(GenericChatEventDetails))]
[Union(16, typeof(TeleportNoticeEventDetails))]
[Union(17, typeof(IntEventDetails))]
[Union(18, typeof(NpcAddEventDetails))]
[Union(19, typeof(NonBlockRemoveEventDetails))]
[Union(20, typeof(InteractEventDetails))]
[Union(21, typeof(DialogueEventDetails))]
[Union(22, typeof(IntPairEventDetails))]
[Union(23, typeof(IntVectorEventDetails))]
[JsonPolymorphic]
[JsonDerivedType(typeof(AutoPingEventDetails), nameof(AutoPingEventDetails))]
[JsonDerivedType(typeof(BlockAddBatchEventDetails), nameof(BlockAddBatchEventDetails))]
[JsonDerivedType(typeof(BlockAddEventDetails), nameof(BlockAddEventDetails))]
[JsonDerivedType(typeof(BlockPresenceGridUpdatedEventDetails), nameof(BlockPresenceGridUpdatedEventDetails))]
[JsonDerivedType(typeof(BlockRemoveBatchEventDetails), nameof(BlockRemoveBatchEventDetails))]
[JsonDerivedType(typeof(BooleanEventDetails), nameof(BooleanEventDetails))]
[JsonDerivedType(typeof(ChatEventDetails), nameof(ChatEventDetails))]
[JsonDerivedType(typeof(ConnectionIdEventDetails), nameof(ConnectionIdEventDetails))]
[JsonDerivedType(typeof(DialogueEventDetails), nameof(DialogueEventDetails))]
[JsonDerivedType(typeof(EntityChangeWorldSegmentEventDetails), nameof(EntityChangeWorldSegmentEventDetails))]
[JsonDerivedType(typeof(EntityDefinitionEventDetails), nameof(EntityDefinitionEventDetails))]
[JsonDerivedType(typeof(EntityDesyncEventDetails), nameof(EntityDesyncEventDetails))]
[JsonDerivedType(typeof(EntityEventDetails), nameof(EntityEventDetails))]
[JsonDerivedType(typeof(EntitySequenceEventDetails), nameof(EntitySequenceEventDetails))]
[JsonDerivedType(typeof(EntityVectorEventDetails), nameof(EntityVectorEventDetails))]
[JsonDerivedType(typeof(ErrorEventDetails), nameof(ErrorEventDetails))]
[JsonDerivedType(typeof(GenericChatEventDetails), nameof(GenericChatEventDetails))]
[JsonDerivedType(typeof(GlobalChatEventDetails), nameof(GlobalChatEventDetails))]
[JsonDerivedType(typeof(GridPositionEventDetails), nameof(GridPositionEventDetails))]
[JsonDerivedType(typeof(IntEventDetails), nameof(IntEventDetails))]
[JsonDerivedType(typeof(IntPairEventDetails), nameof(IntPairEventDetails))]
[JsonDerivedType(typeof(IntVectorEventDetails), nameof(IntVectorEventDetails))]
[JsonDerivedType(typeof(InteractEventDetails), nameof(InteractEventDetails))]
[JsonDerivedType(typeof(KeyValueEventDetails), nameof(KeyValueEventDetails))]
[JsonDerivedType(typeof(LocalChatEventDetails), nameof(LocalChatEventDetails))]
[JsonDerivedType(typeof(MaterialPairEventDetails), nameof(MaterialPairEventDetails))]
[JsonDerivedType(typeof(MoveEventDetails), nameof(MoveEventDetails))]
[JsonDerivedType(typeof(NpcAddEventDetails), nameof(NpcAddEventDetails))]
[JsonDerivedType(typeof(NonBlockRemoveEventDetails), nameof(NonBlockRemoveEventDetails))]
[JsonDerivedType(typeof(RequestMoveEventDetails), nameof(RequestMoveEventDetails))]
[JsonDerivedType(typeof(SelectPlayerEventDetails), nameof(SelectPlayerEventDetails))]
[JsonDerivedType(typeof(SequenceEventDetails), nameof(SequenceEventDetails))]
[JsonDerivedType(typeof(StringEventDetails), nameof(StringEventDetails))]
[JsonDerivedType(typeof(SystemChatEventDetails), nameof(SystemChatEventDetails))]
[JsonDerivedType(typeof(TeleportNoticeEventDetails), nameof(TeleportNoticeEventDetails))]
[JsonDerivedType(typeof(TemplateEntityDefinitionEventDetails), nameof(TemplateEntityDefinitionEventDetails))]
[JsonDerivedType(typeof(TimeEventDetails), nameof(TimeEventDetails))]
[JsonDerivedType(typeof(VectorEventDetails), nameof(VectorEventDetails))]
[JsonDerivedType(typeof(VectorPairEventDetails), nameof(VectorPairEventDetails))]
[JsonDerivedType(typeof(WorldSegmentEventDetails), nameof(WorldSegmentEventDetails))]
[JsonDerivedType(typeof(WorldSegmentSubscriptionEventDetails), nameof(WorldSegmentSubscriptionEventDetails))]
public interface IEventDetails
{
}