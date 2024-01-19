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
using System.Numerics;

namespace Sovereign.EngineCore.Components.Indexers;

/// <summary>
///     Component indexer that tracks moving components (components with non-zero velocity).
/// </summary>
public class MovingComponentIndexer : BaseComponentIndexer<Vector3>
{
    /// <summary>
    ///     Set of moving entities.
    /// </summary>
    private readonly HashSet<ulong> movingEntities = new();

    public MovingComponentIndexer(VelocityComponentCollection velocities) : base(velocities, velocities)
    {
    }

    /// <summary>
    ///     Set of currently moving entities.
    /// </summary>
    public IReadOnlySet<ulong> MovingEntities => movingEntities;

    protected override void ComponentAddedCallback(ulong entityId, Vector3 componentValue, bool isLoad)
    {
        if (!componentValue.Equals(Vector3.Zero)) movingEntities.Add(entityId);
    }

    protected override void ComponentModifiedCallback(ulong entityId, Vector3 componentValue)
    {
        if (!componentValue.Equals(Vector3.Zero)) movingEntities.Add(entityId);
    }

    protected override void ComponentRemovedCallback(ulong entityId, bool isUnload)
    {
        movingEntities.Remove(entityId);
    }
}