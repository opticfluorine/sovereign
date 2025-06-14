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

using Sovereign.EngineUtil.Attributes;

namespace Sovereign.EngineCore.Components.Types;

/// <summary>
///     Entity type for non-block entities.
/// </summary>
[Scriptable]
[ScriptableEnum]
public enum EntityType
{
    /// <summary>
    ///     Entity is an NPC.
    /// </summary>
    Npc = 0,

    /// <summary>
    ///     Entity is an item.
    /// </summary>
    Item = 1,

    /// <summary>
    ///     Entity is a player.
    /// </summary>
    Player = 2,

    /// <summary>
    ///     Entity has no special type. Not explicitly stored; only used in EntityDefinition.
    /// </summary>
    Other = 0x7F
}