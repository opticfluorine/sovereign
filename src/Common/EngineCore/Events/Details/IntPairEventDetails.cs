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

using MessagePack;

namespace Sovereign.EngineCore.Events.Details;

/// <summary>
///     Event details that contains a pair of integers.
/// </summary>
[MessagePackObject]
public class IntPairEventDetails : IEventDetails
{
    /// <summary>
    ///     First integer value. See event ID description for meaning.
    /// </summary>
    [Key(0)]
    public int First { get; set; }

    /// <summary>
    ///     Second integer value. See event ID description for meaning.
    /// </summary>
    [Key(1)]
    public int Second { get; set; }
}