/*
 * Sovereign Engine
 * Copyright (c) 2022 opticfluorine
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

namespace Sovereign.VeldridRenderer.Rendering.Scenes.Game;

public class GameSceneInstaller : IWindsorInstaller
{
    public void Install(IWindsorContainer container, IConfigurationStore store)
    {
        container.Register(Component.For<GameSceneRenderer>().LifestyleSingleton());
        container.Register(Component.For<GameSceneConsumer>().LifestyleSingleton());
        container.Register(Component.For<GameResourceManager>().LifestyleSingleton());
        container.Register(Component.For<WorldRenderer>().LifestyleSingleton());
        container.Register(Component.For<WorldPipeline>().LifestyleSingleton());
        container.Register(Component.For<WorldVertexConstantsUpdater>().LifestyleSingleton());
        container.Register(Component.For<WorldFragmentConstantsUpdater>().LifestyleSingleton());
        container.Register(Component.For<LightingShaderConstantsUpdater>().LifestyleSingleton());
        container.Register(Component.For<PointLightDepthMapRenderer>().LifestyleSingleton());
        container.Register(Component.For<FullPointLightMapRenderer>().LifestyleSingleton());
    }
}