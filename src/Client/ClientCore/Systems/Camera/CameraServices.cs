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
using Sovereign.ClientCore.Rendering.Configuration;
using Sovereign.ClientCore.Rendering.Display;
using Sovereign.ClientCore.Systems.Input;

namespace Sovereign.ClientCore.Systems.Camera;

public class CameraServices
{
    private readonly CameraManager cameraManager;
    private readonly InputServices inputServices;
    private readonly MainDisplay mainDisplay;
    private readonly DisplayViewport viewport;

    public CameraServices(InputServices inputServices, DisplayViewport viewport, CameraManager cameraManager,
        MainDisplay mainDisplay)
    {
        this.inputServices = inputServices;
        this.viewport = viewport;
        this.cameraManager = cameraManager;
        this.mainDisplay = mainDisplay;
    }

    /// <summary>
    ///     Gets the latest mouse position transformed to world coordinates.
    /// </summary>
    /// <returns>World coordinates of mouse position.</returns>
    public Vector3 GetMousePositionWorldCoordinates()
    {
        var mousePosScreen = inputServices.GetMousePosition();

        var scaledTileWidth = viewport.WidthInTiles / mainDisplay.DisplayMode!.Width;
        var scaledTileHeight = viewport.HeightInTiles / mainDisplay.DisplayMode!.Height;
        var cameraPos = cameraManager.Position;

        var mousePosWorldX = mousePosScreen.X * scaledTileWidth + cameraPos.X - viewport.WidthInTiles * 0.5f;
        var mousePosWorldY = (mainDisplay.DisplayMode!.Height - mousePosScreen.Y) * scaledTileHeight
            + cameraPos.Y - viewport.HeightInTiles * 0.5f;

        return new Vector3(mousePosWorldX, mousePosWorldY, cameraPos.Z);
    }
}