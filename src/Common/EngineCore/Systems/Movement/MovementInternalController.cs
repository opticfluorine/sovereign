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

using System.Numerics;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.World;

namespace Sovereign.EngineCore.Systems.Movement;

/// <summary>
///     Internal controller for the Movement system.
/// </summary>
public class MovementInternalController
{
    private readonly IEventSender eventSender;
    private readonly KinematicsComponentCollection kinematics;
    private readonly WorldSegmentResolver resolver;

    public MovementInternalController(IEventSender eventSender, WorldSegmentResolver resolver,
        KinematicsComponentCollection kinematics)
    {
        this.eventSender = eventSender;
        this.resolver = resolver;
        this.kinematics = kinematics;
    }

    /// <summary>
    ///     Moves an entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="position">Position.</param>
    /// <param name="velocity">Velocity.</param>
    public void Move(ulong entityId, Vector3 position, Vector3 velocity)
    {
        var details = new MoveEventDetails
        {
            EntityId = entityId,
            Position = position,
            Velocity = velocity
        };
        var ev = new Event(EventId.Core_Movement_Move, details);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Notifies clients that an entity is teleporting.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="newPosition">New position of entity after teleport.</param>
    public void NotifyTeleport(ulong entityId, Vector3 newPosition)
    {
        var details = new TeleportNoticeEventDetails
        {
            EntityId = entityId,
            FromWorldSegment = resolver.GetWorldSegmentForPosition(kinematics[entityId].Position),
            ToWorldSegment = resolver.GetWorldSegmentForPosition(newPosition)
        };
        var ev = new Event(EventId.Core_Movement_TeleportNotice, details);
        eventSender.SendEvent(ev);
    }
}