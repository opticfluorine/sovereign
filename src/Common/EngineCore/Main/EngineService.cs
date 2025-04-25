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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Timing;

namespace Sovereign.EngineCore.Main;

/// <summary>
///     Runs the engine.
/// </summary>
public class EngineService : BackgroundService
{
    /// <summary>
    ///     Event loop.
    /// </summary>
    private readonly IEventLoop eventLoop;

    private readonly ILogger<EngineService> logger;

    private readonly List<IMainLoopAction> mainLoopActions;

    /// <summary>
    ///     Time manager.
    /// </summary>
    private readonly TimeManager timeManager;

    /// <summary>
    ///     Main loop cycle count.
    /// </summary>
    private ulong cycleCount;

    public EngineService(IEventLoop eventLoop, TimeManager timeManager, IEnumerable<IMainLoopAction> mainLoopActions,
        ILogger<EngineService> logger)
    {
        this.eventLoop = eventLoop;
        this.timeManager = timeManager;
        this.logger = logger;
        this.mainLoopActions = new List<IMainLoopAction>(mainLoopActions);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() => Run(stoppingToken));
    }

    /// <summary>
    ///     Runs the engine.
    /// </summary>
    /// <param name="stoppingToken">Cancellation token.</param>
    private void Run(CancellationToken stoppingToken)
    {
        /* Output diagnostic info. */
        LogDiagnostics();

        /* Start the engine. */
        logger.LogInformation("EngineService is starting.");
        Startup();
        logger.LogInformation("EngineService is started.");

#if DEBUG
        logger.LogWarning("This is a DEBUG build of Sovereign Engine.");
        logger.LogWarning("Debug builds are NOT suitable for production use.");
        logger.LogWarning("Ensure that only Release builds are used in production.");
#endif

        /* Run the engine. */
        RunEngine(stoppingToken);

        /* Stop the engine. */
        logger.LogInformation("EngineService is stopping.");
        Shutdown();
        logger.LogInformation("EngineService is stopped.");
    }

    /// <summary>
    ///     Starts the engine.
    /// </summary>
    private void Startup()
    {
    }

    /// <summary>
    ///     Runs the engine.
    /// </summary>
    /// <param name="stoppingToken">Cancellation token.</param>
    private void RunEngine(CancellationToken stoppingToken)
    {
        /* Enter the main loop. */
        while (!stoppingToken.IsCancellationRequested)
        {
            /* Process any timed actions on the main thread. */
            timeManager.AdvanceTime();

            /* Drive the event loop. */
            var eventsProcessed = eventLoop.PumpEventLoop();

            /* Perform any main loop actions that are ready. */
            PerformMainLoopActions();

            /* Yield to avoid consuming 100% CPU. */
            Thread.Yield();
        }
    }

    /// <summary>
    ///     Stops the engine.
    /// </summary>
    private void Shutdown()
    {
    }

    /// <summary>
    ///     Performs any main loop actions that are ready for execution.
    /// </summary>
    private void PerformMainLoopActions()
    {
        cycleCount++;
        foreach (var action in mainLoopActions)
            if (cycleCount % action.CycleInterval == 0)
                action.Execute();
    }

    /// <summary>
    ///     Logs startup diagnostic information.
    /// </summary>
    private void LogDiagnostics()
    {
        ThreadPool.GetMaxThreads(out var workerThreads, out var completionPortThreads);
        logger.LogDebug("Maximum worker threads = {Threads}.", workerThreads);
        logger.LogDebug("Maximum I/O completion threads = {Threads}.", completionPortThreads);
    }
}