// Sovereign Engine
// Copyright (c) 2024 opticfluorine
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

namespace Sovereign.VeldridRenderer.Rendering.Scenes.Game;

/// <summary>
///     Shader constants for multiple point light related operations.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct PointLightShaderConstants
{
    /// <summary>
    ///     Model-view-projection matrix for light map.
    /// </summary>
    public Matrix4x4 LightTransform;

    /// <summary>
    ///     Light center position in world coordinates.
    /// </summary>
    public Vector3 LightPosition;

    /// <summary>
    ///     Light color.
    /// </summary>
    public Vector3 LightColor;

    /// <summary>
    ///     Radius of the light in world units.
    /// </summary>
    public float Radius;

    /// <summary>
    ///     Light intensity.
    /// </summary>
    public float Intensity;
}