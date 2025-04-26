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
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using Sovereign.ClientCore.Configuration;
using Sovereign.EngineCore.Events;
using Sovereign.NetworkCore.Network.Rest.Data;

namespace Sovereign.ClientCore.Events.Details;

/// <summary>
///     Event details for a register account event.
/// </summary>
public sealed class RegisterAccountEventDetails : IEventDetails
{
    public RegisterAccountEventDetails(RegistrationRequest registrationRequest,
        ConnectionOptions connectionOptions)
    {
        RegistrationRequest = registrationRequest;
        ConnectionOptions = connectionOptions;
    }

    /// <summary>
    ///     Registration request.
    /// </summary>
    public RegistrationRequest RegistrationRequest { get; private set; }

    /// <summary>
    ///     Connection parameters.
    /// </summary>
    public ConnectionOptions ConnectionOptions { get; private set; }
}