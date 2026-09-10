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
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems.Movement;

namespace Sovereign.EngineCore.Systems.Vitals;

/// <summary>
///     Handles the death of entities for the vitals system.
/// </summary>
public class EntityDeathHandler(
    EntityTypeComponentCollection entityTypes,
    HealthComponentCollection healths,
    EntityManager entityManager,
    VitalsController vitalsController,
    MovementController movementController,
    IEventSender eventSender)
{
    /// <summary>
    ///     Handles the death of the given entity.
    /// </summary>
    /// <param name="entityId">Entity ID of the dead entity.</param>
    public void HandleDeath(ulong entityId)
    {
        var isPlayer = entityTypes.TryGetValue(entityId, out var entityType) && entityType == EntityType.Player;
        if (isPlayer)
            HandlePlayerDeath(entityId);
        else
            entityManager.RemoveEntity(entityId);
    }

    /// <summary>
    ///     Handles the death of a player entity by respawning the player at the spawn point.
    /// </summary>
    /// <param name="entityId">Entity ID of the dead player.</param>
    private void HandlePlayerDeath(ulong entityId)
    {
        // TODO Select per-player spawn point.
        var spawnPoint = new Vector3(0, 0, 1);

        if (healths.TryGetValue(entityId, out var health))
            vitalsController.ChangeVitals(eventSender, entityId, VitalType.Health,
                health.MaxValue - health.Value);

        movementController.Teleport(eventSender, entityId, spawnPoint);
    }
}
