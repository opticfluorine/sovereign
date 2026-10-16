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

using Microsoft.Extensions.DependencyInjection;
using Sovereign.ClientCore;
using Sovereign.ClientCore.Network.Infrastructure;
using Sovereign.EngineCore;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Timing;
using TestClientCore.TestSupport;
using Xunit;

namespace TestClientCore;

/// <summary>
///     Dependency injection smoke test for the typed entity data client services.
/// </summary>
public class TestEntityDataServiceRegistration
{
    [Fact]
    public void AddSovereignClient_ResolvesEntityDataServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions();
        services.AddSingleton<IEventLoop>(new FakeEventLoop());
        services.AddSingleton<ISystemTimer>(new FakeSystemTimer());
        services.AddSovereignCore();
        services.AddSovereignClient();

        using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetRequiredService<EntityDataClient>());
        Assert.NotNull(provider.GetRequiredService<TemplateEntityDataClient>());
    }
}
