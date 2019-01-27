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

using Castle.Core.Logging;
using Sovereign.ClientCore.Rendering.Components;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Systems.Block.Components;
using Sovereign.EngineCore.Systems.Movement.Components;
using Sovereign.EngineCore.World.Materials;
using Sovereign.EngineUtil.Collections;
using Sovereign.WorldLib.Materials;
using System.Collections.Generic;
using System.Numerics;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.World
{

    /// <summary>
    /// Groups drawables in range by their layer.
    /// </summary>
    public sealed class WorldLayerGrouper
    {

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        private readonly MaterialComponentCollection materials;
        private readonly MaterialModifierComponentCollection materialModifiers;
        private readonly AnimatedSpriteComponentCollection animatedSprites;
        private readonly AboveBlockComponentCollection aboveBlocks;
        private readonly VelocityComponentCollection velocities;
        private readonly MaterialManager materialManager;

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

        public WorldLayerGrouper(MaterialComponentCollection materials,
            MaterialModifierComponentCollection materialModifiers,
            AnimatedSpriteComponentCollection animatedSprites,
            AboveBlockComponentCollection aboveBlocks,
            VelocityComponentCollection velocities,
            MaterialManager materialManager)
        {
            this.materials = materials;
            this.materialModifiers = materialModifiers;
            this.animatedSprites = animatedSprites;
            this.aboveBlocks = aboveBlocks;
            this.velocities = velocities;
            this.materialManager = materialManager;
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
            /* Get the entity velocity, defaulting to zero if not set. */
            var velocity = velocities.GetComponentForEntity(drawable.EntityId)
                .OrElseDefault(Vector3.Zero);

            /* Route the drawable to the correct rendering list. */
            var material = materials.GetComponentForEntity(drawable.EntityId);
            if (material.HasValue)
                AddMaterial(drawable, material.Value, velocity);
            else
                AddAnimatedSprite(drawable, velocity);
        }

        /// <summary>
        /// Adds a non-material animated sprite to the rendering sequence.
        /// </summary>
        /// <param name="drawable">Drawable to process.</param>
        /// <param name="velocity">Velocity of the entity.</param>
        private void AddAnimatedSprite(PositionedEntity drawable, Vector3 velocity)
        {
            var zFloor = (int)drawable.Position.Z;
            var layer = SelectLayer(zFloor);
            var sprite = animatedSprites.GetComponentForEntity(drawable.EntityId);
            if (sprite.HasValue)
                layer.AnimatedSprites.Add(new PosVelId()
                {
                    Position = drawable.Position,
                    Velocity = velocity,
                    Id = sprite.Value,
                    EntityId = drawable.EntityId
                });
        }

        /// <summary>
        /// Adds a material block to the rendering sequence.
        /// </summary>
        /// <param name="drawable">Drawable to process.</param>
        /// <param name="materialId">Material ID.</param>
        /// <param name="velocity">Velocity of the entity.</param>
        private void AddMaterial(PositionedEntity drawable, int materialId, Vector3 velocity)
        {
            /* The current z contains the top face, the lower z contains the front face */
            var zFloorTop = (int)drawable.Position.Z;
            var zFloorFront = zFloorTop - 1;
            var layerTop = SelectLayer(zFloorTop);
            var layerFront = SelectLayer(zFloorFront);

            MaterialSubtype materialSubtype;
            try
            {
                materialSubtype = GetMaterialSubtype(drawable.EntityId, materialId);
            }
            catch
            {
                Logger.ErrorFormat("Invalid material/modifier combination for entity ID {0}",
                    drawable.EntityId);
                return;
            }

            AddMaterialTopFace(drawable, materialSubtype, velocity, layerTop);
            AddMaterialFrontFace(drawable, materialSubtype, velocity, layerFront);
        }

        /// <summary>
        /// Adds the front face of a material block to its rendering layer.
        /// </summary>
        /// <param name="drawable">Material block information.</param>
        /// <param name="materialSubtype">Material subtype.</param>
        /// <param name="velocity">Velocity of the entity.</param>
        /// <param name="layerFront">Rendering layer.</param>
        private void AddMaterialFrontFace(PositionedEntity drawable, MaterialSubtype materialSubtype,
            Vector3 velocity, WorldLayer layerFront)
        {
            layerFront.FrontFaceTileSprites.Add(new PosVelId()
            {
                Position = drawable.Position,
                Velocity = velocity,
                Id = materialSubtype.SideFaceTileSpriteId,
                EntityId = drawable.EntityId
            });
        }

        /// <summary>
        /// Adds the top face of a material block to its rendering layer.
        /// </summary>
        /// <param name="drawable">Material block information.</param>
        /// <param name="materialSubtype">Material subtype.</param>
        /// <param name="velocity">Velocity of the entity.</param>
        /// <param name="layerTop">Rendering layer.</param>
        private void AddMaterialTopFace(PositionedEntity drawable, MaterialSubtype materialSubtype,
            Vector3 velocity, WorldLayer layerTop)
        {
            var topFaceId = IsTopFaceObscured(drawable.EntityId) ?
                materialSubtype.ObscuredTopFaceTileSpriteId
                : materialSubtype.TopFaceTileSpriteId;
            layerTop.TopFaceTileSprites.Add(new PosVelId()
            {
                Position = new Vector3()
                {
                    X = drawable.Position.X,
                    Y = drawable.Position.Y,
                    Z = drawable.Position.Z - 1.0f
                },
                Velocity = velocity,
                Id = topFaceId,
                EntityId = drawable.EntityId
            });
        }

        /// <summary>
        /// Determines whether the top face of a block is obscured by another block.
        /// </summary>
        /// <param name="entityId">Entity ID of the block.</param>
        /// <returns>true if the top face is obscured, false otherwise.</returns>
        private bool IsTopFaceObscured(ulong entityId)
        {
            return aboveBlocks.HasComponentForEntity(entityId);
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
        /// Looks up the material subtype for the given entity.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        /// <param name="materialId">Material ID.</param>
        /// <returns>Material subtype.</returns>
        private MaterialSubtype GetMaterialSubtype(ulong entityId, int materialId)
        {
            var modifier = materialModifiers
                .GetComponentForEntity(entityId)
                .OrElseDefault(0);
            return materialManager.Materials[materialId].MaterialSubtypes[modifier];
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
