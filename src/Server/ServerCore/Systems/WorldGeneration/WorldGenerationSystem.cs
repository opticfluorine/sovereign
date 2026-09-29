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

using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems;
using Sovereign.ServerCore.Configuration;
using EventId = Sovereign.EngineCore.Events.EventId;

namespace Sovereign.ServerCore.Systems.WorldGeneration;

/// <summary>
///     System responsible for world generation processing. Holds the single world generation
///     job slot; generation algorithms arrive in later cards.
/// </summary>
public class WorldGenerationSystem : ISystem
{
    private readonly ILogger<WorldGenerationSystem> logger;
    private readonly WorldGenScratch scratch;

    public WorldGenerationSystem(EventCommunicator eventCommunicator, IEventLoop eventLoop,
        WorldGenScratch scratch, ILogger<WorldGenerationSystem> logger)
    {
        EventCommunicator = eventCommunicator;
        this.scratch = scratch;
        this.logger = logger;

        eventLoop.RegisterSystem(this);
    }

    public EventCommunicator EventCommunicator { get; }

    public ISet<EventId> EventIdsOfInterest { get; } = new HashSet<EventId>();

    public int WorkloadEstimate => 1;

    /// <summary>
    ///     Status of the single world generation job slot.
    /// </summary>
    public WorldGenerationJobStatus JobStatus { get; private set; }
        = WorldGenerationJobStatus.Idle;

    /// <summary>
    ///     Last human-readable status message reported for the job slot.
    /// </summary>
    public string? LastStatusMessage { get; private set; }

    public void Initialize()
    {
        // Staged plans are session-scoped: no boot-time staging recovery, just TTL cleanup
        // of stale scratch directories. No job can be in progress at startup.
        var removed = scratch.CleanupStaleDirectories();
        if (removed > 0)
        {
            logger.LogInformation("Removed {Count} stale worldgen scratch entries.", removed);
        }
    }

    public void Cleanup()
    {
    }

    public int ExecuteOnce()
    {
        return 0;
    }

    /// <summary>
    ///     Updates the job slot status.
    /// </summary>
    /// <param name="status">New job status.</param>
    /// <param name="message">Human-readable status message.</param>
    public void SetJobStatus(WorldGenerationJobStatus status, string message)
    {
        logger.LogDebug("World generation job status set to {Status}: {Message}", status, message);
        JobStatus = status;
        LastStatusMessage = message;
    }
}
