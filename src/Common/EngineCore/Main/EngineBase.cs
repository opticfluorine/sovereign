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

namespace Sovereign.EngineCore.Main;

/// <summary>
///     Runs the engine.
/// </summary>
public class EngineBase : IEngineBase
{
    private readonly EventDescriptions eventDescriptions;

    /// <summary>
    ///     Event loop.
    /// </summary>
    private readonly IEventLoop eventLoop;

    private readonly List<IMainLoopAction> mainLoopActions;

    /// <summary>
    ///     Time manager.
    /// </summary>
    private readonly TimeManager timeManager;

    /// <summary>
    ///     Main loop cycle count.
    /// </summary>
    private ulong cycleCount;

    public EngineBase(IEventLoop eventLoop, TimeManager timeManager, IList<IMainLoopAction> mainLoopActions,
        ConsoleEventAdapter eventAdapter, EventDescriptions eventDescriptions)
    {
        this.eventLoop = eventLoop;
        this.timeManager = timeManager;
        this.mainLoopActions = new List<IMainLoopAction>(mainLoopActions);
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

#if DEBUG
        Logger.Warn("This is a DEBUG build of Sovereign Engine.");
        Logger.Warn("Debug builds are NOT suitable for production use.");
        Logger.Warn("Ensure that only Release builds are used in production.");
#endif

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
        Logger.DebugFormat("Maximum worker threads = {0}.", workerThreads);
        Logger.DebugFormat("Maximum I/O completion threads = {0}.", completionPortThreads);

        eventDescriptions.LogDebugInfo();
    }
}