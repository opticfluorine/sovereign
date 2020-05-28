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
using Castle.Facilities.Logging;
using Castle.Services.Logging.Log4netIntegration;
using Sovereign.EngineUtil.IoC;

namespace Sovereign.EngineCore.Logging
{

    /// <summary>
    /// IoC installer for logging facilities.
    /// </summary>
    public class LoggingInstaller : IWindsorInstaller
    {

        /// <summary>
        /// Path to the log4net configuration file.
        /// </summary>
        private const string CONFIG_FILE = "Data/Logging.xml";

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            /* Configure log4net. */
            container.AddFacility<LoggingFacility>(f => f.LogUsing<Log4netFactory>()
                .WithConfig(CONFIG_FILE));

            /* IErrorHandler. */
            container.Register(EngineClasses.EngineAssemblies()
                .BasedOn<IErrorHandler>()
                .WithServiceDefaultInterfaces()
                .Unless(handlerType => handlerType.Equals(typeof(NullErrorHandler)))
                .LifestyleSingleton());
        }

    }

}
