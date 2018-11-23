/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.World.Materials.Components;
using Sovereign.EngineUtil.Collections;
using System;
using System.Collections.Generic;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.World
{

    /// <summary>
    /// Groups drawables in range by their layer.
    /// </summary>
    public sealed class WorldLayerGrouper
    {

        private readonly MaterialComponentCollection materials;

        /// <summary>
        /// Reusable pool of world layers.
        /// </summary>
        private readonly ObjectPool<WorldLayer> layerPool
            = new ObjectPool<WorldLayer>();

        /// <summary>
        /// Active layers.
        /// </summary>
        public SortedDictionary<int, WorldLayer> Layers { get; private set; }
            = new SortedDictionary<int, WorldLayer>();

        public WorldLayerGrouper(MaterialComponentCollection materials)
        {
            this.materials = materials;
        }

        /// <summary>
        /// Groups the drawables into their respective layers.
        /// </summary>
        /// <param name="drawables">Unordered list of drawable entities in range.</param>
        public void GroupDrawables(IList<PositionedEntity> drawables)
        {
            ResetLayers();
            foreach (var drawable in drawables)
            {
                ProcessDrawable(drawable);
            }
        }

        /// <summary>
        /// Processes a single drawable.
        /// </summary>
        /// <param name="drawable">Drawable to process.</param>
        private void ProcessDrawable(PositionedEntity drawable)
        {
            var material = materials.GetComponentForEntity(drawable.EntityId);
            if (material.HasValue)
                AddMaterial(drawable, material.Value);
            else
                AddAnimatedSprite(drawable);
        }

        /// <summary>
        /// Adds a non-material animated sprite to the rendering sequence.
        /// </summary>
        /// <param name="drawable">Drawable to process.</param>
        private void AddAnimatedSprite(PositionedEntity drawable)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds a material block to the rendering sequence.
        /// </summary>
        /// <param name="drawable">Drawable to process.</param>
        /// <param name="value">Value to process.</param>
        private void AddMaterial(PositionedEntity drawable, int value)
        {
            /* The current z contains the top face, the lower z contains the front face */
            var zFloorTop = (int)drawable.Position.Z;
            var zFloorFront = zFloorTop - 1;
            var layerTop = SelectLayer(zFloorTop);
            var layerFront = SelectLayer(zFloorFront);
        }

        /// <summary>
        /// Selects the world layer based on the minimum z value.
        /// </summary>
        /// <param name="zFloor">Minimum z value of the layer.</param>
        /// <returns>World layer.</returns>
        private WorldLayer SelectLayer(int zFloor)
        {
            var found = Layers.TryGetValue(zFloor, out var layer);
            if (!found)
            {
                layer = layerPool.TakeObject();
                layer.ZFloor = zFloor;
                Layers[zFloor] = layer;
            }
            return layer;
        }

        /// <summary>
        /// Clears the layers and returns them to the pool for reuse.
        /// </summary>
        private void ResetLayers()
        {
            foreach (var layer in Layers.Values)
            {
                layer.Reset();
                layerPool.ReturnObject(layer);
            }
            Layers.Clear();
        }

    }

}
