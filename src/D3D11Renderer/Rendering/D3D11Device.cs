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

using SharpDX.Direct3D11;
using Sovereign.ClientCore.Rendering.Display;
using Sovereign.D3D11Renderer.Rendering.Configuration;

namespace Sovereign.D3D11Renderer.Rendering
{

    /// <summary>
    /// Responsible for managing a Direct3D 11 device for rendering.
    /// </summary>
    public class D3D11Device
    {

        /// <summary>
        /// Direct3D 11 device.
        /// </summary>
        public Device Device => device;

        /// <summary>
        /// Swap chain connected to the device.
        /// </summary>
        public SharpDX.DXGI.SwapChain SwapChain => swapChain;

        /// <summary>
        /// Internal storage for the device.
        /// </summary>
        private Device device;

        /// <summary>
        /// Internal storage for the swap chain.
        /// </summary>
        private SharpDX.DXGI.SwapChain swapChain;

        /// <summary>
        /// Main display backed by this renderer.
        /// </summary>
        private readonly MainDisplay mainDisplay;

        public D3D11Device(MainDisplay mainDisplay)
        {
            this.mainDisplay = mainDisplay;
        }

        /// <summary>
        /// Creates the Direct3D 11 device.
        /// </summary>
        /// <param name="videoAdapter">Video adapter to use.</param>
        public void CreateDevice(D3D11VideoAdapter videoAdapter)
        {
            /* Get information needed to create the device. */
            var nativeAdapter = videoAdapter.InternalAdapter;
            var nativeMode = ((D3D11DisplayMode)mainDisplay.DisplayMode).InternalModeDescription;
            var swapChainDesc = GetSwapChainDescription(nativeMode);

            /* Create the device and swap chain. */
            Device.CreateWithSwapChain(
                nativeAdapter,
                GetDeviceFlags(),
                swapChainDesc,
                out device,
                out swapChain);
        }

        /// <summary>
        /// Cleans up the device and swapchain.
        /// </summary>
        public void Cleanup()
        {
            swapChain.Dispose();
            swapChain = null;

            device.Dispose();
            device = null;
        }

        /// <summary>
        /// Presents the next frame.
        /// </summary>
        public void Present()
        {
            SwapChain.Present(0, SharpDX.DXGI.PresentFlags.None);
        }

        /// <summary>
        /// Gets the device flags.
        /// </summary>
        /// <returns>Device flags.</returns>
        private DeviceCreationFlags GetDeviceFlags()
        {
            var flags = DeviceCreationFlags.SingleThreaded | DeviceCreationFlags.Debug;
            return flags;
        }

        /// <summary>
        /// Gets the swap chain description.
        /// </summary>
        /// <param name="modeDescription">DXGI mode description.</param>
        /// <returns>Swap chain description.</returns>
        private SharpDX.DXGI.SwapChainDescription GetSwapChainDescription(
            SharpDX.DXGI.ModeDescription modeDescription)
        {
            /* Create the multisampling description. */
            var sampleDesc = new SharpDX.DXGI.SampleDescription
            {
                Count = 1,
                Quality = 0,
            };

            /* Create the swap chain description. */
            var description = new SharpDX.DXGI.SwapChainDescription
            {
                BufferCount = 2,
                SwapEffect = SharpDX.DXGI.SwapEffect.Discard,
                ModeDescription = modeDescription,
                IsWindowed = !mainDisplay.IsFullscreen,
                OutputHandle = mainDisplay.WindowHwnd,
                Usage = SharpDX.DXGI.Usage.RenderTargetOutput,
                SampleDescription = sampleDesc,
                Flags = SharpDX.DXGI.SwapChainFlags.None,
            };

            return description;
        }

    }

}
