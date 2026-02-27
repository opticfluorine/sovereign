// Sovereign Engine
// Copyright (c) 2026 opticfluorine
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
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Sovereign.ClientCore.Configuration;
using Sovereign.ClientCore.Rendering.Sprites;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.EngineCore.Components.Types;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui.ResourceEditor;

/// <summary>
///     Provides support for importing character sprites to animated sprites.
/// </summary>
public sealed class CharacterSpriteImporter(
    SpriteManager spriteManager,
    AnimatedSpriteManager animatedSpriteManager,
    IOptions<EditorOptions> editorOptions)
{
    /// <summary>
    ///     Imports a character sprite as a new animated sprite.
    /// </summary>
    /// <param name="topLeftSpriteId">Sprite ID of top left sprite.</param>
    public void Import(int topLeftSpriteId)
    {
        if (topLeftSpriteId < 0 || topLeftSpriteId >= spriteManager.Sprites.Count)
            throw new IndexOutOfRangeException("Bad sprite ID.");

        var baseSprite = spriteManager.Sprites[topLeftSpriteId];
        var sheetGrid = spriteManager.SpriteSheetCoverage[baseSprite.SpritesheetName];
        var r0 = baseSprite.Row;
        var c0 = baseSprite.Column;
        if (baseSprite.Row + 3 >= sheetGrid.GetLength(0) ||
            baseSprite.Column + 2 >= sheetGrid.GetLength(1))
            throw new IndexOutOfRangeException("Sprite is too close to edge of the spritesheet.");

        const string nullError = "Sprite(s) are missing from selected character sheet.";
        var ss0 = baseSprite;
        var ss1 = sheetGrid[r0, c0 + 1] ?? throw new KeyNotFoundException(nullError);
        var ss2 = sheetGrid[r0, c0 + 2] ?? throw new KeyNotFoundException(nullError);
        var sw0 = sheetGrid[r0 + 1, c0] ?? throw new KeyNotFoundException(nullError);
        var sw1 = sheetGrid[r0 + 1, c0 + 1] ?? throw new KeyNotFoundException(nullError);
        var sw2 = sheetGrid[r0 + 1, c0 + 2] ?? throw new KeyNotFoundException(nullError);
        var se0 = sheetGrid[r0 + 2, c0] ?? throw new KeyNotFoundException(nullError);
        var se1 = sheetGrid[r0 + 2, c0 + 1] ?? throw new KeyNotFoundException(nullError);
        var se2 = sheetGrid[r0 + 2, c0 + 2] ?? throw new KeyNotFoundException(nullError);
        var sn0 = sheetGrid[r0 + 3, c0] ?? throw new KeyNotFoundException(nullError);
        var sn1 = sheetGrid[r0 + 3, c0 + 1] ?? throw new KeyNotFoundException(nullError);
        var sn2 = sheetGrid[r0 + 3, c0 + 2] ?? throw new KeyNotFoundException(nullError);

        var animSprite = new AnimatedSprite
        {
            Phases = new Dictionary<AnimationPhase, AnimatedSprite.AnimationPhaseData>
            {
                {
                    AnimationPhase.Default, new AnimatedSprite.AnimationPhaseData
                    {
                        FrameTime = editorOptions.Value.ImportCharacterSpriteFrameTimeUs,
                        Frames = new Dictionary<Orientation, List<Sprite>>
                        {
                            { Orientation.South, [ss1] },
                            { Orientation.West, [sw1] },
                            { Orientation.East, [se1] },
                            { Orientation.North, [sn1] }
                        }
                    }
                },
                {
                    AnimationPhase.Moving, new AnimatedSprite.AnimationPhaseData
                    {
                        FrameTime = editorOptions.Value.ImportCharacterSpriteFrameTimeUs,
                        Frames = new Dictionary<Orientation, List<Sprite>>
                        {
                            { Orientation.South, [ss1, ss2, ss1, ss0] },
                            { Orientation.West, [sw1, sw2, sw1, sw0] },
                            { Orientation.East, [se1, se2, se1, se0] },
                            { Orientation.North, [sn1, sn2, sn1, sn0] }
                        }
                    }
                }
            }
        };

        var animSpriteId = animatedSpriteManager.AnimatedSprites.Count;
        animatedSpriteManager.InsertNew(animSpriteId);
        animatedSpriteManager.Update(animSpriteId, animSprite);
    }
}