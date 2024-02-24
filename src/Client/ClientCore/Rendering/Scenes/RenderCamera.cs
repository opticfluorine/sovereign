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
using Sovereign.ClientCore.Systems.Camera;
using Sovereign.EngineUtil.Numerics;

namespace Sovereign.ClientCore.Rendering.Scenes;

/// <summary>
///     Responsible for configuring the camera for game scene rendering.
/// </summary>
public sealed class RenderCamera
{
    private readonly CameraManager cameraManager;

    public RenderCamera(CameraManager cameraManager)
    {
        this.cameraManager = cameraManager;
    }

    /// <summary>
    ///     Updates the vertex shader constant buffer to contain the
    ///     correct camera coordinates.
    /// </summary>
    /// <param name="timeSinceTick">Time elapsed from the start of the current tick.</param>
    public Vector3 Aim(float timeSinceTick)
    {
        var pos = cameraManager.Position;
        var vel = cameraManager.Velocity;

        return pos.InterpolateByTime(vel, timeSinceTick);
    }
}