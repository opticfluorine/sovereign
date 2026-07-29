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
using System.Threading;
using Sovereign.EngineUtil.Text;

namespace Sovereign.EngineCore.Components.Indexers;

/// <summary>
///     Indexer that allows case-insensitive lookup of item template entities
///     by name. Unlike player names, item template names are not required to
///     be unique: a single name may map to multiple item template entity IDs.
/// </summary>
public class ItemTemplateNameComponentIndexer : BaseComponentIndexer<string>
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<ulong, byte>> entitiesByName
        = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<ulong, string> nameByEntity = new();
    private readonly FuzzyMatcher fuzzy = new(caseSensitive: false);
    private readonly Lock fuzzyLock = new();

    public ItemTemplateNameComponentIndexer(NameComponentCollection names, ItemTemplateNameComponentFilter filter)
        : base(names, filter)
    {
    }

    /// <summary>
    ///     Tries to get the item template entity IDs associated with the given
    ///     name. A case-insensitive search is performed. Because item template
    ///     names are not required to be unique, multiple entity IDs may be
    ///     returned for a single name.
    /// </summary>
    /// <param name="name">Name to find.</param>
    /// <param name="entityIds">
    ///     Set to a snapshot collection of the matching item template entity
    ///     IDs if any name matched, otherwise set to an empty collection.
    /// </param>
    /// <returns>
    ///     true if at least one item template entity with the given name was
    ///     found, false otherwise.
    /// </returns>
    public bool TryGetByName(string name, out IReadOnlyCollection<ulong> entityIds)
    {
        if (entitiesByName.TryGetValue(name, out var inner))
        {
            // ConcurrentDictionary.Keys returns a snapshot collection; it is
            // safe to enumerate from another thread under concurrent writes.
            entityIds = (IReadOnlyCollection<ulong>)inner.Keys;
            return true;
        }

        entityIds = Array.Empty<ulong>();
        return false;
    }

    /// <summary>
    ///     Appends up to <paramref name="maxResults" /> fuzzy matches for the
    ///     given query name to the caller-supplied <paramref name="results" />
    ///     list. Each entry corresponds to a distinct name; matches are
    ///     ordered by descending similarity score with ties broken
    ///     alphabetically. The caller-supplied list is mutated (append
    ///     semantics); pre-existing entries are preserved.
    /// </summary>
    /// <param name="query">Query name.</param>
    /// <param name="maxResults">Maximum number of distinct names to append.</param>
    /// <param name="results">Caller-supplied list to append to.</param>
    public void AppendFuzzyMatches(string query, int maxResults,
        List<(string Name, float Score)> results)
    {
        lock (fuzzyLock)
        {
            fuzzy.AppendBestMatches(query, entitiesByName.Keys, maxResults, results);
        }
    }

    protected override void ComponentAddedCallback(ulong entityId, string componentValue, bool isLoad)
    {
        var inner = entitiesByName.GetOrAdd(componentValue, _ => new ConcurrentDictionary<ulong, byte>());
        inner.TryAdd(entityId, 0);
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

        if (entitiesByName.TryGetValue(oldName, out var inner))
        {
            inner.TryRemove(entityId, out _);
            if (inner.IsEmpty)
            {
                entitiesByName.TryRemove(oldName, out _);
            }
        }
    }
}
