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

using System.Numerics;
using MessagePack;

namespace Sovereign.EngineCore.Events.Details;

/// <summary>
///     Event details for various movement-related events.
/// </summary>
[MessagePackObject]
public class MoveEventDetails : IEventDetails
{
    /// <summary>
    ///     Latest position.
    /// </summary>
    [Key(0)]
    public Vector3 Position { get; set; }

    /// <summary>
    ///     Velocity.
    /// </summary>
    [Key(1)]
    public Vector3 Velocity { get; set; }

    /// <summary>
    ///     Affected entity ID.
    /// </summary>
    [Key(2)]
    public ulong EntityId { get; set; }
}