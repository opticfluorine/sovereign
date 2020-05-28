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

using Castle.Core.Logging;
using Sovereign.EngineCore.Resources;
using System.IO;

namespace Sovereign.D3D11Renderer.Rendering.Scenes.Game
{

    /// <summary>
    /// Responsible for managing the game scene shader bytecode.
    /// </summary>
    public sealed class GameSceneShaders
    {

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        public const string WorldVertexShaderFilename = "WorldVertexShader.cso";

        public const string WorldPixelShaderFilename = "WorldPixelShader.cso";

        private readonly IResourcePathBuilder resourcePathBuilder;

        /// <summary>
        /// Compiled bytecode for the world vertex shader.
        /// </summary>
        public byte[] WorldVertexShader { get; private set; }

        /// <summary>
        /// Compiled bytecode for the world pixel shader.
        /// </summary>
        public byte[] WorldPixelShader { get; private set; }

        public GameSceneShaders(IResourcePathBuilder resourcePathBuilder)
        {
            this.resourcePathBuilder = resourcePathBuilder;
        }

        /// <summary>
        /// Loads the shaders.
        /// </summary>
        public void Initialize()
        {
            var vertexFilepath = resourcePathBuilder.BuildPathToResource(ResourceType.Shader,
                WorldVertexShaderFilename);
            var pixelFilepath = resourcePathBuilder.BuildPathToResource(ResourceType.Shader,
                WorldPixelShaderFilename);

            WorldVertexShader = LoadShader(vertexFilepath);
            WorldPixelShader = LoadShader(pixelFilepath);

            Logger.Info("Game scene shaders loaded.");
        }

        /// <summary>
        /// Loads a shader.
        /// </summary>
        /// <param name="filepath">Path to the compiled shader object.</param>
        /// <returns>Compiled shader bytecode.</returns>
        private byte[] LoadShader(string filepath)
        {
            using (var srcStream = new FileStream(filepath, FileMode.Open))
            {
                using (var dstStream = new MemoryStream())
                {
                    srcStream.CopyTo(dstStream);
                    return dstStream.ToArray();
                }
            }
        }

    }

}
