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

namespace Sovereign.ClientCore.Rendering.Scenes;

/// <summary>
///     Interface for classes that consume a rendering scene.
/// </summary>
public interface ISceneConsumer
{
    /// <summary>
    ///     Consumes the scene.
    /// </summary>
    /// <param name="scene">Scene to consume.</param>
    void ConsumeScene(IScene scene);
}