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

using Sovereign.ClientCore.Events;
using Sovereign.ClientCore.Network;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;

namespace Sovereign.ClientCore.Systems.ClientNetwork
{

    /// <summary>
    /// Provides a public API to the client network system.
    /// </summary>
    public sealed class ClientNetworkController
    {

        /// <summary>
        /// Sends an event announcing that the connection has been lost.
        /// </summary>
        /// <param name="eventSender">Event sender.</param>
        public void DeclareConnectionLost(IEventSender eventSender)
        {
            var ev = new Event(EventId.Client_Network_ConnectionLost);
            eventSender.SendEvent(ev);
        }

        /// <summary>
        /// Sends an event commanding the system to begin a connection to the server.
        /// </summary>
        /// <param name="eventSender">Event sender.</param>
        /// <param name="connectionParameters">Connection parameters.</param>
        /// <param name="loginParameters">Login parameters.</param>
        public void BeginConnection(IEventSender eventSender, ClientConnectionParameters connectionParameters, LoginParameters loginParameters)
        {
            var ev = new Event(EventId.Client_Network_BeginConnection,
                new BeginConnectionEventDetails(connectionParameters, loginParameters));
            eventSender.SendEvent(ev);
        }

        /// <summary>
        /// Sends an event announcing that a login attempt failed.
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
        /// Sends an event announcing that a connection attempt failed after successful authentication.
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
        /// Sends an event announcing that the connection to the event server has been established.
        /// </summary>
        /// <param name="eventSender">Event sender.</param>
        public void Connected(IEventSender eventSender)
        {
            var ev = new Event(EventId.Client_Network_Connected);
            eventSender.SendEvent(ev);
        }

    }

}
