/*
 * Sovereign Engine
 * Copyright (c) 2022 opticfluorine
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

using Sovereign.ClientCore.Rendering.Display;
using System;
using System.Runtime.InteropServices;
using SDL2;
using Veldrid;
using Veldrid.Vk;
using Sovereign.EngineCore.Resources;
using System.IO;
using Sovereign.ClientCore.Rendering;
using Sovereign.ClientCore.Rendering.Configuration;

namespace Sovereign.VeldridRenderer.Rendering;

/// <summary>
/// Manages the Veldrid graphics device.
/// </summary>
public class VeldridDevice : IDisposable
{

    /// <summary>
    /// Main display.
    /// </summary>
    private readonly MainDisplay mainDisplay;
    private readonly IResourcePathBuilder pathBuilder;

    /// <summary>
    /// Backing Veldrid graphics device.
    /// </summary>
    public GraphicsDevice Device { get; private set; }

    /// <summary>
    /// Provides access to the currently selected display mode.
    /// </summary>
    public IDisplayMode DisplayMode => mainDisplay.DisplayMode;

    public VeldridDevice(MainDisplay mainDisplay, IResourcePathBuilder pathBuilder)
    {
        this.mainDisplay = mainDisplay;
        this.pathBuilder = pathBuilder;
    }

    /// <summary>
    /// Creates the Veldrid device.
    /// </summary>
    public void CreateDevice()
    {
        // No fancy options for now, just enable debug mode.
        // TODO Control debug mode via settings.
        // TODO Control vsync via settings.
        var options = new GraphicsDeviceOptions(true);

        // We use the Vulkan renderer since it is reasonably cross-platform
        // and more stable than the OpenGL renderer on Linux.
        Device = GraphicsDevice.CreateVulkan(
            options: options,
            surfaceSource: CreateVkSurfaceSource(),
            width: (uint)DisplayMode.Width,
            height: (uint)DisplayMode.Height
        );
    }


    /// <summary>
    /// Destroys the graphics device when done using it.
    /// </summary>
    public void Dispose()
    {
        if (Device != null)
        {
            Device.Dispose();
        }
    }

    /// <summary>
    /// Utility method that loads shader bytes from a file.
    /// </summary>
    /// <param name="shaderName">Shader name.</param>
    /// <returns>Shader bytes.</returns>
    public byte[] LoadShaderBytes(string shaderName)
    {
        // Locate shader.
        var shaderPath = pathBuilder.BuildPathToResource(ResourceType.Shader,
            shaderName);

        // Read shader.
        return File.ReadAllBytes(shaderPath);
    }

    /// <summary>
    /// Creates a VkSurfaceSource for Vulkan rendering.
    /// </summary>
    /// <returns>VkSurfaceSource for the main display.</returns>
    private VkSurfaceSource CreateVkSurfaceSource()
    {
        var wmInfo = mainDisplay.WMinfo;
        switch (wmInfo.subsystem)
        {
            case SDL.SDL_SYSWM_TYPE.SDL_SYSWM_WINDOWS:
                return VkSurfaceSource.CreateWin32(wmInfo.info.win.hinstance,
                    wmInfo.info.win.window);

            case SDL.SDL_SYSWM_TYPE.SDL_SYSWM_X11:
                unsafe
                {
                    var display = (Vulkan.Xlib.Display*)wmInfo.info.x11.display;
                    var window = new Vulkan.Xlib.Window { Value = wmInfo.info.x11.window };
                    return VkSurfaceSource.CreateXlib(display, window);
                }

            default:
                // Not supported.
                throw new RendererInitializationException("Unsupported window system.");
        }
    }

}
