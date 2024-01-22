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
        EventId.Client_EntitySynchronization_Update,
        EventId.Core_Movement_Move
    };
}