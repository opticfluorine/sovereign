// Sovereign Engine
// Copyright (c) 2024 opticfluorine
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;

namespace Sovereign.EngineCore.Entities;

public class EntityTable
{
    /// <summary>
    ///     Set of all entities that are currently held in memory.
    /// </summary>
    private readonly HashSet<ulong> entities = new();

    /// <summary>
    ///     Set of all entities that are enqueued to be added to the table.
    /// </summary>
    private readonly HashSet<ulong> pendingAdds = new();

    /// <summary>
    ///     Set of all entities that are enqueued to be removed from the table.
    /// </summary>
    private readonly HashSet<ulong> pendingRemoves = new();

    /// <summary>
    ///     Checks whether the given entity is current in memory.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <returns>true if in memory, false otherwise.</returns>
    public bool Exists(ulong entityId)
    {
        return entities.Contains(entityId);
    }

    /// <summary>
    ///     Enqueues an entity to be added to the table.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    public void Add(ulong entityId)
    {
        if (Exists(entityId)) return;
        pendingAdds.Add(entityId);
    }

    /// <summary>
    ///     Enqueues an entity to be removed from the table.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    public void Remove(ulong entityId)
    {
        if (!Exists(entityId)) return;
        pendingRemoves.Remove(entityId);
    }

    /// <summary>
    ///     Applies all pending entity table updates.
    /// </summary>
    public void UpdateAllEntities()
    {
        // Additions.
        foreach (var entityId in pendingAdds) entities.Add(entityId);

        // Removals.
        foreach (var entityId in pendingRemoves) entities.Remove(entityId);

        // Reset pending sets.
        pendingAdds.Clear();
        pendingRemoves.Clear();
    }
}