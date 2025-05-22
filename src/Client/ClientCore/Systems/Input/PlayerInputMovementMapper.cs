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
using Microsoft.Extensions.Options;
using Sovereign.EngineCore.Configuration;
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

    private readonly MovementOptions movementOptions;

    /// <summary>
    ///     Current target velocity in XY plane for the player based on user input.
    /// </summary>
    private Vector2 currentRelativeVelocity = Vector2.Zero;

    /// <summary>
    ///     Current player entity ID.
    /// </summary>
    private ulong playerEntityId;

    /// <summary>
    ///     Flag indicating whether a player is currently selected.
    /// </summary>
    private bool playerSelected;

    /// <summary>
    ///     Ticks remaining until repeat.
    /// </summary>
    private int ticksUntilRepeat = -1;

    public PlayerInputMovementMapper(IEventSender eventSender,
        MovementController movementController, IOptions<MovementOptions> movementOptions)
    {
        this.eventSender = eventSender;
        this.movementController = movementController;
        this.movementOptions = movementOptions.Value;
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
            currentRelativeVelocity = Vector2.Zero;
            movementController.RequestMovement(eventSender, playerEntityId, currentRelativeVelocity);
        }
        else
        {
            // Move.
            currentRelativeVelocity = new Vector2(dx, dy);
            currentRelativeVelocity /= currentRelativeVelocity.Length();
            RequestNextMovement();
        }
    }

    /// <summary>
    ///     Initiates a jump for the player.
    /// </summary>
    public void Jump()
    {
        if (playerSelected) movementController.Jump(eventSender, playerEntityId);
    }

    /// <summary>
    ///     Called at the start of a new tick.
    /// </summary>
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
        if (currentRelativeVelocity != Vector2.Zero) ticksUntilRepeat = movementOptions.RequestIntervalTicks;
    }
}