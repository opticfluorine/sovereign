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

using Sovereign.ClientCore.Systems.ClientState;
using Sovereign.EngineCore.Events;

namespace Sovereign.ClientCore.Events.Details;

/// <summary>
///     Event details for updating a client state flag.
/// </summary>
public class ClientStateFlagEventDetails : IEventDetails
{
    /// <summary>
    ///     Flag to be updated.
    /// </summary>
    public ClientStateFlag Flag { get; set; }

    /// <summary>
    ///     New value for the flag.
    /// </summary>
    public bool NewValue { get; set; }
}