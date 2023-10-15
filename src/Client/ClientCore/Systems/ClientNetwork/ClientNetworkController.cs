﻿/*
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

using Sovereign.ClientCore.Events;
using Sovereign.ClientCore.Network;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.NetworkCore.Network.Rest.Data;

namespace Sovereign.ClientCore.Systems.ClientNetwork;

/// <summary>
///     Provides a public API to the client network system.
/// </summary>
public sealed class ClientNetworkController
{
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
    /// <param name="connectionParameters">Connection parameters.</param>
    /// <param name="loginParameters">Login parameters.</param>
    public void BeginConnection(IEventSender eventSender, ClientConnectionParameters connectionParameters,
        LoginParameters loginParameters)
    {
        var ev = new Event(EventId.Client_Network_BeginConnection,
            new BeginConnectionEventDetails(connectionParameters, loginParameters));
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

    /// <summary>
    ///     Sends an event commanding an account registration.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="request">Registration request.</param>
    /// <param name="connectionParameters">Connection parameters.</param>
    public void RegisterAccount(IEventSender eventSender, RegistrationRequest request,
        ClientConnectionParameters connectionParameters)
    {
        var ev = new Event(EventId.Client_Network_RegisterAccount,
            new RegisterAccountEventDetails(request, connectionParameters));
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Sends an event announcing a successful registration.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    public void RegistrationSucceeded(IEventSender eventSender)
    {
        var ev = new Event(EventId.Client_Network_RegisterSuccess);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Sends an event announcing a failed registration.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="error">Error message.</param>
    public void RegistrationFailed(IEventSender eventSender, string error)
    {
        var ev = new Event(EventId.Client_Network_RegisterFailed,
            new ErrorEventDetails(error));
        eventSender.SendEvent(ev);
    }
}