using Engine8.D3D11Renderer.Rendering.Configuration;
using SharpDX.Direct3D11;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine8.ClientCore.Rendering.Display;

namespace Engine8.D3D11Renderer.Rendering
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
        private MainDisplay mainDisplay;

        /// <summary>
        /// Creates the Direct3D 11 device.
        /// </summary>
        /// <param name="mainDisplay">Main display that will be managed by this renderer.</param>
        /// <param name="videoAdapter">Video adapter to use.</param>
        public D3D11Device(MainDisplay mainDisplay, D3D11VideoAdapter videoAdapter)
        {
            this.mainDisplay = mainDisplay;

            /* Get information needed to create the device. */
            var nativeAdapter = videoAdapter.InternalAdapter;
            var swapChainDesc = GetSwapChainDescription();

            /* Create the device and swap chain. */
            Device.CreateWithSwapChain(
                nativeAdapter,
                DeviceCreationFlags.Debug | DeviceCreationFlags.SingleThreaded
                | DeviceCreationFlags.Debuggable, 
                swapChainDesc, 
                out device, 
                out swapChain);

            
        }

        private DeviceCreationFlags GetDeviceFlags()
        {
            var flags = DeviceCreationFlags.SingleThreaded | DeviceCreationFlags.Debug |
                    DeviceCreationFlags.Debuggable;
            return flags;
        }


        private SharpDX.DXGI.SwapChainDescription GetSwapChainDescription()
        {
            /* Create the back buffer mode description. */
            var modeDesc = new SharpDX.DXGI.ModeDescription
            {
                Width = 0,
                Height = 0,
                
            };

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
                ModeDescription = modeDesc,
                IsWindowed = true,
                OutputHandle = mainDisplay.WindowHwnd,
                Usage = SharpDX.DXGI.Usage.RenderTargetOutput,
                SampleDescription = sampleDesc,
                Flags = SharpDX.DXGI.SwapChainFlags.None,
            };

            return description;
        }

    }

}
