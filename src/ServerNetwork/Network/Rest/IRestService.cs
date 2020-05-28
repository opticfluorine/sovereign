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

using System;
using System.Collections.Generic;
using System.Text;
using WatsonWebserver;

namespace Sovereign.ServerNetwork.Network.Rest
{

    /// <summary>
    /// Exposes a REST service.
    /// </summary>
    public interface IRestService
    {

        /// <summary>
        /// Path at which the service is exposed.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// HTTP request type.
        /// </summary>
        HttpMethod RequestType { get; }

        /// <summary>
        /// Called when a request is received.
        /// </summary>
        /// <param name="req">HTTP request.</param>
        /// <returns>HTTP response.</returns>
        HttpResponse OnRequest(HttpRequest req);

    }

}
