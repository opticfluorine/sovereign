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
using Sovereign.ClientCore.Components;
using Sovereign.ClientCore.Components.Indexers;
using Sovereign.ClientCore.Configuration;
using Sovereign.ClientCore.Entities;
using Sovereign.ClientCore.Events;
using Sovereign.ClientCore.Logging;
using Sovereign.ClientCore.Network;
using Sovereign.ClientCore.Network.Infrastructure;
using Sovereign.ClientCore.Network.Pipeline.Inbound;
using Sovereign.ClientCore.Network.Pipeline.Outbound;
using Sovereign.ClientCore.Network.Rest;
using Sovereign.ClientCore.Rendering;
using Sovereign.ClientCore.Rendering.Components.Indexers;
using Sovereign.ClientCore.Rendering.Configuration;
using Sovereign.ClientCore.Rendering.Display;
using Sovereign.ClientCore.Rendering.Gui;
using Sovereign.ClientCore.Rendering.Materials;
using Sovereign.ClientCore.Resources;
using Sovereign.ClientCore.Systems.Network;
using Sovereign.ClientCore.Timing;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Configuration;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Main;
using Sovereign.EngineCore.Resources;
using Sovereign.EngineCore.Timing;
using Sovereign.NetworkCore.Network.Infrastructure;
using Sovereign.NetworkCore.Network.Pipeline.Inbound;
using Sovereign.NetworkCore.Network.Pipeline.Outbound;
using Sovereign.NetworkCore.Systems.Network;

namespace Sovereign.ClientCore;

/// <summary>
///     Manages service registration for Sovereign.ClientCore.
/// </summary>
public static class ClientServiceCollectionExtensions
{
    /// <summary>
    ///     Adds Sovereign.ClientCore classes to the service collection.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <returns>Service collection.</returns>
    public static IServiceCollection AddSovereignClient(this IServiceCollection services)
    {
        AddClientImplementations(services);
        AddEvents(services);
        AddMain(services);
        AddInboundPipeline(services);
        AddComponents(services);
        AddConfiguration(services);
        AddEntities(services);
        AddClientNetwork(services);
        AddRendering(services);
        AddGui(services);
        AddResources(services);

        return services;
    }

    private static void AddEvents(IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IEventAdapter, SDLEventAdapter>());
    }

    private static void AddClientImplementations(IServiceCollection services)
    {
        services.TryAddSingleton<IErrorHandler, ErrorHandler>();
        services.TryAddSingleton<IEntityFactory, ClientEntityFactory>();
        services.TryAddSingleton<IEngineConfiguration, ClientEngineConfiguration>();
        services.TryAddSingleton<IResourcePathBuilder, ClientResourcePathBuilder>();
        services.TryAddSingleton<ISystemTimer, SDLSystemTimer>();
        services.TryAddSingleton<INetworkManager, ClientNetworkManager>();
        services
            .TryAddSingleton<IConnectionMappingOutboundPipelineStage, ClientConnectionMappingOutboundPipelineStage>();
        services.TryAddSingleton<IOutboundEventSet, ClientOutboundEventSet>();
    }

    private static void AddMain(IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IMainLoopAction, RenderingMainLoopAction>());
    }

    private static void AddInboundPipeline(IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor
            .Singleton<IInboundPipelineStage, ClientAllowedEventsInboundPipelineStage>());
    }

    private static void AddComponents(IServiceCollection services)
    {
        services.TryAddComponentCollection<AnimationPhaseComponentCollection>();

        services.TryAddSingleton<BlockTemplateEntityFilter>();
        services.TryAddSingleton<BlockTemplateEntityIndexer>();
    }

    private static void AddConfiguration(IServiceCollection services)
    {
        services.TryAddSingleton<ClientConfigurationManager>();
    }

    private static void AddEntities(IServiceCollection services)
    {
        services.TryAddSingleton<TemplateEntityDataLoader>();
    }

    private static void AddClientNetwork(IServiceCollection services)
    {
        services.TryAddSingleton<RestClient>();
        services.TryAddSingleton<AuthenticationClient>();
        services.TryAddSingleton<PlayerManagementClient>();
        services.TryAddSingleton<RegistrationClient>();
        services.TryAddSingleton<TemplateEntityDataClient>();
        services.TryAddSingleton<WorldSegmentDataClient>();
        services.TryAddSingleton<ClientNetworkInternalController>();
        services.TryAddSingleton<INetworkClient, NetworkClient>();
    }

    private static void AddRendering(IServiceCollection services)
    {
        services.TryAddSingleton<DrawablePositionEventFilter>();
        services.TryAddSingleton<DrawablePositionComponentIndexer>();
        services.TryAddSingleton<AdapterSelector>();
        services.TryAddSingleton<DisplayModeSelector>();
        services.TryAddSingleton<DisplayViewport>();
        services.TryAddSingleton<IDisplayModeEnumerator, SDLDisplayModeEnumerator>();
        services.TryAddSingleton<MainDisplay>();
    }

    private static void AddGui(IServiceCollection services)
    {
        services.TryAddSingleton<CommonGuiManager>();
        services.TryAddSingleton<GuiFontAtlas>();
        services.TryAddSingleton<GuiExtensions>();
        services.TryAddSingleton<GuiTextureMapper>();
        services.TryAddSingleton<GuiComponentEditors>();
    }

    private static void AddResources(IServiceCollection services)
    {
        services.TryAddSingleton<MaterialManager>();
        services.TryAddSingleton<MaterialDefinitionsValidator>();
        services.TryAddSingleton<MaterialDefinitionsLoader>();
    }
}