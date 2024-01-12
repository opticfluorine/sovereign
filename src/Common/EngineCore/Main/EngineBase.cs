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
using Castle.Core.Logging;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Timing;
using Sovereign.EngineCore.World;

namespace Sovereign.EngineCore.Main;

/// <summary>
///     Runs the engine.
/// </summary>
public class EngineBase : IEngineBase
{
    /// <summary>
    ///     If the main loop processes less than this many events in a single pass,
    ///     the main thread will sleep instead of yielding its timeslice. This reduces
    ///     CPU utilization during light workloads in exchange for a temporary increase
    ///     in main loop latency.
    /// </summary>
    /// <remarks>
    ///     This can be set to zero to disable.
    /// </remarks>
    private const int ThreadSleepEventLimit = 0;

    private readonly EventDescriptions eventDescriptions;

    /// <summary>
    ///     Event loop.
    /// </summary>
    private readonly IEventLoop eventLoop;

    private readonly IList<IMainLoopAction> mainLoopActions;

    /// <summary>
    ///     Time manager.
    /// </summary>
    private readonly TimeManager timeManager;

    /// <summary>
    ///     World manager.
    /// </summary>
    private readonly WorldManager worldManager;

    /// <summary>
    ///     Main loop cycle count.
    /// </summary>
    private ulong cycleCount;

    public EngineBase(IEventLoop eventLoop, TimeManager timeManager,
        WorldManager worldManager, IList<IMainLoopAction> mainLoopActions,
        ConsoleEventAdapter eventAdapter, EventDescriptions eventDescriptions)
    {
        this.eventLoop = eventLoop;
        this.timeManager = timeManager;
        this.worldManager = worldManager;
        this.mainLoopActions = mainLoopActions;
        this.eventDescriptions = eventDescriptions;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    public void Run()
    {
        /* Output diagnostic info. */
        LogDiagnostics();

        /* Start the engine. */
        Logger.Info("EngineBase is starting.");
        Startup();
        Logger.Info("EngineBase is started.");

        /* Run the engine. */
        RunEngine();

        /* Stop the engine. */
        Logger.Info("EngineBase is stopping.");
        Shutdown();
        Logger.Info("EngineBase is stopped.");
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
    private void RunEngine()
    {
        /* Enter the main loop. */
        while (!eventLoop.Terminated)
        {
            /* Process any timed actions on the main thread. */
            timeManager.AdvanceTime();

            /* Drive the event loop. */
            var eventsProcessed = eventLoop.PumpEventLoop();

            /* Perform any main loop actions that are ready. */
            PerformMainLoopActions();

            /* Yield to avoid consuming 100% CPU. */
            Thread.Sleep(eventsProcessed < ThreadSleepEventLimit ? 1 : 0);
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
        Logger.DebugFormat("Maximum worker threads = {0}.", workerThreads);
        Logger.DebugFormat("Maximum I/O completion threads = {0}.", completionPortThreads);

        eventDescriptions.LogDebugInfo();
    }
}