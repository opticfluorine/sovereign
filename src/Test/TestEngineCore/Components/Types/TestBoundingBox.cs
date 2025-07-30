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
using Xunit;

namespace Sovereign.EngineCore.Components.Types;

/// <summary>
///     Unit tests for the BoundingBox class.
/// </summary>
public class TestBoundingBox
{
    [Fact]
    public void Translate_ChangesPositionCorrectly()
    {
        var box = new BoundingBox { Position = new Vector3(1, 1, 1), Size = new Vector3(2, 2, 2) };
        var translation = new Vector3(3, 3, 3);
        var translatedBox = box.Translate(translation);
        Assert.Equal(new Vector3(4, 4, 4), translatedBox.Position);
        Assert.Equal(box.Size, translatedBox.Size);
    }

    [Fact]
    public void Intersects_ReturnsTrueForOverlappingBoxes()
    {
        var box1 = new BoundingBox { Position = Vector3.Zero, Size = new Vector3(2, 2, 2) };
        var box2 = new BoundingBox { Position = new Vector3(1.5f, 1, 1), Size = new Vector3(2, 2, 2) };
        var velocity = new Vector3(1.0f, 0.0f, 0.0f);
        var result = box1.Intersects(box2, velocity, out var resolvingTranslation, out var minimumAbsOverlap,
            out var surfaceNormal);
        Assert.True(result);
        Assert.Equal(0.5f, minimumAbsOverlap);
        Assert.Equal(new Vector3(-0.5f, 0, 0), resolvingTranslation);
        Assert.Equal(new Vector3(1.0f, 0.0f, 0.0f), surfaceNormal);
    }

    [Fact]
    public void Intersects_ReturnsFalseForNonOverlappingBoxes()
    {
        var box1 = new BoundingBox { Position = Vector3.Zero, Size = new Vector3(2, 2, 2) };
        var box2 = new BoundingBox { Position = new Vector3(3, 3, 3), Size = new Vector3(2, 2, 2) };
        var result = box1.Intersects(box2, Vector3.One, out var resolvingTranslation, out var minimumAbsOverlap,
            out var surfaceNormal);
        Assert.False(result);
    }

    [Fact]
    public void Intersects_ReturnsTrueForTouchingBoxesX()
    {
        var box1 = new BoundingBox { Position = Vector3.Zero, Size = new Vector3(2, 2, 2) };
        var box2 = new BoundingBox { Position = new Vector3(2, 0, 0), Size = new Vector3(2, 2, 2) };
        var result = box1.Intersects(box2, Vector3.One, out var resolvingTranslation, out var minimumAbsOverlap,
            out var surfaceNormal);
        Assert.True(result);
        Assert.Equal(Vector3.Zero, resolvingTranslation);
        Assert.Equal(0, minimumAbsOverlap);
    }

    [Fact]
    public void Intersects_ReturnsTrueForTouchingBoxesY()
    {
        var box1 = new BoundingBox { Position = Vector3.Zero, Size = new Vector3(2, 2, 2) };
        var box2 = new BoundingBox { Position = new Vector3(0, 2, 0), Size = new Vector3(2, 2, 2) };
        var result = box1.Intersects(box2, Vector3.One, out var resolvingTranslation, out var minimumAbsOverlap,
            out var surfaceNormal);
        Assert.True(result);
        Assert.Equal(Vector3.Zero, resolvingTranslation);
        Assert.Equal(0, minimumAbsOverlap);
    }

    [Fact]
    public void Intersects_ReturnsTrueForTouchingBoxesZ()
    {
        var box1 = new BoundingBox { Position = Vector3.Zero, Size = new Vector3(2, 2, 2) };
        var box2 = new BoundingBox { Position = new Vector3(0, 0, 2), Size = new Vector3(2, 2, 2) };
        var result = box1.Intersects(box2, Vector3.One, out var resolvingTranslation, out var minimumAbsOverlap,
            out var surfaceNormal);
        Assert.True(result);
        Assert.Equal(Vector3.Zero, resolvingTranslation);
        Assert.Equal(0, minimumAbsOverlap);
    }

    [Fact]
    public void Intersects_ReturnsFalseForAlignedButNonOverlappingBoxes()
    {
        var box1 = new BoundingBox { Position = new Vector3(0, 0, 0), Size = new Vector3(2, 2, 2) };
        var box2 = new BoundingBox { Position = new Vector3(2, 1, 3), Size = new Vector3(2, 2, 2) };
        var result = box1.Intersects(box2, Vector3.One, out var resolvingTranslation, out var minimumAbsOverlap,
            out var surfaceNormal);
        Assert.False(result);
    }

    [Fact]
    public void Intersects_ResolvesIntersectionCorrectly()
    {
        var box1 = new BoundingBox { Position = Vector3.Zero, Size = new Vector3(2, 2, 2) };
        var box2 = new BoundingBox { Position = new Vector3(1, 1, 1), Size = new Vector3(2, 2, 2) };

        // Step 1: Check initial intersection
        var initialResult = box1.Intersects(box2, Vector3.One, out var resolvingTranslation, out var minimumAbsOverlap,
            out var surfaceNormal);
        Assert.True(initialResult);

        // Step 2: Translate box1 by the resolving translation
        var translatedBox1 = box1.Translate(resolvingTranslation);

        // Step 3: Check intersection after translation
        var finalResult =
            translatedBox1.Intersects(box2, Vector3.One, out var finalResolvingTranslation,
                out var finalMinimumAbsOverlap, out var finalSurfaceNormal);
        Assert.True(finalResult);
        Assert.Equal(Vector3.Zero, finalResolvingTranslation);
    }

    /// <summary>
    ///     Tests that partial overlaps in z from above are resolved in the correct direction.
    ///     This is the common case for gravity pulling an entity onto a surface.
    /// </summary>
    [Fact]
    public void Intersects_ResolvesZOverlapCorrectly()
    {
        var box1 = new BoundingBox { Position = new Vector3(0, 0, 0.9f), Size = Vector3.One };
        var box2 = new BoundingBox { Position = Vector3.Zero, Size = Vector3.One };

        // Step 1: Check initial intersection (using negative velocity to mimic gravity)
        var velocity = new Vector3(0.0f, 0.0f, -1.0f);
        var initialResult =
            box1.Intersects(box2, velocity, out var resolvingTranslation, out var minimumAbsOverlap,
                out var surfaceNormal);
        Assert.True(initialResult);
        Assert.Equal(new Vector3(0, 0, 1), surfaceNormal);

        // Step 2: Translate box1 by the resolving translation
        var translatedBox1 = box1.Translate(resolvingTranslation);

        // Step 3: Check that the translated box1 has a larger z position than box2
        Assert.True(translatedBox1.Position.Z > box2.Position.Z);

        // Step 4: Check that the x and y positions are equal for both boxes
        Assert.Equal(translatedBox1.Position.X, box2.Position.X);
        Assert.Equal(translatedBox1.Position.Y, box2.Position.Y);

        // Step 5: Check that Intersects called on translatedBox1 with box2 gives zero minimumAbsOverlap
        var finalResult =
            translatedBox1.Intersects(box2, Vector3.One, out var finalResolvingTranslation,
                out var finalMinimumAbsOverlap, out var finalSurfaceNormal);
        Assert.True(finalResult);
        Assert.Equal(0.0f, finalMinimumAbsOverlap);
    }
}