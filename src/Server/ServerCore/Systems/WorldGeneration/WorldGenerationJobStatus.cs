// Sovereign Engine
// Copyright (c) 2026 opticfluorine
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

namespace Sovereign.ServerCore.Systems.WorldGeneration;

/// <summary>
///     Outcome of a worldgen abort request.
/// </summary>
public enum WorldGenAbortOutcome
{
    /// <summary>
    ///     No job is running; the slot is Idle. The job may have completed just before the
    ///     request was observed.
    /// </summary>
    NotRunning,

    /// <summary>
    ///     The running job was asked to cancel.
    /// </summary>
    Requested,

    /// <summary>
    ///     Cancellation was already requested and the job has yet to unwind.
    /// </summary>
    AlreadyRequested
}

/// <summary>
///     Status of the single world generation job slot.
/// </summary>
public enum WorldGenerationJobStatus
{
    /// <summary>
    ///     No world generation job is running.
    /// </summary>
    Idle,

    /// <summary>
    ///     A world generation plan is being computed.
    /// </summary>
    Planning,

    /// <summary>
    ///     A computed world generation plan is being committed to the world.
    /// </summary>
    Committing,

    /// <summary>
    ///     A running job has been asked to abort and has yet to observe the cancellation.
    ///     The underlying work is still Planning or Committing, but further abort requests
    ///     need no answer.
    /// </summary>
    Cancelling
}
