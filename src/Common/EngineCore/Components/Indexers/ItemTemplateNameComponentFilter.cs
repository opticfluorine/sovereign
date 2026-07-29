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

using Sovereign.EngineCore.Components.Types;

namespace Sovereign.EngineCore.Components.Indexers;

/// <summary>
///     Component filter that only accepts events from the Name component
///     attached to an item template entity.
/// </summary>
public class ItemTemplateNameComponentFilter : TemplateEntityComponentFilter<string>
{
    private readonly EntityTypeComponentCollection entityTypes;

    public ItemTemplateNameComponentFilter(EntityTypeComponentCollection entityTypes, NameComponentCollection names)
        : base(names, names)
    {
        this.entityTypes = entityTypes;
    }

    protected override bool ShouldAccept(ulong entityId)
    {
        return base.ShouldAccept(entityId)
               && entityTypes.TryGetValue(entityId, out var entityType)
               && entityType == EntityType.Item;
    }
}
