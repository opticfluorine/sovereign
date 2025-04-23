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

using System.Numerics;
using System.Runtime.InteropServices;

namespace Sovereign.VeldridRenderer.Rendering.Scenes.Game.NonBlockShadow;

/// <summary>
///     Shader constants used for the non-block entity shadow map.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct NonBlockShadowShaderConstants
{
    /// <summary>
    ///     Shadow map transformation matrix.
    /// </summary>
    public Matrix4x4 Transform;

    /// <summary>
    ///     Time since the last tick, in seconds.
    /// </summary>
    public float TimeSinceTick;

    /// <summary>
    ///     Unused.
    /// </summary>
    public Vector3 Unused;
}