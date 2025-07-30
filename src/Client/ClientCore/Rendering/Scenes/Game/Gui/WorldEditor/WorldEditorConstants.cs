// Sovereign Engine
// Copyright (c) 2025 opticfluorine
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

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui.WorldEditor;

public static class WorldEditorConstants
{
    /// <summary>
    ///     Color used by help text.
    /// </summary>
    public static readonly Vector4 HelpTextColor = new(0.7f, 0.7f, 0.7f, 1.0f);

    /// <summary>
    ///     Color used for selected buttons.
    /// </summary>
    public static readonly Vector4 SelectedColor = new(0.06f, 0.53f, 0.98f, 1.0f);

    /// <summary>
    ///     Color used for unselected buttons.
    /// </summary>
    public static readonly Vector4 UnselectedColor = new(0.26f, 0.59f, 0.98f, 0.40f);
}