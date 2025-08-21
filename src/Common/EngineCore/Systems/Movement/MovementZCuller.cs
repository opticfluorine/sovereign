// Sovereign Engine
// Copyright (c) 2025 opticfluorine
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
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Logging;

namespace Sovereign.EngineCore.Systems.Movement;

public interface IMovementZCuller
{
    /// <summary>
    ///     Culls the given entity for crossing the global minimum Z cutoff.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    void Cull(ulong entityId);
}

/// <summary>
///     No-op implementation of Z culler.
/// </summary>
public sealed class NullMovementZCuller : IMovementZCuller
{
    public void Cull(ulong entityId)
    {
    }
}

/// <summary>
///     Responsible for handling entities that fall below the global Z cutoff.
/// </summary>
public sealed class MovementZCuller(
    EntityTypeComponentCollection entityTypes,
    EntityManager entityManager,
    ILogger<MovementZCuller> logger,
    LoggingUtil loggingUtil,
    IEventSender eventSender,
    MovementController movementController) : IMovementZCuller
{
    public void Cull(ulong entityId)
    {
        var entityType = entityTypes.TryGetValue(entityId, out var value) ? value : EntityType.Other;
        switch (entityType)
        {
            case EntityType.Player:
                CullPlayer(entityId);
                break;

            case EntityType.Npc:
            case EntityType.Item:
            case EntityType.Other:
            default:
                CullNonPlayer(entityId);
                break;
        }
    }

    /// <summary>
    ///     Handles global Z culling for a player entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    public void CullPlayer(ulong entityId)
    {
        logger.LogInformation("Player {Player} fell below Z cutoff, respawning.", loggingUtil.FormatEntity(entityId));

        // TODO Configurable and player-specific spawn points.
        var spawnPoint = new Vector3(0, 0, 1);
        movementController.Teleport(eventSender, entityId, spawnPoint);
    }

    /// <summary>
    ///     Handles global Z culling for a non-player entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    public void CullNonPlayer(ulong entityId)
    {
        entityManager.RemoveEntity(entityId);
    }
}