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
using Sovereign.D3D11Renderer.Rendering.Scenes.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.D3D11Renderer.Rendering
{

    /// <summary>
    /// Top-level scene consumer for the D3D11 renderer.
    /// </summary>
    public sealed class D3D11SceneConsumer : ISceneConsumer
    {

        private readonly GameSceneConsumer gameSceneConsumer;

        public D3D11SceneConsumer(GameSceneConsumer gameSceneConsumer)
        {
            this.gameSceneConsumer = gameSceneConsumer;
        }

        public void ConsumeScene(IScene scene)
        {
            /* Dispatch. */
            switch (scene.SceneType)
            {
                case SceneType.Game:
                    gameSceneConsumer.ConsumeScene(scene);
                    break;

                default:
                    /* Unsupported scene. */
                    break;
            }
        }

    }

}
