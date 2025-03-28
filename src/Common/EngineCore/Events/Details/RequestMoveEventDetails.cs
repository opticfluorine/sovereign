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

[MessagePackObject]
public class RequestMoveEventDetails : IEventDetails
{
    /// <summary>
    ///     Relative velocity in the XY plane. Norm is not to exceed 1.0.
    /// </summary>
    [Key(0)]
    public Vector2 RelativeVelocity { get; set; }

    /// <summary>
    ///     Affected entity ID.
    /// </summary>
    [Key(1)]
    public ulong EntityId { get; set; }

    /// <summary>
    ///     Sequence number. Used for synchronization.
    /// </summary>
    [Key(2)]
    public byte Sequence { get; set; }
}