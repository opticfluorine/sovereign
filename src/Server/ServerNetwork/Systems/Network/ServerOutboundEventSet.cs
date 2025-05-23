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

namespace Sovereign.ServerNetwork.Systems.Network;

/// <summary>
///     Provides the set of event IDs that the server will forward out to the network.
/// </summary>
public class ServerOutboundEventSet : IOutboundEventSet
{
    public HashSet<EventId> EventIdsToSend { get; } = new()
    {
        EventId.Core_Ping_Ping,
        EventId.Core_WorldManagement_Subscribe,
        EventId.Core_WorldManagement_Unsubscribe,
        EventId.Core_EntitySync_Sync,
        EventId.Core_EntitySync_Desync,
        EventId.Core_EntitySync_SyncTemplate,
        EventId.Core_Movement_Move,
        EventId.Core_Movement_TeleportNotice,
        EventId.Core_WorldManagement_EntityLeaveWorldSegment,
        EventId.Core_Chat_Local,
        EventId.Core_Chat_Global,
        EventId.Core_Chat_System,
        EventId.Core_Chat_Generic,
        EventId.Core_Block_ModifyNotice,
        EventId.Core_Block_RemoveNotice,
        EventId.Core_Time_Clock
    };
}