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

namespace Sovereign.EngineCore.Entities;

/// <summary>
///     Provides methods for creating new entities.
/// </summary>
public interface IEntityFactory
{
    /// <summary>
    ///     Gets an entity builder.
    /// </summary>
    /// <param name="entityType">Entity type.</param>
    /// <returns>Entity builder.</returns>
    IEntityBuilder GetBuilder(EntityType entityType = EntityType.Standard);

    /// <summary>
    ///     Gets an entity builder for the given entity ID.
    /// </summary>
    /// <param name="entityId">Entity ID for the new entity.</param>
    /// <param name="load">If true, loads rather than creates the entity.</param>
    /// <returns>Entity builder.</returns>
    IEntityBuilder GetBuilder(ulong entityId, bool load = false);
}