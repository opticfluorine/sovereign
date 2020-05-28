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

using Castle.Facilities.Logging;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Services.Logging.Log4netIntegration;
using Castle.Windsor;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sovereign.StandaloneWorldGen.Logging
{

    /// <summary>
    /// IoC installer for logging in the StandaloneWorldGen utility.
    /// </summary>
    public class StandaloneWorldGenLoggingInstaller : IWindsorInstaller
    {

        /// <summary>
        /// Name of the logging configuration file.
        /// </summary>
        private const string LogConfigFilepath = "Logging.xml";

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            /* Configure log4net. */
            container.AddFacility<LoggingFacility>(f => f.LogUsing<Log4netFactory>()
                .WithConfig(LogConfigFilepath));
        }

    }

}
