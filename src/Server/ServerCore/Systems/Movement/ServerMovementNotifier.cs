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

using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Configuration;
using Sovereign.EngineCore.Systems.Movement;

namespace Sovereign.ServerCore.Systems.Movement;

/// <summary>
///     Server-side implementation of IMovementNotifier.
/// </summary>
public class ServerMovementNotifier : IMovementNotifier
{
    private readonly MovementInternalController controller;
    private readonly KinematicsComponentCollection kinematics;
    private readonly Dictionary<ulong, ulong> scheduledCountByEntityId = new();
    private readonly List<ulong> toRemove = new();
    private ulong sendCount;
    private readonly MovementOptions movementOptions;

    public ServerMovementNotifier(KinematicsComponentCollection kinematics, MovementInternalController controller, 
        IOptions<MovementOptions> movementOptions)
    {
        this.kinematics = kinematics;
        this.controller = controller;
        this.movementOptions = movementOptions.Value;
    }

    public void ScheduleEntity(ulong entityId, bool immediate)
    {
        if (scheduledCountByEntityId.TryGetValue(entityId, out var count) && count >= sendCount) return;
        scheduledCountByEntityId[entityId] = sendCount + (immediate ? 1 : (ulong)movementOptions.UpdateIntervalTicks);
    }

    public void SendScheduled()
    {
        toRemove.Clear();
        foreach (var (entityId, schedule) in scheduledCountByEntityId)
        {
            if (schedule > sendCount) continue;
            if (schedule < sendCount - 1)
            {
                // Scheduled notification has expired.
                toRemove.Add(entityId);
                continue;
            }

            if (!kinematics.TryGetValue(entityId, out var posVel))
            {
                // Entity must have been removed/unloaded since it was scheduled - skip silently.
                continue;
            }

            controller.Move(entityId, posVel.Position, posVel.Velocity);
        }

        foreach (var entityId in toRemove)
        {
            scheduledCountByEntityId.Remove(entityId);
        }

        sendCount++;
    }
}