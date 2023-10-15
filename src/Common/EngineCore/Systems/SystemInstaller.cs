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
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Sovereign.EngineUtil.IoC;

namespace Sovereign.EngineCore.Systems;

/// <summary>
///     IoC installer for all systems across all present assemblies.
/// </summary>
public class SystemInstaller : IWindsorInstaller
{
    public void Install(IWindsorContainer container, IConfigurationStore store)
    {
        /* ISystem. */
        container.Register(EngineClasses.EngineAssemblies()
            .BasedOn<ISystem>()
            .WithService.DefaultInterfaces()
            .LifestyleSingleton()
        );

        /* SystemManager. */
        container.Register(Component.For<SystemManager>()
            .LifestyleSingleton()
            .Start());

        /* SystemExecutor. */
        container.Register(Component.For<SystemExecutor>()
            .LifestyleTransient());
        container.Register(Component.For<ISystemExecutorFactory>()
            .LifestyleTransient()
            .AsFactory());
    }
}