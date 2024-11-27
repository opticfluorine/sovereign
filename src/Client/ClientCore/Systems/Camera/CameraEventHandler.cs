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

using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineUtil.Monads;

namespace Sovereign.ClientCore.Systems.Camera;

/// <summary>
///     Responsible for handling camera-related events.
/// </summary>
public sealed class CameraEventHandler
{
    private readonly CameraManager manager;

    /// <summary>
    ///     Default entity for tracking.
    /// </summary>
    private Maybe<ulong> defaultEntity = new();

    public CameraEventHandler(CameraManager manager)
    {
        this.manager = manager;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    /// <summary>
    ///     Handles a camera-related event.
    /// </summary>
    /// <param name="ev">Event.</param>
    public void HandleEvent(Event ev)
    {
        switch (ev.EventId)
        {
            case EventId.Client_Camera_Attach:
                if (ev.EventDetails == null)
                {
                    logger.LogError("Received Attach without details.");
                    break;
                }

                HandleAttachEvent((EntityEventDetails)ev.EventDetails);
                break;

            case EventId.Client_Camera_Detach:
                HandleDetachEvent();
                break;

            case EventId.Client_Network_PlayerEntitySelected:
                if (ev.EventDetails == null)
                {
                    logger.LogError("Received PlayerEntitySelected without details.");
                    break;
                }

                HandlePlayerSelect((EntityEventDetails)ev.EventDetails);
                break;
        }
    }

    /// <summary>
    ///     Handles player selection by setting the player entity as the default camera target.
    /// </summary>
    /// <param name="details">Details.</param>
    private void HandlePlayerSelect(EntityEventDetails details)
    {
        defaultEntity = new Maybe<ulong>(details.EntityId);
        Attach(details.EntityId);
    }

    /// <summary>
    ///     Attaches the camera to an entity.
    /// </summary>
    /// <param name="details">Event details.</param>
    private void HandleAttachEvent(EntityEventDetails details)
    {
        Attach(details.EntityId);
    }

    /// <summary>
    ///     Detaches the camera from an entity. If a default entity is set, the camera is reattached to the
    ///     default entity.
    /// </summary>
    private void HandleDetachEvent()
    {
        if (defaultEntity.HasValue)
        {
            Attach(defaultEntity.Value);
        }
        else
        {
            manager.SetCameraState(false, 0);
            manager.UpdateCamera();
        }
    }

    /// <summary>
    ///     Attaches the camera to an entity.
    /// </summary>
    /// <param name="entityId">Entity to track.</param>
    private void Attach(ulong entityId)
    {
        manager.SetCameraState(true, entityId);
        manager.UpdateCamera();
    }
}