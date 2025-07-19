// Sovereign Engine
// Copyright (c) 2025 opticfluorine
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

using System.Collections.Generic;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Timing;

namespace Sovereign.ServerCore.Systems.Interaction;

/// <summary>
///     Responsible for determining whether an interaction is allowed.
/// </summary>
internal sealed class InteractionValidator(KinematicsComponentCollection kinematics, ISystemTimer timer)
{
    /// <summary>
    ///     Square of maximum distance (in world units) at which interactions are allowed.
    /// </summary>
    private const float MaxRangeSquared = 0.125f;

    /// <summary>
    ///     Minimum time between interactions.
    /// </summary>
    private const ulong MinTimeBetweenInteractionsUs = 100000;

    private readonly Dictionary<ulong, ulong> lastInteractionTimes = new();

    /// <summary>
    ///     Determines whether an interaction between two entities is permissible (even if that
    ///     interaction has no effect).
    /// </summary>
    /// <param name="sourceEntityId">Source entity ID.</param>
    /// <param name="targetEntityId">Target entity ID.</param>
    /// <param name="toolEntityId">Tool entity ID.</param>
    /// <returns>true if the interaction is permissible, false otherwise.</returns>
    public bool IsInteractionAllowed(ulong sourceEntityId, ulong targetEntityId, ulong toolEntityId)
    {
        return IsWithinRange(sourceEntityId, targetEntityId) &&
               ToolBelongsToSource(sourceEntityId, toolEntityId) &&
               IsRateLimitOk(sourceEntityId);
    }

    /// <summary>
    ///     Checks whether the interaction is within the allowed range.
    /// </summary>
    /// <param name="sourceEntityId">Source entity ID.</param>
    /// <param name="targetEntityId">Target entity ID.</param>
    /// <returns>true if the interaction is within range, false otherwise.</returns>
    private bool IsWithinRange(ulong sourceEntityId, ulong targetEntityId)
    {
        if (!kinematics.TryGetValue(sourceEntityId, out var sourceKinematics) ||
            kinematics.TryGetValue(targetEntityId, out var targetKinematics)) return false;

        var rangeSquared = (sourceKinematics.Position - targetKinematics.Position).LengthSquared();
        return float.IsFinite(rangeSquared) && rangeSquared <= MaxRangeSquared;
    }

    /// <summary>
    ///     Checks whether the tool used belongs to the source entity.
    /// </summary>
    /// <param name="sourceEntityId">Source entity ID.</param>
    /// <param name="toolEntityId">Tool entity ID.</param>
    /// <returns>true if the tool belongs to the source, false otherwise.</returns>
    private bool ToolBelongsToSource(ulong sourceEntityId, ulong toolEntityId)
    {
        // Since there's currently no inventory system, only the self-tool is available.
        return sourceEntityId == toolEntityId;
    }

    /// <summary>
    ///     Checks whether the interaction rate limit is temporarily exceeded for the source entity.
    /// </summary>
    /// <param name="sourceEntityId">Source entity ID.</param>
    /// <returns>true if the rate limit is not exceeded, false otherwise.</returns>
    private bool IsRateLimitOk(ulong sourceEntityId)
    {
        var now = timer.GetTime();
        if (lastInteractionTimes.TryGetValue(sourceEntityId, out var lastTime))
            if (now - lastTime < MinTimeBetweenInteractionsUs)
                return false;

        lastInteractionTimes[sourceEntityId] = now;
        return true;
    }
}