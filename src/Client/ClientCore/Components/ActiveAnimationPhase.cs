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

using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;

namespace Sovereign.ClientCore.Components;

/// <summary>
///     Client-only component that overrides an entity's animation phase for a bounded number
///     of animation loops before clearing itself.
/// </summary>
public struct ActiveAnimationPhase
{
    /// <summary>
    ///     Animation phase to render while the override is active.
    /// </summary>
    public AnimationPhase Phase;

    /// <summary>
    ///     Number of animation loops to play. Always at least one.
    /// </summary>
    public int Loops;

    /// <summary>
    ///     System time in microseconds at which the override was created.
    /// </summary>
    public ulong StartTimeTicks;

    /// <summary>
    ///     Loop duration of the rendered override phase in microseconds, resolved on the first
    ///     countdown scan. Zero means not yet resolved.
    /// </summary>
    public ulong LoopDurationTicks;
}
