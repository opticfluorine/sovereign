// Sovereign Engine
// Copyright (c) 2023 opticfluorine
// 
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

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