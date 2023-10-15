/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using Sovereign.EngineCore.Components;

namespace Sovereign.ClientCore.Rendering.Components;

/// <summary>
///     Describes a component that indicates whether an entity can be drawn.
/// </summary>
/// This component is of type bool, but the value has no meaning; the existence of
/// any Drawable component for an entity indicates that the entity is drawable.
public sealed class DrawableComponentCollection : BaseComponentCollection<bool>
{
    private const int BaseSize = 65536;

    public DrawableComponentCollection(ComponentManager componentManager)
        : base(componentManager, BaseSize, ComponentOperators.BoolOperators,
            ComponentType.Drawable)
    {
    }
}