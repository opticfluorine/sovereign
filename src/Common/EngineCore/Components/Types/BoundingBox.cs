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
using System.Diagnostics;
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
    ///     Range of the Z edges of the box.
    /// </summary>
    [IgnoreMember]
    public Vector2 ZRange => new(Position.Z, Position.Z + Size.Z);

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
    ///     Gets the center point in the XY plane at the Z depth of the base of the box.
    /// </summary>
    [IgnoreMember]
    public Vector3 CenterXy => (Position + 0.5f * Size) with { Z = Position.Z };

    /// <summary>
    ///     Determines whether this bounding box intersects another. If it does, determines the
    ///     translation vector that resolves the intersection when applied to this BoundingBox.
    /// </summary>
    /// <param name="other">Other BoundingBox to test against.</param>
    /// <param name="velocity">Velocity of the entity to which this BoundingBox is attached.</param>
    /// <param name="resolvingTranslation">
    ///     Smallest translation that resolves the intersection to zero overlap when applied
    ///     to this BoundingBox. Only valid if this method returns true.
    /// </param>
    /// <param name="minimumAbsOverlap">Smallest absolute overlap among all three axes.</param>
    /// <param name="resolvedSurfaceNormal">
    ///     If resolvingTranslation is nonzero, set to a normal vector to the collision
    ///     surface.
    /// </param>
    /// <returns>true if the boxes intersect (including touching), false otherwise.</returns>
    public bool Intersects(BoundingBox other, Vector3 velocity, out Vector3 resolvingTranslation,
        out float minimumAbsOverlap, out Vector3 resolvedSurfaceNormal)
    {
        resolvingTranslation = Vector3.Zero;
        resolvedSurfaceNormal = Vector3.Zero;
        minimumAbsOverlap = 0.0f;

        var amin = Position;
        var amax = amin + Size;
        var bmin = other.Position;
        var bmax = bmin + other.Size;

        var intersects = amin.X <= bmax.X && amax.X >= bmin.X
                                          && amin.Y <= bmax.Y && amax.Y >= bmin.Y
                                          && amin.Z <= bmax.Z && amax.Z >= bmin.Z;
        if (!intersects) return false;

        var a0Diff = amin - bmax;
        var a1Diff = amax - bmin;
        var shifts = Vector3.MinMagnitude(a0Diff, a1Diff);
        minimumAbsOverlap = Math.Abs(AbsMin(shifts.X, AbsMin(shifts.Y, shifts.Z)));

        // If we have a static collision, leave the resolving vector as zero.
        if (velocity == Vector3.Zero) return true;

        // If we have a dynamic collision, find the reverse time delta (which I'm calling
        // "alpha") that rewinds motion to the beginning of collision where the overlap
        // volume is zero.
        var a0Alpha = a0Diff / velocity;
        var a1Alpha = a1Diff / velocity;

        // The smallest non-negative alpha gives the correct rollback to the moment of collision.
        // Some of the alpha may be +/- infinity if velocity has zero-valued components; this
        // is OK, and they will be discarded by this logic. As long as the velocity is nonzero,
        // there will always be at least two finite alpha, one of which will always be positive
        // (or zero for a non-overlapping collision which does not require resolution).
        var minNonNegativeAlpha = float.MaxValue;
        if (a0Alpha.X >= 0.0f)
        {
            minNonNegativeAlpha = a0Alpha.X;
            resolvedSurfaceNormal = new Vector3(1, 0, 0);
        }

        if (a0Alpha.Y >= 0.0f && a0Alpha.Y < minNonNegativeAlpha)
        {
            minNonNegativeAlpha = a0Alpha.Y;
            resolvedSurfaceNormal = new Vector3(0, 1, 0);
        }

        if (a0Alpha.Z >= 0.0f && a0Alpha.Z < minNonNegativeAlpha)
        {
            minNonNegativeAlpha = a0Alpha.Z;
            resolvedSurfaceNormal = new Vector3(0, 0, 1);
        }

        if (a1Alpha.X >= 0.0f && a1Alpha.X < minNonNegativeAlpha)
        {
            minNonNegativeAlpha = a1Alpha.X;
            resolvedSurfaceNormal = new Vector3(1, 0, 0);
        }

        if (a1Alpha.Y >= 0.0f && a1Alpha.Y < minNonNegativeAlpha)
        {
            minNonNegativeAlpha = a1Alpha.Y;
            resolvedSurfaceNormal = new Vector3(0, 1, 0);
        }

        if (a1Alpha.Z >= 0.0f && a1Alpha.Z < minNonNegativeAlpha)
        {
            minNonNegativeAlpha = a1Alpha.Z;
            resolvedSurfaceNormal = new Vector3(0, 0, 1);
        }

        resolvingTranslation = -minNonNegativeAlpha * velocity;
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

    /// <summary>
    ///     Finds the point where the given vector intercepts the faces when marched forward
    ///     from the center of the bounding box in the center z plane of the bounding box.
    /// </summary>
    /// <param name="aimVector">Non-zero vector to march from the box center.</param>
    /// <returns>Intercept point.</returns>
    /// <remarks>
    ///     If the bounding box has any zero-length sides, those dimensions are ignored. The bounding box must
    ///     have at least one side with a non-zero length.
    /// </remarks>
    public Vector3 FindInterceptFromCenter(Vector3 aimVector)
    {
        Debug.Assert(aimVector != Vector3.Zero);

        if (Size == Vector3.Zero) return Position;

        // If the entity has a bounding box, we should check the interaction by a forward-facing
        // point on the box instead of the entity's point position. Note that if we start from the center
        // of the box, the symmetry of the box means that the two intercepts are the same distance (opposite sign)
        // from the center, so we only need to compute the absolute distance and apply a positive translation by
        // that amount.
        var p0 = Position;
        var pC = p0 + 0.5f * Size;

        var minAlpha = float.MaxValue;
        if (float.IsNormal(aimVector.X) && float.IsNormal(Size.X))
        {
            // Intercept could be on a constant-x face.
            minAlpha = Math.Abs((p0.X - pC.X) / aimVector.X);
        }

        if (float.IsNormal(aimVector.Y) && float.IsNormal(Size.Y))
        {
            // Intercept could be on a constant-y face.
            var alpha = Math.Abs((p0.Y - pC.Y) / aimVector.Y);
            minAlpha = Math.Min(alpha, minAlpha);
        }

        if (float.IsNormal(aimVector.Z) && float.IsNormal(Size.Z))
        {
            // Intercept could be on a constant-z face.
            var alpha = Math.Abs((p0.Z - pC.Z) / aimVector.Z);
            minAlpha = Math.Min(alpha, minAlpha);
        }

        return pC + minAlpha * aimVector;
    }
}