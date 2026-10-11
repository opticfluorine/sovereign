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
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.EngineCore.Events;

namespace Sovereign.ClientCore.Events.Details;

/// <summary>
///     Event details for starting or restarting a one-shot animation override.
/// </summary>
[MessagePackObject]
public class ActiveAnimationPhaseEventDetails : IEventDetails
{
    /// <summary>
    ///     Entity to animate.
    /// </summary>
    [Key(0)]
    public ulong EntityId { get; set; }

    /// <summary>
    ///     Phase to render while the override is active.
    /// </summary>
    [Key(1)]
    public AnimationPhase Phase { get; set; }

    /// <summary>
    ///     Number of loops to play. Always at least one.
    /// </summary>
    [Key(2)]
    public int Loops { get; set; }
}
