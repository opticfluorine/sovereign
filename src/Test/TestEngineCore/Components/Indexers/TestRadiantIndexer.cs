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

using System.Numerics;
using Microsoft.Extensions.Options;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Configuration;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.World;
using Xunit;

namespace Sovereign.EngineCore.Components.Indexers;

/// <summary>
///     Unit tests for RadiantIndexer, covering linear field evaluation, category
///     filtering, parent position resolution, static-position semantics, and index
///     maintenance on component modification and removal.
/// </summary>
public class TestRadiantIndexer
{
    private const float Epsilon = 1e-3f;

    private readonly EntityTable entityTable;
    private readonly KinematicsComponentCollection kinematics;
    private readonly ParentComponentCollection parents;
    private readonly RadiantDataComponentCollection radiantDatas;
    private readonly RadiantIndexer indexer;

    public TestRadiantIndexer()
    {
        entityTable = new EntityTable();
        var notifier = new EntityNotifier();
        var componentManager = new ComponentManager(notifier);
        kinematics = new KinematicsComponentCollection(entityTable, componentManager);
        parents = new ParentComponentCollection(entityTable, componentManager);
        radiantDatas = new RadiantDataComponentCollection(entityTable, componentManager);
        indexer = new RadiantIndexer(kinematics, parents, radiantDatas, entityTable,
            new WorldSegmentResolver(), Options.Create(new RadiantOptions()));
    }

    /// <summary>
    ///     Creates a linear radiant field value.
    /// </summary>
    /// <param name="category">Field category.</param>
    /// <param name="slope">Linear slope (Param0).</param>
    /// <param name="intercept">Linear intercept (Param1).</param>
    /// <returns>Radiant data value.</returns>
    private static RadiantData LinearField(RadiantCategory category, float slope, float intercept)
    {
        return new RadiantData
        {
            Category = category,
            Function = RadiantFunction.Linear,
            Param0 = slope,
            Param1 = intercept
        };
    }

    /// <summary>
    ///     Gives an entity its own position.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="position">Position.</param>
    private void AddPositioned(ulong entityId, Vector3 position)
    {
        kinematics.AddOrUpdateComponent(entityId, new Kinematics { Position = position, Velocity = Vector3.Zero });
        kinematics.ApplyComponentUpdates();
    }

    /// <summary>
    ///     Asserts that two float values are nearly equal.
    /// </summary>
    /// <param name="expected">Expected value.</param>
    /// <param name="actual">Actual value.</param>
    private void AssertNear(float expected, float actual)
    {
        Assert.True(MathF.Abs(expected - actual) < Epsilon,
            $"expected {expected} but got {actual}");
    }

    [Fact]
    public void GetValue_EvaluatesLinearFunctionOfDistance()
    {
        const ulong entityId = 1;
        AddPositioned(entityId, Vector3.Zero);
        radiantDatas.AddOrUpdateComponent(entityId, LinearField(RadiantCategory.MineralDrop, -2.0f, 11.0f));
        radiantDatas.ApplyComponentUpdates();

        // Distance from (3, 4, 0) to the origin is 5, so the field value is -2 * 5 + 11 = 1.
        AssertNear(1.0f, indexer.GetValue(RadiantCategory.MineralDrop, new Vector3(3.0f, 4.0f, 0.0f)));
    }

    [Fact]
    public void GetValue_SumsSameCategoryAndFiltersOthers()
    {
        const ulong entityA = 1;
        const ulong entityB = 2;
        const ulong entityOther = 3;
        AddPositioned(entityA, Vector3.Zero);
        AddPositioned(entityB, new Vector3(6.0f, 0.0f, 0.0f));
        AddPositioned(entityOther, new Vector3(0.0f, 3.0f, 0.0f));

        radiantDatas.AddOrUpdateComponent(entityA, LinearField(RadiantCategory.MineralDrop, -1.0f, 10.0f));
        radiantDatas.AddOrUpdateComponent(entityB, LinearField(RadiantCategory.MineralDrop, -1.0f, 10.0f));
        radiantDatas.AddOrUpdateComponent(entityOther, LinearField((RadiantCategory)7, 1.0f, 0.0f));
        radiantDatas.ApplyComponentUpdates();

        // Query at (0, 3, 0): distances 3 and sqrt(45) from A and B; the other category does not contribute.
        var expected = 10.0f - 3.0f + 10.0f - MathF.Sqrt(45.0f);
        AssertNear(expected, indexer.GetValue(RadiantCategory.MineralDrop, new Vector3(0.0f, 3.0f, 0.0f)));
    }

