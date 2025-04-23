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

using System;
using System.Numerics;
using Microsoft.Extensions.Logging;
using Sovereign.ClientCore.Rendering.Resources.Buffers;
using Sovereign.ClientCore.Rendering.Sprites.Atlas;
using Sovereign.EngineCore.Components.Types;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.World;

/// <summary>
///     Loads non-block entity shadow data into the render plan.
/// </summary>
public class NonBlockShadowPlanner
{
    private const int VerticesPerShadow = 4;
    private const int IndicesPerShadow = 6;
    private readonly AtlasMap atlasMap;
    private readonly ILogger<NonBlockShadowPlanner> logger;

    public NonBlockShadowPlanner(AtlasMap atlasMap, ILogger<NonBlockShadowPlanner> logger)
    {
        this.atlasMap = atlasMap;
        this.logger = logger;
    }

    /// <summary>
    ///     Adds a non-block shadow to the render plan.
    /// </summary>
    /// <param name="renderPlan">Render plan.</param>
    /// <param name="shadow">Shadow details.</param>
    /// <param name="posVel">Kinematics for the entity.</param>
    /// <param name="spriteId">Sprite ID.</param>
    public void AddNonBlockShadow(RenderPlan renderPlan, Shadow shadow, Kinematics posVel, int spriteId)
    {
        // Determine extents of quad containing shadow.
        var spriteInfo = atlasMap.MapElements[spriteId];
        var center = new Vector2(posVel.Position.X + 0.5f * spriteInfo.WidthInTiles, posVel.Position.Y);
        var halfDim = new Vector2(shadow.Radius);
        var start = new Vector3(center - halfDim, posVel.Position.Z);
        var end = new Vector3(center + halfDim, posVel.Position.Z);

        // Add shadow to render plan.
        if (!renderPlan.TryAddVertices(VerticesPerShadow, out var vertices, out var baseIndex))
        {
            logger.LogError("Not enough space in vertex buffer.");
            return;
        }

        if (!renderPlan.TryAddShadowIndices(IndicesPerShadow, out var indices, out var indexBaseIndex))
        {
            logger.LogError("Not enough space in index buffer.");
            return;
        }

        AddVertices(vertices, start, end, posVel.Velocity, shadow.Radius, center);
        AddIndices(indices, baseIndex);
    }

    /// <summary>
    ///     Adds vertices for a shadow quad to the vertex buffer.
    /// </summary>
    /// <param name="vertices">Vertex buffer slice.</param>
    /// <param name="start">Bottom left of shadow quad.</param>
    /// <param name="end">Top right of shadow quad.</param>
    /// <param name="velocity">Entity velocity.</param>
    /// <param name="radius">Shadow radius.</param>
    /// <param name="center">Center of shadow in the xy plane.</param>
    private void AddVertices(Span<WorldVertex> vertices, Vector3 start, Vector3 end, Vector3 velocity, float radius,
        Vector2 center)
    {
        // The shadow map pipeline overloads a few of the vertex attributes:
        // - Texture coordinates - used for the center of the shadow in the xy plane.
        // - Light factor - used for the radius of the shadow.

        // Vertex 0, bottom left.
        vertices[0] = new WorldVertex
        {
            PosX = start.X,
            PosY = start.Y,
            PosZ = start.Z,
            VelX = velocity.X,
            VelY = velocity.Y,
            VelZ = velocity.Z,
            TexX = center.X,
            TexY = center.Y,
            LightFactor = radius
        };

        // Vertex 1, bottom right.
        vertices[1] = new WorldVertex
        {
            PosX = end.X,
            PosY = start.Y,
            PosZ = start.Z,
            VelX = velocity.X,
            VelY = velocity.Y,
            VelZ = velocity.Z,
            TexX = center.X,
            TexY = center.Y,
            LightFactor = radius
        };

        // Vertex 2, top left.
        vertices[2] = new WorldVertex
        {
            PosX = start.X,
            PosY = end.Y,
            PosZ = start.Z,
            VelX = velocity.X,
            VelY = velocity.Y,
            VelZ = velocity.Z,
            TexX = center.X,
            TexY = center.Y,
            LightFactor = radius
        };

        // Vertex 3, top right.
        vertices[3] = new WorldVertex
        {
            PosX = end.X,
            PosY = end.Y,
            PosZ = start.Z,
            VelX = velocity.X,
            VelY = velocity.Y,
            VelZ = velocity.Z,
            TexX = center.X,
            TexY = center.Y,
            LightFactor = radius
        };
    }

    /// <summary>
    ///     Adds indices to the index buffer.
    /// </summary>
    /// <param name="indices">Index buffer slice.</param>
    /// <param name="baseIndex">Index of the first vertex.</param>
    private void AddIndices(Span<uint> indices, uint baseIndex)
    {
        indices[0] = baseIndex + 0;
        indices[1] = baseIndex + 1;
        indices[2] = baseIndex + 2;
        indices[3] = baseIndex + 2;
        indices[4] = baseIndex + 1;
        indices[5] = baseIndex + 3;
    }
}