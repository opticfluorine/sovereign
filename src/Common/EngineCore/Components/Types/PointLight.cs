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
using MessagePack;

namespace Sovereign.EngineCore.Components.Types;

/// <summary>
///     Describes a point light source.
/// </summary>
[MessagePackObject]
public struct PointLight
{
    /// <summary>
    ///     Radius of light source, in blocks.
    /// </summary>
    [Key(0)] public float Radius;

    /// <summary>
    ///     Intensity of the light source.
    /// </summary>
    [Key(1)] public float Intensity;

    /// <summary>
    ///     RGB color of light source.
    /// </summary>
    [Key(2)] public Vector3 Color;

    /// <summary>
    ///     Position offset relative to the position of the associated entity.
    /// </summary>
    [Key(3)] public Vector3 PositionOffset;
}