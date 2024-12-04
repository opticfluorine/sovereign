// Sovereign Engine
// Copyright (c) 2024 opticfluorine
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
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sovereign.EngineCore.Systems;
using Sovereign.NetworkCore.Network.Infrastructure;
using Sovereign.NetworkCore.Network.Pipeline.Inbound;
using Sovereign.NetworkCore.Network.Pipeline.Outbound;
using Sovereign.NetworkCore.Network.Service;
using Sovereign.NetworkCore.Systems.Network;
using Sovereign.NetworkCore.Systems.Ping;

namespace Sovereign.NetworkCore;

/// <summary>
///     Service registration manager for Sovereign.NetworkCore.
/// </summary>
public static class NetworkServiceCollectionExtensions
{
    public static IServiceCollection AddSovereignNetworkCore(this IServiceCollection services)
    {
        AddEvents(services);
        AddInfrastructure(services);
        AddInboundPipeline(services);
        AddOutboundPipeline(services);
        AddNetworkingService(services);
        AddSystems(services);

        return services;
    }

    private static void AddEvents(IServiceCollection services)
    {
        services.TryAddSingleton<NetworkEventAdapter>();
    }

    private static void AddInfrastructure(IServiceCollection services)
    {
        services.TryAddSingleton<NetLogger>();
        services.TryAddSingleton<NetworkSerializer>();
        services.TryAddSingleton<NetworkConnectionManager>();
    }

    private static void AddInboundPipeline(IServiceCollection services)
    {
        services.TryAddSingleton<InboundNetworkPipeline>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IInboundPipelineStage, ValidationInboundPipelineStage>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IInboundPipelineStage, FinalInboundPipelineStage>());
    }

    private static void AddOutboundPipeline(IServiceCollection services)
    {
        services.TryAddSingleton<OutboundNetworkPipeline>();
        services.TryAddSingleton<DeliveryMethodOutboundPipelineStage>();
        services.TryAddSingleton<FinalOutboundPipelineStage>();
    }

    private static void AddNetworkingService(IServiceCollection services)
    {
        services.TryAddSingleton<NetworkingService>();
        services.TryAddSingleton<ReceivedEventQueue>();
    }

    private static void AddSystems(IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISystem, NetworkSystem>());
        services.TryAddSingleton<PingController>();
    }
}