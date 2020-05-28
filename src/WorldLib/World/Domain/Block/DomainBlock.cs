/*
 * Sovereign Engine
 * Copyright (c) 2020 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sovereign.WorldLib.World.Domain.Block
{

    /// <summary>
    /// Describes a single block within a domain.
    /// </summary>
    [MessagePackObject]
    public class DomainBlock
    {

        /// <summary>
        /// X coordinate of the top-northwest corner of the block.
        /// </summary>
        [Key(0)]
        public int X { get; set; }

        /// <summary>
        /// Y coordinate of the top-northwest corner of the block.
        /// </summary>
        [Key(1)]
        public int Y { get; set; }

        /// <summary>
        /// Z coordinate of the top-northwest corner of the block.
        /// </summary>
        [Key(2)]
        public int Z { get; set; }

        /// <summary>
        /// Material ID of this block.
        /// </summary>
        [Key(3)]
        public uint MaterialId { get; set; }

        /// <summary>
        /// Material modifier of this block.
        /// </summary>
        [Key(4)]
        public uint MaterialModifier { get; set; }

    }
}
