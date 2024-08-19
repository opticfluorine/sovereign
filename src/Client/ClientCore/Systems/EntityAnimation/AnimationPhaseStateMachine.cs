// Sovereign Engine
// Copyright (c) 2024 opticfluorine
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

using System.Numerics;
using Sovereign.ClientCore.Components;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.EngineCore.Events.Details;

namespace Sovereign.ClientCore.Systems.EntityAnimation;

/// <summary>
///     Governs the animation phase of all known entities.
/// </summary>
public class AnimationPhaseStateMachine
{
    private readonly AnimationPhaseComponentCollection animationPhases;

    public AnimationPhaseStateMachine(AnimationPhaseComponentCollection animationPhases)
    {
        this.animationPhases = animationPhases;
    }

    /// <summary>
    ///     Called when there is a change in the movement of an entity.
    /// </summary>
    /// <param name="details"></param>
    public void OnMovement(MoveEventDetails details)
    {
        animationPhases.AddOrUpdateComponent(details.EntityId,
            details.Velocity == Vector3.Zero ? AnimationPhase.Default : AnimationPhase.Moving);
    }
}