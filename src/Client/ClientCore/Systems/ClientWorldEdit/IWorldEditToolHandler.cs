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

namespace Sovereign.ClientCore.Systems.ClientWorldEdit;

/// <summary>
///     Interface for world editor tool handlers.
/// </summary>
public interface IWorldEditToolHandler
{
    /// <summary>
    ///     Processes a draw action for the tool handler.
    /// </summary>
    void ProcessDraw();

    /// <summary>
    ///     Processes an erase action for the tool handler.
    /// </summary>
    void ProcessErase();

    /// <summary>
    ///     Resets the tool handler state.
    /// </summary>
    void Reset();
}