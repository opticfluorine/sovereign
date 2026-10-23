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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Configuration;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Systems.Scripting;
using Sovereign.EngineCore.Systems.WorldManagement;
using Xunit;

namespace Sovereign.EngineCore.Systems.Inventory;

/// <summary>
///     Unit tests for the <see cref="InventoryManager.UseItem" /> target-type gating.
/// </summary>
public class TestInventoryManager
{
    private const ulong PlayerId = 1;
    private const ulong SlotId = 2;
    private const ulong NpcTargetId = 4;
    private const ulong PlayerTargetId = 5;
    private const ulong ItemTargetId = 6;
    private const ulong SlotTargetId = 7;

    private readonly EntityTable entityTable = new();
    private readonly EntityNotifier entityNotifier = new();
    private readonly ComponentManager componentManager;
    private readonly EntityManager entityManager;
    private readonly ParentComponentCollection parents;
    private readonly EntityTypeComponentCollection entityTypes;
    private readonly KinematicsComponentCollection kinematics;
    private readonly UseRangeComponentCollection useRanges;
    private readonly ItemUseComponentCollection itemUses;
    private readonly RecordingLogger<InventoryManager> logger = new();
    private readonly FakeScriptingController scriptingController = new();
    private readonly InventoryManager manager;
    private ulong nextToolId = 100;

    public TestInventoryManager()
    {
        componentManager = new ComponentManager(entityNotifier);
        entityManager = new EntityManager(componentManager, entityNotifier, entityTable);

        parents = new ParentComponentCollection(entityTable, componentManager);
        entityTypes = new EntityTypeComponentCollection(entityTable, componentManager);
        kinematics = new KinematicsComponentCollection(entityTable, componentManager);
        useRanges = new UseRangeComponentCollection(entityTable, componentManager);
        itemUses = new ItemUseComponentCollection(entityTable, componentManager);
        var equipmentTypes = new EquipmentTypeComponentCollection(entityTable, componentManager);
        var boundingBoxes = new BoundingBoxComponentCollection(entityTable, componentManager);
        var quantities = new QuantityComponentCollection(entityTable, componentManager);
        var stackable = new StackableTagCollection(entityTable, componentManager);
        var names = new NameComponentCollection(entityTable, componentManager);
        var npcFlags = new NpcFlagsComponentCollection(entityTable, componentManager);
        var loggingUtil = new LoggingUtil(names);

        var engineConfiguration = new StubEngineConfiguration { IsAuthoritative = true };
        var hierarchyIndexer = new EntityHierarchyIndexer(parents, entityTypes, engineConfiguration, entityManager);
        var slotIndexer = new SlotIndexer(entityTypes, new SlotComponentEventFilter(entityTypes), parents,
            NullLogger<SlotIndexer>.Instance, entityTable, hierarchyIndexer);
        var equipmentIndexer = new PlayerEquipmentIndexer(parents, entityTypes, equipmentTypes, entityTable,
            NullLogger<PlayerEquipmentIndexer>.Instance);
        var equipmentSlotIndexer = new EquipmentSlotIndexer(equipmentTypes, entityTypes, parents, entityTable,
            NullLogger<EquipmentSlotIndexer>.Instance);

        manager = new InventoryManager(slotIndexer, hierarchyIndexer, logger, loggingUtil, parents, kinematics,
            boundingBoxes, entityTypes, Options.Create(new InventoryOptions()), Mock.Of<IEntityFactory>(),
            entityManager, new WorldManagementController(), stackable, entityTable, quantities, useRanges, itemUses,
            equipmentTypes, equipmentIndexer, equipmentSlotIndexer, engineConfiguration, Mock.Of<IEventSender>(),
            new InventoryPermissionService(npcFlags), scriptingController);

        // Player with a single empty hotbar slot at index 0.
        RegisterEntity(PlayerId, EntityType.Player);
        AddKinematics(PlayerId, Vector3.Zero);
        RegisterEntity(SlotId, EntityType.Slot, PlayerId);

        // In-range entity targets of each mappable type.
        RegisterEntity(NpcTargetId, EntityType.Npc);
        AddKinematics(NpcTargetId, new Vector3(1.0f, 0.0f, 0.0f));
        RegisterEntity(PlayerTargetId, EntityType.Player);
        AddKinematics(PlayerTargetId, new Vector3(2.0f, 0.0f, 0.0f));
        RegisterEntity(ItemTargetId, EntityType.Item);
        AddKinematics(ItemTargetId, new Vector3(3.0f, 0.0f, 0.0f));
        RegisterEntity(SlotTargetId, EntityType.Slot, PlayerTargetId);
        AddKinematics(SlotTargetId, new Vector3(4.0f, 0.0f, 0.0f));
    }

