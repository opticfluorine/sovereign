/*
 * Sovereign Engine
 * Copyright (c) 2020 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Sovereign.EngineUtil.IoC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sovereign.NetworkCore.Network.Pipeline
{

    /// <summary>
    /// IoC installer for the network pipelines.
    /// </summary>
    public sealed class NetworkPipelineInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(EngineClasses.EngineAssemblies()
                .BasedOn<IInboundPipelineStage>()
                .WithServiceDefaultInterfaces()
                .LifestyleSingleton()
                .AllowMultipleMatches());

            container.Register(EngineClasses.EngineAssemblies()
                .BasedOn<IOutboundPipelineStage>()
                .WithServiceDefaultInterfaces()
                .LifestyleSingleton()
                .AllowMultipleMatches());

            container.Register(Component.For<InboundNetworkPipeline>()
                .LifestyleSingleton());

            container.Register(Component.For<OutboundNetworkPipeline>()
                .LifestyleSingleton());
        }
    }

}
