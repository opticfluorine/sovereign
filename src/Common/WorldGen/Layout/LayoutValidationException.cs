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

using System;

namespace Sovereign.WorldGen.Layout;

/// <summary>
///     Raised when a strict layout fails validation after all resample attempts. The message
///     is a consolidated description of the unmatched and shared-mass anchors.
/// </summary>
public sealed class LayoutValidationException : Exception
{
    /// <summary>
    ///     Creates a layout validation exception with the given consolidated message.
    /// </summary>
    /// <param name="message">Consolidated failure message.</param>
    public LayoutValidationException(string message) : base(message)
    {
    }
}
