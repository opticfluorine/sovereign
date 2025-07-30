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
using System.Numerics;
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Timing;
using Sovereign.EngineUtil.Ranges;

namespace Sovereign.ServerCore.Systems.Interaction;

/// <summary>
///     Responsible for determining whether an interaction is allowed.
/// </summary>
internal sealed class InteractionValidator(
    KinematicsComponentCollection kinematics,
    ISystemTimer timer,
    BoundingBoxComponentCollection boundingBoxes,
    OrientationComponentCollection orientations,
    ILogger<InteractionValidator> logger,
    LoggingUtil loggingUtil)
{
    /// <summary>
    ///     Square of maximum distance (in world units) in the xy plane at which interactions are allowed.
    /// </summary>
    /// <remarks>
    ///     This should be slightly larger than the corresponding client-side value in PlayerInteractionHandler
    ///     to account for network latency, packet reordering, etc.
    /// </remarks>
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
            !kinematics.TryGetValue(targetEntityId, out var targetKinematics) ||
            !boundingBoxes.TryGetValue(sourceEntityId, out var sourceBox) ||
            !boundingBoxes.TryGetValue(targetEntityId, out var targetBox) ||
            sourceBox.Size == Vector3.Zero ||
            targetBox.Size == Vector3.Zero)
        {
            logger.LogError("Not enough information to check range of {Source} and {Target}.",
                loggingUtil.FormatEntity(sourceEntityId),
                loggingUtil.FormatEntity(targetEntityId));
            return false;
        }

        if (!orientations.TryGetValue(sourceEntityId, out var orientation)) orientation = Orientation.South;

        sourceBox = sourceBox.Translate(sourceKinematics.Position);
        targetBox = targetBox.Translate(targetKinematics.Position);

        var sourcePos = sourceBox.FindInterceptFromCenter(OrientationUtil.GetUnitVector(orientation));

        var targetCenter = targetKinematics.Position + 0.5f * targetBox.Size;
        var targetAim = sourcePos - targetCenter;
        var targetPos = targetBox.FindInterceptFromCenter(targetAim);

        // Note that we skip the orientation check that is done in the client version of this logic.
        // This is to avoid the case where the client and server orientation are out of sync (e.g. reordering
        // of packets resulting in a change of orientation before the interaction request is received).

        // Range comparison is in the xy plane. Check z for simple overlap of bounding boxes.
        var delta = targetPos - sourcePos;
        var delta2 = delta * delta;
        var rangeSquared = delta2.X + delta2.Y;
        return float.IsFinite(rangeSquared) &&
               rangeSquared <= MaxRangeSquared &&
               RangeUtil.RangesIntersect(sourceBox.ZRange, targetBox.ZRange);
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