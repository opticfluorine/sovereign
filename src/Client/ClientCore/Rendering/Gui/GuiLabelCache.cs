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

using System;
using System.Collections.Generic;

namespace Sovereign.ClientCore.Rendering.Gui;

/// <summary>
///     Auto-resizing cache of indexed labels for GUI components.
/// </summary>
/// <param name="prefix">Label prefix.</param>
public sealed class GuiLabelCache(string prefix)
{
    private readonly List<string> labels = new();

    /// <summary>
    ///     Gets the given label.
    /// </summary>
    /// <param name="index">Label index.</param>
    /// <exception cref="IndexOutOfRangeException">Thrown if the index is negative.</exception>
    public string this[int index]
    {
        get
        {
            if (index < 0) throw new IndexOutOfRangeException("Index must be nonnegative.");
            if (index >= labels.Count)
            {
                for (var i = labels.Count; i <= index; ++i)
                {
                    labels.Add($"{prefix}{i}");
                }
            }

            return labels[index];
        }
    }
}