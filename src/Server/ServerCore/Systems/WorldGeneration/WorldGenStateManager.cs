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

using System.Threading;
using Microsoft.Extensions.Logging;

namespace Sovereign.ServerCore.Systems.WorldGeneration;

/// <summary>
///     Holds the state of the single world generation job slot and its active job's
///     cancellation source for the WorldGeneration system. Job runners observe the token
///     and return the slot to Idle when the job settles.
/// </summary>
public class WorldGenStateManager
{
    private readonly ILogger<WorldGenStateManager> logger;

    /// <summary>
    ///     Guards the job slot state and the active job's cancellation source.
    /// </summary>
    private readonly object jobLock = new();

    /// <summary>
    ///     Cancellation source of the running job, or null when the slot is Idle.
    /// </summary>
    private CancellationTokenSource? cancellationTokenSource;

    public WorldGenStateManager(ILogger<WorldGenStateManager> logger)
    {
        this.logger = logger;
    }

    /// <summary>
    ///     Status of the single world generation job slot.
    /// </summary>
    public WorldGenerationJobStatus JobStatus { get; private set; }
        = WorldGenerationJobStatus.Idle;

    /// <summary>
    ///     Last human-readable status message reported for the job slot.
    /// </summary>
    public string? LastStatusMessage { get; private set; }

    /// <summary>
    ///     Whether the running job has been asked to abort. True from the abort request until
    ///     the job settles; also true on top of a <see cref="WorldGenerationJobStatus.Cancelling" />
    ///     slot.
    /// </summary>
    public bool IsCancelling { get; private set; }

    /// <summary>
    ///     Begins a job: records the status and returns the cancellation token the job must
    ///     observe. A previous source, if any, is discarded; only one job may run at a time.
    /// </summary>
    /// <param name="status">Initial job status.</param>
    /// <param name="message">Human-readable status message.</param>
    /// <returns>Cancellation token for the job.</returns>
    public CancellationToken BeginJob(WorldGenerationJobStatus status, string message)
    {
        lock (jobLock)
        {
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = new CancellationTokenSource();
            IsCancelling = false;
            SetJobStatus(status, message);
            return cancellationTokenSource.Token;
        }
    }

    /// <summary>
    ///     Cancellation token of the running job, or <see cref="CancellationToken.None" />
    ///     when the slot is Idle. Runners snapshot this right after starting a job.
    /// </summary>
    public CancellationToken JobCancellationToken
    {
        get
        {
            lock (jobLock)
            {
                return cancellationTokenSource?.Token ?? CancellationToken.None;
            }
        }
    }

    /// <summary>
    ///     Ends a job: the job slot returns to Idle with the given settlement message and
    ///     the cancellation source is retired. Safe on an Idle slot.
    /// </summary>
    /// <param name="message">Final human-readable status message of the settled job.</param>
    public void EndJob(string message)
    {
        lock (jobLock)
        {
            JobStatus = WorldGenerationJobStatus.Idle;
            LastStatusMessage = message;
            IsCancelling = false;
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
        }
    }

    /// <summary>
    ///     Requests cancellation of the running job, if any, and records the job slot as
    ///     CancellationTokenRequested.
    /// </summary>
    /// <param name="message">Status message describing the request.</param>
    /// <returns>true when a running job was asked to cancel, false when no job can be
    ///     cancelled because the slot is Idle.</returns>
    public bool RequestCancellation(string message)
    {
        lock (jobLock)
        {
            if (JobStatus == WorldGenerationJobStatus.Idle || cancellationTokenSource is null)
            {
                return false;
            }

            cancellationTokenSource.Cancel();
            IsCancelling = true;
            JobStatus = WorldGenerationJobStatus.Cancelling;
            LastStatusMessage = message;
            return true;
        }
    }

    /// <summary>
    ///     Determines whether the running job has been asked to abort.
    /// </summary>
    /// <returns>true when a cancellation has been requested, false otherwise.</returns>
    public bool IsCancellationRequested()
    {
        lock (jobLock)
        {
            return IsCancelling;
        }
    }

    /// <summary>
    ///     Updates the job slot status.
    /// </summary>
    /// <param name="status">New job status.</param>
    /// <param name="message">Human-readable status message.</param>
    public void SetJobStatus(WorldGenerationJobStatus status, string message)
    {
        lock (jobLock)
        {
            logger.LogDebug("World generation job status set to {Status}: {Message}", status, message);
            JobStatus = status;
            LastStatusMessage = message;
        }
    }
}
