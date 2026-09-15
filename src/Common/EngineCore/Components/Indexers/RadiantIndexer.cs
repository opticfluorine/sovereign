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
using System.Numerics;
using System.Threading;
using Microsoft.Extensions.Options;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Configuration;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.World;

namespace Sovereign.EngineCore.Components.Indexers;

/// <summary>
///     Indexes RadiantData components by category and world segment, providing
///     summed scalar field evaluation over the segments near a queried position.
/// </summary>
/// <remarks>
///     Radiant entities are assumed to be static: the contributing position is
///     resolved once, when the RadiantData component is added or modified, and
///     movement is never re-indexed.
/// </remarks>
public class RadiantIndexer : BaseComponentIndexer<RadiantData>
{
    /// <summary>
    ///     An indexed radiant field contribution.
    /// </summary>
    private readonly record struct Entry(ulong EntityId, Vector3 Position, RadiantData Data);

    private readonly Dictionary<(RadiantCategory Category, GridPosition Segment), List<Entry>> buckets = new();

    private readonly Dictionary<ulong, (RadiantCategory Category, GridPosition Segment)> bucketOf = new();

    private readonly Lock accessLock = new();

    private readonly KinematicsComponentCollection kinematics;
    private readonly ParentComponentCollection parents;
    private readonly EntityTable entityTable;
    private readonly WorldSegmentResolver resolver;
    private readonly float searchRange;

    public RadiantIndexer(KinematicsComponentCollection kinematics, ParentComponentCollection parents,
        RadiantDataComponentCollection radiantDatas, EntityTable entityTable,
        WorldSegmentResolver resolver, IOptions<RadiantOptions> radiantOptions)
        : base(radiantDatas, radiantDatas)
    {
        this.kinematics = kinematics;
        this.parents = parents;
        this.entityTable = entityTable;
        this.resolver = resolver;
        searchRange = radiantOptions.Value.SearchRange;

        entityTable.OnTemplateSet += OnTemplateSet;
        entityTable.OnEntityRemoved += OnEntityRemoved;
    }

    public override void Dispose()
    {
        entityTable.OnEntityRemoved -= OnEntityRemoved;
        entityTable.OnTemplateSet -= OnTemplateSet;
        base.Dispose();
    }

    /// <summary>
    ///     Evaluates the radiant scalar field of the given category at the queried position.
    /// </summary>
    /// <param name="category">Radiant field category.</param>
    /// <param name="position">Queried position.</param>
    /// <returns>Sum of the field contributions of all indexed entries in the scanned segments.</returns>
    public float GetValue(RadiantCategory category, Vector3 position)
    {
        lock (accessLock)
        {
            var minSegment = resolver.GetWorldSegmentForPosition(position - new Vector3(searchRange));
            var maxSegment = resolver.GetWorldSegmentForPosition(position + new Vector3(searchRange));

            var sum = 0.0f;
            for (var x = minSegment.X; x <= maxSegment.X; ++x)
            for (var y = minSegment.Y; y <= maxSegment.Y; ++y)
            for (var z = minSegment.Z; z <= maxSegment.Z; ++z)
            {
                if (!buckets.TryGetValue((category, new GridPosition(x, y, z)), out var entries)) continue;
                foreach (var entry in entries)
                    sum += EvaluateEntry(entry, position);
            }

            return sum;
        }
    }

    protected override void ComponentAddedCallback(ulong entityId, RadiantData componentValue, bool isLoad)
    {
        if (EntityUtil.IsTemplateEntity(entityId))
        {
            // Template entities are never indexed; apply the change to the current instances instead.
            var instanceIds = entityTable.GetInstancesOfTemplate(entityId);
            lock (accessLock)
            {
                foreach (var instanceId in instanceIds) ReindexEntity(instanceId);
            }

            return;
        }

        lock (accessLock)
        {
            ReindexEntity(entityId);
        }
    }

