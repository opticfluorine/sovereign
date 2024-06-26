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

using System.Numerics;
using Sovereign.ClientCore.Rendering.Resources.Buffers;

namespace Sovereign.ClientCore.Rendering.Scenes;

/// <summary>
///     Describes a renderable scene.
/// </summary>
public interface IScene
{
    /// <summary>
    ///     Type of scene. This determines how the renderer processes the scene.
    /// </summary>
    SceneType SceneType { get; }

    /// <summary>
    ///     Flag indicating whether the GUI should be rendered in this scene.
    /// </summary>
    bool RenderGui { get; }

    /// <summary>
    ///     Called when the renderer starts rendering this scene.
    /// </summary>
    void BeginScene();

    /// <summary>
    ///     Called when the renderer finishes rendering this scene.
    /// </summary>
    void EndScene();

    /// <summary>
    ///     Populates the buffers used by the renderer.
    /// </summary>
    /// The low-level renderer will call this method on the active scene when it is
    /// time to update the buffers. To update the buffers, simply update the supplied
    /// arrays. These local buffers will be copied to the GPU buffers after this method
    /// returns.
    /// 
    /// The maximum number of elements in each buffer is defined by the low-level renderer
    /// and may be determined from the length of the arrays.
    /// <param name="vertexBuffer">Vertex buffer for update.</param>
    /// <param name="indexBuffer">Index buffer for update.</param>
    /// <param name="drawLengths">Number of vertices to use for each sequential draw.</param>
    /// <param name="vertexCount">Number of vertices that were added to the buffer.</param>
    /// <param name="indexCount">Number of indices that were added to the buffer.</param>
    /// <param name="drawCount">Number of draws to perform.</param>
    void PopulateBuffers(WorldVertex[] vertexBuffer, uint[] indexBuffer,
        int[] drawLengths, out int vertexCount, out int indexCount, out int drawCount);

    /// <summary>
    ///     Populates the world rendering vertex constants buffer, if applicable.
    /// </summary>
    /// <param name="widthInTiles">Width of the display in tiles.</param>
    /// <param name="heightInTiles">Height of the display in tiles.</param>
    /// <param name="cameraPos">Camera position.</param>
    /// <param name="timeSinceTick">Time since the last tick, in seconds.</param>
    void PopulateWorldVertexConstants(out float widthInTiles, out float heightInTiles,
        out Vector3 cameraPos, out float timeSinceTick);

    /// <summary>
    ///     Called to update the GUI for the scene.
    /// </summary>
    /// <remarks>
    ///     This method will only be called if the RenderGui property is true.
    /// </remarks>
    void UpdateGui();
}