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

namespace Sovereign.ClientCore.Configuration;

/// <summary>
///     Main configuration interface for client constants.
/// </summary>
public interface IClientConfiguration
{
    /// <summary>
    ///     Width of a tile in pixels.
    /// </summary>
    int TileWidth { get; }

    /// <summary>
    ///     Extra tiles to search for renderable entities along the x axis.
    /// </summary>
    float RenderSearchSpacerX { get; }

    /// <summary>
    ///     Extra tiles to search for renderable entities along the y axis.
    /// </summary>
    float RenderSearchSpacerY { get; }

    /// <summary>
    ///     Maximum framerate (in frames per second).
    /// </summary>
    int MaxFramerate { get; }

    /// <summary>
    ///     Whether to run in fullscreen mode.
    /// </summary>
    bool Fullscreen { get; }
}