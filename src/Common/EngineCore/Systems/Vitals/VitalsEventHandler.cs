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
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Logging;
using EventId = Sovereign.EngineCore.Events.EventId;

namespace Sovereign.EngineCore.Systems.Vitals;

/// <summary>
///     Handles vitals events for the vitals system.
/// </summary>
public class VitalsEventHandler
{
    private readonly EntityDeathHandler deathHandler;
    private readonly HealthComponentCollection healths;
    private readonly ManaComponentCollection manas;
    private readonly StaminaComponentCollection staminas;
    private readonly ILogger<VitalsEventHandler> logger;

    /// <summary>
    ///     Number of Core_Tick events processed so far.
    /// </summary>
    internal ulong TickCount;

    /// <summary>
    ///     Entity IDs with zero health detected during the most recent health component pass.
    /// </summary>
    internal readonly List<ulong> ZeroHealthEntities = new();

    public VitalsEventHandler(EntityDeathHandler deathHandler, HealthComponentCollection healths,
        StaminaComponentCollection staminas, ManaComponentCollection manas,
        ILogger<VitalsEventHandler> logger)
    {
        this.deathHandler = deathHandler;
        this.healths = healths;
        this.staminas = staminas;
        this.manas = manas;
        this.logger = logger;
    }

    /// <summary>
    ///     Handles a vitals event.
    /// </summary>
    /// <param name="ev">Vitals event.</param>
    public void HandleEvent(Event ev)
    {
        switch (ev.EventId)
        {
            case EventId.Core_Vitals_Kill:
            {
                if (ev.EventDetails is not EntityEventDetails details)
                {
                    logger.LogError("Received Kill event with bad details.");
                    break;
                }

                deathHandler.HandleDeath(details.EntityId);
                break;
            }

            case EventId.Core_Vitals_ChangeVitals:
            {
                if (ev.EventDetails is not ChangeVitalsEventDetails details)
                {
                    logger.LogError("Received ChangeVitals event with bad details.");
                    break;
                }

                HandleChangeVitals(details);
                break;
            }

            case EventId.Core_Tick:
            {
                ++TickCount;
                DrainZeroHealthEntities();
                break;
            }
        }
    }

    /// <summary>
    ///     Applies a ChangeVitals event to the appropriate component collection.
    /// </summary>
    /// <param name="details">Change details.</param>
    private void HandleChangeVitals(ChangeVitalsEventDetails details)
    {
        if (!Enum.IsDefined(typeof(VitalType), details.Vital))
        {
            logger.LogError("Received ChangeVitals event with undefined vital type.");
            return;
        }

        BaseComponentCollection<Vital>? collection = details.Vital switch
        {
            VitalType.Health => healths,
            VitalType.Stamina => staminas,
            VitalType.Mana => manas,
            _ => null
        };

        if (collection == null)
        {
            logger.LogError("Received ChangeVitals event with undefined vital type.");
            return;
        }

        collection.ModifyComponent(details.EntityId, ComponentOperation.AddValue,
            new Vital { Value = details.ChangeAmount });
    }

    /// <summary>
    ///     Handles all entities with zero health detected during the last component update transition.
    /// </summary>
    private void DrainZeroHealthEntities()
    {
        foreach (var entityId in ZeroHealthEntities) deathHandler.HandleDeath(entityId);

        ZeroHealthEntities.Clear();
    }
}
