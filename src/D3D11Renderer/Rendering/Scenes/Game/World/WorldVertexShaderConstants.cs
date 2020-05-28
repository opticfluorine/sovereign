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

using SharpDX.Mathematics.Interop;
using System.Runtime.InteropServices;

namespace Sovereign.D3D11Renderer.Rendering.Scenes.Game.World
{

    /// <summary>
    /// Vertex shader constants for world rendering.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct WorldVertexShaderConstants
    {

        /// <summary>
        /// World/view transformation matrix.
        /// </summary>
        public RawMatrix WorldViewTransform;

        /// <summary>
        /// Time since the last tick, in seconds.
        /// </summary>
        public float TimeSinceTick;

        /// <summary>
        /// Reserved for future use.
        /// </summary>
        public float Reserved1;

        /// <summary>
        /// Reserved for future use.
        /// </summary>
        public float Reserved2;

        /// <summary>
        /// Reserved for future use.
        /// </summary>
        public float Reserved3;

    }

}
