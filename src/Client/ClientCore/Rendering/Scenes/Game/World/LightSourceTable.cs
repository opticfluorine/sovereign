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
using System.Threading;
using Microsoft.Extensions.Logging;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.ClientCore.Rendering.Sprites.Atlas;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;
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
    private readonly AnimatedSpriteManager animatedSpriteManager;
    private readonly AnimatedSpriteComponentCollection animatedSprites;
    private readonly AtlasMap atlasMap;
    private readonly EntityTable entityTable;
    private readonly KinematicsComponentCollection kinematics;

    private readonly HashSet<ulong> knownSources = new();
    private readonly ILogger<LightSourceTable> logger;
    private readonly ParentComponentCollection parents;
    private readonly PointLightSourceComponentCollection pointLightSources;
    private readonly Lock sourcesLock = new();

    public LightSourceTable(PointLightSourceComponentCollection pointLightSources,
        KinematicsComponentCollection kinematics, ParentComponentCollection parents,
        ILogger<LightSourceTable> logger, AnimatedSpriteComponentCollection animatedSprites,
        AnimatedSpriteManager animatedSpriteManager, AtlasMap atlasMap, EntityTable entityTable)
    {
        this.pointLightSources = pointLightSources;
        this.kinematics = kinematics;
        this.parents = parents;
        this.logger = logger;
        this.animatedSprites = animatedSprites;
        this.animatedSpriteManager = animatedSpriteManager;
        this.atlasMap = atlasMap;
        this.entityTable = entityTable;

        pointLightSources.OnComponentAdded += OnLightAdded;
        pointLightSources.OnComponentRemoved += OnLightRemoved;

        entityTable.OnTemplateSet += OnTemplateSet;
        entityTable.OnEntityRemoved += OnEntityRemoved;
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
        lock (sourcesLock)
        {
            foreach (var lightEntityId in knownSources)
            {
                if (!kinematics.TryFindNearest(lightEntityId, parents, out var posVel, out var parentEntityId))
                {
                    logger.LogWarning("Entity {EntityId:X} has unpositioned light source.", lightEntityId);
                    continue;
                }

                if (!pointLightSources.TryGetValue(lightEntityId, out var details))
                {
                    logger.LogWarning("Entity {EntityId:X} in light list without light data.", lightEntityId);
                    continue;
                }

                // Ignore lights with no emission.
                if (details.Intensity <= 0.0f) continue;

                // Determine true center of light.
                var basePos = posVel.Position + timeSinceTick * posVel.Velocity;
                var offset = Vector3.Zero;
                if (animatedSprites.HasComponentForEntity(parentEntityId))
                {
                    // Compute a relative offset based on the sprite size.
                    // Assume the sprite size is constant across all phases, orientations, and frames.
                    var sprite = animatedSpriteManager.AnimatedSprites[animatedSprites[parentEntityId]]
                        .GetDefaultSprite();
                    var spriteInfo = atlasMap.MapElements[sprite.Id];
                    offset = details.PositionOffset * new Vector3(spriteInfo.WidthInTiles, spriteInfo.HeightInTiles,
                        spriteInfo.HeightInTiles);
                }

                var lightCenter = basePos + offset;

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
    }

    /// <summary>
    ///     Called when a point light source is added.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="value">Unused.</param>
    /// <param name="isLoad">Unused.</param>
    private void OnLightAdded(ulong entityId, PointLight value, bool isLoad)
    {
        if (EntityUtil.IsTemplateEntity(entityId))
        {
            // If a template entity received a new light source, its instances need to be added to the table.
            lock (sourcesLock)
            {
                foreach (var instanceId in entityTable.GetInstancesOfTemplate(entityId))
                {
                    knownSources.Add(instanceId);
                }
            }

            return;
        }

        lock (sourcesLock)
        {
            knownSources.Add(entityId);
        }
    }

    /// <summary>
    ///     Called when a point light source is removed.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="isUnload">Unused.</param>
    private void OnLightRemoved(ulong entityId, bool isUnload)
    {
        if (EntityUtil.IsTemplateEntity(entityId))
        {
            // If a template entity lost a light source, then its instances need to be removed if they do not
            // have their own light source.
            lock (sourcesLock)
            {
                foreach (var instanceId in entityTable.GetInstancesOfTemplate(entityId))
                {
                    if (pointLightSources.HasLocalComponentForEntity(instanceId)) continue;
                    knownSources.Remove(instanceId);
                }
            }

            return;
        }

        lock (sourcesLock)
        {
            knownSources.Remove(entityId);
        }
    }

    /// <summary>
    ///     Called when a template is set to an entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="templateEntityId">Template entity ID.</param>
    /// <param name="oldTemplateId">Old template, or zero if there is no old template.</param>
    /// <param name="isLoad">Load flag.</param>
    /// <param name="isNew">New flag.</param>
    private void OnTemplateSet(ulong entityId, ulong templateEntityId, ulong oldTemplateId, bool isLoad, bool isNew)
    {
        lock (sourcesLock)
        {
            knownSources.Remove(entityId);
            if (pointLightSources.HasComponentForEntity(entityId)) knownSources.Add(entityId);
        }
    }

    /// <summary>
    ///     Called when an entity is removed or unloaded.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="isUnload">Unload flag.</param>
    private void OnEntityRemoved(ulong entityId, bool isUnload)
    {
        lock (sourcesLock)
        {
            knownSources.Remove(entityId);
        }
    }
}