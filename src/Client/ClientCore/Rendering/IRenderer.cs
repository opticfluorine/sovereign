﻿/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
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

using Sovereign.ClientCore.Rendering.Configuration;

namespace Sovereign.ClientCore.Rendering;

/// <summary>
///     Implemented by renderers.
/// </summary>
public interface IRenderer
{
    /// <summary>
    ///     Initializes the renderer.
    /// </summary>
    /// <param name="videoAdapter">Video adapter to use.</param>
    /// <exception cref="RendererInitializationException">
    ///     Thrown if an error occurs while initializing the renderer.
    /// </exception>
    void Initialize(IVideoAdapter videoAdapter);

    /// <summary>
    ///     Shuts down and cleans up the renderer.
    /// </summary>
    void Cleanup();

    /// <summary>
    ///     Reloads device-side resources (e.g. textures) that are currently loaded.
    /// </summary>
    void ReloadResources();

    /// <summary>
    ///     Renders the next frame.
    /// </summary>
    void Render();
}