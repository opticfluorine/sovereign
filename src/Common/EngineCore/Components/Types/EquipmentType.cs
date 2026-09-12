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
///     Type of equipment that an item may be equipped as or that an equipment slot accepts.
/// </summary>
/// <remarks>
///     Values are explicitly indexed from zero so that they can be used to index arrays;
///     there is deliberately no "none" value, since absence of the component means "not equippable".
/// </remarks>
[Scriptable]
[ScriptableEnum]
public enum EquipmentType
{
    /// <summary>
    ///     Weapon.
    /// </summary>
    Weapon = 0,

    /// <summary>
    ///     Offhand item such as a shield.
    /// </summary>
    Offhand = 1,

    /// <summary>
    ///     Helmet.
    /// </summary>
    Helmet = 2,

    /// <summary>
    ///     Chest armor.
    /// </summary>
    Chest = 3,

    /// <summary>
    ///     Leggings.
    /// </summary>
    Leggings = 4,

    /// <summary>
    ///     Necklace.
    /// </summary>
    Necklace = 5,

    /// <summary>
    ///     Ring worn on the left hand.
    /// </summary>
    LeftRing = 6,

    /// <summary>
    ///     Ring worn on the right hand.
    /// </summary>
    RightRing = 7
}