    [Fact]
    public void UseItem_NoItemUseComponent_IsWildcard()
    {
        var toolId = CreateTool(null, 5.0f);

        manager.UseItem(PlayerId, toolId, NpcTargetId);

        var call = Assert.Single(scriptingController.InteractCalls);
        Assert.Equal(PlayerId, call.ActorId);
        Assert.Equal(toolId, call.ToolId);
        Assert.Equal(NpcTargetId, call.TargetId);
    }

    [Fact]
    public void UseItem_MatchingFlag_InvokesCallback()
    {
        var toolId = CreateTool(ItemUse.Npc, 5.0f);

        manager.UseItem(PlayerId, toolId, NpcTargetId);

        Assert.Single(scriptingController.InteractCalls);
    }

    [Fact]
    public void UseItem_MismatchedFlag_DoesNotInvokeCallback()
    {
        var toolId = CreateTool(ItemUse.Player, 5.0f);

        manager.UseItem(PlayerId, toolId, NpcTargetId);

        Assert.Empty(scriptingController.InteractCalls);
        Assert.Contains(logger.Messages, m => m.Contains("[SECURITY]") && m.Contains("ineligible target"));
    }

    [Fact]
    public void UseItem_BlockEntityTarget_RejectedWithSecurityLog()
    {
        RegisterEntityOnly(EntityConstants.FirstBlockEntityId);
        AddKinematics(EntityConstants.FirstBlockEntityId, new Vector3(1.0f, 0.0f, 0.0f));
        var toolId = CreateTool(ItemUse.Item, 5.0f);

        manager.UseItem(PlayerId, toolId, EntityConstants.FirstBlockEntityId);

        Assert.Empty(scriptingController.InteractCalls);
        Assert.Contains(logger.Messages, m => m.Contains("[SECURITY]") && m.Contains("ineligible target"));
    }

    [Fact]
    public void UseItem_MultiFlag_MatchesAnySetFlag()
    {
        var toolId = CreateTool(ItemUse.Npc | ItemUse.Item, 5.0f);

        manager.UseItem(PlayerId, toolId, NpcTargetId);
        manager.UseItem(PlayerId, toolId, ItemTargetId);
        manager.UseItem(PlayerId, toolId, PlayerTargetId);

        Assert.Equal(2, scriptingController.InteractCalls.Count);
        Assert.Equal(NpcTargetId, scriptingController.InteractCalls[0].TargetId);
        Assert.Equal(ItemTargetId, scriptingController.InteractCalls[1].TargetId);
    }

    [Fact]
    public void UseItem_SlotTypedTarget_Rejected()
    {
        var toolId = CreateTool(ItemUse.Npc | ItemUse.Player | ItemUse.Item, 5.0f);

        manager.UseItem(PlayerId, toolId, SlotTargetId);

        Assert.Empty(scriptingController.InteractCalls);
        Assert.Contains(logger.Messages, m => m.Contains("[SECURITY]") && m.Contains("ineligible target"));
    }

    [Fact]
    public void UseItem_EmptyFlags_RejectedOnEveryTargetType()
    {
        var toolId = CreateTool(ItemUse.None, 5.0f);

        manager.UseItem(PlayerId, toolId, NpcTargetId);
        manager.UseItem(PlayerId, toolId, PlayerTargetId);
        manager.UseItem(PlayerId, toolId, ItemTargetId);

        Assert.Empty(scriptingController.InteractCalls);
    }

