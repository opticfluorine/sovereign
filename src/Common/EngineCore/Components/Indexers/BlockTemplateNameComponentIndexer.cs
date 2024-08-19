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

using System.Collections.Generic;
using Castle.Core.Logging;
using Sovereign.EngineCore.Logging;

namespace Sovereign.EngineCore.Components.Indexers;

/// <summary>
///     Indexer that allows case-insensitive lookup of block template entities by name.
/// </summary>
public class BlockTemplateNameComponentIndexer : BaseComponentIndexer<string>
{
    private readonly LoggingUtil loggingUtil;

    /// <summary>
    ///     Map from lowercase name to block template entity ID.
    /// </summary>
    private readonly Dictionary<string, ulong> namedBlocks = new();

    /// <summary>
    ///     Reverse mapping used when name changes occur.
    /// </summary>
    private readonly Dictionary<ulong, string> reverseMap = new();

    public BlockTemplateNameComponentIndexer(NameComponentCollection names, BlockTemplateNameComponentFilter filter,
        LoggingUtil loggingUtil)
        : base(names, filter)
    {
        this.loggingUtil = loggingUtil;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    /// <summary>
    ///     Tries to get the block template entity with the given entity, if any. A case-insensitive
    ///     search is performed.
    /// </summary>
    /// <param name="name">Name to find.</param>
    /// <param name="entityId">Set to the corresponding entity ID if any, otherwise set to zero.</param>
    /// <returns>true if a block template entity with the given name was found, false otherwise.</returns>
    public bool TryGetByName(string name, out ulong entityId)
    {
        var lowerName = name.ToLower();
        return namedBlocks.TryGetValue(lowerName, out entityId);
    }

    protected override void ComponentAddedCallback(ulong entityId, string componentValue, bool isLoad)
    {
        var lowerName = componentValue.ToLower();
        if (namedBlocks.TryGetValue(lowerName, out var otherEntityId))
        {
            Logger.WarnFormat("Entity {0} conflicts with entity {1}; it will not be searchable by name.",
                componentValue, loggingUtil.FormatEntity(entityId));
            return;
        }

        namedBlocks[lowerName] = entityId;
        reverseMap[entityId] = lowerName;
    }

    protected override void ComponentModifiedCallback(ulong entityId, string componentValue)
    {
        ComponentRemovedCallback(entityId, false);
        ComponentAddedCallback(entityId, componentValue, false);
    }

    protected override void ComponentRemovedCallback(ulong entityId, bool isUnload)
    {
        if (!reverseMap.TryGetValue(entityId, out var oldLowerName)) return;

        namedBlocks.Remove(oldLowerName);
        reverseMap.Remove(entityId);
    }
}