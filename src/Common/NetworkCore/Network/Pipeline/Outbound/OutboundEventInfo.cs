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

using LiteNetLib;
using Sovereign.EngineCore.Events;
using Sovereign.NetworkCore.Network.Infrastructure;

namespace Sovereign.NetworkCore.Network.Pipeline.Outbound;

/// <summary>
///     Information (partial or complete) for an outbound event in the pipeline.
/// </summary>
public struct OutboundEventInfo
{
    /// <summary>
    ///     Event. Required.
    /// </summary>
    public Event Event { get; }

    /// <summary>
    ///     Connection to send event with. Optional, but must be specified by the end of the pipeline.
    /// </summary>
    public NetworkConnection Connection { get; }

    /// <summary>
    ///     Delivery method for sending this event.
    /// </summary>
    public DeliveryMethod DeliveryMethod { get; }

    /// <summary>
    ///     Creates an outbound event info.
    /// </summary>
    /// <param name="ev">Event.</param>
    /// <param name="connection">Connection.</param>
    /// <param name="deliveryMethod">Delivery method.</param>
    public OutboundEventInfo(Event ev, NetworkConnection connection = null,
        DeliveryMethod deliveryMethod = DeliveryMethod.Unreliable)
    {
        Event = ev;
        Connection = connection;
        DeliveryMethod = deliveryMethod;
    }

    /// <summary>
    ///     Convenience constructor for adding a connection to an existing event info.
    /// </summary>
    /// <param name="info">Event info.</param>
    /// <param name="connection">Connection.</param>
    public OutboundEventInfo(OutboundEventInfo info, NetworkConnection connection)
    {
        Event = info.Event;
        Connection = connection;
        DeliveryMethod = info.DeliveryMethod;
    }

    /// <summary>
    ///     Convenience constructor for adding a delivery method to an existing event info.
    /// </summary>
    /// <param name="info"></param>
    /// <param name="deliveryMethod"></param>
    public OutboundEventInfo(OutboundEventInfo info, DeliveryMethod deliveryMethod)
    {
        Event = info.Event;
        Connection = info.Connection;
        DeliveryMethod = deliveryMethod;
    }
}