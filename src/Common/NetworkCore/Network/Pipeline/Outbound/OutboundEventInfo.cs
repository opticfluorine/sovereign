// Sovereign Engine
// Copyright (c) 2023 opticfluorine
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

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