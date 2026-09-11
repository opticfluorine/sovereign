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

using System.Collections.Generic;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;

namespace Sovereign.EngineCore.Systems.Vitals;

public class VitalsSystem : ISystem
{
    private readonly VitalsEventHandler eventHandler;
    private readonly HealthComponentCollection healths;
    private readonly ManaComponentCollection manas;
    private readonly StaminaComponentCollection staminas;

    public VitalsSystem(EventCommunicator eventCommunicator, IEventLoop eventLoop,
        VitalsEventHandler eventHandler, HealthComponentCollection healths,
        StaminaComponentCollection staminas, ManaComponentCollection manas)
    {
        this.eventHandler = eventHandler;
        this.healths = healths;
        this.staminas = staminas;
        this.manas = manas;
        EventCommunicator = eventCommunicator;

        healths.OnBeginDirectAccess += ProcessHealth;
        staminas.OnBeginDirectAccess += ProcessStamina;
        manas.OnBeginDirectAccess += ProcessMana;

        eventLoop.RegisterSystem(this);
    }

    public EventCommunicator EventCommunicator { get; }

    public ISet<EventId> EventIdsOfInterest { get; } = new HashSet<EventId>
    {
        EventId.Core_Vitals_Kill,
        EventId.Core_Vitals_ChangeVitals,
        EventId.Core_Tick
    };

    public int WorkloadEstimate => 200;

    public void Initialize()
    {
    }

    public void Cleanup()
    {
    }

    public int ExecuteOnce()
    {
        var eventsProcessed = 0;
        while (EventCommunicator.GetIncomingEvent(out var ev))
        {
            eventHandler.HandleEvent(ev);
            eventsProcessed++;
        }

        return eventsProcessed;
    }

    /// <summary>
    ///     Advances the health components during the component update transition.
    /// </summary>
    /// <param name="modifiedIndices">Modified component indices to append to.</param>
    /// <returns>Number of indices added.</returns>
    private int ProcessHealth(int[] modifiedIndices)
    {
        eventHandler.ZeroHealthEntities.Clear();
        return ProcessVitals(healths, true, modifiedIndices);
    }

    /// <summary>
    ///     Advances the stamina components during the component update transition.
    /// </summary>
    /// <param name="modifiedIndices">Modified component indices to append to.</param>
    /// <returns>Number of indices added.</returns>
    private int ProcessStamina(int[] modifiedIndices)
    {
        return ProcessVitals(staminas, false, modifiedIndices);
    }

    /// <summary>
    ///     Advances the mana components during the component update transition.
    /// </summary>
    /// <param name="modifiedIndices">Modified component indices to append to.</param>
    /// <returns>Number of indices added.</returns>
    private int ProcessMana(int[] modifiedIndices)
    {
        return ProcessVitals(manas, false, modifiedIndices);
    }

    /// <summary>
    ///     Advances the vitals of a single component collection, applying periodic changes,
    ///     clamping to the valid range, and detecting zero-health entities.
    /// </summary>
    /// <param name="components">Component collection.</param>
    /// <param name="isHealth">Whether the collection holds health components.</param>
    /// <param name="modifiedIndices">Modified component indices to append to.</param>
    /// <returns>Number of indices added.</returns>
    private int ProcessVitals(BaseComponentCollection<Vital> components, bool isHealth, int[] modifiedIndices)
    {
        var modCount = 0;
        var tickCount = eventHandler.TickCount;
        var componentList = components.Components;

        for (var i = 0; i < components.ComponentCount; ++i)
        {
            if (!components.TryGetEntityForIndex(i, out var entityId)) continue;
            if (entityId is >= EntityConstants.FirstTemplateEntityId
                and <= EntityConstants.LastTemplateEntityId)
                continue;

            var value = componentList[i].Value;
            if (componentList[i].ChangeInterval > 0 && tickCount % componentList[i].ChangeInterval == 0)
                value += componentList[i].ChangeRate;
            if (value > componentList[i].MaxValue) value = componentList[i].MaxValue;
            if (value < 0) value = 0;
            if (isHealth && value == 0) eventHandler.ZeroHealthEntities.Add(entityId);

            componentList[i].Value = value;
            modifiedIndices[modCount++] = i;
        }

        return modCount;
    }
}
