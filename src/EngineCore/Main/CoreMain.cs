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

using Castle.Core.Logging;
using Castle.Windsor;
using ProtoBuf.Meta;
using Sovereign.EngineCore.Util;
using Sovereign.EngineUtil.IoC;
using System;
using System.Numerics;
using System.Text;
using System.Threading;

namespace Sovereign.EngineCore.Main
{

    /// <summary>
    /// Common main class for the engine.
    /// </summary>
    public class CoreMain
    {

        public void RunEngine()
        {
            /* Name the thread. */
            Thread.CurrentThread.Name = "Main";

            /* Configure Protobuf. */
            RuntimeTypeModel.Default[typeof(Vector3)]
                .SetSurrogate(typeof(Vector3Surrogate));

            /* Start up the IoC container */
            IWindsorContainer iocContainer = null;
            try
            {
                iocContainer = IoCUtil.InitializeIoC();
                var engineBase = iocContainer.Resolve<IEngineBase>();
                engineBase.Run();
            }
            catch (FatalErrorException)
            {
                /* Handled fatal error - halt the engine. */
            }
            catch (Exception e)
            {
                /* Unhandled exception - this usually indicates a fatal error. */
                LogEarlyError("Unhandled exception while running engine.", e, iocContainer);
            }

            /* Shut down the IoC container */
            ShutdownIoC(iocContainer);
        }

        private void ShutdownIoC(IWindsorContainer iocContainer)
        {
            if (iocContainer != null)
            {
                iocContainer.Dispose();
            }
        }

        /// <summary>
        /// Attempts to log an unhandled exception that escaped the IoC scope.
        /// </summary>
        /// <param name="message">Message to log.</param>
        /// <param name="e">Exception to log.</param>
        /// <param name="iocContainer">IoC container.</param>
        private void LogEarlyError(string message, Exception e, IWindsorContainer iocContainer)
        {
            /* Attempt to resolve a logger. */
            try
            {
                var logger = iocContainer.Resolve<ILogger>();
                logger.Fatal(message, e);
            }
            catch
            {
                /* Logger not available - fall back on stderr. */
                var sb = new StringBuilder();
                sb.Append("FATAL: ").Append(message).Append("\n")
                  .Append(e.GetType().Name).Append(": ").Append(e.Message);
                Console.Error.WriteLine(sb.ToString());
            }
        }

    }

}
