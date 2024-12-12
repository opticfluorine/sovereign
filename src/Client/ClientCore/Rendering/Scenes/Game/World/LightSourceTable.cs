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

using System.Collections.Generic;
using System.Numerics;
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineUtil.Ranges;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.World;

/// <summary>
///     Summarizes information about a point light source required for rendering.
/// </summary>
public readonly struct PositionedLight(int index, Vector3 position, PointLight details)
{
    /// <summary>
    ///     Light index for this frame.
    /// </summary>
    public readonly int Index = index;

    /// <summary>
    ///     Light position.
    /// </summary>
    public readonly Vector3 Position = position;

    /// <summary>
    ///     Light details.
    /// </summary>
    public readonly PointLight Details = details;
}

/// <summary>
///     Responsible for tracking point light sources which need to be considered for rendering.
/// </summary>
public class LightSourceTable
{
    private readonly KinematicsComponentCollection kinematics;

    private readonly List<ulong> knownSources = new();
    private readonly ILogger<LightSourceTable> logger;
    private readonly ParentComponentCollection parents;
    private readonly PointLightSourceComponentCollection pointLightSources;

    public LightSourceTable(PointLightSourceComponentCollection pointLightSources,
        KinematicsComponentCollection kinematics, ParentComponentCollection parents,
        ILogger<LightSourceTable> logger)
    {
        this.pointLightSources = pointLightSources;
        this.kinematics = kinematics;
        this.parents = parents;
        this.logger = logger;

        pointLightSources.OnComponentAdded += OnLightAdded;
        pointLightSources.OnComponentRemoved += OnLightRemoved;
    }

    /// <summary>
    ///     Gets the point light sources whose emissions overlap the given range.
    /// </summary>
    /// <param name="minExtent">Minimum extent of search range.</param>
    /// <param name="maxExtent">Maximum extent of search range.</param>
    /// <param name="timeSinceTick">Time since beginning of tick, in seconds.</param>
    /// <param name="lights">List to append with the positioned light data.</param>
    public void GetLightsInRange(Vector3 minExtent, Vector3 maxExtent, float timeSinceTick,
        List<PositionedLight> lights)
    {
        var nextIndex = 0;
        foreach (var lightEntityId in knownSources)
        {
            if (!kinematics.TryFindNearest(lightEntityId, parents, out var posVel))
            {
                logger.LogWarning("Entity {0} has unpositioned light source.", lightEntityId);
                continue;
            }

            if (!pointLightSources.TryGetValue(lightEntityId, out var details))
            {
                logger.LogWarning("Entity {0} in light list without light data.", lightEntityId);
                continue;
            }

            // Ignore lights with no emission.
            if (details.Intensity <= 0.0f) continue;

            // Determine true center of light.
            var basePos = posVel.Position + timeSinceTick * posVel.Velocity;
            var lightCenter = basePos + details.PositionOffset;

            // Expand the search range by the radius of the light, then check whether the true
            // center lies within this expanded range. If so, the light needs to be considered
            // for rendering.
            var radiusVector = new Vector3(details.Radius);
            var adjMinExtent = minExtent - radiusVector;
            var adjMaxExtent = maxExtent + radiusVector;
            if (!RangeUtil.IsPointInRange(adjMinExtent, adjMaxExtent, lightCenter)) continue;

            // If we get here, we need to include the light when rendering.
            lights.Add(new PositionedLight(nextIndex++, lightCenter, details));
        }
    }

    /// <summary>
    ///     Called when a point light source is added.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="value">Unused.</param>
    /// <param name="isLoad">Unused.</param>
    private void OnLightAdded(ulong entityId, PointLight value, bool isLoad)
    {
        knownSources.Add(entityId);
    }

    /// <summary>
    ///     Called when a point light source is removed.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="isUnload">Unused.</param>
    private void OnLightRemoved(ulong entityId, bool isUnload)
    {
        knownSources.Remove(entityId);
    }
}