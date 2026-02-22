// Sovereign Engine
// Copyright (c) 2025 opticfluorine
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
///     Event details that contains an integer and a Vector3.
/// </summary>
[MessagePackObject]
public sealed class IntVectorEventDetails : IEventDetails
{
    /// <summary>
    ///     Integer value.
    /// </summary>
    [Key(0)]
    public int IntValue { get; set; }

    /// <summary>
    ///     Vector value.
    /// </summary>
    [Key(1)]
    public Vector3 VectorValue { get; set; }
}