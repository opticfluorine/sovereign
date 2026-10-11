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
///     Selects the animation phase and effective animation time for an entity, applying any
///     active one-shot override on top of the movement-driven base phase.
/// </summary>
public sealed class AnimationPhaseSelector
{
    private readonly ActiveAnimationPhaseComponentCollection activePhases;
    private readonly AnimationPhaseComponentCollection basePhases;

    /// <summary>
    ///     Creates a new animation phase selector.
    /// </summary>
    /// <param name="basePhases">Movement-driven base animation phase collection.</param>
    /// <param name="activePhases">Active animation phase override collection.</param>
    public AnimationPhaseSelector(AnimationPhaseComponentCollection basePhases,
        ActiveAnimationPhaseComponentCollection activePhases)
    {
        this.basePhases = basePhases;
        this.activePhases = activePhases;
    }

    /// <summary>
    ///     Selects the animation phase and effective time to use when rendering the given entity.
    ///     While an override is active, the effective time is shifted so the override starts at its
    ///     first frame from the moment it was set.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="systemTime">System time of the current frame, in microseconds.</param>
    /// <param name="phase">Selected animation phase.</param>
    /// <param name="effectiveTime">
    ///     Effective animation time for frame selection, in microseconds. Shifted so that active
    ///     overrides start from their first frame.
    /// </param>
    public void Select(ulong entityId, ulong systemTime, out AnimationPhase phase, out ulong effectiveTime)
    {
        if (activePhases.TryGetValue(entityId, out var active))
        {
            phase = active.Phase;
            effectiveTime = systemTime - active.StartTimeUs;
            return;
        }

        phase = basePhases.TryGetValue(entityId, out var basePhase) ? basePhase : AnimationPhase.Default;
        effectiveTime = systemTime;
    }
}
