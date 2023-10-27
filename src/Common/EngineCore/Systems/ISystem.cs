/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
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

using System.Collections.Generic;
using Sovereign.EngineCore.Events;

namespace Sovereign.EngineCore.Systems;

/// <summary>
///     Interface implemented by all Systems in the ECS framework.
/// </summary>
public interface ISystem
{
    /// <summary>
    ///     Event communicator for the given service.
    /// </summary>
    EventCommunicator EventCommunicator { get; }

    /// <summary>
    ///     The set of event IDs that this system is listening for.
    /// </summary>
    ISet<EventId> EventIdsOfInterest { get; }

    /// <summary>
    ///     Estimated scale of the workload performed by this system.
    ///     Larger values indicate greater expected workloads.
    ///     This value is used to balance the system threads.
    /// </summary>
    int WorkloadEstimate { get; }

    /// <summary>
    ///     Initializes the system. Called from the system thread.
    /// </summary>
    void Initialize();

    /// <summary>
    ///     Cleans up the system. Called from the system thread.
    /// </summary>
    void Cleanup();

    /// <summary>
    ///     Executes the system once.
    /// </summary>
    /// <returns>Number of events processed by the system during this call.</returns>
    int ExecuteOnce();
}