/*
 * Sovereign Engine
 * Copyright (c) 2025 opticfluorine
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
layout (location = 2) in vec2 shadowCenter;
layout (location = 3) in float radius;

layout (location = 0) out vec2 relPosition;
layout (location = 1) out float radius2;

layout (binding = 0) uniform NonBlockShadowShaderConstants
{
    mat4 g_transform;
    float g_timeSinceTick;
};

void main()
{
    vec3 position = position + velocity * g_timeSinceTick;
    gl_Position = g_transform * vec4(position, 1.0f);
    relPosition = position.xy - shadowCenter;
    radius2 = radius * radius;
}
