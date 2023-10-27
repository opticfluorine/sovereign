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

namespace Sovereign.ClientCore.Systems.Camera;

/// <summary>
///     Responsible for handling camera-related events.
/// </summary>
public sealed class CameraEventHandler
{
    private readonly CameraManager manager;

    public CameraEventHandler(CameraManager manager)
    {
        this.manager = manager;
    }

    /// <summary>
    ///     Handles a camera-related event.
    /// </summary>
    /// <param name="ev">Event.</param>
    public void HandleEvent(Event ev)
    {
        switch (ev.EventId)
        {
            case EventId.Client_Camera_Attach:
                HandleAttachEvent((EntityEventDetails)ev.EventDetails);
                break;

            case EventId.Client_Camera_Detach:
                HandleDetachEvent();
                break;

            case EventId.Core_Tick:
                manager.UpdateCamera();
                break;
        }
    }

    /// <summary>
    ///     Attaches the camera to an entity.
    /// </summary>
    /// <param name="details">Event details.</param>
    private void HandleAttachEvent(EntityEventDetails details)
    {
        manager.SetCameraState(true, details.EntityId);
        manager.UpdateCamera();
    }

    /// <summary>
    ///     Detaches the camera from an entity.
    /// </summary>
    private void HandleDetachEvent()
    {
        manager.SetCameraState(false, 0);
        manager.UpdateCamera();
    }
}