/*
 * Sovereign Engine
 * Copyright (c) 2024 opticfluorine
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

// We only use the position, but specify the full format of the vertex so that we can
// reuse the same vertex buffer as the world vertex shader.
layout (location = 0) in vec3 vPosition;
layout (location = 1) in vec3 vVelocity;
layout (location = 2) in vec2 vTexCoord;
layout (location = 3) in float unused;
layout (location = 4) in float unused2;
layout (location = 5) in float unused3;

layout (binding = 0) uniform ShaderConstants
{
    mat4 g_transform; // projection * rotation
} shaderConstants;

void main()
{
    // Rotate by the camera angle, then apply the orthographic projection.
    // No translation is necessary due to the symmetry of the projection.
    // These operations are combined into a single 4x4 matrix.
    gl_Position = shaderConstants.g_transform * vec4(vPosition, 1.0);
}
