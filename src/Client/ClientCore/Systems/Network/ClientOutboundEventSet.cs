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

using System.Collections.Generic;
using Sovereign.EngineCore.Events;
using Sovereign.NetworkCore.Systems.Network;

namespace Sovereign.ClientCore.Systems.Network;

/// <summary>
///     Provides the set of event IDs that the client will forward to the network.
/// </summary>
public class ClientOutboundEventSet : IOutboundEventSet
{
    public HashSet<EventId> EventIdsToSend { get; } = new()
    {
        EventId.Core_Ping_Pong,
        EventId.Core_Movement_RequestMove,
        EventId.Core_Movement_Jump,
        EventId.Core_Network_Logout,
        EventId.Core_Chat_Send,
        EventId.Core_Interaction_Interact,
        EventId.Core_Inventory_PickUp,
        EventId.Core_Inventory_Drop,
        EventId.Core_Inventory_DropAtPosition,
        EventId.Core_Inventory_Swap,
        EventId.Server_TemplateEntity_Update,
        EventId.Server_WorldEdit_SetBlock,
        EventId.Server_WorldEdit_RemoveBlock,
        EventId.Server_WorldEdit_AddNonBlock,
        EventId.Server_WorldEdit_RemoveNonBlock
    };
}