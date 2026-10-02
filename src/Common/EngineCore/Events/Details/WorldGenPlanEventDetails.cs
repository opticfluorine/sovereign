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
///     Event details for a request to compute a new world generation plan.
/// </summary>
[MessagePackObject]
public class WorldGenPlanEventDetails : IEventDetails
{
    /// <summary>
    ///     World generation seed.
    /// </summary>
    [Key(0)]
    public ulong Seed { get; set; }

    /// <summary>
    ///     World generation profile name, or null for the default profile.
    /// </summary>
    [Key(1)]
    public string? ProfileName { get; set; }

    /// <summary>
    ///     Origin of the generated world region, or null for the default origin.
    /// </summary>
    [Key(2)]
    public GridPosition? Origin { get; set; }

    /// <summary>
    ///     Entity to reply to.
    /// </summary>
    [Key(3)]
    public ulong SenderEntityId { get; set; }
}
