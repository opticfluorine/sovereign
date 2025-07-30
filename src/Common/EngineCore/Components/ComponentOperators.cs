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

using System;
using System.Collections.Generic;
using System.Numerics;
using Sovereign.EngineCore.Components.Types;

namespace Sovereign.EngineCore.Components;

/// <summary>
///     Standard operators for common component types.
/// </summary>
public static class ComponentOperators
{
    /// <summary>
    ///     Standard operators for int-valued components.
    /// </summary>
    public static readonly Dictionary<ComponentOperation, Func<int, int, int>>
        IntOperators = new()
        {
            { ComponentOperation.Set, (_, b) => b },
            { ComponentOperation.Add, (a, b) => a + b },
            { ComponentOperation.Multiply, (a, b) => a * b },
            { ComponentOperation.Divide, (a, b) => a / b }
        };

    /// <summary>
    ///     Standard operators for ulong-valued components.
    /// </summary>
    public static readonly Dictionary<ComponentOperation, Func<ulong, ulong, ulong>>
        UlongOperators = new()
        {
            { ComponentOperation.Set, (_, b) => b },
            { ComponentOperation.Add, (a, b) => a + b },
            { ComponentOperation.Multiply, (a, b) => a * b },
            { ComponentOperation.Divide, (a, b) => a / b }
        };

    /// <summary>
    ///     Standard operators for float-valued components.
    /// </summary>
    public static readonly Dictionary<ComponentOperation, Func<float, float, float>>
        FloatOperators = new()
        {
            { ComponentOperation.Set, (_, b) => b },
            { ComponentOperation.Add, (a, b) => a + b },
            { ComponentOperation.Multiply, (a, b) => a * b },
            { ComponentOperation.Divide, (a, b) => a / b }
        };

    /// <summary>
    ///     Standard operators for Vector2-valued components.
    /// </summary>
    public static readonly Dictionary<ComponentOperation, Func<Vector2, Vector2, Vector2>>
        Vector2Operators = new()
        {
            { ComponentOperation.Set, (_, b) => b },
            { ComponentOperation.Add, Vector2.Add }
        };

    /// <summary>
    ///     Standard operators for Vector3-valued components.
    /// </summary>
    public static readonly Dictionary<ComponentOperation, Func<Vector3, Vector3, Vector3>>
        Vector3Operators = new()
        {
            { ComponentOperation.Set, (_, b) => b },
            { ComponentOperation.Add, Vector3.Add }
        };

    /// <summary>
    ///     Standard operators for GridPosition-valued components.
    /// </summary>
    public static readonly Dictionary<ComponentOperation, Func<GridPosition, GridPosition, GridPosition>>
        GridPositionOperators = new()
        {
            { ComponentOperation.Set, (_, b) => b },
            { ComponentOperation.Add, GridPosition.Add }
        };

    /// <summary>
    ///     Standard operators for boolean-valued components.
    /// </summary>
    public static readonly Dictionary<ComponentOperation, Func<bool, bool, bool>>
        BoolOperators = new()
        {
            { ComponentOperation.Set, (_, b) => b }
        };

    /// <summary>
    ///     Standard operators for string-valued components.
    /// </summary>
    public static readonly Dictionary<ComponentOperation, Func<string, string, string>>
        StringOperators = new()
        {
            { ComponentOperation.Set, (_, b) => b }
        };

    /// <summary>
    ///     Standard operators for GUID-valued components.
    /// </summary>
    public static readonly Dictionary<ComponentOperation, Func<Guid, Guid, Guid>>
        GuidOperators = new()
        {
            { ComponentOperation.Set, (_, b) => b }
        };

    /// <summary>
    ///     Standard operators for orientation-valued components.
    /// </summary>
    public static readonly Dictionary<ComponentOperation, Func<Orientation, Orientation, Orientation>>
        OrientationOperators = new()
        {
            { ComponentOperation.Set, (_, b) => b }
        };

    /// <summary>
    ///     Standard operators for kinematics-valued components.
    /// </summary>
    public static readonly Dictionary<ComponentOperation, Func<Kinematics, Kinematics, Kinematics>>
        KinematicsOperators = new()
        {
            { ComponentOperation.Set, (_, b) => b },
            { ComponentOperation.SetVelocity, (a, b) => a with { Velocity = b.Velocity } },
            { ComponentOperation.AddPosition, (a, b) => a with { Position = a.Position + b.Position } }
        };

    /// <summary>
    ///     Standard operators for PointLight-valued components.
    /// </summary>
    public static readonly Dictionary<ComponentOperation, Func<PointLight, PointLight, PointLight>>
        PointLightOperators = new()
        {
            { ComponentOperation.Set, (_, b) => b }
        };

    /// <summary>
    ///     Standard operators for BoundingBox-valued components.
    /// </summary>
    public static readonly Dictionary<ComponentOperation, Func<BoundingBox, BoundingBox, BoundingBox>>
        BoundingBoxOperators = new()
        {
            { ComponentOperation.Set, (_, b) => b }
        };

    /// <summary>
    ///     Standard operators for Shadow-valued components.
    /// </summary>
    public static readonly Dictionary<ComponentOperation, Func<Shadow, Shadow, Shadow>>
        ShadowOperators = new()
        {
            { ComponentOperation.Set, (_, b) => b }
        };

    /// <summary>
    ///     Standard operators for EntityType-valued components.
    /// </summary>
    public static readonly Dictionary<ComponentOperation, Func<EntityType, EntityType, EntityType>>
        EntityTypeOperators = new()
        {
            { ComponentOperation.Set, (_, b) => b }
        };
}