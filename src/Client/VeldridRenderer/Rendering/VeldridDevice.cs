/*
 * Sovereign Engine
 * Copyright (c) 2022 opticfluorine
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

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using SDL2;
using Sovereign.ClientCore.Rendering;
using Sovereign.ClientCore.Rendering.Configuration;
using Sovereign.ClientCore.Rendering.Display;
using Sovereign.EngineCore.Resources;
using Veldrid;
using Veldrid.Vk;
using Vulkan.Xlib;

namespace Sovereign.VeldridRenderer.Rendering;

/// <summary>
///     Manages the Veldrid graphics device.
/// </summary>
public class VeldridDevice : IDisposable
{
    /// <summary>
    ///     Main display.
    /// </summary>
    private readonly MainDisplay mainDisplay;

    private readonly IResourcePathBuilder pathBuilder;

    public VeldridDevice(MainDisplay mainDisplay, IResourcePathBuilder pathBuilder)
    {
        this.mainDisplay = mainDisplay;
        this.pathBuilder = pathBuilder;
    }

    /// <summary>
    ///     Backing Veldrid graphics device.
    /// </summary>
    public GraphicsDevice? Device { get; private set; }

    /// <summary>
    ///     Provides access to the currently selected display mode.
    /// </summary>
    public IDisplayMode? DisplayMode => mainDisplay.DisplayMode;


    /// <summary>
    ///     Destroys the graphics device when done using it.
    /// </summary>
    public void Dispose()
    {
        if (Device != null) Device.Dispose();
    }

    /// <summary>
    ///     Creates the Veldrid device.
    /// </summary>
    [MemberNotNull("Device")]
    public void CreateDevice()
    {
        var options = new GraphicsDeviceOptions(false, PixelFormat.R32_Float, false);

        // We use the Vulkan renderer since it is reasonably cross-platform
        // and more stable than the OpenGL renderer on Linux.
        if (DisplayMode == null)
            throw new InvalidOperationException("Tried to create device without display mode.");

        Device = GraphicsDevice.CreateVulkan(
            options,
            CreateVkSurfaceSource(),
            (uint)DisplayMode.Width,
            (uint)DisplayMode.Height
        );
    }

    /// <summary>
    ///     Utility method that loads shader bytes from a file.
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
    ///     Creates a VkSurfaceSource for Vulkan rendering.
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
                    var display = (Display*)wmInfo.info.x11.display;
                    var window = new Window { Value = wmInfo.info.x11.window };
                    return VkSurfaceSource.CreateXlib(display, window);
                }

            default:
                // Not supported.
                throw new RendererInitializationException("Unsupported window system.");
        }
    }
}