// Sovereign Engine
// Copyright (c) 2023 opticfluorine
// 
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using Sovereign.EngineCore.Events;
using Sovereign.ServerCore.Events;

namespace Sovereign.ServerNetwork.Network.ServerNetwork;

/// <summary>
///     Provides the public API for the server network system.
/// </summary>
public class ServerNetworkController
{
    /// <summary>
    ///     Requests that the client with the given connection ID be disconnected from the server.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="connectionId">Connection ID.</param>
    public void Disconnect(IEventSender eventSender, int connectionId)
    {
        var ev = new Event(EventId.Server_Network_DisconnectClient,
            new ConnectionIdEventDetails { ConnectionId = connectionId });
        eventSender.SendEvent(ev);
    }
}