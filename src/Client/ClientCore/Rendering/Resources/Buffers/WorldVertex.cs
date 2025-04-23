/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
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

using System.Runtime.InteropServices;

namespace Sovereign.ClientCore.Rendering.Resources.Buffers;

/// <summary>
///     Vertex type for world rendering.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public struct WorldVertex
{
    [FieldOffset(0)] public float PosX;
    [FieldOffset(sizeof(float))] public float PosY;
    [FieldOffset(2 * sizeof(float))] public float PosZ;

    [FieldOffset(3 * sizeof(float))] public float VelX;
    [FieldOffset(4 * sizeof(float))] public float VelY;
    [FieldOffset(5 * sizeof(float))] public float VelZ;

    /// <summary>
    ///     Context-specific vector value (x component).
    /// </summary>
    /// <remarks>
    ///     In C this would be a union based on the specific rendering
    ///     pipeline in use. Uses include:
    ///     * World rendering: texture coordinate
    ///     * Non-block shadow map: shadow center in xy plane
    /// </remarks>
    [FieldOffset(6 * sizeof(float))] public float TexX;

    /// <summary>
    ///     Context-specific vector value (x component).
    /// </summary>
    /// <remarks>
    ///     In C this would be a union based on the specific rendering
    ///     pipeline in use. Uses include:
    ///     * World rendering: texture coordinate
    ///     * Non-block shadow map: shadow center in xy plane
    /// </remarks>
    [FieldOffset(7 * sizeof(float))] public float TexY;

    /// <summary>
    ///     Context-specific scalar value.
    /// </summary>
    /// <remarks>
    ///     In C this would be a union based on the specific rendering
    ///     pipeline in use. Uses include:
    ///     * World rendering: light factor
    ///     * Non-block shadow map: shadow radius
    /// </remarks>
    [FieldOffset(8 * sizeof(float))] public float LightFactor;
}