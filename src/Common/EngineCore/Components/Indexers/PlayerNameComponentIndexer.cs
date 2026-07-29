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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Sovereign.EngineUtil.Text;

namespace Sovereign.EngineCore.Components.Indexers;

/// <summary>
///     Indexes players by name.
/// </summary>
public class PlayerNameComponentIndexer : BaseComponentIndexer<string>
{
    /// <summary>
    ///     Map from player name to entity ID.
    /// </summary>
    private readonly ConcurrentDictionary<string, ulong> entityIdsByName
        = new(StringComparer.OrdinalIgnoreCase);

    private readonly FuzzyMatcher fuzzy = new();
    private readonly Lock fuzzyLock = new();

    public PlayerNameComponentIndexer(NameComponentCollection names, PlayerNameEventFilter filter)
        : base(names, filter)
    {
    }

    /// <summary>
    ///     Gets the currently logged in player associated with the given name, if any.
    ///     The lookup is case-insensitive.
    /// </summary>
    /// <param name="name">Player name.</param>
    /// <param name="playerEntityId">Player entity ID.</param>
    /// <returns>true if the player is currently logged in, false otherwise.</returns>
    public bool TryGetPlayerByName(string name, out ulong playerEntityId)
    {
        return entityIdsByName.TryGetValue(name, out playerEntityId);
    }

    /// <summary>
    ///     Appends up to <paramref name="maxResults" /> fuzzy matches for the
    ///     given query name to the caller-supplied <paramref name="results" />
    ///     list. Matches are ordered by descending similarity score; ties are
    ///     broken alphabetically. The caller-supplied list is mutated
    ///     (append semantics); pre-existing entries are preserved.
    /// </summary>
    /// <param name="query">Query name.</param>
    /// <param name="maxResults">Maximum number of results to append.</param>
    /// <param name="results">Caller-supplied list to append to.</param>
    public void AppendFuzzyMatches(string query, int maxResults,
        List<(string Name, float Score)> results)
    {
        lock (fuzzyLock)
        {
            fuzzy.AppendBestMatches(query, entityIdsByName.Keys, maxResults, results);
        }
    }

    protected override void ComponentAddedCallback(ulong entityId, string componentValue, bool isLoad)
    {
        entityIdsByName[componentValue] = entityId;
    }

    protected override void ComponentModifiedCallback(ulong entityId, string componentValue)
    {
        entityIdsByName[componentValue] = entityId;
    }

    protected override void ComponentRemovedCallback(ulong entityId, bool isUnload)
    {
        var maybeName = components.GetComponentForEntity(entityId, true);
        if (maybeName.HasValue) entityIdsByName.Remove(maybeName.Value, out _);
    }
}