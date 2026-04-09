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

using MessagePack;
using Sovereign.EngineUtil.Attributes;

namespace Sovereign.EngineCore.Components.Types;

/// <summary>
///     Component type that specifies tilings of a block.
/// </summary>
[MessagePackObject]
[Scriptable]
public struct BlockTile
{
    /// <summary>
    ///     Tile sprite ID of the front face.
    /// </summary>
    [Key(0)]
    [ScriptableField]
    public int FrontFaceId { get; set; }

    /// <summary>
    ///     Tile sprite ID of the top face.
    /// </summary>
    [Key(1)]
    [ScriptableField]
    public int TopFaceId { get; set; }
}