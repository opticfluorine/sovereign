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
using Sovereign.WorldLib.World.Domain.Block;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sovereign.WorldLib.World.Domain
{

    /// <summary>
    /// Describes a single world domain.
    /// </summary>
    [MessagePackObject]
    public class WorldDomain
    {

        /// <summary>
        /// Domain ID. Unique within a World.
        /// </summary>
        [Key(0)]
        public uint DomainId { get; set; }

        /// <summary>
        /// Domain blocks within this domain.
        /// </summary>
        [Key(1)]
        public IList<DomainBlock> DomainBlocks { get; set; }

    }

}
