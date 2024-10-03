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

using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.World;

/// <summary>
///     IoC installer for the high-level world rendering classes.
/// </summary>
public sealed class GeneralGameSceneWorldInstaller : IWindsorInstaller
{
    public void Install(IWindsorContainer container, IConfigurationStore store)
    {
        container.Register(Component.For<WorldVertexSequencer>()
            .LifestyleSingleton());

        container.Register(Component.For<WorldLayerGrouper>()
            .LifestyleSingleton());

        container.Register(Component.For<WorldLayerVertexSequencer>()
            .LifestyleSingleton());

        container.Register(Component.For<WorldEntityRetriever>()
            .LifestyleSingleton());

        container.Register(Component.For<WorldSpriteSequencer>()
            .LifestyleSingleton());

        container.Register(Component.For<LightSourceTable>().LifestyleSingleton());
    }
}