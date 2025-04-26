/*
 * Sovereign Engine
 * Copyright (c) 2023 opticfluorine
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

using Sovereign.ClientCore.Configuration;
using Sovereign.ClientCore.Network;
using Sovereign.EngineCore.Events;

namespace Sovereign.ClientCore.Events.Details;

/// <summary>
///     Event details for a connection attempt.
/// </summary>
public sealed class BeginConnectionEventDetails : IEventDetails
{
    public BeginConnectionEventDetails(ConnectionOptions connectionOptions, LoginParameters loginParameters)
    {
        ConnectionOptions = connectionOptions;
        LoginParameters = loginParameters;
    }

    /// <summary>
    ///     Connection parameters to be used for the connection.
    /// </summary>
    public ConnectionOptions ConnectionOptions { get; private set; }

    public LoginParameters LoginParameters { get; private set; }
}