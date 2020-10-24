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
