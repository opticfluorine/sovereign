/*
 * Sovereign Engine
 * Copyright (c) 2026 opticfluorine
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

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sovereign.EngineCore.Configuration;
using Sovereign.EngineCore.Systems;

namespace Sovereign.EngineCore.Main;

/// <summary>
///     Main loop action that executes all systems on the main thread when single-threaded
///     execution is enabled.
/// </summary>
public sealed class SystemExecutionMainLoopAction : IMainLoopAction
{
    private readonly ILogger<SystemExecutionMainLoopAction> logger;
    private readonly PerformanceOptions performanceOptions;
    private readonly List<ISystem> systems;

    public SystemExecutionMainLoopAction(IEnumerable<ISystem> systems,
        IOptions<PerformanceOptions> performanceOptions,
        ILogger<SystemExecutionMainLoopAction> logger)
    {
        this.systems = new List<ISystem>(systems);
        this.performanceOptions = performanceOptions.Value;
        this.logger = logger;
    }

    public ulong CycleInterval => 1;

    public void Execute()
    {
        /* Systems are executed on their own threads in multi-threaded mode. */
        if (performanceOptions.SystemExecutorCount > 0) return;

        foreach (var system in systems)
            try
            {
                system.ExecuteOnce();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unhandled exception in system {System}.", system.GetType().Name);
            }
    }
}
