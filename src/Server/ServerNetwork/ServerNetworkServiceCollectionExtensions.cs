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
using Sovereign.NetworkCore.Systems.Network;
using Sovereign.ServerCore.Systems.ServerChat;
using Sovereign.ServerNetwork.Entities.Players;
using Sovereign.ServerNetwork.Network.Connections;
using Sovereign.ServerNetwork.Network.Infrastructure;
using Sovereign.ServerNetwork.Network.Pipeline.Inbound;
using Sovereign.ServerNetwork.Network.Pipeline.Outbound;
using Sovereign.ServerNetwork.Network.Pipeline.Outbound.ConnectionMappers;
using Sovereign.ServerNetwork.Network.Rest;
using Sovereign.ServerNetwork.Network.Rest.Accounts;
using Sovereign.ServerNetwork.Network.Rest.Players;
using Sovereign.ServerNetwork.Network.Rest.TemplateEntities;
using Sovereign.ServerNetwork.Network.Rest.WorldSegment;
using Sovereign.ServerNetwork.Network.ServerNetwork;
using Sovereign.ServerNetwork.Systems.Network;
using Sovereign.ServerNetwork.Systems.ServerChat;
using Sovereign.ServerNetwork.Systems.ServerNetwork;

namespace Sovereign.ServerNetwork;

public static class ServerNetworkServiceCollectionExtensions
{
    public static IServiceCollection AddSovereignServerNetwork(this IServiceCollection services)
    {
        AddServerImplementations(services);
        AddEntities(services);
        AddInboundPipeline(services);
        AddChat(services);
        AddConnections(services);
        AddConnectionMappers(services);
        AddRest(services);
        AddServerNetworkSystem(services);

        return services;
    }

    private static void AddServerImplementations(IServiceCollection services)
    {
        services.TryAddSingleton<INetworkManager, ServerNetworkManager>();
        services
            .TryAddSingleton<IConnectionMappingOutboundPipelineStage, ServerConnectionMappingOutboundPipelineStage>();
        services.TryAddSingleton<IOutboundEventSet, ServerOutboundEventSet>();
    }

    private static void AddEntities(IServiceCollection services)
    {
        services.TryAddSingleton<PlayerBuilder>();
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

    private static void AddConnections(IServiceCollection services)
    {
        services.TryAddSingleton<NewConnectionProcessor>();
    }

    private static void AddConnectionMappers(IServiceCollection services)
    {
        services.TryAddSingleton<GlobalConnectionMapper>();
        services.TryAddSingleton<SingleEntityConnectionMapperFactory>();
        services.TryAddSingleton<EntityWorldSegmentConnectionMapperFactory>();
        services.TryAddSingleton<WorldSegmentConnectionMapperFactory>();
        services.TryAddSingleton<RegionalConnectionMapCache>();
        services.TryAddSingleton<GenericChatConnectionMapper>();
    }

    private static void AddRest(IServiceCollection services)
    {
        services.TryAddSingleton<CreatePlayerRequestValidator>();

        services.TryAddSingleton<AccountRegistrationRestService>();
        services.TryAddSingleton<AuthenticationRestService>();
        services.TryAddSingleton<CreatePlayerRestService>();
        services.TryAddSingleton<DeletePlayerRestService>();
        services.TryAddSingleton<EntityDataRestService>();
        services.TryAddSingleton<ListPlayersRestService>();
        services.TryAddSingleton<ScriptInfoRestService>();
        services.TryAddSingleton<SelectPlayerRestService>();
        services.TryAddSingleton<SetTemplateEntityRestService>();
        services.TryAddSingleton<TemplateEntitiesRestService>();
        services.TryAddSingleton<WorldSegmentRestService>();

        services.TryAddSingleton<RestServiceProvider>();
    }

    private static void AddServerNetworkSystem(IServiceCollection services)
    {
        services.TryAddSingleton<ServerNetworkController>();

        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISystem, ServerNetworkSystem>());
    }
}