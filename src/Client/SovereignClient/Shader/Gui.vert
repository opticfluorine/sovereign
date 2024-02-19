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

layout (binding = 0) uniform ShaderConstants
{
    mat4 g_projection;
    vec2 g_texStart;
    vec2 g_texEnd;
} shaderConstants;

layout (location = 0) in vec2 vPosition;
layout (location = 1) in vec2 vTexCoord;
layout (location = 2) in vec4 vColor;

layout (location = 0) out vec2 vTexCoordOut;
layout (location = 1) out vec4 vColorOut;

void main()
{
    gl_Position = shaderConstants.g_projection * vec4(vPosition, 0, 1);
    vColorOut = vColor;
    vTexCoordOut = shaderConstants.g_Start + vTexCoord * (shaderConstants.g_texEnd - shaderConstants.g_texStart);
}
