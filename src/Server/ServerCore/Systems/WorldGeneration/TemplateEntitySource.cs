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
using System.Collections.Generic;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Entities;

namespace Sovereign.ServerCore.Systems.WorldGeneration;

/// <summary>
///     Default IWorldGenTemplateSource, backed by the live template entities tracked by
///     the entity table and the name component collection.
/// </summary>
public sealed class TemplateEntitySource : IWorldGenTemplateSource
{
    private readonly EntityTable entityTable;
    private readonly NameComponentCollection names;

    public TemplateEntitySource(EntityTable entityTable, NameComponentCollection names)
    {
        this.entityTable = entityTable;
        this.names = names;
    }

    /// <summary>
    ///     Gets every named template entity available for resolution.
    /// </summary>
    /// <returns>Template names mapped to template entity IDs. Later duplicates of the
    ///     same name (case-insensitively) overwrite earlier ones.</returns>
    public IReadOnlyDictionary<string, ulong> GetNamedTemplates()
    {
        var templates = new Dictionary<string, ulong>(StringComparer.Ordinal);
        for (var id = EntityConstants.FirstTemplateEntityId;
             id < entityTable.NextTemplateEntityId;
             ++id)
        {
            if (!entityTable.Exists(id)) continue;
            if (!names.HasComponentForEntity(id)) continue;
            templates[names[id]] = id;
        }

        return templates;
    }
}
