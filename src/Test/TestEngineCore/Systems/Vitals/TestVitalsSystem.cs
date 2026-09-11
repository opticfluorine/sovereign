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

using Microsoft.Extensions.Logging;
using Moq;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Systems.Movement;
using Xunit;
using EventId = Sovereign.EngineCore.Events.EventId;

namespace Sovereign.EngineCore.Systems.Vitals;
/// <summary>
///     Unit tests for VitalsSystem and its supporting classes.
/// </summary>
public class TestVitalsSystem
{
    private const ulong NpcEntityId = 0x7fff000000000001;
    private const ulong PlayerEntityId = 0x7fff000000000002;

    private readonly EntityTable entityTable = new();
    private readonly ComponentManager componentManager;
    private readonly EntityManager entityManager;
    private readonly HealthComponentCollection healths;
    private readonly StaminaComponentCollection staminas;
    private readonly ManaComponentCollection manas;
    private readonly EntityTypeComponentCollection entityTypes;
    private readonly EventCommunicator eventCommunicator = new();
    private readonly Mock<IEventSender> mockEventSender = new();
    private readonly List<Event> sentEvents = new();
    private readonly VitalsEventHandler eventHandler;
    private readonly VitalsSystem system;

    public TestVitalsSystem()
    {
        var entityNotifier = new EntityNotifier();
        componentManager = new ComponentManager(entityNotifier);
        entityManager = new EntityManager(componentManager, entityNotifier, entityTable);
        healths = new HealthComponentCollection(entityTable, componentManager);
        staminas = new StaminaComponentCollection(entityTable, componentManager);
        manas = new ManaComponentCollection(entityTable, componentManager);
        entityTypes = new EntityTypeComponentCollection(entityTable, componentManager);

        mockEventSender.Setup(s => s.SendEvent(It.IsAny<Event>()))
            .Callback<Event>(sentEvents.Add);

        var vitalsController = new VitalsController();
        var movementController = new MovementController();
        var deathHandler = new EntityDeathHandler(entityTypes, healths, entityManager, vitalsController,
            movementController, mockEventSender.Object, Mock.Of<ILogger<EntityDeathHandler>>());
        eventHandler = new VitalsEventHandler(deathHandler, healths, staminas, manas,
            Mock.Of<ILogger<VitalsEventHandler>>());
        system = new VitalsSystem(eventCommunicator, new Mock<IEventLoop>().Object, eventHandler,
            healths, staminas, manas);
    }

    [Fact]
    public void ProcessVitals_RegeneratesOnInterval()
    {
        healths.AddComponent(NpcEntityId, new Vital { Value = 5, MaxValue = 10, ChangeRate = 1, ChangeInterval = 2 });
        healths.ApplyComponentUpdates();
        Assert.Equal(6, healths[NpcEntityId].Value);

        Tick();
        healths.ApplyComponentUpdates();
        Assert.Equal(6, healths[NpcEntityId].Value);

        Tick();
        healths.ApplyComponentUpdates();
        Assert.Equal(7, healths[NpcEntityId].Value);
    }

    [Fact]
    public void ProcessVitals_ClampsAtMaxValue()
    {
        healths.AddComponent(NpcEntityId, new Vital { Value = 10, MaxValue = 10, ChangeRate = 1, ChangeInterval = 1 });
        healths.ApplyComponentUpdates();
        Assert.Equal(10, healths[NpcEntityId].Value);
    }

    [Fact]
    public void ProcessVitals_ClampsAtZeroValue()
    {
        manas.AddComponent(NpcEntityId, new Vital { Value = 1, MaxValue = 10, ChangeRate = -1, ChangeInterval = 1 });
        manas.ApplyComponentUpdates();
        Assert.Equal(0, manas[NpcEntityId].Value);
    }

    [Fact]
    public void ProcessVitals_SkipsChangeWhenIntervalIsZero()
    {
        staminas.AddComponent(NpcEntityId, new Vital { Value = 4, MaxValue = 10, ChangeRate = 5, ChangeInterval = 0 });
        staminas.ApplyComponentUpdates();
        Assert.Equal(4, staminas[NpcEntityId].Value);
    }

    [Fact]
    public void ProcessVitals_AddsZeroHealthEntityToDeathList()
    {
        healths.AddComponent(NpcEntityId, new Vital { Value = 1, MaxValue = 10, ChangeRate = -1, ChangeInterval = 1 });
        healths.ApplyComponentUpdates();

        Assert.Contains(NpcEntityId, eventHandler.ZeroHealthEntities);
    }

