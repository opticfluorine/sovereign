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

using MessagePack;
using Sovereign.EngineCore.Components.Types;

namespace Sovereign.EngineCore.Events.Details;

/// <summary>
///     Event details describing a change to a vital of an entity.
/// </summary>
[MessagePackObject]
public class ChangeVitalsEventDetails : IEventDetails
{
    /// <summary>
    ///     ID of the affected entity.
    /// </summary>
    [Key(0)]
    public ulong EntityId { get; set; }

    /// <summary>
    ///     Vital to be changed.
    /// </summary>
    [Key(1)]
    public VitalType Vital { get; set; }

    /// <summary>
    ///     Amount by which to change the vital's current value.
    /// </summary>
    [Key(2)]
    public int ChangeAmount { get; set; }
}
