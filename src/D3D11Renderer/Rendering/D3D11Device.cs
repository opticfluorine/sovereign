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

using SharpDX.Direct3D11;
using Sovereign.ClientCore.Rendering.Configuration;
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
        /// Render target view pointing to the back buffer.
        /// </summary>
        public RenderTargetView BackBufferView { get; private set; }

        /// <summary>
        /// Display mode associated with the device.
        /// </summary>
        public IDisplayMode DisplayMode => mainDisplay.DisplayMode;

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

            /* Initialize the back buffer view. */
            UpdateBackBufferView();
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
            swapChain.Present(0, SharpDX.DXGI.PresentFlags.None);
            UpdateBackBufferView();
        }

        /// <summary>
        /// Gets the device flags.
        /// </summary>
        /// <returns>Device flags.</returns>
        private DeviceCreationFlags GetDeviceFlags()
        {
            var flags = DeviceCreationFlags.SingleThreaded;
#if DEBUG
            flags |= DeviceCreationFlags.Debug;
#endif
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

        /// <summary>
        /// Updates the back buffer view.
        /// </summary>
        private void UpdateBackBufferView()
        {
            BackBufferView?.Dispose();
            BackBufferView = new RenderTargetView(device,
                swapChain.GetBackBuffer<Resource>(0));
        }

    }

}
