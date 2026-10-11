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
using Sovereign.ClientCore.Events.Details;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.EngineCore.Events;
using EventId = Sovereign.EngineCore.Events.EventId;

namespace Sovereign.ClientCore.Systems.EntityAnimation;

/// <summary>
///     Public API for starting one-shot animation overrides on client entities.
///     Requests are delivered asynchronously as events.
/// </summary>
public sealed class EntityAnimationController
{
    /// <summary>
    ///     Starts an animation override that plays the given phase for exactly the given number of
    ///     loops beginning at the first frame, restarting any override already in progress.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="entityId">Entity to animate.</param>
    /// <param name="phase">Phase to render while the override is active.</param>
    /// <param name="loops">Number of loops to play. Must be at least one.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="loops" /> is less than one.</exception>
    public void PlayAnimation(IEventSender eventSender, ulong entityId, AnimationPhase phase, int loops = 1)
    {
        if (loops < 1)
            throw new ArgumentOutOfRangeException(nameof(loops), loops, "Loop count must be at least one.");

        var details = new ActiveAnimationPhaseEventDetails
        {
            EntityId = entityId,
            Phase = phase,
            Loops = loops
        };
        var ev = new Event(EventId.Client_EntityAnimation_PlayOverride, details);
        eventSender.SendEvent(ev);
    }
}
