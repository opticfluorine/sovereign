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
using System.Threading;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineUtil.Attributes;

namespace Sovereign.ServerCore.Systems.Scripting;

/// <summary>
///     Lua scripting library for working with items.
/// </summary>
[ScriptableLibrary("Items")]
public sealed class ItemsScripting(ItemTemplateNameComponentIndexer itemTemplateNameIndex)
{
    /// <summary>
    ///     Server-side cap on the number of fuzzy results returned per call.
    /// </summary>
    private const int MaxFuzzyResults = 64;

    /// <summary>
    ///     Lock object serializing all public method calls so that the
    ///     reusable scratch is not accessed concurrently across Lua hosts.
    /// </summary>
    private readonly Lock syncRoot = new();

    /// <summary>
    ///     Reusable scratch list for fuzzy (name, score) tuples. Cleared and
    ///     refilled on each <see cref="FindByFuzzyName" /> call. Only ever
    ///     touched while holding <see cref="syncRoot" />.
    /// </summary>
    private readonly List<(string Name, float Score)> fuzzyScratch = new();

    /// <summary>
    ///     Looks up item template entities by name (case-insensitive).
    ///     Because item template names are not unique, all matching entity IDs
    ///     are returned.
    /// </summary>
    /// <param name="name">Item template name.</param>
    /// <returns>
    ///     A freshly allocated list of item template entity IDs matching the
    ///     name (empty if none). The list is owned by the caller and safe to
    ///     read after this method returns.
    /// </returns>
    [ScriptableFunction("FindByName")]
    public List<ulong> FindByName(string name)
    {
        lock (syncRoot)
        {
            return itemTemplateNameIndex.TryGetByName(name, out var ids)
                ? new List<ulong>(ids)
                : new List<ulong>();
        }
    }

    /// <summary>
    ///     Performs a fuzzy lookup of item template entities by name,
    ///     returning the best matches ordered by descending similarity score
    ///     (ties broken alphabetically). <paramref name="maxResults" /> caps
    ///     the number of distinct names selected; each selected name is then
    ///     expanded to all of its entity IDs, so the returned list may contain
    ///     more than <paramref name="maxResults" /> entries.
    /// </summary>
    /// <param name="name">Query name.</param>
    /// <param name="maxResults">
    ///     Maximum number of distinct names to select. Clamped to [1, 64]
    ///     server-side.
    /// </param>
    /// <returns>
    ///     Best-first matches in a freshly allocated list owned by the caller.
    ///     The list is safe to read after this method returns because it is
    ///     not retained internally.
    /// </returns>
    [ScriptableFunction("FindByFuzzyName")]
    public List<ItemTemplateMatch> FindByFuzzyName(string name, int maxResults)
    {
        if (maxResults < 1) maxResults = 1;
        if (maxResults > MaxFuzzyResults) maxResults = MaxFuzzyResults;

        lock (syncRoot)
        {
            fuzzyScratch.Clear();
            itemTemplateNameIndex.AppendFuzzyMatches(name, maxResults, fuzzyScratch);

            var results = new List<ItemTemplateMatch>();
            foreach (var (n, s) in fuzzyScratch)
            {
                if (!itemTemplateNameIndex.TryGetByName(n, out var ids)) continue;
                foreach (var eid in ids)
                {
                    results.Add(new ItemTemplateMatch
                    {
                        EntityId = eid,
                        Name = n,
                        Score = s
                    });
                }
            }

            return results;
        }
    }
}
