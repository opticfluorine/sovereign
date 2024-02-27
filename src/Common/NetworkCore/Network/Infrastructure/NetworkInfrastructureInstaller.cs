/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
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
using Sovereign.EngineUtil.IoC;

namespace Sovereign.NetworkCore.Network.Infrastructure;

public sealed class NetworkInfrastructureInstaller : IWindsorInstaller
{
    public void Install(IWindsorContainer container, IConfigurationStore store)
    {
        container.Register(EngineClasses.EngineAssemblies()
            .BasedOn<INetworkManager>()
            .WithServiceDefaultInterfaces()
            .WithServiceAllInterfaces()
            .WithServiceSelf()
            .LifestyleSingleton());

        container.Register(Component.For<NetLogger>()
            .LifestyleSingleton());

        container.Register(Component.For<NetworkSerializer>()
            .LifestyleSingleton());

        container.Register(Component.For<NetworkConnectionManager>()
            .LifestyleSingleton());
    }
}