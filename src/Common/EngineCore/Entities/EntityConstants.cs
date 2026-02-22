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

using System.Globalization;

namespace Sovereign.EngineCore.Entities;

/// <summary>
///     Contains global constants related to entities.
/// </summary>
public static class EntityConstants
{
    /// <summary>
    ///     Special entity ID meaning no entity.
    /// </summary>
    public const ulong NoEntity = 0;

    /// <summary>
    ///     Entity ID of the first template entity.
    /// </summary>
    public const ulong FirstTemplateEntityId = 0x7ffe000000000000;

    /// <summary>
    ///     Entity ID of the last possible template entity.
    /// </summary>
    public const ulong LastTemplateEntityId = 0x7ffeffffffffffff;

    /// <summary>
    ///     Entity ID of the first persisted entity.
    /// </summary>
    public const ulong FirstPersistedEntityId = 0x7fff000000000000;

    /// <summary>
    ///     Entity ID of the first block entity.
    /// </summary>
    public const ulong FirstBlockEntityId = 0x6fff000000000000;

    /// <summary>
    ///     Entity ID of the last block entity.
    /// </summary>
    public const ulong LastBlockEntityId = 0x6fffffffffffffff;

    /// <summary>
    ///     Maximum entity name length.
    /// </summary>
    public const int MaxNameLength = 32;

    /// <summary>
    ///     Callback name for entity added.
    /// </summary>
    public const string AddCallbackName = "OnEntityAdded";

    /// <summary>
    ///     Entity data key for entity added callback script name.
    /// </summary>
    public const string AddCallbackScriptKey = $"__{AddCallbackName}_Script";

    /// <summary>
    ///     Entity data key for entity added callback function name.
    /// </summary>
    public const string AddCallbackFunctionKey = $"__{AddCallbackName}_Function";

    /// <summary>
    ///     Callback name for entity removed.
    /// </summary>
    public const string RemoveCallbackName = "OnEntityRemoved";

    /// <summary>
    ///     Entity data key for entity removed callback script name.
    /// </summary>
    public const string RemoveCallbackScriptKey = $"__{RemoveCallbackName}_Script";

    /// <summary>
    ///     Entity data key for entity removed callback function name.
    /// </summary>
    public const string RemoveCallbackFunctionKey = $"__{RemoveCallbackName}_Function";

    /// <summary>
    ///     Callback name for entity loaded.
    /// </summary>
    public const string LoadCallbackName = "OnEntityLoaded";

    /// <summary>
    ///     Entity data key for entity loaded callback script name.
    /// </summary>
    public const string LoadCallbackScriptKey = $"__{LoadCallbackName}_Script";

    /// <summary>
    ///     Entity data key for entity loaded callback function name.
    /// </summary>
    public const string LoadCallbackFunctionKey = $"__{LoadCallbackName}_Function";

    /// <summary>
    ///     Callback name for entity unloaded.
    /// </summary>
    public const string UnloadCallbackName = "OnEntityUnloaded";

    /// <summary>
    ///     Entity data key for entity unloaded callback script name.
    /// </summary>
    public const string UnloadCallbackScriptKey = $"__{UnloadCallbackName}_Script";

    /// <summary>
    ///     Entity data key for entity unloaded callback function name.
    /// </summary>
    public const string UnloadCallbackFunctionKey = $"__{UnloadCallbackName}_Function";

    /// <summary>
    ///     Callback name for entity interact.
    /// </summary>
    public const string InteractCallbackName = "OnInteract";

    /// <summary>
    ///     Entity data key for interact callback script name.
    /// </summary>
    public const string InteractScriptKey = $"__{InteractCallbackName}_Script";

    /// <summary>
    ///     Entity data key for interact callback function name.
    /// </summary>
    public const string InteractFunctionKey = $"__{InteractCallbackName}_Function";

    /// <summary>
    ///     Maximum length in bytes of an entity's key-value data key.
    /// </summary>
    public const int MaxEntityDataKeyLength = 64;

    /// <summary>
    ///     Maximum length in bytes of an entity's key-value data value.
    /// </summary>
    public const int MaxEntityDataValueLength = 128;

    /// <summary>
    ///     CultureInfo for formatting and parsing entity data when string conversions are necessary.
    /// </summary>
    public static readonly CultureInfo EntityDataCulture = new("en-us");
}

/// <summary>
///     Helper functions for entities.
/// </summary>
public static class EntityUtil
{
    /// <summary>
    ///     Checks whether the given entity is a template entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <returns>true if template entity, false otherwise.</returns>
    public static bool IsTemplateEntity(ulong entityId)
    {
        return entityId is >= EntityConstants.FirstTemplateEntityId and <= EntityConstants.LastTemplateEntityId;
    }

    public static bool IsBlockEntity(ulong entityId)
    {
        return entityId is >= EntityConstants.FirstBlockEntityId and <= EntityConstants.LastBlockEntityId;
    }
}