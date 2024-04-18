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

    public AnimatedSprite()
    {
        Phases[AnimationPhase.Default] = new AnimationPhaseData();
    }

    public AnimatedSprite(AnimatedSpriteDefinition definition, SpriteManager spriteManager)
    {
        foreach (var phase in definition.Phases) Phases[phase.Key] = new AnimationPhaseData(phase.Value, spriteManager);
    }

    /// <summary>
    ///     Copy constructor.
    /// </summary>
    /// <param name="other">Animated sprite to copy.</param>
    public AnimatedSprite(AnimatedSprite other)
    {
        foreach (var phase in other.Phases) Phases[phase.Key] = new AnimationPhaseData(phase.Value);
    }

    /// <summary>
    ///     Map from animation phase, to map from entity orientation to frame list (sprite IDs).
    /// </summary>
    public Dictionary<AnimationPhase, AnimationPhaseData> Phases { get; set; } = new();

    /// <summary>
    ///     Gets the phase data for the corresponding animation phase, falling back to default if the
    ///     animated sprite does not contain an explicit definition for the given animation phase.
    /// </summary>
    /// <param name="phase"></param>
    /// <returns></returns>
    public AnimationPhaseData GetPhaseData(AnimationPhase phase)
    {
        return Phases.TryGetValue(phase, out var result) ? result : Phases[AnimationPhase.Default];
    }

    /// <summary>
    ///     Animation data for a single animation phase.
    /// </summary>
    public class AnimationPhaseData
    {
        public Dictionary<Orientation, List<Sprite>> Frames = new();

        public AnimationPhaseData(AnimatedSpritePhaseDefinition definition, SpriteManager spriteManager)
        {
            FrameTime = definition.AnimationTimestep;
            foreach (var face in definition.Faces)
                Frames[face.Key] = face.Value.SpriteIds
                    .Select(id => spriteManager.Sprites[id])
                    .ToList();
        }

        public AnimationPhaseData(AnimationPhaseData other)
        {
            FrameTime = other.FrameTime;
            foreach (var frames in other.Frames) Frames[frames.Key] = new List<Sprite>(frames.Value);
        }

        public AnimationPhaseData()
        {
        }

        public ulong FrameTime { get; set; } = 1;

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
            while (!Frames.ContainsKey(nextOrientation) && nextOrientation != DefaultOrientation)
                nextOrientation = FallbackOrientations[nextOrientation];

            if (!Frames.ContainsKey(nextOrientation))
                throw new InvalidOperationException("No suitable face found for sprite.");

            return Frames[nextOrientation];
        }
    }
}