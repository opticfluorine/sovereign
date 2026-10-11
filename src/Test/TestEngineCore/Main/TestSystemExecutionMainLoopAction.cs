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

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Sovereign.EngineCore.Configuration;
using Sovereign.EngineCore.Systems;
using Xunit;

namespace Sovereign.EngineCore.Main;

/// <summary>
///     Unit tests for SystemExecutionMainLoopAction.
/// </summary>
public class TestSystemExecutionMainLoopAction
{
    [Fact]
    public void CycleInterval_IsOne()
    {
        var action = CreateAction(Array.Empty<ISystem>(), 0);

        Assert.Equal(1ul, action.CycleInterval);
    }

    [Fact]
    public void Execute_SingleThreaded_ExecutesAllSystemsOnce()
    {
        var system1 = new Mock<ISystem>();
        var system2 = new Mock<ISystem>();
        var action = CreateAction(new[] { system1.Object, system2.Object }, 0);

        action.Execute();

        system1.Verify(s => s.ExecuteOnce(), Times.Once);
        system2.Verify(s => s.ExecuteOnce(), Times.Once);
    }

    [Fact]
    public void Execute_MultiThreaded_DoesNotExecuteSystems()
    {
        var system = new Mock<ISystem>();
        var action = CreateAction(new[] { system.Object }, 1);

        action.Execute();

        system.Verify(s => s.ExecuteOnce(), Times.Never);
    }

    [Fact]
    public void Execute_SystemThrows_ContinuesExecutingRemainingSystems()
    {
        var throwingSystem = new Mock<ISystem>();
        throwingSystem.Setup(s => s.ExecuteOnce()).Throws(new InvalidOperationException("Test exception."));
        var otherSystem = new Mock<ISystem>();
        var action = CreateAction(new[] { throwingSystem.Object, otherSystem.Object }, 0);

        action.Execute();

        throwingSystem.Verify(s => s.ExecuteOnce(), Times.Once);
        otherSystem.Verify(s => s.ExecuteOnce(), Times.Once);
    }

    /// <summary>
    ///     Creates a SystemExecutionMainLoopAction for testing.
    /// </summary>
    /// <param name="systems">Systems to execute.</param>
    /// <param name="systemExecutorCount">Configured system executor count.</param>
    /// <returns>Action under test.</returns>
    private static SystemExecutionMainLoopAction CreateAction(IEnumerable<ISystem> systems, int systemExecutorCount)
    {
        var performanceOptions = Options.Create(new PerformanceOptions
        {
            SystemExecutorCount = systemExecutorCount
        });
        return new SystemExecutionMainLoopAction(systems, performanceOptions,
            NullLogger<SystemExecutionMainLoopAction>.Instance);
    }
}
