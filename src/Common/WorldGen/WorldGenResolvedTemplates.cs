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

namespace Sovereign.WorldGen;

/// <summary>
///     Template names of a world generation profile resolved against the live template
///     entity set. All later pipeline stages consume template entity IDs only.
/// </summary>
public sealed class WorldGenResolvedTemplates
{
    /// <summary>
    ///     Case-insensitive lookup from template name to template entity ID.
    /// </summary>
    private readonly Dictionary<string, ulong> idsByLowerName = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     Creates a resolved template set.
    /// </summary>
    /// <param name="resolved">Resolved name-to-ID pairs, one per distinct template name
    /// referenced by the profile.</param>
    public WorldGenResolvedTemplates(IEnumerable<(string Name, ulong TemplateEntityId)> resolved)
    {
        foreach (var (name, id) in resolved)
        {
            idsByLowerName[name] = id;
        }
    }

    /// <summary>
    ///     Resolved template entity IDs by template name, preserving the profile's name
    ///     spelling.
    /// </summary>
    public IReadOnlyDictionary<string, ulong> IdsByName => idsByLowerName;

    /// <summary>
    ///     Gets the template entity ID for the given template name.
    /// </summary>
    /// <param name="name">Template name as written in the profile.</param>
    /// <param name="templateEntityId">Resolved template entity ID.</param>
    /// <returns>true if the name was resolved, false otherwise.</returns>
    public bool TryGetId(string name, out ulong templateEntityId)
    {
        return idsByLowerName.TryGetValue(name, out templateEntityId);
    }
}
