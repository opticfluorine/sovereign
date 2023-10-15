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

layout(location = 0) in vec3 vPosition;
layout(location = 1) in vec3 vVelocity;
layout(location = 2) in vec2 vTexCoord;

layout(location = 0) out vec2 vTexCoordOut;

layout(binding = 0) uniform ShaderConstants
{
    mat4 g_transform;
    float g_timeSinceTick;
    float g_reserved0;
    float g_reserved1;
    float g_reserved2;
} shaderConstants;

void main()
{
    // Interpolate the position within the current game tick.
    // This assumes no acceleration is present.
    vec3 interpolated = vPosition + shaderConstants.g_timeSinceTick * vVelocity;
    
    // Embed the position in a vec4 and apply the 2D perspective transform.
    gl_Position = shaderConstants.g_transform * vec4(interpolated, 1.0);
    
    // Forward the texture coordinate for the vertex.
    vTexCoordOut = vTexCoord;
}
