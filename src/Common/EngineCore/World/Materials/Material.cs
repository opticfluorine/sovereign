/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;

namespace Sovereign.EngineCore.World.Materials;

/// <summary>
///     Describes a material.
/// </summary>
public sealed class Material
{
    /// <summary>
    ///     Reserved ID value for the "air" block.
    /// </summary>
    public const int Air = 0;

    /// <summary>
    ///     Material ID. Unique.
    ///     ID 0 is special and indicates a vacant block (no material/air).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///     Name of the material.
    /// </summary>
    public string MaterialName { get; set; } = "";

    /// <summary>
    ///     Associated material subtypes.
    /// </summary>
    public List<MaterialSubtype> MaterialSubtypes { get; set; } = new();
}