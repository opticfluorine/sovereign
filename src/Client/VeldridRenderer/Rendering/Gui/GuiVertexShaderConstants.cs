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

namespace Sovereign.VeldridRenderer.Rendering.Gui;

/// <summary>
///     Vertex shader constants for GUI rendering.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct GuiVertexShaderConstants
{
    /// <summary>
    ///     Projection transform matrix.
    /// </summary>
    public Matrix4x4 ProjectionMatrix;

    /// <summary>
    ///     Upper-left corner of texture in normalized units.
    /// </summary>
    public Vector2 TextureStart;

    /// <summary>
    ///     Bottom-right corner of texture in normalized units.
    /// </summary>
    public Vector2 TextureEnd;
}