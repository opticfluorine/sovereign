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
using MessagePack;
using Sovereign.EngineUtil.Attributes;

namespace Sovereign.EngineCore.Components.Types;

/// <summary>
///     Represents a bounding box used by the physics engine.
/// </summary>
[Scriptable]
[MessagePackObject]
public struct BoundingBox
{
    /// <summary>
    ///     Position of the upper-top-left corner of the bounding box specified in
    ///     world coordinates relative to the upper-top-left corner of the entity.
    /// </summary>
    [ScriptableField] [Key(0)] public Vector3 Position;

    /// <summary>
    ///     Size of the bounding box specified in world coordinates. Note the inversion
    ///     of the y axis so that (entity position) + Position + Size gives the bottom-right
    ///     corner of the bounding box.
    /// </summary>
    [ScriptableField] [Key(1)] public Vector3 Size;
}