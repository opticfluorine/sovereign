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

using Sovereign.ClientCore.Rendering.Scenes.Game;

namespace Sovereign.ClientCore.Rendering.Scenes;

/// <summary>
///     Manages the renderable scenes.
/// </summary>
public sealed class SceneManager
{
    public SceneManager(GameScene gameScene)
    {
        ActiveScene = gameScene;
    }

    /// <summary>
    ///     Current active scene.
    /// </summary>
    public IScene ActiveScene { get; set; }
}