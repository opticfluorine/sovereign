/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
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
using System.Threading;

namespace Sovereign.Persistence.Entities;

/// <summary>
///     Maps persisted entity IDs to volatile entity IDs and vice versa.
/// </summary>
public sealed class ExistingEntitySet
{
    private const int DefaultSize = 65536;
    private readonly HashSet<ulong> knownEntities = new(DefaultSize);
    private readonly Lock setLock = new();

    /// <summary>
    ///     Marks the entity as existing so that the persistence system does not try to recreate it.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <returns>true if the entity needs to be created, false otherwise.</returns>
    public bool MarkAsExists(ulong entityId)
    {
        lock (setLock)
        {
            if (knownEntities.Contains(entityId)) return false;
            knownEntities.Add(entityId);
            return true;
        }
    }

    /// <summary>
    ///     Checks whether the entity is known to exist.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <returns>true if entity exists, false otherwise.</returns>
    public bool Exists(ulong entityId)
    {
        lock (setLock)
        {
            return knownEntities.Contains(entityId);
        }
    }
}