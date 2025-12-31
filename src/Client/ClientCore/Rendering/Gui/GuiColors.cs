// Sovereign Engine
// Copyright (c) 2026 opticfluorine
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

namespace Sovereign.ClientCore.Rendering.Gui;

/// <summary>
///     Common colors for GUI elements.
/// </summary>
public static class GuiColors
{
    /// <summary>
    ///     Text color for disabled/greyed-out text.
    /// </summary>
    public static readonly Vector4 DisabledText = new(0.7f, 0.7f, 0.7f, 1.0f);
}