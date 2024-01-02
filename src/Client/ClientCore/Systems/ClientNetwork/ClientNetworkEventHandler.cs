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

using Castle.Core.Logging;
using Sovereign.ClientCore.Events;
using Sovereign.ClientCore.Network;
using Sovereign.ClientCore.Network.Infrastructure;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;

namespace Sovereign.ClientCore.Systems.ClientNetwork;

/// <summary>
///     Responsible for handling events related to the client network system.
/// </summary>
public sealed class ClientNetworkEventHandler
{
    private readonly IEventSender eventSender;
    private readonly INetworkClient networkClient;
    private readonly ClientNetworkController networkController;
    private readonly RegistrationClient registrationClient;
    private readonly ClientWorldSegmentSubscriptionManager worldSegmentSubscriptionManager;

    public ClientNetworkEventHandler(INetworkClient networkClient, RegistrationClient registrationClient,
        ClientNetworkController networkController, IEventSender eventSender,
        ClientWorldSegmentSubscriptionManager worldSegmentSubscriptionManager)
    {
        this.networkClient = networkClient;
        this.registrationClient = registrationClient;
        this.networkController = networkController;
        this.eventSender = eventSender;
        this.worldSegmentSubscriptionManager = worldSegmentSubscriptionManager;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    /// <summary>
    ///     Handles a client network related event.
    /// </summary>
    /// <param name="ev">Event.</param>
    public void HandleEvent(Event ev)
    {
        switch (ev.EventId)
        {
            case EventId.Client_Network_ConnectionLost:
                HandleConnectionLostEvent();
                break;

            case EventId.Client_Network_BeginConnection:
                if (ev.EventDetails == null)
                {
                    Logger.Error("Received BeginConnection without details.");
                    break;
                }

                HandleBeginConnection((BeginConnectionEventDetails)ev.EventDetails);
                break;

            case EventId.Client_Network_RegisterAccount:
                if (ev.EventDetails == null)
                {
                    Logger.Error("Received RegisterAccount without details.");
                    break;
                }

                HandleRegisterAccount((RegisterAccountEventDetails)ev.EventDetails);
                break;

            case EventId.Core_WorldManagement_Subscribe:
                if (ev.EventDetails == null)
                {
                    Logger.Error("Received Subscribe without details.");
                    break;
                }

                HandleWorldSegmentSubscribe((WorldSegmentSubscriptionEventDetails)ev.EventDetails);
                break;

            case EventId.Core_WorldManagement_Unsubscribe:
                if (ev.EventDetails == null)
                {
                    Logger.Error("Received Unsubscribe without details.");
                    break;
                }

                HandleWorldSegmentUnsubscribe((WorldSegmentSubscriptionEventDetails)ev.EventDetails);
                break;

            default:
                Logger.WarnFormat("Unhandled event {0} in ClientNetworkEventHandler.", ev.EventId);
                break;
        }
    }

    /// <summary>
    ///     Handles a world segment subscribe event.
    /// </summary>
    /// <param name="details">Event details.</param>
    private void HandleWorldSegmentUnsubscribe(WorldSegmentSubscriptionEventDetails details)
    {
        worldSegmentSubscriptionManager.Subscribe(details.SegmentIndex);
    }

    /// <summary>
    ///     Handles a world segment unsubscribe event.
    /// </summary>
    /// <param name="details">Event details.</param>
    private void HandleWorldSegmentSubscribe(WorldSegmentSubscriptionEventDetails details)
    {
        worldSegmentSubscriptionManager.Unsubscribe(details.SegmentIndex);
    }

    /// <summary>
    ///     Handles a register account event.
    /// </summary>
    /// <param name="eventDetails">Registration details.</param>
    private void HandleRegisterAccount(RegisterAccountEventDetails eventDetails)
    {
        registrationClient.RegisterAsync(eventDetails.ConnectionParameters, eventDetails.RegistrationRequest)
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    networkController.RegistrationFailed(eventSender, "Registration failed due to an internal error.");
                    return;
                }

                var option = task.Result;
                if (option.HasFirst)
                    networkController.RegistrationSucceeded(eventSender);
                else
                    networkController.RegistrationFailed(eventSender, option.Second);
            });
    }

    /// <summary>
    ///     Handles a begin connection request.
    /// </summary>
    /// <param name="details">Connection details.</param>
    private void HandleBeginConnection(BeginConnectionEventDetails details)
    {
        networkClient.BeginConnection(details.ConnectionParameters, details.LoginParameters);
    }

    /// <summary>
    ///     Handles a connection lost event.
    /// </summary>
    private void HandleConnectionLostEvent()
    {
        // Only pass this along if we're actually connected or connecting.
        if (networkClient.ClientState == NetworkClientState.Connected
            || networkClient.ClientState == NetworkClientState.Connecting)
        {
            Logger.Error("Connection to server lost.");
            networkClient.EndConnection();
        }
    }
}