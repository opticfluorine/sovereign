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
using Sovereign.EngineCore;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems;
using Sovereign.Persistence;
using Sovereign.ServerCore;
using Sovereign.ServerCore.Configuration;
using Sovereign.ServerCore.Systems.WorldGeneration;
using Sovereign.WorldGen;
using Xunit;

namespace TestWorldGen;

public class TestWorldGenServiceRegistration
{
    [Fact]
    public void AddSovereignServer_BuildsProvider_AndResolvesWorldGenServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IEventLoop>(new FakeEventLoop());
        services.AddSingleton<IEventSender>(new FakeEventSender());
        services.AddLogging();
        services.AddOptions();
        services.Configure<WorldGenOptions>(_ => { });
        services.AddSovereignCore();
        services.AddSovereignServer();
        services.AddSovereignPersistence();

        using var provider = services.BuildServiceProvider();

        var system = provider.GetRequiredService<WorldGenerationSystem>();
        Assert.Equal(WorldGenerationJobStatus.Idle, system.JobStatus);
        Assert.Contains(services,
            d => d.ServiceType == typeof(ISystem) && d.ImplementationFactory is not null);
        Assert.NotNull(provider.GetRequiredService<WorldGenerationServices>());
        Assert.NotNull(provider.GetRequiredService<WorldGenerationController>());
        Assert.NotNull(provider.GetRequiredService<WorldGenChatCommandHandler>());
        Assert.NotNull(provider.GetRequiredService<WorldGenPlanJobRunner>());
        Assert.NotNull(provider.GetRequiredService<WorldGenCommitRunner>());
        Assert.NotNull(provider.GetRequiredService<WorldGenScratch>());
        Assert.NotNull(provider.GetRequiredService<IWorldGenPipeline>());
        Assert.NotNull(provider.GetRequiredService<ProfileLoader>());
        Assert.NotNull(provider.GetRequiredService<ProfileValidator>());
        Assert.NotNull(provider.GetRequiredService<IWorldGenCommitWriter>());
        Assert.NotNull(provider.GetRequiredService<IWorldGenWorldRegistryStore>());
    }
}
