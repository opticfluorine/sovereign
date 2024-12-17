// Sovereign Engine
// Copyright (c) 2024 opticfluorine
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.Numerics;

namespace Sovereign.EngineUtil.Types;

/// <summary>
///     Utility functions for the scripting system.
/// </summary>
public static class ColorUtil
{
    private const uint RMask = 0xff000000;
    private const uint GMask = 0x00ff0000;
    private const uint BMask = 0x0000ff00;
    private const uint AMask = 0x000000ff;

    /// <summary>
    ///     Unpacks a color that was created with the 'colors.rgb' Lua function.
    /// </summary>
    /// <param name="packedColor">Packed color.</param>
    /// <returns>Unpacked color.</returns>
    public static Vector3 UnpackColorRgb(uint packedColor)
    {
        var r = (byte)((packedColor & RMask) >> 24);
        var g = (byte)((packedColor & GMask) >> 16);
        var b = (byte)((packedColor & BMask) >> 8);

        return new Vector3(r / 255.0f, g / 255.0f, b / 255.0f);
    }

    /// <summary>
    ///     Unpacks a color that was created with the 'colors.rgba' Lua function.
    /// </summary>
    /// <param name="packedColor">Packed color.</param>
    /// <returns>Unpacked color.</returns>
    public static Vector4 UnpackColorRgba(uint packedColor)
    {
        var r = (byte)((packedColor & RMask) >> 24);
        var g = (byte)((packedColor & GMask) >> 16);
        var b = (byte)((packedColor & BMask) >> 8);
        var a = (byte)(packedColor & AMask);

        return new Vector4(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f);
    }
}