using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine8.D3D11Renderer.Rendering.World
{

    /// <summary>
    /// Responsible for rendering the game world with associated state.
    /// </summary>
    public class D3D11WorldRenderStage : IRenderStage
    {

        /// <summary>
        /// Rendering device.
        /// </summary>
        private readonly D3D11Device device;

        /// <summary>
        /// Vertex shader.
        /// </summary>
        private VertexShader vertexShader;

        /// <summary>
        /// Pixel shader sampler state.
        /// </summary>
        private SamplerState pixelSampler;

        /// <summary>
        /// Pixel shader.
        /// </summary>
        private PixelShader pixelShader;

        public int Priority => 1;

        public D3D11WorldRenderStage(D3D11Device device)
        {
            this.device = device;
        }

        public void Initialize()
        {
            /* Build shaders. */
            vertexShader = CreateVertexShader();
        }

        public void Render()
        {
            /* Prepare for rendering. */
            ConfigurePipeline();

            /* Perform rendering. */
        }

        public void Cleanup()
        {
            /* Release shaders. */
            vertexShader.Dispose();
        }

        /// <summary>
        /// Configures the rendering pipeline for world rendering.
        /// </summary>
        private void ConfigurePipeline()
        {
            /* Update the immediate context. */
            var context = device.Device.ImmediateContext;

            /* Input assembler stage. */
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            /* Vertex shader. */
            context.VertexShader.SetShader(vertexShader, null, 0);

            /* Rasterizer. */

            /* Pixel shader. */
            context.PixelShader.SetSampler(0, pixelSampler);
            context.PixelShader.SetShader(pixelShader, null, 0);
        }

        private VertexShader CreateVertexShader()
        {
            return null;
        }

        private void CreatePixelShader()
        {
            /* Texture atlas sampler. */
            var samplerStateDesc = new SamplerStateDescription()
            {
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                Filter = Filter.MinMagMipPoint,
            };
            pixelSampler = new SamplerState(device.Device, samplerStateDesc);

            /* Pixel shader. */
            pixelShader = new PixelShader(device.Device, null);
        }
 
    }

}
