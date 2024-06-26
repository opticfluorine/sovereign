﻿/*
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
using Sovereign.ClientCore.Rendering.Configuration;
using Sovereign.ClientCore.Rendering.Gui;

namespace Sovereign.ClientCore.Rendering.Sprites.Atlas;

/// <summary>
///     Responsible for collecting spritesheets into a single texture atlas.
/// </summary>
public class TextureAtlas : IDisposable
{
    /// <summary>
    ///     Maximum width of the texture atlas.
    /// </summary>
    private const int MaxWidth = 16384;

    /// <summary>
    ///     Maximum height of the texture atlas.
    /// </summary>
    private const int MaxHeight = 16384;

    /// <summary>
    ///     Maps spritesheet names to top-left coordinate in the atlas.
    /// </summary>
    public IDictionary<string, Tuple<int, int>> SpriteSheetMap
        = new Dictionary<string, Tuple<int, int>>();

    /// <summary>
    ///     Creates and packs a texture atlas from the given collection
    ///     of spritesheets.
    /// </summary>
    /// <param name="spriteSheets">Spritesheets to pack.</param>
    /// <param name="fontAtlas">Font atlas.</param>
    /// <param name="format">Pixel format for the atlas.</param>
    /// <exception cref="TextureAtlasException">
    ///     Thrown if the texture atlas cannot be created.
    /// </exception>
    public TextureAtlas(IEnumerable<SpriteSheet> spriteSheets, GuiFontAtlas fontAtlas,
        DisplayFormat format)
    {
        /* Order spritesheets for packing. */
        var orderedSheets = OrderSheetsForPacking(spriteSheets);

        /* Compute the packing plan. */
        var plan = PlanPacking(orderedSheets, fontAtlas, out var atlasWidth,
            out var atlasHeight);
        Width = atlasWidth;
        Height = atlasHeight;

        /* Create the atlas. */
        CreateAtlas(plan, atlasWidth, atlasHeight, format);
    }

    /// <summary>
    ///     Texture atlas surface.
    /// </summary>
    public Surface AtlasSurface { get; private set; }

    /// <summary>
    ///     Width of the atlas surface in pixels.
    /// </summary>
    public int Width { get; private set; }

    /// <summary>
    ///     Height of the atlas surface in pixels.
    /// </summary>
    public int Height { get; private set; }

    /// <summary>
    ///     Top-left point of the font atlas within the texture atlas.
    /// </summary>
    public Tuple<int, int> FontAtlasPosition { get; private set; } = new(0, 0);

    /// <summary>
    ///     Relative coordinates of the font atlas bounds ((topLeftX, topLeftY), (bottomRightX, bottomRightY)).
    /// </summary>
    public Tuple<float, float, float, float> FontAtlasBounds { get; private set; } = new(0.0f, 0.0f, 0.0f, 0.0f);

    public void Dispose()
    {
        AtlasSurface.Dispose();
    }

    /// <summary>
    ///     Orders the spritesheets for packing.
    /// </summary>
    /// <param name="spriteSheets">Spritesheets.</param>
    /// <returns>Ordered spritesheets.</returns>
    private IOrderedEnumerable<SpriteSheet> OrderSheetsForPacking(
        IEnumerable<SpriteSheet> spriteSheets)
    {
        /* Order by width. */
        return spriteSheets.OrderBy(sheet => sheet.Surface.Properties.Height);
    }

    /// <summary>
    ///     Plans the packing of the spritesheets.
    /// </summary>
    /// <param name="spriteSheets">Spritesheets in packing order.</param>
    /// <param name="fontAtlas">Font atlas.</param>
    /// <param name="atlasWidth">Width of the texture atlas.</param>
    /// <param name="atlasHeight">Height of the texture atlas.</param>
    /// <returns>Planned positioning of the spritesheets.</returns>
    /// <exception cref="TextureAtlasException">
    ///     Thrown if no suitable packing is found.
    /// </exception>
    private IList<PlanElement> PlanPacking(IOrderedEnumerable<SpriteSheet> spriteSheets,
        GuiFontAtlas fontAtlas, out int atlasWidth, out int atlasHeight)
    {
        /* Allocate the plan. */
        var plan = new List<PlanElement>(spriteSheets.Count());

        /* Iterate through the spritesheets. */
        var baseX = 0;
        var baseY = 0;
        var maximumRowWidth = 0;
        var currentRowHeight = 0;
        int nextWidth;
        int nextHeight;
        int newBaseX;
        foreach (var spriteSheet in spriteSheets)
        {
            /* Get information for next sheet. */
            nextWidth = spriteSheet.Surface.Properties.Width;
            nextHeight = spriteSheet.Surface.Properties.Height;

            /* Place the next sheet. */
            AdvancePosition(nextWidth, nextHeight, ref baseX, ref baseY,
                ref maximumRowWidth, ref currentRowHeight, out newBaseX);

            /* Add the sheet. */
            plan.Add(new PlanElement
            {
                PlanElementType = PlanElementType.Spritesheet,
                Surface = spriteSheet.Surface,
                Name = spriteSheet.Definition.Filename,
                TopLeftX = baseX,
                TopLeftY = baseY
            });

            /* Advance to the next position. */
            baseX = newBaseX;
        }

        /* Add the font atlas to the plan. */
        nextWidth = fontAtlas.Width;
        nextHeight = fontAtlas.Height;
        AdvancePosition(nextWidth, nextHeight, ref baseX, ref baseY,
            ref maximumRowWidth, ref currentRowHeight, out newBaseX);
        plan.Add(new PlanElement
        {
            PlanElementType = PlanElementType.FontAtlas,
            Surface = fontAtlas.FontAtlasSurface,
            Name = "",
            TopLeftX = baseX,
            TopLeftY = baseY
        });

        /* Output the plan. */
        atlasWidth = maximumRowWidth;
        atlasHeight = baseY + currentRowHeight;
        return plan;
    }

    /// <summary>
    ///     Advances to the next position in the atlas plan.
    /// </summary>
    /// <param name="width">Width of the next surface to be added.</param>
    /// <param name="height">Height of the next surface to be added.</param>
    /// <param name="baseX">X position at which the next surface should be placed.</param>
    /// <param name="baseY">Y position at which the next surface should be placed.</param>
    /// <param name="maximumRowWidth">Maximum width of all rows.</param>
    /// <param name="currentRowHeight">Height of the current row.</param>
    /// <param name="newBaseX">Next value of baseX.</param>
    private void AdvancePosition(int width, int height, ref int baseX, ref int baseY,
        ref int maximumRowWidth, ref int currentRowHeight, out int newBaseX)
    {
        /* Check if the sheet can be fit in the current row. */
        newBaseX = baseX + width;
        if (newBaseX > MaxWidth)
        {
            /* Advance to next row. */
            baseX = 0;
            baseY += currentRowHeight;
            newBaseX = width;
            if (baseY > MaxHeight)
                /* Cannot fit spritesheets. */
                throw new TextureAtlasException("Cannot pack spritesheets into texture atlas.");
            currentRowHeight = 0;
        }

        /* Advance to the next position. */
        maximumRowWidth = Math.Max(maximumRowWidth, newBaseX);
        currentRowHeight = Math.Max(currentRowHeight, height);
    }

    /// <summary>
    ///     Creates the texture atlas according to the given plan.
    /// </summary>
    /// <param name="plan">Packing plan.</param>
    /// <param name="atlasWidth">Atlas width.</param>
    /// <param name="atlasHeight">Atlas height.</param>
    /// <param name="format">Display format.</param>
    [MemberNotNull("AtlasSurface")]
    private void CreateAtlas(IList<PlanElement> plan, int atlasWidth, int atlasHeight,
        DisplayFormat format)
    {
        /* Create the surface to hold the atlas. */
        CreateAtlasSurface(atlasWidth, atlasHeight, format);

        /* Pack the atlas. */
        PackAtlas(plan, atlasWidth, atlasHeight);
    }

    /// <summary>
    ///     Creates the surface to hold the texture atlas.
    /// </summary>
    /// <param name="atlasWidth">Atlas width.</param>
    /// <param name="atlasHeight">Atlas height.</param>
    /// <param name="format">Format.</param>
    [MemberNotNull("AtlasSurface")]
    private void CreateAtlasSurface(int atlasWidth, int atlasHeight,
        DisplayFormat format)
    {
        AtlasSurface = Surface.CreateSurface(atlasWidth, atlasHeight, format);
    }

    /// <summary>
    ///     Packs the atlas.
    /// </summary>
    /// <param name="plan">Packing plan.</param>
    /// <param name="atlasWidth">Total texture atlas width.</param>
    /// <param name="atlasHeight">Total texture atlas height.</param>
    private void PackAtlas(IList<PlanElement> plan, int atlasWidth, int atlasHeight)
    {
        /* Iterate over the plan. */
        foreach (var planElement in plan)
        {
            /* Execute the plan step. */
            var srcSurface = planElement.Surface;
            srcSurface.Blit(AtlasSurface, planElement.TopLeftX,
                planElement.TopLeftY);

            /* Perform any type-specific processing. */
            if (planElement.PlanElementType == PlanElementType.Spritesheet)
            {
                SpriteSheetMap[planElement.Name]
                    = new Tuple<int, int>(planElement.TopLeftX, planElement.TopLeftY);
            }
            else if (planElement.PlanElementType == PlanElementType.FontAtlas)
            {
                FontAtlasPosition = new Tuple<int, int>(
                    planElement.TopLeftX, planElement.TopLeftY);
                FontAtlasBounds = new Tuple<float, float, float, float>(
                    planElement.TopLeftX / (float)atlasWidth,
                    planElement.TopLeftY / (float)atlasHeight,
                    (planElement.TopLeftX + planElement.Surface.Properties.Width) / (float)atlasWidth,
                    (planElement.TopLeftY + planElement.Surface.Properties.Height) / (float)atlasHeight
                );
            }
        }
    }

    /// <summary>
    ///     Enumeration of plan element types.
    /// </summary>
    private enum PlanElementType
    {
        /// <summary>
        ///     Plan element is a spritesheet.
        /// </summary>
        Spritesheet,

        /// <summary>
        ///     Plan element is the font atlas.
        /// </summary>
        FontAtlas
    }

    /// <summary>
    ///     Planned placement of a spritesheet.
    /// </summary>
    private struct PlanElement
    {
        /// <summary>
        ///     Surface to be placed.
        /// </summary>
        public Surface Surface;

        /// <summary>
        ///     Type of this plan element.
        /// </summary>
        public PlanElementType PlanElementType;

        /// <summary>
        ///     Surface name. Only used if PlanElementType is Spritesheet.
        /// </summary>
        public string Name;

        /// <summary>
        ///     Planned x coordinate of the top left corner of the spritesheet.
        /// </summary>
        public int TopLeftX;

        /// <summary>
        ///     Planned y coordinate of the top left corner of the spritesheet.
        /// </summary>
        public int TopLeftY;
    }
}