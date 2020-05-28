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

namespace Sovereign.NetworkCore.Network.Rest.Data
{

    /// <summary>
    /// Login response data record.
    /// </summary>
    public class LoginResponse
    {

        /// <summary>
        /// Human-readable string describing the result of the login attempt.
        /// </summary>
        public string Result;

        /// <summary>
        /// User ID. Used for connection handoff.
        /// </summary>
        public string UserId;

        /// <summary>
        /// Shared secret between the client and server.
        /// </summary>
        public string SharedSecret;

        /// <summary>
        /// Hostname of the server to connect to.
        /// </summary>
        public string ServerHost;

        /// <summary>
        /// Port of the server to connect to.
        /// </summary>
        public ushort ServerPort;

    }

}