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

using LiteNetLib;
using Microsoft.Extensions.Logging;

namespace Sovereign.NetworkCore.Network.Infrastructure;

/// <summary>
///     Class for logging debug info from LiteNetLib.
/// </summary>
public sealed class NetLogger : INetLogger
{
    private readonly ILogger<NetLogger> logger;

    public NetLogger(ILogger<NetLogger> logger)
    {
        this.logger = logger;
        NetDebug.Logger = this;
    }

    public void WriteNet(NetLogLevel level, string str, params object[] args)
    {
        switch (level)
        {
            case NetLogLevel.Error:
                logger.LogError(str, args);
                break;

            case NetLogLevel.Warning:
                logger.LogWarning(str, args);
                break;

            case NetLogLevel.Info:
                logger.LogInformation(str, args);
                break;

            case NetLogLevel.Trace:
                logger.LogDebug(str, args);
                break;
        }
    }
}