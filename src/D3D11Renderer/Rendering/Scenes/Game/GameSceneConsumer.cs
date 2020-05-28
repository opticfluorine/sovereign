/*
 * Sovereign Engine
 * Copyright (c) 2020 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using Sovereign.ClientCore.Rendering.Scenes;
using Sovereign.D3D11Renderer.Rendering.Scenes.Game.World;

namespace Sovereign.D3D11Renderer.Rendering.Scenes.Game
{

    /// <summary>
    /// Consumer that renders the in-game scene.
    /// </summary>
    public sealed class GameSceneConsumer : ISceneConsumer
    {

        private readonly GameResourceManager gameResourceManager;
        private readonly GameSceneRenderer gameSceneRenderer;
        private readonly WorldVertexConstantsUpdater worldVertexConstantsUpdater;

        public GameSceneConsumer(GameResourceManager gameResourceManager,
            GameSceneRenderer gameSceneRenderer,
            WorldVertexConstantsUpdater worldVertexConstantsUpdater)
        {
            this.gameResourceManager = gameResourceManager;
            this.gameSceneRenderer = gameSceneRenderer;
            this.worldVertexConstantsUpdater = worldVertexConstantsUpdater;
        }

        public void ConsumeScene(IScene scene)
        {
            /* Handle general processing. */
            scene.PopulateBuffers(gameResourceManager.VertexBuffer.Buffer,
                gameResourceManager.IndexBuffer.Buffer,
                gameResourceManager.DrawBuffer,
                out var drawCount);
            gameResourceManager.DrawCount = drawCount;
            worldVertexConstantsUpdater.Update(scene);

            /* Post updates to the buffers. */
            gameResourceManager.UpdateBuffers();

            /* Render. */
            gameSceneRenderer.Render();
        }

    }

}