    /// <summary>
    ///     Creates an item in the player's hotbar with the given ItemUse flags and UseRange.
    /// </summary>
    /// <param name="use">ItemUse flags, or null to omit the component.</param>
    /// <param name="useRange">Use range.</param>
    /// <returns>Tool item entity ID.</returns>
    private ulong CreateTool(ItemUse? use, float useRange)
    {
        var toolId = nextToolId++;
        RegisterEntity(toolId, EntityType.Item, SlotId);

        useRanges.AddComponent(toolId, useRange);
        useRanges.ApplyComponentUpdates();

        if (use.HasValue)
        {
            itemUses.AddComponent(toolId, use.Value);
            itemUses.ApplyComponentUpdates();
        }

        return toolId;
    }

    /// <summary>
    ///     Registers an entity and, if given, its type and parent.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="entityType">Entity type to apply.</param>
    /// <param name="parentId">Parent entity ID, or 0 for none.</param>
    private void RegisterEntity(ulong entityId, EntityType entityType, ulong parentId = 0)
    {
        RegisterEntityOnly(entityId);
        if (parentId != 0)
        {
            parents.AddComponent(entityId, parentId);
            parents.ApplyComponentUpdates();
        }

        entityTypes.AddComponent(entityId, entityType);
        entityTypes.ApplyComponentUpdates();
    }

    /// <summary>
    ///     Registers an entity in the entity table without any components.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    private void RegisterEntityOnly(ulong entityId)
    {
        entityTable.Add(entityId, 0, false, false, false);
        entityTable.UpdateAllEntities();
    }

    /// <summary>
    ///     Adds a kinematics component to an entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="position">Position.</param>
    private void AddKinematics(ulong entityId, Vector3 position)
    {
        kinematics.AddComponent(entityId, new Kinematics { Position = position, Velocity = Vector3.Zero });
        kinematics.ApplyComponentUpdates();
    }

    /// <summary>
    ///     Engine configuration stub that defaults to authoritative.
    /// </summary>
    private sealed class StubEngineConfiguration : IEngineConfiguration
    {
        public int ExecutorThreadSleepInterval => 0;

        public bool IsAuthoritative { get; init; }
    }

    /// <summary>
    ///     Scripting controller fake that records interact callback invocations.
    /// </summary>
    private sealed class FakeScriptingController : IScriptingController
    {
        /// <summary>
        ///     Recorded interact callback invocations.
        /// </summary>
        public List<(ulong ActorId, ulong ToolId, ulong TargetId)> InteractCalls { get; } = new();

        public void ReloadAllScripts(IEventSender eventSender)
        {
        }

        public void ReloadScript(IEventSender eventSender, string scriptName)
        {
        }

        public void LoadNewScripts(IEventSender eventSender)
        {
        }

        public void ReloadEntity(IEventSender eventSender, ulong entityId)
        {
        }

        public void ReloadTemplate(IEventSender eventSender, ulong templateId)
        {
        }

        public void RunTests(IEventSender eventSender)
        {
        }

        public void CallFunctionAsync(string scriptName, string functionName, params ulong[] args)
        {
        }

        public void InvokeInteractCallback(IEventSender eventSender, ulong actorEntityId, ulong toolEntityId,
            ulong targetEntityId)
        {
            InteractCalls.Add((actorEntityId, toolEntityId, targetEntityId));
        }
    }

    /// <summary>
    ///     Recording logger that captures formatted log messages.
    /// </summary>
    /// <typeparam name="T">Logger category type.</typeparam>
    private sealed class RecordingLogger<T> : ILogger<T>
    {
        /// <summary>
        ///     Captured log messages, in order.
        /// </summary>
        public List<string> Messages { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, Microsoft.Extensions.Logging.EventId eventId, TState state,
            Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
        }
    }
}
