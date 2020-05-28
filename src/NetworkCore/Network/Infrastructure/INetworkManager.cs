﻿/*
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

using Sovereign.EngineCore.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sovereign.NetworkCore.Network.Infrastructure
{

    /// <summary>
    /// Delegate type for received packets.
    /// </summary>
    /// <param name="ev">Received event.</param>
    /// <param name="connection">Associated connection.</param>
    public delegate void OnNetworkReceive(Event ev, NetworkConnection connection);

    /// <summary>
    /// Interface for managing the network for a client or server.
    /// </summary>
    public interface INetworkManager : IDisposable
    {
        /// <summary>
        /// Event invoked when a packet is received.
        /// </summary>
        event OnNetworkReceive OnNetworkReceive;

        /// <summary>
        /// Initializes the network manager.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Polls the network.
        /// </summary>
        void Poll();

    }

}
