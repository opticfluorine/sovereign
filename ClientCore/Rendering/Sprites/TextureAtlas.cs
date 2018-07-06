using Engine8.ClientCore.Rendering.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine8.ClientCore.Rendering.Sprites
{

    /// <summary>
    /// Responsible for collecting spritesheets into a single texture atlas.
    /// </summary>
    public class TextureAtlas : IDisposable
    {

        /// <summary>
        /// Maximum width of the texture atlas.
        /// </summary>
        private const int MaxWidth = 16384;

        /// <summary>
        /// Maximum height of the texture atlas.
        /// </summary>
        private const int MaxHeight = 16384;

        /// <summary>
        /// Texture atlas surface.
        /// </summary>
        public Surface AtlasSurface { get; private set; }

        /// <summary>
        /// Creates and packs a texture atlas from the given collection
        /// of spritesheets.
        /// </summary>
        /// <param name="spriteSheets">Spritesheets to pack.</param>
        /// <param name="format">Pixel format for the atlas.</param>
        /// <exception cref="TextureAtlasException">
        /// Thrown if the texture atlas cannot be created.
        /// </exception>
        public TextureAtlas(IEnumerable<SpriteSheet> spriteSheets, DisplayFormat format)
        {
            /* Order spritesheets for packing. */
            var orderedSheets = OrderSheetsForPacking(spriteSheets);

            /* Compute the packing plan. */
            var plan = PlanPacking(orderedSheets, out int atlasWidth, 
                out int atlasHeight);

            /* Create the atlas. */
            CreateAtlas(plan, atlasWidth, atlasHeight, format);
        }

        public void Dispose()
        {
            AtlasSurface.Dispose();
        }

        /// <summary>
        /// Orders the spritesheets for packing.
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
        /// Plans the packing of the spritesheets.
        /// </summary>
        /// <param name="spriteSheets">Spritesheets in packing order.</param>
        /// <param name="atlasWidth">Width of the texture atlas.</param>
        /// <param name="atlasHeight">Height of the texture atlas.</param>
        /// <returns>Planned positioning of the spritesheets.</returns>
        /// <exception cref="TextureAtlasException">
        /// Thrown if no suitable packing is found.
        /// </exception>
        private IList<PlanElement> PlanPacking(IOrderedEnumerable<SpriteSheet> spriteSheets,
            out int atlasWidth, out int atlasHeight)
        {
            /* Allocate the plan. */
            var plan = new List<PlanElement>(spriteSheets.Count());

            /* Iterate through the spritesheets. */
            int baseX = 0;
            int baseY = 0;
            int maximumRowWidth = 0;
            int currentRowHeight = 0;
            foreach (var spriteSheet in spriteSheets)
            {
                /* Get information for next sheet. */
                int nextWidth = spriteSheet.Surface.Properties.Width;
                int nextHeight = spriteSheet.Surface.Properties.Height;

                /* Check if the sheet can be fit in the current row. */
                int newBaseX = baseX + nextWidth;
                if (newBaseX > MaxWidth)
                {
                    /* Advance to next row. */
                    maximumRowWidth = Math.Max(maximumRowWidth, baseX);
                    baseX = 0;
                    baseY += currentRowHeight;
                    newBaseX = nextWidth;
                    if (baseY > MaxHeight)
                    {
                        /* Cannot fit spritesheets. */
                        throw new TextureAtlasException("Cannot pack spritesheets into texture atlas.");
                    }
                    currentRowHeight = 0;
                }

                /* Add the sheet. */
                plan.Add(new PlanElement()
                {
                    SpriteSheet = spriteSheet,
                    TopLeftX = baseX,
                    TopLeftY = baseY,
                });

                /* Advance to the next position. */
                baseX = newBaseX;
                currentRowHeight = Math.Max(currentRowHeight, nextHeight);
            }

            /* Output the plan. */
            atlasWidth = maximumRowWidth;
            atlasHeight = baseY + currentRowHeight;
            return plan;
        }

        /// <summary>
        /// Creates the texture atlas according to the given plan.
        /// </summary>
        /// <param name="plan">Packing plan.</param>
        /// <param name="atlasWidth">Atlas width.</param>
        /// <param name="atlasHeight">Atlas height.</param>
        private void CreateAtlas(IList<PlanElement> plan, int atlasWidth, int atlasHeight,
            DisplayFormat format)
        {
            /* Create the surface to hold the atlas. */
            CreateAtlasSurface(atlasWidth, atlasHeight, format);

            /* Pack the atlas. */
            PackAtlas(plan);
        }

        /// <summary>
        /// Creates the surface to hold the texture atlas.
        /// </summary>
        /// <param name="atlasWidth">Atlas width.</param>
        /// <param name="atlasHeight">Atlas height.</param>
        /// <param name="format">Format.</param>
        private void CreateAtlasSurface(int atlasWidth, int atlasHeight, 
            DisplayFormat format)
        {
            AtlasSurface = Surface.CreateSurface(atlasWidth, atlasHeight, format);
        }

        /// <summary>
        /// Packs the atlas.
        /// </summary>
        /// <param name="plan">Packing plan.</param>
        private void PackAtlas(IList<PlanElement> plan)
        {
            /* Iterate over the plan. */
            foreach (var planElement in plan)
            {
                /* Execute the plan step. */
                var srcSurface = planElement.SpriteSheet.Surface;
                srcSurface.Blit(AtlasSurface, planElement.TopLeftX,
                    planElement.TopLeftY);
            }
        }

        /// <summary>
        /// Planned placement of a spritesheet.
        /// </summary>
        private struct PlanElement
        {

            /// <summary>
            /// Spritesheet to be placed.
            /// </summary>
            public SpriteSheet SpriteSheet;

            /// <summary>
            /// Planned x coordinate of the top left corner of the spritesheet.
            /// </summary>
            public int TopLeftX;

            /// <summary>
            /// Planned y coordinate of the top left corner of the spritesheet.
            /// </summary>
            public int TopLeftY;

        }

    }

}
