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

namespace Sovereign.EngineCore.Components
{

    /// <summary>
    /// Interface for removing components.
    /// </summary>
    public interface IComponentRemover
    {

        /// <summary>
        /// Enqueues the removal of the component associated with the given entity ID.
        /// If no entity is associated with the given entity, no action is performed.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        void RemoveComponent(ulong entityId);

        /// <summary>
        /// Enqueues the unloading of the component associated with the given entity ID.
        /// If no entity is associated with the given entity, no action is performed.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        void UnloadComponent(ulong entityId);

    }

}