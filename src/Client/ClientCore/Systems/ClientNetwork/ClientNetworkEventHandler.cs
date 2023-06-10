/*
 * Sovereign Engine
 * Copyright (c) 2023 opticfluorine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using Castle.Core.Logging;
using Sovereign.ClientCore.Events;
using Sovereign.ClientCore.Network;
using Sovereign.ClientCore.Network.Infrastructure;
using Sovereign.EngineCore.Events;
using System;

namespace Sovereign.ClientCore.Systems.ClientNetwork
{

    /// <summary>
    /// Responsible for handling events related to the client network system.
    /// </summary>
    public sealed class ClientNetworkEventHandler
    {
        private readonly INetworkClient networkClient;
        private readonly RegistrationClient registrationClient;
        private readonly ClientNetworkController networkController;
        private readonly IEventSender eventSender;

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        public ClientNetworkEventHandler(INetworkClient networkClient, RegistrationClient registrationClient,
            ClientNetworkController networkController, IEventSender eventSender)
        {
            this.networkClient = networkClient;
            this.registrationClient = registrationClient;
            this.networkController = networkController;
            this.eventSender = eventSender;
        }

        /// <summary>
        /// Handles a client network related event.
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
                    HandleBeginConnection((BeginConnectionEventDetails)ev.EventDetails);
                    break;

                case EventId.Client_Network_RegisterAccount:
                    HandleRegisterAccount((RegisterAccountEventDetails)ev.EventDetails);
                    break;

                default:
                    Logger.WarnFormat("Unhandled event {0} in ClientNetworkEventHandler.", ev.EventId);
                    break;
            }
        }

        /// <summary>
        /// Handles a register account event.
        /// </summary>
        /// <param name="eventDetails">Registration details.</param>
        private void HandleRegisterAccount(RegisterAccountEventDetails eventDetails)
        {
            registrationClient.RegisterAsync(eventDetails.ConnectionParameters, eventDetails.RegistrationRequest)
                .ContinueWith((task) =>
                {
                    if (task.IsFaulted)
                    {
                        networkController.RegistrationFailed(eventSender, "Registration failed due to an internal error.");
                        return;
                    }

                    var option = task.Result;
                    if (option.HasFirst)
                    {
                        networkController.RegistrationSucceeded(eventSender);
                    }
                    else
                    {
                        networkController.RegistrationFailed(eventSender, option.Second);
                    }
                });
        }

        /// <summary>
        /// Handles a begin connection request.
        /// </summary>
        /// <param name="details">Connection details.</param>
        private void HandleBeginConnection(BeginConnectionEventDetails details)
        {
            networkClient.BeginConnection(details.ConnectionParameters, details.LoginParameters);
        }

        /// <summary>
        /// Handles a connection lost event.
        /// </summary>
        private void HandleConnectionLostEvent()
        {
            /* Log the error, then gracefully close out the connections as usual. */
            Logger.Error("Connection to server lost.");
            networkClient.EndConnection();
        }

    }

}

