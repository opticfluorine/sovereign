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
using System.Globalization;
using Sovereign.Persistence.Database;
using Sovereign.ServerCore.Systems.WorldGeneration;

namespace Sovereign.Persistence.WorldGen;

/// <summary>
///     Default IWorldGenWorldRegistryStore, backed by the global key-value store.
/// </summary>
public sealed class WorldGenWorldRegistryStore : IWorldGenWorldRegistryStore
{
    private readonly PersistenceProviderManager providerManager;

    public WorldGenWorldRegistryStore(PersistenceProviderManager providerManager)
    {
        this.providerManager = providerManager;
    }

    /// <summary>
    ///     Loads every registered world from the database.
    /// </summary>
    /// <returns>Registered worlds, in key order. Malformed entries are skipped.</returns>
    public IReadOnlyList<WorldGenRegistryEntry> LoadWorlds()
    {
        var provider = providerManager.PersistenceProvider;
        var entries = new SortedDictionary<int, WorldGenRegistryEntry>();
        provider.TransactionLock.Acquire();
        try
        {
            using var reader = provider.GetGlobalKeyValuePairsQuery.GetGlobalKeyValuePairs();
            while (reader.Reader.Read())
            {
                var key = reader.Reader.GetString(0);
                if (key is null || !key.StartsWith(WorldGenWorldRegistry.KeyPrefix,
                        StringComparison.Ordinal)) continue;
                if (!int.TryParse(key[WorldGenWorldRegistry.KeyPrefix.Length..],
                        NumberStyles.Integer, CultureInfo.InvariantCulture, out var index)) continue;

                var entry = WorldGenRegistryEntry.FromJson(reader.Reader.GetString(1) ?? "");
                if (entry is not null) entries[index] = entry;
            }
        }
        finally
        {
            provider.TransactionLock.Release();
        }

        return new List<WorldGenRegistryEntry>(entries.Values);
    }

    /// <summary>
    ///     Appends a world to the registry.
    /// </summary>
    /// <param name="entry">World to register.</param>
    public void AppendWorld(WorldGenRegistryEntry entry)
    {
        var provider = providerManager.PersistenceProvider;
        var index = 0;
        provider.TransactionLock.Acquire();
        try
        {
            using (var reader = provider.GetGlobalKeyValuePairsQuery.GetGlobalKeyValuePairs())
            {
                while (reader.Reader.Read())
                {
                    var key = reader.Reader.GetString(0);
                    if (key is null || !key.StartsWith(WorldGenWorldRegistry.KeyPrefix,
                            StringComparison.Ordinal)) continue;
                    if (int.TryParse(key[WorldGenWorldRegistry.KeyPrefix.Length..],
                            NumberStyles.Integer, CultureInfo.InvariantCulture, out var existing))
                    {
                        index = Math.Max(index, existing + 1);
                    }
                }
            }

            using var transaction = provider.Connection.BeginTransaction();
            provider.UpdateGlobalKeyValuePairQuery.UpdateGlobalKeyValuePair(
                WorldGenWorldRegistry.KeyPrefix + index.ToString(CultureInfo.InvariantCulture),
                entry.ToJson(), transaction);
            transaction.Commit();
        }
        finally
        {
            provider.TransactionLock.Release();
        }
    }
}
