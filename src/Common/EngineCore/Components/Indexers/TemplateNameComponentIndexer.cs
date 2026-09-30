// Sovereign Engine
// Copyright (c) 2026 opticfluorine
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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Sovereign.EngineCore.Components.Indexers;

/// <summary>
///     Indexes template entities, block and non-block, by name. Template names are
///     expected to be unique; the most recently indexed entity wins for a duplicate name.
/// </summary>
public class TemplateNameComponentIndexer : BaseComponentIndexer<string>
{
    /// <summary>
    ///     Map from template name to template entity ID.
    /// </summary>
    private readonly ConcurrentDictionary<string, ulong> entityIdsByName
        = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     Map from template entity ID to its indexed name, used to drop stale entries on
    ///     rename and removal.
    /// </summary>
    private readonly ConcurrentDictionary<ulong, string> nameByEntity = new();

    public TemplateNameComponentIndexer(NameComponentCollection names,
        TemplateNameComponentFilter filter)
        : base(names, filter)
    {
    }

    /// <summary>
    ///     Gets every indexed template entity, keyed by name. The lookup is
    ///     case-insensitive.
    /// </summary>
    public IReadOnlyDictionary<string, ulong> EntitiesByName => entityIdsByName;

    /// <summary>
    ///     Gets the template entity associated with the given name, if any. The lookup is
    ///     case-insensitive.
    /// </summary>
    /// <param name="name">Template name.</param>
    /// <param name="templateEntityId">Template entity ID.</param>
    /// <returns>true if a template with the name is indexed, false otherwise.</returns>
    public bool TryGetByName(string name, out ulong templateEntityId)
    {
        return entityIdsByName.TryGetValue(name, out templateEntityId);
    }

    protected override void ComponentAddedCallback(ulong entityId, string componentValue, bool isLoad)
    {
        entityIdsByName[componentValue] = entityId;
        nameByEntity[entityId] = componentValue;
    }

    protected override void ComponentModifiedCallback(ulong entityId, string componentValue)
    {
        ComponentRemovedCallback(entityId, false);
        ComponentAddedCallback(entityId, componentValue, false);
    }

    protected override void ComponentRemovedCallback(ulong entityId, bool isUnload)
    {
        if (!nameByEntity.TryRemove(entityId, out var oldName)) return;

        entityIdsByName.TryRemove(oldName, out _);
    }
}
