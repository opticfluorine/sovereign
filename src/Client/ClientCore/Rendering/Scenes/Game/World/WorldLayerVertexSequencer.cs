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

namespace Sovereign.ClientCore.Rendering.Scenes.Game.World;

/// <summary>
///     Sequences the vertices for a single world layer.
/// </summary>
public sealed class WorldLayerVertexSequencer
{
    private readonly WorldSpriteSequencer spriteSequencer;

    public WorldLayerVertexSequencer(WorldSpriteSequencer spriteSequencer)
    {
        this.spriteSequencer = spriteSequencer;
    }

    /// <summary>
    ///     Adds a single layer to the vertex buffer.
    /// </summary>
    /// <param name="layer">Layer to be added.</param>
    /// <param name="renderPlan">Rendering plan to populate.</param>
    /// <param name="systemTime">System time of the current frame.</param>
    public void AddLayer(WorldLayer layer, RenderPlan renderPlan, ulong systemTime)
    {
        spriteSequencer.SequenceSprites(layer.FrontFaceTileSprites, renderPlan, SpritePlane.XZ, out var frontBaseIndex,
            out var frontIndexCount);
        spriteSequencer.SequenceSprites(layer.TopFaceTileSprites, renderPlan, SpritePlane.XY, out var topBaseIndex,
            out var topIndexCount);
        spriteSequencer.SequenceSprites(layer.AnimatedSprites, renderPlan, SpritePlane.XY, out var spriteBaseIndex,
            out var spriteIndexCount);

        renderPlan.PushDebugGroup($"Layer {layer.ZFloor}");

        renderPlan.PushDebugGroup("Block Front Faces");
        renderPlan.DrawSprites(frontBaseIndex, frontIndexCount);
        renderPlan.PopDebugGroup();

        renderPlan.PushDebugGroup("Block Top Faces");
        renderPlan.DrawSprites(topBaseIndex, topIndexCount);
        renderPlan.PopDebugGroup();

        renderPlan.PushDebugGroup("Animated Sprites");
        renderPlan.DrawSprites(spriteBaseIndex, spriteIndexCount);
        renderPlan.PopDebugGroup();

        renderPlan.PopDebugGroup();
    }
}