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
///     Sequences the vertex buffer for world rendering.
/// </summary>
public sealed class WorldVertexSequencer
{
    /// <summary>
    ///     Default size of the drawable buffer.
    /// </summary>
    public const int DefaultDrawableListSize = 4096;

    private readonly WorldEntityRetriever entityRetriever;

    private readonly WorldLayerGrouper grouper;
    private readonly WorldLayerVertexSequencer layerVertexSequencer;

    public WorldVertexSequencer(WorldLayerGrouper grouper,
        WorldLayerVertexSequencer layerVertexSequencer,
        WorldEntityRetriever entityRetriever)
    {
        this.grouper = grouper;
        this.layerVertexSequencer = layerVertexSequencer;
        this.entityRetriever = entityRetriever;
    }

    /// <summary>
    ///     Produces the vertex buffer for world rendering.
    /// </summary>
    /// <param name="renderPlan">Render plan to populate.</param>
    /// <param name="timeSinceTick">Time since the last tick, in seconds.</param>
    /// <param name="systemTime">System time of the current frame.</param>
    public void SequenceVertices(RenderPlan renderPlan, float timeSinceTick, ulong systemTime)
    {
        RetrieveEntities(timeSinceTick, systemTime);
        PrepareLayers(renderPlan, systemTime);
    }

    /// <summary>
    ///     Retrieves the drawable entities in range.
    /// </summary>
    /// <param name="timeSinceTick">Time since the last tick, in seconds.</param>
    /// <param name="systemTime">System time of the current frame.</param>
    private void RetrieveEntities(float timeSinceTick, ulong systemTime)
    {
        entityRetriever.RetrieveEntities(timeSinceTick, systemTime);
    }

    /// <summary>
    ///     Prepares the layers and populates the vertex buffer.
    /// </summary>
    /// <param name="renderPlan">Rendering plan to populate.</param>
    /// <param name="systemTime">System time of the current frame.</param>
    private void PrepareLayers(RenderPlan renderPlan, ulong systemTime)
    {
        foreach (var layer in grouper.Layers.Values) AddLayerToVertexBuffer(layer, renderPlan, systemTime);
    }

    /// <summary>
    ///     Adds a single layer to the vertex buffer.
    /// </summary>
    /// <param name="layer">Layer to be added.</param>
    /// <param name="renderPlan">Rendering plan to populate.</param>
    /// <param name="systemTime">System time of the current frame.</param>
    private void AddLayerToVertexBuffer(WorldLayer layer, RenderPlan renderPlan, ulong systemTime)
    {
        layerVertexSequencer.AddLayer(layer, renderPlan, systemTime);
    }
}