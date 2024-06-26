// Sovereign Engine
// Copyright (c) 2023 opticfluorine
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
using MessagePack;
using Sovereign.EngineCore.Entities;

namespace Sovereign.EngineCore.Events.Details;

/// <summary>
///     Event details specifying the definition of one or more entities for replication
///     from server to client. Also used for updating template entities when
///     sent from client to server.
/// </summary>
[MessagePackObject]
public class EntityDefinitionEventDetails : IEventDetails
{
    /// <summary>
    ///     Entity ID of the player sending or receiving this synchronization event.
    /// </summary>
    [IgnoreMember]
    public ulong PlayerEntityId { get; set; }

    /// <summary>
    ///     Entity definitions.
    /// </summary>
    [Key(0)]
    public List<EntityDefinition> EntityDefinitions { get; set; } = new();
}