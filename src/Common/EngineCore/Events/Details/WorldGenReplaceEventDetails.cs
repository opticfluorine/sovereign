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
///     Event details for a request to commit the staged world generation plan, replacing
///     a registered world.
/// </summary>
[MessagePackObject]
public class WorldGenReplaceEventDetails : IEventDetails
{
    /// <summary>
    ///     Seed of the staged plan.
    /// </summary>
    [Key(0)]
    public ulong StagedSeed { get; set; }

    /// <summary>
    ///     Seed of the registered world to replace.
    /// </summary>
    [Key(1)]
    public ulong OldSeed { get; set; }

    /// <summary>
    ///     Whether to proceed despite subscribed players.
    /// </summary>
    [Key(2)]
    public bool Force { get; set; }

    /// <summary>
    ///     Entity to reply to.
    /// </summary>
    [Key(3)]
    public ulong SenderEntityId { get; set; }
}
