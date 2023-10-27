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

using Castle.Facilities.Startable;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Sovereign.EngineUtil.IoC;

namespace Sovereign.ClientCore.Rendering;

/// <summary>
/// </summary>
public class RenderingInstaller : IWindsorInstaller
{
    public void Install(IWindsorContainer container, IConfigurationStore store)
    {
        /* RenderingManager. */
        container.Register(Component.For<RenderingManager>()
            .LifestyleSingleton()
            .Start());

        /* IRenderer. */
        container.Register(EngineClasses.EngineAssemblies()
            .BasedOn<IRenderer>()
            .WithServiceDefaultInterfaces()
            .LifestyleSingleton());

        /* RenderingResourceManager. */
        container.Register(Component.For<RenderingResourceManager>()
            .LifestyleSingleton());
    }
}