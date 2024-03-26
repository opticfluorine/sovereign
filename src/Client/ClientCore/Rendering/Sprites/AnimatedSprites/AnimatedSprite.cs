/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Sovereign.EngineCore.Components.Types;
using static Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites.AnimatedSpriteDefinitions;

namespace Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;

/// <summary>
///     Describes a single animated sprite.
/// </summary>
public sealed class AnimatedSprite
{
    /// <summary>
    ///     Default orientation.
    /// </summary>
    private const Orientation DefaultOrientation = Orientation.South;

    /// <summary>
    ///     Offset to add to an animated sprite ID to get the GUI texture handle.
    /// </summary>
    private const int guiTextureOffset = 2;

    /// <summary>
    ///     Specifies which orientation to try next if a given orientation is not defined for a sprite.
    /// </summary>
    private static readonly Dictionary<Orientation, Orientation> FallbackOrientations = new()
    {
        { Orientation.Northeast, Orientation.East },
        { Orientation.Southeast, Orientation.East },
        { Orientation.Northwest, Orientation.West },
        { Orientation.Southwest, Orientation.West },
        { Orientation.North, Orientation.South },
        { Orientation.East, Orientation.South },
        { Orientation.West, Orientation.South }
    };

    private readonly AnimatedSpriteDefinition definition;
    private readonly SpriteManager spriteManager;

    public AnimatedSprite(AnimatedSpriteDefinition definition, SpriteManager spriteManager)
    {
        this.definition = definition;
        this.spriteManager = spriteManager;

        RebuildFrames();
    }

    /// <summary>
    ///     Time to display each frame, in microseconds.
    /// </summary>
    public ulong FrameTime { get; set; }

    /// <summary>
    ///     Sprite IDs for each defined orientation.
    /// </summary>
    public Dictionary<Orientation, List<Sprite>> Faces { get; set; }

    /// <summary>
    ///     Rebuilds the frame data if the definition and/or the sprite table is updated.
    /// </summary>
    [MemberNotNull("FrameTime")]
    [MemberNotNull("Faces")]
    public void RebuildFrames()
    {
        FrameTime = definition.AnimationTimestep;
        Faces = definition.Faces
            .Select(pair => new Tuple<Orientation, List<Sprite>>(pair.Key,
                pair.Value.SpriteIds
                    .Select(id => spriteManager.Sprites[id])
                    .ToList()))
            .ToDictionary(
                pair => pair.Item1,
                pair => pair.Item2);
    }

    /// <summary>
    ///     Converts an animated sprite ID to the corresponding GUI texture handle.
    /// </summary>
    /// <param name="animatedSpriteId">Animated sprite ID.</param>
    /// <returns>GUI texture handle.</returns>
    public static IntPtr ToGuiTextureHandle(int animatedSpriteId)
    {
        return new IntPtr(animatedSpriteId + guiTextureOffset);
    }

    /// <summary>
    ///     Converts a GUI texture handle to the corresponding animated sprite ID.
    /// </summary>
    /// <param name="textureHandle">GUI texture handle.</param>
    /// <returns>Animated sprite ID.</returns>
    public static int FromGuiTextureHandle(IntPtr textureHandle)
    {
        return (int)textureHandle - guiTextureOffset;
    }

    /// <summary>
    ///     Gets the sprite for the given system time and orientation.
    /// </summary>
    /// <param name="systemTime">System time.</param>
    /// <param name="orientation">Orientation.</param>
    /// <returns>Sprite for the given system time and orientation.</returns>
    public Sprite GetSpriteForTime(ulong systemTime, Orientation orientation)
    {
        var sprites = GetSpritesForOrientation(orientation);
        var spriteFrame = (int)(systemTime / FrameTime) % sprites.Count;
        return sprites[spriteFrame];
    }

    /// <summary>
    ///     Gets the sprite list for the given orientation, applying fallback logic if the specified
    ///     orientation does not have a defined face for this sprite.
    /// </summary>
    /// <param name="orientation">Orientation.</param>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if no suitable face is found for this sprite.
    /// </exception>
    /// <returns></returns>
    private List<Sprite> GetSpritesForOrientation(Orientation orientation)
    {
        // Iterate over the fallback table until we reach an orientation with a face definition.
        var nextOrientation = orientation;
        while (!Faces.ContainsKey(nextOrientation) && nextOrientation != DefaultOrientation)
            nextOrientation = FallbackOrientations[nextOrientation];

        if (!Faces.ContainsKey(nextOrientation))
            throw new InvalidOperationException("No suitable face found for sprite.");

        return Faces[nextOrientation];
    }
}