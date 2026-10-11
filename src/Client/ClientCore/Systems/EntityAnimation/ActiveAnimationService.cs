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

using System;
using Sovereign.ClientCore.Components;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.EngineCore.Timing;

namespace Sovereign.ClientCore.Systems.EntityAnimation;

/// <summary>
///     Public entry point for starting one-shot animation overrides on client entities.
/// </summary>
public sealed class ActiveAnimationService
{
    private readonly ActiveAnimationPhaseComponentCollection activePhases;
    private readonly ISystemTimer systemTimer;

    /// <summary>
    ///     Creates a new active animation service.
    /// </summary>
    /// <param name="activePhases">Active animation phase override collection.</param>
    /// <param name="systemTimer">System timer.</param>
    public ActiveAnimationService(ActiveAnimationPhaseComponentCollection activePhases, ISystemTimer systemTimer)
    {
        this.activePhases = activePhases;
        this.systemTimer = systemTimer;
    }

    /// <summary>
    ///     Starts an animation override that plays the given phase for exactly the given number of
    ///     loops beginning at the first frame, restarting any override already in progress.
    /// </summary>
    /// <param name="entityId">Entity to animate.</param>
    /// <param name="phase">Phase to render while the override is active.</param>
    /// <param name="loops">Number of loops to play. Must be at least one.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="loops" /> is less than one.</exception>
    public void PlayAnimation(ulong entityId, AnimationPhase phase, int loops = 1)
    {
        if (loops < 1)
            throw new ArgumentOutOfRangeException(nameof(loops), loops, "Loop count must be at least one.");

        /* Cancel any pending expiry removal so that the re-triggered override cannot be
           silently deleted by the stale removal when component updates are next applied. */
        activePhases.CancelPendingRemove(entityId);

        activePhases.AddOrUpdateComponent(entityId, new ActiveAnimationPhase
        {
            Phase = phase,
            Loops = loops,
            StartTimeTicks = systemTimer.GetTime(),
            LoopDurationTicks = 0
        });
    }
}
