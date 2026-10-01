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

using Sovereign.WorldGen;

namespace Sovereign.ServerCore.Systems.WorldGeneration;

/// <summary>
///     Provides a public API for accessing world generation job state and completed plans.
/// </summary>
public class WorldGenerationServices
{
    private readonly WorldGenerationSystem system;

    public WorldGenerationServices(WorldGenerationSystem system)
    {
        this.system = system;
    }

    /// <summary>
    ///     Gets the status of the single world generation job slot.
    /// </summary>
    public WorldGenerationJobStatus JobStatus => system.JobStatus;

    /// <summary>
    ///     Gets the last human-readable status message reported for the job slot.
    /// </summary>
    public string? LastStatusMessage => system.LastStatusMessage;

    /// <summary>
    ///     Gets the last plan completed in this server session, or null if none has completed.
    /// </summary>
    public WorldGenPlan? LastCompletedPlan { get; private set; }

    /// <summary>
    ///     Records a completed world generation plan for later re-reporting.
    /// </summary>
    /// <param name="plan">Completed plan.</param>
    public void RecordCompletedPlan(WorldGenPlan plan)
    {
        LastCompletedPlan = plan;
    }

    /// <summary>
    ///     Requests that the running world generation job be aborted.
    /// </summary>
    /// <returns>Outcome of the request.</returns>
    public WorldGenAbortOutcome RequestAbort()
    {
        if (JobStatus == WorldGenerationJobStatus.Idle) return WorldGenAbortOutcome.NotRunning;
        if (JobStatus == WorldGenerationJobStatus.Cancelling || system.IsCancelling)
        {
            return WorldGenAbortOutcome.AlreadyRequested;
        }

        return system.RequestCancellation("Cancellation requested")
            ? WorldGenAbortOutcome.Requested
            : WorldGenAbortOutcome.NotRunning;
    }
}
