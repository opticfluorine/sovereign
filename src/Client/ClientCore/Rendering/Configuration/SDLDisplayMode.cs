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

namespace Sovereign.ClientCore.Rendering.Configuration;

/// <summary>
///     Display format type produced by SDLDisplayModeEnumerator.
/// </summary>
public class SDLDisplayMode : IDisplayMode
{
    public SDLDisplayMode(int width, int height, DisplayFormat format)
    {
        Width = width;
        Height = height;
        DisplayFormat = format;
    }

    public int Width { get; }

    public int Height { get; }

    public DisplayFormat DisplayFormat { get; }
}