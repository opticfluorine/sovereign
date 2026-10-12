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
using Microsoft.Extensions.DependencyInjection;
using Sovereign.ClientCore;
using Sovereign.EngineCore;
using Sovereign.EngineCore.Configuration;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Timing;

namespace TestClientCore.TestSupport;

/// <summary>
///     Builds the real client dependency injection graph for tests, with the event loop, system
///     timer, and event sender faked for control and inspection.
/// </summary>
public sealed class ClientTestHarness : IDisposable
{
    /// <summary>
    ///     Recording event sender registered as the injected IEventSender.
    /// </summary>
    public RecordingEventSender EventSender { get; } = new();

    /// <summary>
    ///     System timer fake whose time can be set by tests.
    /// </summary>
    public FakeSystemTimer SystemTimer { get; } = new();

    /// <summary>
    ///     Service provider for the client graph.
    /// </summary>
    public ServiceProvider Provider { get; }

    /// <summary>
    ///     Builds the client dependency injection graph.
    /// </summary>
    /// <param name="configureCombat">Optional callback to adjust the combat options before use.</param>
    public ClientTestHarness(Action<CombatOptions>? configureCombat = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions();
        services.AddSingleton<IEventSender>(EventSender);
        services.AddSingleton<IEventLoop>(new FakeEventLoop());
        services.AddSingleton<ISystemTimer>(SystemTimer);
        if (configureCombat != null) services.Configure(configureCombat);
        services.AddSovereignCore();
        services.AddSovereignClient();

        Provider = services.BuildServiceProvider();
    }

    /// <summary>
    ///     Resolves a required service from the client graph.
    /// </summary>
    /// <typeparam name="T">Service type.</typeparam>
    /// <returns>Resolved service.</returns>
    public T Get<T>()
        where T : notnull
    {
        return Provider.GetRequiredService<T>();
    }

    public void Dispose()
    {
        Provider.Dispose();
    }
}
