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
using LiteNetLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sovereign.NetworkCore.Network.Infrastructure
{

    /// <summary>
    /// Class for logging debug info from LiteNetLib.
    /// </summary>
    public sealed class NetLogger : INetLogger
    {

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        public NetLogger()
        {
            NetDebug.Logger = this;
        }

        public void WriteNet(NetLogLevel level, string str, params object[] args)
        {
            switch (level)
            {
                case NetLogLevel.Error:
                    Logger.ErrorFormat(str, args);
                    break;

                case NetLogLevel.Warning:
                    Logger.WarnFormat(str, args);
                    break;

                case NetLogLevel.Info:
                    Logger.InfoFormat(str, args);
                    break;

                case NetLogLevel.Trace:
                    Logger.DebugFormat(str, args);
                    break;
            }
        }

    }

}
