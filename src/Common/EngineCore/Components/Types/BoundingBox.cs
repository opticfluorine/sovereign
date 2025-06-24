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

using System;
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
    ///     Position of the bottom-front-left corner of the bounding box specified in
    ///     world coordinates relative to the upper-top-left corner of the entity.
    /// </summary>
    [ScriptableField]
    [Key(0)]
    public Vector3 Position { get; set; }

    /// <summary>
    ///     Size of the bounding box specified in world coordinates.
    /// </summary>
    [ScriptableField]
    [Key(1)]
    public Vector3 Size { get; set; }

    /// <summary>
    ///     Generates a new BoundingBox by applying a translation to the current BoundingBox.
    /// </summary>
    /// <param name="translation">Translation to apply.</param>
    /// <returns>Translated BoundingBox.</returns>
    public BoundingBox Translate(Vector3 translation)
    {
        return this with { Position = Position + translation };
    }

    /// <summary>
    ///     Determines whether this bounding box intersects another. If it does, determines the
    ///     translation vector that resolves the intersection when applied to this BoundingBox.
    /// </summary>
    /// <param name="other">Other BoundingBox to test against.</param>
    /// <param name="resolvingTranslation">
    ///     Smallest translation that resolves the intersection to zero overlap when applied
    ///     to this BoundingBox. Only valid if this method returns true.
    /// </param>
    /// <param name="minimumAbsOverlap">Smallest </param>
    /// <returns>true if the boxes intersect (including touching), false otherwise.</returns>
    public bool Intersects(BoundingBox other, out Vector3 resolvingTranslation, out float minimumAbsOverlap)
    {
        resolvingTranslation = Vector3.Zero;
        minimumAbsOverlap = 0.0f;

        var amin = Position;
        var amax = amin + Size;
        var bmin = other.Position;
        var bmax = bmin + other.Size;

        var intersects = amin.X <= bmax.X && amax.X >= bmin.X
                                          && amin.Y <= bmax.Y && amax.Y >= bmin.Y
                                          && amin.Z <= bmax.Z && amax.Z >= bmin.Z;
        if (!intersects) return false;

        var shifts = new Vector3(
            AbsMin(bmax.X - amin.X, bmin.X - amax.X),
            AbsMin(bmax.Y - amin.Y, bmin.Y - amax.Y),
            AbsMin(bmax.Z - amin.Z, bmin.Z - amax.Z)
        );

        minimumAbsOverlap = Math.Abs(AbsMin(shifts.X, AbsMin(shifts.Y, shifts.Z)));
        resolvingTranslation = new Vector3(
            Math.Abs(shifts.X) > minimumAbsOverlap ? 0.0f : shifts.X,
            Math.Abs(shifts.Y) > minimumAbsOverlap ? 0.0f : shifts.Y,
            Math.Abs(shifts.Z) > minimumAbsOverlap ? 0.0f : shifts.Z
        );

        return true;
    }

    /// <summary>
    ///     Selects the value with the minimum absolute value. If the absolute values are equal, returns the first.
    /// </summary>
    /// <param name="a">First number.</param>
    /// <param name="b">Second number.</param>
    /// <returns>Number with smallest absolute value, or the first number if absolute values are equal.</returns>
    private static float AbsMin(float a, float b)
    {
        return Math.Abs(a) <= Math.Abs(b) ? a : b;
    }
}