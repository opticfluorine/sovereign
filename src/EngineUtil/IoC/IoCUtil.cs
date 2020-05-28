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

using Castle.Facilities.Startable;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Castle.Windsor.Installer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sovereign.EngineUtil.IoC
{

    /// <summary>
    /// IoC-related utility methods.
    /// </summary>
    public static class IoCUtil
    {

        /// <summary>
        /// Creates an IoC container with default settings.
        /// </summary>
        /// <returns>IoC container.</returns>
        public static IWindsorContainer InitializeIoC()
        {
            /* Create the container. */
            var iocContainer = new WindsorContainer();

            /* Add additional resolvers. */
            iocContainer.Kernel.Resolver.AddSubResolver(
                new CollectionResolver(iocContainer.Kernel, true));

            /* Add facilities. */
            iocContainer.AddFacility<StartableFacility>(f => f.DeferredStart());
            iocContainer.AddFacility<TypedFactoryFacility>();

            /* Install components. */
            iocContainer.Install(FromAssembly.InDirectory(
                EngineClasses.EngineAssemblyFilter()));

            return iocContainer;
        }

    }

}
