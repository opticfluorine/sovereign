// Sovereign Engine
// Copyright (c) 2025 opticfluorine
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

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Sovereign.EngineCore.Systems.Data;

/// <summary>
///     Data store for globally scoped key-value pairs.
/// </summary>
internal class GlobalKeyValueStore
{
    private readonly DataInternalController internalController;
    private readonly ILogger<GlobalKeyValueStore> logger;

    public GlobalKeyValueStore(DataInternalController internalController,
        ILogger<GlobalKeyValueStore> logger)
    {
        this.internalController = internalController;
        this.logger = logger;
    }

    /// <summary>
    ///     Key-value store.
    /// </summary>
    public ConcurrentDictionary<string, string> KeyValueStore { get; } = new();

    /// <summary>
    ///     Adds or updates a global key-value pair.
    /// </summary>
    /// <param name="key">Global key.</param>
    /// <param name="value">Global value.</param>
    public void SetGlobal(string key, string value)
    {
        logger.LogDebug("Global: Set {Key} = {Value}.", key, value);

        var isNewKey = KeyValueStore.ContainsKey(key);
        KeyValueStore[key] = value;

        internalController.GlobalSet(key, value);
    }

    /// <summary>
    ///     Removes a global key-value pair.
    /// </summary>
    /// <param name="key">Global key.</param>
    public void RemoveGlobal(string key)
    {
        if (KeyValueStore.TryRemove(key, out _))
        {
            logger.LogDebug("Global: Remove {Key}.", key);
            internalController.GlobalRemoved(key);
        }
    }
}