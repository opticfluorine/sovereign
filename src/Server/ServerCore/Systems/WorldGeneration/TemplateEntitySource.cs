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

using System.Collections.Generic;
using Sovereign.EngineCore.Components.Indexers;

namespace Sovereign.ServerCore.Systems.WorldGeneration;

/// <summary>
///     Default IWorldGenTemplateSource, backed by the template name component indexer.
/// </summary>
public sealed class TemplateEntitySource : IWorldGenTemplateSource
{
    private readonly TemplateNameComponentIndexer templateNames;

    public TemplateEntitySource(TemplateNameComponentIndexer templateNames)
    {
        this.templateNames = templateNames;
    }

    /// <summary>
    ///     Gets every named template entity available for resolution.
    /// </summary>
    /// <returns>Template names mapped to template entity IDs; the lookup is
    ///     case-insensitive.</returns>
    public IReadOnlyDictionary<string, ulong> GetNamedTemplates()
    {
        return templateNames.EntitiesByName;
    }
}
