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

namespace Sovereign.EngineCore.Events.Details;

/// <summary>
///     Event details for a request to commit the staged world generation plan.
/// </summary>
[MessagePackObject]
public class WorldGenCommitEventDetails : IEventDetails
{
    /// <summary>
    ///     Seed to confirm, or null to confirm the staged seed.
    /// </summary>
    [Key(0)]
    public ulong? Seed { get; set; }

    /// <summary>
    ///     Whether to proceed despite subscribed players.
    /// </summary>
    [Key(1)]
    public bool Force { get; set; }

    /// <summary>
    ///     Entity to reply to.
    /// </summary>
    [Key(2)]
    public ulong SenderEntityId { get; set; }
}
