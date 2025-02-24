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
///     Shader constants used by the world fragment shader.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct WorldFragmentShaderConstants
{
    /// <summary>
    ///     Ambient light color (e.g. shadows, areas with no lighting).
    /// </summary>
    public Vector4 AmbientLightColor;

    /// <summary>
    ///     Global light color (e.g. sun, moon).
    /// </summary>
    public Vector4 GlobalLightColor;

    /// <summary>
    ///     Viewport size in pixels.
    /// </summary>
    public Vector2 ViewportSize;

    /// <summary>
    ///     Unused.
    /// </summary>
    public Vector2 Unused;
}