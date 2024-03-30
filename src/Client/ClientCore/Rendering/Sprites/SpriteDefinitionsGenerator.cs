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

using System.Collections.Generic;

namespace Sovereign.ClientCore.Rendering.Sprites;

/// <summary>
///     Generates sprite definitions covering a spritesheet.
/// </summary>
public class SpriteDefinitionsGenerator
{
    private readonly SpriteManager spriteManager;
    private readonly SpriteSheetManager spriteSheetManager;

    public SpriteDefinitionsGenerator(SpriteManager spriteManager, SpriteSheetManager spriteSheetManager)
    {
        this.spriteManager = spriteManager;
        this.spriteSheetManager = spriteSheetManager;
    }

    /// <summary>
    ///     Generates any missing sprites for the spritesheet with the given name.
    /// </summary>
    /// <param name="spritesheetName">Spritesheet name.</param>
    /// <exception cref="KeyNotFoundException">Thrown if there is no spritesheet with the given name.</exception>
    public void GenerateMissingSpritesForSheet(string spritesheetName)
    {
        var coverageMap = spriteManager.SpriteSheetCoverage[spritesheetName];
        for (var i = 0; i < coverageMap.GetLength(0); ++i)
        {
            for (var j = 0; j < coverageMap.GetLength(1); ++j)
            {
                if (coverageMap[i, j] != null) continue;

                // Sprite definition is missing for this position.
                AddSpriteDefinition(spritesheetName, i, j);
            }
        }
    }

    /// <summary>
    ///     Generates the missing sprite definition at the given sheet, row, and column.
    /// </summary>
    /// <param name="spritesheetName"></param>
    /// <param name="row"></param>
    /// <param name="col"></param>
    private void AddSpriteDefinition(string spritesheetName, int row, int col)
    {
        var nextId = spriteManager.Sprites.Count;
        spriteManager.Sprites.Add(new Sprite
        {
            Id = nextId,
            Row = row,
            Column = col,
            SpritesheetName = spritesheetName
        });
    }
}