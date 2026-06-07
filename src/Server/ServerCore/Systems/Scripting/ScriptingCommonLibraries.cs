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

namespace Sovereign.ServerCore.Systems.Scripting;

/// <summary>
///     Common Lua libraries to be included with each script host.
/// </summary>
public static class ScriptingCommonLibraries
{
    /// <summary>
    ///     Lua code for the global 'colors' module.
    /// </summary>
    public const string Color = @"
        Color = {}
        
        Color.Rgba = function (r, g, b, a)
            return ((r & 0xFF) << 24) | ((g & 0xFF) << 16) | ((b & 0xFF) << 8) | (a & 0xFF)
        end

        Color.Rgb = function (r, g, b)
            return Color.Rgba(r, g, b, 0xFF)
        end

        -- Color constants by Color.
        Color.WHITE = Color.Rgb(255, 255, 255)
        Color.BLACK = Color.Rgb(0, 0, 0)
        Color.RED   = Color.Rgb(255, 0, 0)
        Color.GREEN = Color.Rgb(0, 255, 0)
        Color.BLUE  = Color.Rgb(0, 0, 255)

        -- Color constants by purpose.
        Color.MOTD        = Color.Rgb(160, 160, 240)
        Color.ALERT       = Color.Rgb(210, 80, 80)
        Color.CHAT_LOCAL  = Color.Rgb(179, 179, 179)
        Color.CHAT_GLOBAL = Color.Rgb(255, 255, 255)
        Color.CHAT_SYSTEM = Color.Rgb(128, 128, 128)
    ";
}