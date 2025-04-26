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

using Microsoft.Extensions.Logging;
using Sovereign.ClientCore.Configuration;
using Sovereign.ClientCore.Events.Details;
using Sovereign.ClientCore.Network;
using Sovereign.ClientCore.Systems.ClientState;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using EventId = Sovereign.EngineCore.Events.EventId;

namespace Sovereign.ClientCore.Systems.ClientNetwork;

/// <summary>
///     Provides a public API to the client network system.
/// </summary>
public sealed class ClientNetworkController
{
    private readonly ILogger<ClientNetworkController> logger;
    private readonly ClientStateServices stateServices;

    public ClientNetworkController(ClientStateServices stateServices, ILogger<ClientNetworkController> logger)
    {
        this.stateServices = stateServices;
        this.logger = logger;
    }

    /// <summary>
    ///     Sends an event announcing that the connection has been lost.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    public void DeclareConnectionLost(IEventSender eventSender)
    {
        var ev = new Event(EventId.Client_Network_ConnectionLost);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Sends an event commanding the system to begin a connection to the server.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="connectionOptions">Connection parameters.</param>
    /// <param name="loginParameters">Login parameters.</param>
    public void BeginConnection(IEventSender eventSender, ConnectionOptions connectionOptions,
        LoginParameters loginParameters)
    {
        var ev = new Event(EventId.Client_Network_BeginConnection,
            new BeginConnectionEventDetails(connectionOptions, loginParameters));
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Sends an event commanding the system to end its connection to the server.
    /// </summary>
    /// <param name="eventSender"></param>
    public void EndConnection(IEventSender eventSender)
    {
        var ev = new Event(EventId.Client_Network_EndConnection);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Logs out to player selection.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    public void LogoutPlayer(IEventSender eventSender)
    {
        if (!stateServices.TryGetSelectedPlayer(out var playerEntityId))
        {
            logger.LogError("No player is selected.");
            return;
        }

        var details = new EntityEventDetails
        {
            EntityId = playerEntityId
        };
        var ev = new Event(EventId.Core_Network_Logout, details);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Sends an event announcing that a login attempt failed.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="reason">Login failure reason.</param>
    public void LoginFailed(IEventSender eventSender, string reason)
    {
        var ev = new Event(EventId.Client_Network_LoginFailed,
            new ErrorEventDetails(reason));
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Sends an event announcing that a connection attempt failed after successful authentication.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="reason">Connection failure reason.</param>
    public void ConnectionAttemptFailed(IEventSender eventSender, string reason)
    {
        var ev = new Event(EventId.Client_Network_ConnectionAttemptFailed,
            new ErrorEventDetails(reason));
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Sends an event announcing that the connection to the event server has been established.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    public void Connected(IEventSender eventSender)
    {
        var ev = new Event(EventId.Client_Network_Connected);
        eventSender.SendEvent(ev);
    }
}