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

using System.Collections.Generic;

namespace Sovereign.ClientCore.Systems.Block.Caches
{

    /// <summary>
    /// Interface to the block system's animated sprite cache.
    /// </summary>
    public interface IBlockAnimatedSpriteCache
    {

        /// <summary>
        /// Gets the front face animated sprite IDs for the given block.
        /// </summary>
        /// <param name="blockId">Block entity ID.</param>
        /// <returns>Front face animated sprite IDs.</returns>
        IList<int> GetFrontFaceAnimatedSpriteIds(ulong blockId);

        /// <summary>
        /// Gets the top face animated sprite IDs for the given block.
        /// </summary>
        /// <param name="blockId">Block entity ID.</param>
        /// <returns>Top face animated sprite IDs.</returns>
        IList<int> GetTopFaceAnimatedSpriteIds(ulong blockId);

    }

}

