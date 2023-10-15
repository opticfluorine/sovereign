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

using System.Text;
using Sovereign.EngineCore.Components.Indexers;

namespace Sovereign.EngineCore.Systems.Block.Events;

/// <summary>
///     Creation record for a single block.
/// </summary>
public struct BlockRecord
{
    /// <summary>
    ///     Block position.
    /// </summary>
    public GridPosition Position;

    /// <summary>
    ///     Material ID.
    /// </summary>
    public int Material;

    /// <summary>
    ///     Material modifier.
    /// </summary>
    public int MaterialModifier;

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("(")
            .Append(Position.X).Append(", ")
            .Append(Position.Y).Append(", ")
            .Append(Position.Z).Append(", ")
            .Append(Material).Append(", ")
            .Append(MaterialModifier).Append(")");
        return sb.ToString();
    }
}