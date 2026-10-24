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

using Sovereign.EngineUtil.Attributes;

namespace Sovereign.EngineCore.Components.Types;

/// <summary>
///     Face of a block targeted by an item use.
/// </summary>
[Scriptable]
[ScriptableEnum]
public enum ItemUseFace
{
    /// <summary>
    ///     No block face was targeted.
    /// </summary>
    None = 0,

    /// <summary>
    ///     Front face of a block.
    /// </summary>
    BlockFrontFace = 1,

    /// <summary>
    ///     Top face of a block.
    /// </summary>
    BlockTopFace = 2
}
