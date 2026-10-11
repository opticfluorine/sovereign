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

using Sovereign.ClientCore.Components;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;

namespace Sovereign.ClientCore.Systems.EntityAnimation;

/// <summary>
///     Read access to the active animation overrides.
/// </summary>
public interface IActiveAnimationServices
{
    /// <summary>
    ///     Tries to get the active animation override of an entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="phase">Override phase being rendered.</param>
    /// <returns>true if the entity has an active override, false otherwise.</returns>
    bool TryGetActiveOverride(ulong entityId, out AnimationPhase phase);
}

/// <summary>
///     Implementation of IActiveAnimationServices.
/// </summary>
internal class ActiveAnimationServices(ActiveAnimationPhaseComponentCollection activePhases)
    : IActiveAnimationServices
{
    public bool TryGetActiveOverride(ulong entityId, out AnimationPhase phase)
    {
        if (activePhases.TryGetValue(entityId, out var active))
        {
            phase = active.Phase;
            return true;
        }

        phase = default;
        return false;
    }
}
