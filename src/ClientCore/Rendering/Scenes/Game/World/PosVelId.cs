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

using System.Numerics;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.World
{

    /// <summary>
    /// Pairs entity metadata with an ID (e.g. tile sprite ID, 
    /// animated sprite ID, etc.).
    /// </summary>
    public struct PosVelId
    {

        /// <summary>
        /// Position.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Velocity.
        /// </summary>
        public Vector3 Velocity;

        /// <summary>
        /// ID.
        /// </summary>
        public int Id;

        /// <summary>
        /// Associated entity ID.
        /// </summary>
        public ulong EntityId;

    }
}
