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
    protected BaseTagCollection(ComponentManager componentManager, int initialSize, ComponentType componentType)
        : base(componentManager, initialSize, ComponentOperators.BoolOperators, componentType)
    {
    }

    /// <summary>
    ///     Determines whether the given entity is tagged.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <returns>true if tagged, false otherwise.</returns>
    public bool HasTagForEntity(ulong entityId)
    {
        var val = GetComponentForEntity(entityId);
        return val.HasValue && val.Value;
    }

    /// <summary>
    ///     Sets a tag on the given entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    public void TagEntity(ulong entityId)
    {
        if (!HasTagForEntity(entityId)) AddComponent(entityId, true);
    }

    /// <summary>
    ///     Removes a tag from the given entity. If the entity is not tagged, this method silently fails.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    public void UntagEntity(ulong entityId)
    {
        if (HasTagForEntity(entityId)) RemoveComponent(entityId);
    }
}