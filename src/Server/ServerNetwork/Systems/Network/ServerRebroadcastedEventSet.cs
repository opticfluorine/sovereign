// Sovereign Engine
// Copyright (c) 2026 opticfluorine
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

namespace Sovereign.ServerNetwork.Systems.Network;

/// <summary>
///     Provides the set of event IDs that the server will rebroadcast to other subscribers
///     after receiving them from a client.
/// </summary>
public class ServerRebroadcastedEventSet
{
    /// <summary>
    ///     Set of event IDs to be rebroadcast.
    /// </summary>
    public IReadOnlySet<EventId> EventIds { get; } = new HashSet<EventId>
    {
        EventId.Server_Combat_Attack
    };
}
