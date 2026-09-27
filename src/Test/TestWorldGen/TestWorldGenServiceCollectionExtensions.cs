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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems;
using Sovereign.ServerCore.Systems.ServerChat;
using Sovereign.ServerCore.Systems.WorldGeneration;
using Sovereign.WorldGen;
using Xunit;

namespace TestWorldGen;

/// <summary>
///     Dependency injection smoke tests for the WorldGeneration system registrations.
/// </summary>
public class TestWorldGenServiceCollectionExtensions
{
    [Fact]
    public void AddWorldGenerationSystem_BuildsProvider_AndResolvesServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IEventLoop>(new FakeEventLoop());
        services.AddSingleton<IEventSender>(new FakeEventSender());
        services.AddSingleton<EventCommunicator>();
        services.AddSingleton<ILogger<WorldGenerationSystem>>(NullLogger<WorldGenerationSystem>.Instance);
        services.AddSingleton<ServerChatInternalController>();
        services.AddWorldGenerationSystem();

        using var provider = services.BuildServiceProvider();

        var system = provider.GetRequiredService<WorldGenerationSystem>();
        Assert.Equal(WorldGenerationJobStatus.Idle, system.JobStatus);
        Assert.Same(system, provider.GetRequiredService<ISystem>());
        Assert.NotNull(provider.GetRequiredService<WorldGenerationServices>());
        Assert.NotNull(provider.GetRequiredService<WorldGenerationController>());
        Assert.NotNull(provider.GetRequiredService<WorldGenChatCommandHandler>());
        Assert.NotNull(provider.GetRequiredService<ProfileLoader>());
        Assert.NotNull(provider.GetRequiredService<ProfileValidator>());
    }
}
