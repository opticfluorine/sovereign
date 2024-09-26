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

using System;
using Sovereign.ClientCore.Rendering.Scenes;

namespace Sovereign.VeldridRenderer.Rendering.Scenes.Game;

/// <summary>
///     Responsible for updating the world fragment shader constants.
/// </summary>
public class WorldFragmentConstantsUpdater
{
    private readonly GameResourceManager gameResMgr;

    public WorldFragmentConstantsUpdater(GameResourceManager gameResMgr)
    {
        this.gameResMgr = gameResMgr;
    }

    /// <summary>
    ///     Updates the world fragment shader constants.
    /// </summary>
    /// <param name="scene">Active scene.</param>
    public void Update(IScene scene)
    {
        if (gameResMgr.FragmentUniformBuffer == null)
            throw new InvalidOperationException("Fragment uniform buffer not ready.");

        scene.PopulateWorldFragmentConstants(out var ambientLightColor, out var globalLightColor);

        var buf = gameResMgr.FragmentUniformBuffer.Buffer;
        buf[0].AmbientLightColor = ambientLightColor;
        buf[0].GlobalLightColor = globalLightColor;
    }
}