    [Fact]
    public void CoreTick_HandlesZeroHealthEntityDeath()
    {
        entityTypes.AddComponent(NpcEntityId, EntityType.Npc);
        healths.AddComponent(NpcEntityId, new Vital { Value = 1, MaxValue = 10, ChangeRate = -1, ChangeInterval = 1 });
        healths.ApplyComponentUpdates();

        Tick();
        healths.ApplyComponentUpdates();

        Assert.False(healths.HasComponentForEntity(NpcEntityId));
    }

    [Fact]
    public void KillEvent_RemovesNonPlayerEntity()
    {
        entityTypes.AddComponent(NpcEntityId, EntityType.Npc);
        healths.AddComponent(NpcEntityId, new Vital { Value = 5, MaxValue = 10 });
        healths.ApplyComponentUpdates();

        SendKillEvent(NpcEntityId);
        healths.ApplyComponentUpdates();

        Assert.False(healths.HasComponentForEntity(NpcEntityId));
    }

    [Fact]
    public void KillEvent_RespawnsPlayerAtSpawnPoint()
    {
        entityTypes.AddComponent(PlayerEntityId, EntityType.Player);
        healths.AddComponent(PlayerEntityId, new Vital { Value = 0, MaxValue = 10 });

        SendKillEvent(PlayerEntityId);

        var sentEventIds = CaptureSentEventIds();
        Assert.Contains(EventId.Core_Vitals_ChangeVitals, sentEventIds);
        Assert.Contains(EventId.Core_Movement_Teleport, sentEventIds);
    }

    [Fact]
    public void KillEvent_PlayerDeathResetsHealth()
    {
        entityTypes.AddComponent(PlayerEntityId, EntityType.Player);
        healths.AddComponent(PlayerEntityId, new Vital { Value = 0, MaxValue = 10, ChangeRate = 1, ChangeInterval = 1 });
        healths.ApplyComponentUpdates();

        SendKillEvent(PlayerEntityId);
        PumpSentEvents();
        system.ExecuteOnce();
        healths.ApplyComponentUpdates();

        Assert.Equal(10, healths[PlayerEntityId].Value);
    }

    [Fact]
    public void ChangeVitalsEvent_AppliesToSelectedCollection()
    {
        healths.AddComponent(NpcEntityId, new Vital { Value = 5, MaxValue = 10 });
        staminas.AddComponent(NpcEntityId, new Vital { Value = 3, MaxValue = 10 });
        healths.ApplyComponentUpdates();
        staminas.ApplyComponentUpdates();

        eventCommunicator.SendEventToSystem(new Event(EventId.Core_Vitals_ChangeVitals,
            new ChangeVitalsEventDetails
            {
                EntityId = NpcEntityId,
                Vital = VitalType.Health,
                ChangeAmount = 4
            }));
        system.ExecuteOnce();
        healths.ApplyComponentUpdates();
        staminas.ApplyComponentUpdates();

        Assert.Equal(9, healths[NpcEntityId].Value);
        Assert.Equal(3, staminas[NpcEntityId].Value);
    }

    /// <summary>
    ///     Sends a Core_Tick event to the system and processes it.
    /// </summary>
    private void Tick()
    {
        eventCommunicator.SendEventToSystem(new Event(EventId.Core_Tick));
        system.ExecuteOnce();
    }

    /// <summary>
    ///     Sends a Core_Vitals_Kill event to the system and processes it.
    /// </summary>
    /// <param name="entityId">Entity ID to kill.</param>
    private void SendKillEvent(ulong entityId)
    {
        eventCommunicator.SendEventToSystem(new Event(EventId.Core_Vitals_Kill,
            new EntityEventDetails { EntityId = entityId }));
        system.ExecuteOnce();
    }

    /// <summary>
    ///     Feeds all events sent through the mock event sender back into the system's
    ///     event communicator, simulating the event loop round trip.
    /// </summary>
    private void PumpSentEvents()
    {
        foreach (var ev in sentEvents) eventCommunicator.SendEventToSystem(ev);
        sentEvents.Clear();
    }

    /// <summary>
    ///     Gets the event IDs of all events sent through the mock event sender.
    /// </summary>
    /// <returns>Set of sent event IDs.</returns>
    private HashSet<EventId> CaptureSentEventIds()
    {
        return sentEvents.Select(ev => ev.EventId).ToHashSet();
    }
}
