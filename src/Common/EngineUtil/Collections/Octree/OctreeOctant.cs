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

using System;

namespace Sovereign.EngineUtil.Collections.Octree;

/// <summary>
///     Enumeration of octants in an OctreeNode.
/// </summary>
[Flags]
internal enum OctreeOctant
{
    /// <summary>
    ///     Indicates the octant is in the top half (greater z) of the space
    ///     spanned by the OctreeNode.
    /// </summary>
    Top = 1 << 0,

    /// <summary>
    ///     Indicates the octant is in the north half (greater y) of the space
    ///     spanned by the OctreeNode.
    /// </summary>
    North = 1 << 1,

    /// <summary>
    ///     Indicates the octant is in the eastern half (greater x) of the space
    ///     spanned by the OctreeNode.
    /// </summary>
    East = 1 << 2
}