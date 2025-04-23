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

layout (location = 0) in vec2 relPosition;
layout (location = 1) in float radius2;

void main()
{
    float r2 = dot(relPosition, relPosition);
    gl_FragDepth = r2 <= radius2 ? gl_FragCoord.z : 1.0f;
}
