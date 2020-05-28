/*
 * Sovereign Engine
 * Copyright (c) 2020 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Runtime.InteropServices;

namespace Sovereign.ClientCore.Rendering.Resources.Buffers
{

    /// <summary>
    /// Vertex type for world rendering.
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

        [FieldOffset(6 * sizeof(float))] public float TexX;
        [FieldOffset(7 * sizeof(float))] public float TexY;
    }

}
