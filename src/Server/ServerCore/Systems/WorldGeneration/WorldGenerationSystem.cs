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
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Systems;
using EventId = Sovereign.EngineCore.Events.EventId;

namespace Sovereign.ServerCore.Systems.WorldGeneration;

/// <summary>
///     System responsible for world generation processing. Receives worldgen request events
///     and dispatches them to the job runners, which own the single job slot held by
///     <see cref="WorldGenStateManager" />.
/// </summary>
public class WorldGenerationSystem : ISystem
{
    private readonly ILogger<WorldGenerationSystem> logger;
    private readonly WorldGenScratch scratch;
    private readonly WorldGenStateManager stateManager;
    private readonly WorldGenPlanJobRunner planRunner;
    private readonly WorldGenCommitRunner commitRunner;

    public WorldGenerationSystem(EventCommunicator eventCommunicator, IEventLoop eventLoop,
        WorldGenStateManager stateManager, WorldGenPlanJobRunner planRunner,
        WorldGenCommitRunner commitRunner, WorldGenScratch scratch,
        ILogger<WorldGenerationSystem> logger)
    {
        EventCommunicator = eventCommunicator;
        this.stateManager = stateManager;
        this.planRunner = planRunner;
        this.commitRunner = commitRunner;
        this.scratch = scratch;
        this.logger = logger;

        eventLoop.RegisterSystem(this);
    }

    public EventCommunicator EventCommunicator { get; }

    public ISet<EventId> EventIdsOfInterest { get; } = new HashSet<EventId>
    {
        EventId.Server_WorldGen_Plan,
        EventId.Server_WorldGen_Commit,
        EventId.Server_WorldGen_Replace,
        EventId.Server_WorldGen_Abort
    };

    public int WorkloadEstimate => 1;

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
        var processed = 0;
        while (EventCommunicator.GetIncomingEvent(out var ev))
        {
            ++processed;
            switch (ev.EventId)
            {
                case EventId.Server_WorldGen_Plan:
                {
                    if (ev.EventDetails is not WorldGenPlanEventDetails details)
                    {
                        logger.LogError("Received WorldGen Plan with no details.");
                        break;
                    }

                    planRunner.BeginPlan(details.Seed, details.ProfileName, details.Origin,
                        details.SenderEntityId);
                    break;
                }

                case EventId.Server_WorldGen_Commit:
                {
                    if (ev.EventDetails is not WorldGenCommitEventDetails details)
                    {
                        logger.LogError("Received WorldGen Commit with no details.");
                        break;
                    }

                    commitRunner.BeginCommit(details.Seed, details.Force, details.SenderEntityId);
                    break;
                }

                case EventId.Server_WorldGen_Replace:
                {
                    if (ev.EventDetails is not WorldGenReplaceEventDetails details)
                    {
                        logger.LogError("Received WorldGen Replace with no details.");
                        break;
                    }

                    commitRunner.BeginReplace(details.StagedSeed, details.OldSeed, details.Force,
                        details.SenderEntityId);
                    break;
                }

                case EventId.Server_WorldGen_Abort:
                    stateManager.RequestCancellation("Cancellation requested");
                    break;
            }
        }

        return processed;
    }
}
