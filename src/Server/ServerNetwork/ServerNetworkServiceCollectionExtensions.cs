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
using Sovereign.NetworkCore.Network.Infrastructure;
using Sovereign.NetworkCore.Network.Pipeline.Inbound;
using Sovereign.NetworkCore.Network.Pipeline.Outbound;
using Sovereign.NetworkCore.Systems.Network;
using Sovereign.ServerCore.Systems.ServerChat;
using Sovereign.ServerNetwork.Network.Infrastructure;
using Sovereign.ServerNetwork.Network.Pipeline.Inbound;
using Sovereign.ServerNetwork.Network.Pipeline.Outbound;
using Sovereign.ServerNetwork.Systems.Network;
using Sovereign.ServerNetwork.Systems.ServerChat;

namespace Sovereign.ServerNetwork;

public static class ServerNetworkServiceCollectionExtensions
{
    public static IServiceCollection AddSovereignServerNetwork(this IServiceCollection services)
    {
        AddServerImplementations(services);
        AddInboundPipeline(services);
        AddChat(services);

        return services;
    }

    private static void AddServerImplementations(IServiceCollection services)
    {
        services.TryAddSingleton<INetworkManager, ServerNetworkManager>();
        services
            .TryAddSingleton<IConnectionMappingOutboundPipelineStage, ServerConnectionMappingOutboundPipelineStage>();
        services.TryAddSingleton<IOutboundEventSet, ServerOutboundEventSet>();
    }

    private static void AddInboundPipeline(IServiceCollection services)
    {
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IInboundPipelineStage, PlayerFilterInboundPipelineStage>());
        services.TryAddEnumerable(ServiceDescriptor
            .Singleton<IInboundPipelineStage, SourceEntityMappingInboundPipelineStage>());
        services.TryAddEnumerable(ServiceDescriptor
            .Singleton<IInboundPipelineStage, ServerAllowedEventsInboundPipelineStage>());
    }

    private static void AddChat(IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IChatProcessor, AdminChatProcessor>());
    }
}