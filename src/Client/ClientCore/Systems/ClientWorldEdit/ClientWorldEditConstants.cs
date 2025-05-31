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

namespace Sovereign.ClientCore.Systems.ClientWorldEdit;

/// <summary>
///     Client-side world editor constants.
/// </summary>
public static class ClientWorldEditConstants
{
    /// <summary>
    ///     Minimum value of Z offset.
    /// </summary>
    public const int MinZOffset = -10;

    /// <summary>
    ///     Maximum value of Z offset.
    /// </summary>
    public const int MaxZOffset = 10;

    /// <summary>
    ///     Minimum pen width in blocks.
    /// </summary>
    public const int MinPenWidth = 1;

    /// <summary>
    ///     Maximum pen width in blocks.
    /// </summary>
    public const int MaxPenWidth = 7;
}