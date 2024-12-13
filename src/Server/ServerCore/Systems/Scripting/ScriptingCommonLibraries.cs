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
        color = {}
        
        color.rgba = function (r, g, b, a)
            return ((r & 0xFF) << 24) | ((g & 0xFF) << 16) | ((b & 0xFF) << 8) | (a & 0xFF)
        end

        color.rgb = function (r, g, b)
            return color.rgba(r, g, b, 0xFF)
        end

        -- Color constants by color.
        color.WHITE = color.rgb(255, 255, 255)
        color.BLACK = color.rgb(0, 0, 0)
        color.RED   = color.rgb(255, 0, 0)
        color.GREEN = color.rgb(0, 255, 0)
        color.BLUE  = color.rgb(0, 0, 255)

        -- Color constants by purpose.
        color.MOTD        = color.rgb(160, 160, 240)
        color.ALERT       = color.rgb(210, 80, 80)
        color.CHAT_LOCAL  = color.rgb(179, 179, 179)
        color.CHAT_GLOBAL = color.rgb(255, 255, 255)
        color.CHAT_SYSTEM = color.rgb(128, 128, 128)
    ";
}