    [Fact]
    public void GetValue_UsesParentPositionForUnpositionedChild()
    {
        const ulong childId = 100;
        const ulong parentId = 101;
        AddPositioned(parentId, new Vector3(10.0f, 0.0f, 0.0f));
        parents.AddOrUpdateComponent(childId, parentId);
        parents.ApplyComponentUpdates();

        radiantDatas.AddOrUpdateComponent(childId, LinearField(RadiantCategory.MineralDrop, -1.0f, 4.0f));
        radiantDatas.ApplyComponentUpdates();

        // The child has no own position; the field is evaluated against the parent's snapshot.
        // Distance from (6, 0, 0) to the parent's position is 4, so the value is -1 * 4 + 4 = 0.
        AssertNear(0.0f, indexer.GetValue(RadiantCategory.MineralDrop, new Vector3(6.0f, 0.0f, 0.0f)));
    }

    [Fact]
    public void RemoveComponent_RemovesContribution()
    {
        const ulong entityId = 1;
        AddPositioned(entityId, Vector3.Zero);
        radiantDatas.AddOrUpdateComponent(entityId, LinearField(RadiantCategory.MineralDrop, -1.0f, 50.0f));
        radiantDatas.ApplyComponentUpdates();

        AssertNear(50.0f, indexer.GetValue(RadiantCategory.MineralDrop, Vector3.Zero));

        radiantDatas.RemoveComponent(entityId);
        radiantDatas.ApplyComponentUpdates();

        AssertNear(0.0f, indexer.GetValue(RadiantCategory.MineralDrop, Vector3.Zero));
    }

    [Fact]
    public void ModifyComponent_ReindexesEntity()
    {
        const ulong entityId = 1;
        AddPositioned(entityId, Vector3.Zero);
        radiantDatas.AddOrUpdateComponent(entityId, LinearField(RadiantCategory.MineralDrop, -1.0f, 50.0f));
        radiantDatas.ApplyComponentUpdates();

        // Modify the category and parameters; the old bucket must no longer contribute.
        radiantDatas.AddOrUpdateComponent(entityId, LinearField((RadiantCategory)3, -1.0f, 20.0f));
        radiantDatas.ApplyComponentUpdates();

        AssertNear(0.0f, indexer.GetValue(RadiantCategory.MineralDrop, Vector3.Zero));
        AssertNear(20.0f, indexer.GetValue((RadiantCategory)3, Vector3.Zero));
    }

    [Fact]
    public void Movement_DoesNotReindexStaticSnapshot()
    {
        const ulong entityId = 1;
        AddPositioned(entityId, new Vector3(10.0f, 0.0f, 0.0f));
        radiantDatas.AddOrUpdateComponent(entityId, LinearField(RadiantCategory.MineralDrop, -1.0f, 100.0f));
        radiantDatas.ApplyComponentUpdates();

        // Move the entity; the index keeps the position snapshot from RadiantData add time.
        kinematics.AddOrUpdateComponent(entityId,
            new Kinematics { Position = new Vector3(100.0f, 0.0f, 0.0f), Velocity = Vector3.Zero });
        kinematics.ApplyComponentUpdates();

        // Distance from the origin to the snapshotted position (10, 0, 0) is 10.
        AssertNear(90.0f, indexer.GetValue(RadiantCategory.MineralDrop, Vector3.Zero));
    }

    [Fact]
    public void UnpositionedWithoutPositionedAncestor_IsNotIndexed()
    {
        const ulong entityId = 1;
        radiantDatas.AddOrUpdateComponent(entityId, LinearField(RadiantCategory.MineralDrop, -1.0f, 50.0f));
        radiantDatas.ApplyComponentUpdates();

        AssertNear(0.0f, indexer.GetValue(RadiantCategory.MineralDrop, Vector3.Zero));
    }

    [Fact]
    public void ReapplyingComponentAfterPosition_IsIndexed()
    {
        const ulong entityId = 1;

        // The entity is unpositioned when RadiantData is first added, so it is not indexed.
        radiantDatas.AddOrUpdateComponent(entityId, LinearField(RadiantCategory.MineralDrop, -1.0f, 50.0f));
        radiantDatas.ApplyComponentUpdates();
        AssertNear(0.0f, indexer.GetValue(RadiantCategory.MineralDrop, Vector3.Zero));

        // Re-applying the component once positioned picks up the position snapshot.
        AddPositioned(entityId, Vector3.Zero);
        radiantDatas.AddOrUpdateComponent(entityId, LinearField(RadiantCategory.MineralDrop, -1.0f, 50.0f));
        radiantDatas.ApplyComponentUpdates();

        AssertNear(50.0f, indexer.GetValue(RadiantCategory.MineralDrop, Vector3.Zero));
    }
}
