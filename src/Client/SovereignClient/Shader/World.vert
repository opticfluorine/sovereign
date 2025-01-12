/*
 * Sovereign Engine
 * Copyright (c) 2022 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

#version 450

layout (location = 0) in vec3 position;
layout (location = 1) in vec3 velocity;
layout (location = 2) in vec2 texCoord;
layout (location = 3) in float lightFactor;

layout (location = 0) out vec2 texCoordOut;
layout (location = 1) out vec4 color;
layout (location = 2) out vec4 shadowPosition;
layout (location = 3) out float vertexDepth;

layout (binding = 0) uniform ShaderConstants
{
    mat4 g_transform;
    mat4 g_shadowMapTransform;
    float g_timeSinceTick;
    float g_yDepthScale;
    float g_yDepthOffset;
    float g_reserved2;
};

void main()
{
    // Interpolate the position within the current game tick.
    // This assumes no acceleration is present.
    vec3 interpolated = position + g_timeSinceTick * velocity;

    // Embed the position in a vec4 and apply the 2D perspective transform.
    vec4 position4 = vec4(interpolated, 1.0);
    gl_Position = g_transform * position4;
    shadowPosition = g_shadowMapTransform * position4;
    vertexDepth = g_yDepthScale * position4.y + g_yDepthOffset;

    // Forward the texture coordinate for the vertex.
    texCoordOut = texCoord;

    // Do not transform the color further in the fragment shader.
    color = vec4(1, 1, 1, 1);
}
