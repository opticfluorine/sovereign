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

using Sovereign.EngineCore.Events;
using Sovereign.NetworkCore.Network.Infrastructure;

namespace Sovereign.NetworkCore.Network.Pipeline.Inbound;

/// <summary>
///     Interface to an individual stage in the inbound network pipeline.
/// </summary>
/// <remarks>
///     Inbound pipeline stages may accept or reject outbound events as well
///     as modify them. If an outbound event is accepted, it is passed to the
///     next stage of the pipeline after applying any transformation. An event
///     may be rejected simply by not passing anything to the next stage of
///     the pipeline.
/// </remarks>
public interface IInboundPipelineStage
{
    /// <summary>
    ///     Stage priority. Lower priorities are executed earlier. The ordering
    ///     of stages having equal priority is undefined.
    /// </summary>
    int Priority { get; }

    /// <summary>
    ///     Next stage in the inbound network pipeline.
    /// </summary>
    IInboundPipelineStage NextStage { get; set; }

    /// <summary>
    ///     Processes an inbound event.
    /// </summary>
    /// <param name="ev">Inbound event.</param>
    /// <param name="connection">Associated connection.</param>
    void ProcessEvent(Event ev, NetworkConnection connection);
}