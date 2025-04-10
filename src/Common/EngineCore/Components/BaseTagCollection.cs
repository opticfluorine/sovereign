// Sovereign Engine
// Copyright (c) 2023 opticfluorine
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

using Sovereign.EngineCore.Entities;

namespace Sovereign.EngineCore.Components;

/// <summary>
///     Base class for component collections which serve as "tags" on entities.
/// </summary>
/// <remarks>
///     Tags are valueless components which impart a property onto an entity simply by their existence.
///     They are implemented as boolean-valued components whose value is always true. This implementation
///     gives them a canonical representation when stored to a database which is independent of the
///     degree of normalization of the entity table - a denormalized entity view would still contain a
///     boolean column just the same as the fully normalized form.
/// </remarks>
public class BaseTagCollection : BaseComponentCollection<bool>
{
    /// <summary>
    ///     Base constructor for tag collections.
    /// </summary>
    /// <param name="componentManager">Component manager, typically supplied by dependency injection.</param>
    /// <param name="initialSize">Initial size of the tag collection.</param>
    /// <param name="componentType">Component type for the tag.</param>
    protected BaseTagCollection(EntityTable entityTable, ComponentManager componentManager, int initialSize,
        ComponentType componentType)
        : base(entityTable, componentManager, initialSize, ComponentOperators.BoolOperators, componentType)
    {
    }

    /// <summary>
    ///     Determines whether the given entity is tagged.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="lookback">If true, consider newly removed tags that were removed in the same tick.</param>
    /// <returns>true if tagged, false otherwise.</returns>
    public bool HasTagForEntity(ulong entityId, bool lookback = false)
    {
        return HasComponentForEntity(entityId, lookback) && this[entityId];
    }

    /// <summary>
    ///     Determines whether the given entity is explicitly tagged (i.e. not tagged through its template
    ///     entity).
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="lookback">If true, consider newly removed tags that were removed in the same tick.</param>
    /// <returns>true if explicitly tagged, false otherwise.</returns>
    public bool HasLocalTagForEntity(ulong entityId, bool lookback = false)
    {
        return HasLocalComponentForEntity(entityId, lookback) && this[entityId];
    }

    /// <summary>
    ///     Determines whether the given entity will be newly tagged in the following tick.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <returns>true if will be tagged, false otherwise.</returns>
    /// <remarks>
    ///     It is possible that this will return true but the entity will not be tagged
    ///     if the tag value is false. However, false tags are unsupported behavior, so
    ///     this is also unsupported behavior.
    /// </remarks>
    public bool HasPendingTagForEntity(ulong entityId)
    {
        return HasPendingComponentForEntity(entityId);
    }

    /// <summary>
    ///     Sets a tag on the given entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="isLoad">Whether to treat as a load instead of an add.</param>
    public void TagEntity(ulong entityId, bool isLoad = false)
    {
        if (!HasTagForEntity(entityId)) AddComponent(entityId, true, isLoad);
    }

    /// <summary>
    ///     Removes a tag from the given entity. If the entity is not tagged, this method silently fails.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="isUnload">Whether to treat as an unload isntead of a remove.</param>
    public void UntagEntity(ulong entityId, bool isUnload = false)
    {
        if (HasTagForEntity(entityId)) RemoveComponent(entityId, isUnload);
    }
}