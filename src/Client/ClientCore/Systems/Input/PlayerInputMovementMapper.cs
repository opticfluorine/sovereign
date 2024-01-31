/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Numerics;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Systems.Movement;

namespace Sovereign.ClientCore.Systems.Input;

/// <summary>
///     Maps player input to movement events.
/// </summary>
public class PlayerInputMovementMapper
{
    /// <summary>
    ///     Event communicator.
    /// </summary>
    private readonly IEventSender eventSender;

    private readonly MovementController movementController;

    /// <summary>
    ///     Current target velocity for the player based on user input.
    /// </summary>
    private Vector3 currentRelativeVelocity = Vector3.Zero;

    /// <summary>
    ///     Current player entity ID.
    /// </summary>
    private ulong playerEntityId;

    /// <summary>
    ///     Flag indicating whether a player is currently selected.
    /// </summary>
    private bool playerSelected;

    /// <summary>
    ///     Current sequence value for repeat movement tracking.
    /// </summary>
    private uint sequenceCount;

    /// <summary>
    ///     Ticks remaining until repeat.
    /// </summary>
    private int ticksUntilRepeat = -1;

    public PlayerInputMovementMapper(IEventSender eventSender,
        MovementController movementController)
    {
        this.eventSender = eventSender;
        this.movementController = movementController;
    }

    /// <summary>
    ///     Updates player movement based on player input.
    /// </summary>
    /// <param name="up">Input state for upward movement.</param>
    /// <param name="down">Input state for downward movement.</param>
    /// <param name="left">Input state for leftward movement.</param>
    /// <param name="right">Input state for rightward movement.</param>
    public void UpdateMovement(bool up, bool down, bool left, bool right)
    {
        // Only handle a movement update if a player is currently logged in.
        if (!playerSelected) return;

        // Compute direction of movement in the xy plane.
        // z movement will be handled by a rotation applied in the Movement system.
        var dx = (right ? 1 : 0) - (left ? 1 : 0);
        var dy = (up ? 1 : 0) - (down ? 1 : 0);
        if (dx == 0 && dy == 0)
        {
            // Stop movement.
            currentRelativeVelocity = Vector3.Zero;
            movementController.RequestMovement(eventSender, playerEntityId, currentRelativeVelocity);
            sequenceCount++;
        }
        else
        {
            // Move.
            currentRelativeVelocity = new Vector3(dx, dy, 0.0f);
            currentRelativeVelocity /= currentRelativeVelocity.Length();
            RequestNextMovement();
        }
    }

    public void OnTick()
    {
        if (ticksUntilRepeat > 0)
        {
            ticksUntilRepeat--;
            if (ticksUntilRepeat == 0) RequestNextMovement();
        }
    }

    /// <summary>
    ///     Called when a player character entity is selected.
    /// </summary>
    /// <param name="details">Event details.</param>
    public void SelectPlayer(EntityEventDetails details)
    {
        playerEntityId = details.EntityId;
        playerSelected = true;
    }

    /// <summary>
    ///     Requests a player movement and schedules a repeat event for the future.
    /// </summary>
    private void RequestNextMovement()
    {
        // Request player movement.
        movementController.RequestMovement(eventSender, playerEntityId, currentRelativeVelocity);

        // Schedule a repeat movement.
        if (currentRelativeVelocity != Vector3.Zero)
        {
            sequenceCount++;
            ticksUntilRepeat = MovementConfiguration.DefaultMovementLengthTicks - 1;
        }
    }
}