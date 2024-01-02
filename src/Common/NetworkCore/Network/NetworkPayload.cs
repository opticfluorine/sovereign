/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
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

using MessagePack;
using Sovereign.EngineCore.Events;

namespace Sovereign.NetworkCore.Network;

/// <summary>
///     Payload of a network packet.
/// </summary>
[MessagePackObject]
public sealed class NetworkPayload
{
    /// <summary>
    ///     Creates a new payload.
    /// </summary>
    /// <param name="nonce">Nonce.</param>
    /// <param name="ev">Event.</param>
    public NetworkPayload(uint nonce, Event ev)
    {
        Nonce = nonce;
        Event = ev;
    }

    /// <summary>
    ///     Nonce for this payload.
    /// </summary>
    /// <remarks>
    ///     The nonce only needs to be unique for the connection.
    /// </remarks>
    [Key(0)]
    public uint Nonce { get; set; }

    /// <summary>
    ///     Event for this payload.
    /// </summary>
    [Key(1)]
    public Event Event { get; set; }
}