    protected override void ComponentModifiedCallback(ulong entityId, RadiantData componentValue)
    {
        if (EntityUtil.IsTemplateEntity(entityId))
        {
            var instanceIds = entityTable.GetInstancesOfTemplate(entityId);
            lock (accessLock)
            {
                foreach (var instanceId in instanceIds) ReindexEntity(instanceId);
            }

            return;
        }

        lock (accessLock)
        {
            ReindexEntity(entityId);
        }
    }

    protected override void ComponentRemovedCallback(ulong entityId, bool isUnload)
    {
        if (EntityUtil.IsTemplateEntity(entityId))
        {
            // Instances that rely on the template's component lose it; instances with their own
            // RadiantData component keep their own index entries.
            var instanceIds = entityTable.GetInstancesOfTemplate(entityId);
            lock (accessLock)
            {
                foreach (var instanceId in instanceIds)
                {
                    if (components.HasLocalComponentForEntity(instanceId)) continue;
                    RemoveEntity(instanceId);
                }
            }

            return;
        }

        lock (accessLock)
        {
            RemoveEntity(entityId);
        }
    }

    /// <summary>
    ///     Called when a template is set to an entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="templateEntityId">Template entity ID.</param>
    /// <param name="oldTemplateId">Old template, or zero if there is no old template.</param>
    /// <param name="isLoad">Load flag.</param>
    /// <param name="isNew">New flag.</param>
    private void OnTemplateSet(ulong entityId, ulong templateEntityId, ulong oldTemplateId, bool isLoad, bool isNew)
    {
        lock (accessLock)
        {
            RemoveEntity(entityId);
            if (components.HasComponentForEntity(entityId)) ReindexEntity(entityId);
        }
    }

    /// <summary>
    ///     Called when an entity is removed or unloaded.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="isUnload">Unload flag.</param>
    private void OnEntityRemoved(ulong entityId, bool isUnload)
    {
        lock (accessLock)
        {
            RemoveEntity(entityId);
        }
    }

    /// <summary>
    ///     Evaluates the radiant field contribution of an entry at the queried position.
    /// </summary>
    /// <param name="entry">Indexed radiant field contribution.</param>
    /// <param name="position">Queried position.</param>
    /// <returns>Field contribution of the entry at the queried position.</returns>
    /// <exception cref="ArgumentException">Thrown if the entry has an unrecognized radiant function.</exception>
    private float EvaluateEntry(in Entry entry, Vector3 position)
    {
        return entry.Data.Function switch
        {
            RadiantFunction.Linear => entry.Data.Param0 * Vector3.Distance(entry.Position, position)
                                      + entry.Data.Param1,
            _ => throw new ArgumentException("Unrecognized radiant function.")
        };
    }

    /// <summary>
    ///     Adds or refreshes the index entry for a radiant entity, snapshotting its position once.
    /// </summary>
    /// <param name="entityId">Entity ID with a RadiantData component.</param>
    private void ReindexEntity(ulong entityId)
    {
        RemoveEntity(entityId);

        if (!kinematics.TryFindNearest(entityId, parents, out var posVel, out _)) return;

        var data = components[entityId];
        var segmentIndex = resolver.GetWorldSegmentForPosition(posVel.Position);
        var bucketKey = (data.Category, segmentIndex);
        if (!buckets.TryGetValue(bucketKey, out var entries))
        {
            entries = new List<Entry>();
            buckets[bucketKey] = entries;
        }

        entries.Add(new Entry(entityId, posVel.Position, data));
        bucketOf[entityId] = bucketKey;
    }

    /// <summary>
    ///     Removes the index entry for a radiant entity, if any.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    private void RemoveEntity(ulong entityId)
    {
        if (!bucketOf.Remove(entityId, out var bucketKey)) return;
        if (!buckets.TryGetValue(bucketKey, out var entries)) return;
        entries.RemoveAll(entry => entry.EntityId == entityId);
    }
}
