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
    [ScriptableField] [Key(0)] public Vector3 Position;

    /// <summary>
    ///     Size of the bounding box specified in world coordinates.
    /// </summary>
    [ScriptableField] [Key(1)] public Vector3 Size;

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
            Math.Min(bmax.X - amin.X, bmin.X - amax.X),
            Math.Min(bmax.Y - amin.Y, bmin.Y - amax.Y),
            Math.Min(bmax.Z - amin.Z, bmin.Z - amax.Z)
        );
        var absShifts = new Vector3(Math.Abs(shifts.X), Math.Abs(shifts.Y), Math.Abs(shifts.Z));
        minimumAbsOverlap = Math.Min(absShifts.X, Math.Min(absShifts.Y, absShifts.Z));
        resolvingTranslation = new Vector3(
            absShifts.X > minimumAbsOverlap ? 0.0f : shifts.X,
            absShifts.Y > minimumAbsOverlap ? 0.0f : shifts.Y,
            absShifts.Z > minimumAbsOverlap ? 0.0f : shifts.Z
        );

        return true;
    }
}