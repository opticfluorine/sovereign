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
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;

namespace Sovereign.EngineCore.Systems.Movement;

/// <summary>
///     Public API for the movement system.
/// </summary>
public class MovementController
{
    /// <summary>
    ///     Sequence value for the next movement request.
    /// </summary>
    private byte nextRequestSequence;

    /// <summary>
    ///     Requests player character movement.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="entityId">Player entity ID.</param>
    /// <param name="relativeVelocity">Relative velocity in the XY plane, norm not to exceed 1.0.</param>
    public void RequestMovement(IEventSender eventSender, ulong entityId, Vector2 relativeVelocity)
    {
        var details = new RequestMoveEventDetails
        {
            EntityId = entityId,
            RelativeVelocity = relativeVelocity,
            Sequence = nextRequestSequence++
        };
        var ev = new Event(EventId.Core_Movement_RequestMove, details);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Initiates a jump.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="entityId">Jumping entity ID.</param>
    public void Jump(IEventSender eventSender, ulong entityId)
    {
        var details = new EntityEventDetails
        {
            EntityId = entityId
        };
        var ev = new Event(EventId.Core_Movement_Jump, details);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Teleports an entity to a new position.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="newPosition">New position.</param>
    /// <remarks>
    ///     This is generally only used for server-side teleportation of entities.
    ///     If used on the client side, the effect will be rolled back by the next
    ///     authoritative position update from the server.
    /// </remarks>
    public void Teleport(IEventSender eventSender, ulong entityId, Vector3 newPosition)
    {
        var details = new EntityVectorEventDetails
        {
            EntityId = entityId,
            Vector = newPosition
        };
        var ev = new Event(EventId.Core_Movement_Teleport, details);
        eventSender.SendEvent(ev);
    }
}