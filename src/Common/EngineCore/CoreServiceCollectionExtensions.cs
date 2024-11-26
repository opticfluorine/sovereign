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
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Components.Validators;
using Sovereign.EngineCore.Configuration;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details.Validators;
using Sovereign.EngineCore.Main;

namespace Sovereign.EngineCore;

/// <summary>
///     Top-level service registration manager for Sovereign.EngineCore.
/// </summary>
public static class CoreServiceCollectionExtensions
{
    /// <summary>
    ///     Adds service classes for Sovereign.EngineCore.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <returns>Service collection.</returns>
    public static IServiceCollection AddSovereignCore(this IServiceCollection services)
    {
        AddComponents(services);
        AddComponentIndexers(services);
        AddComponentValidators(services);
        AddConfiguration(services);
        AddEntities(services);
        AddEvents(services);
        AddEventValidators(services);

        return services;
    }

    /// <summary>
    ///     Adds background services for Sovereign.EngineCore.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <returns>Service collection.</returns>
    public static IServiceCollection AddSovereignCoreHostedServices(this IServiceCollection services)
    {
        services.AddHostedService<EngineService>();

        return services;
    }

    private static void AddComponents(IServiceCollection services)
    {
        services.TryAddSingleton<ComponentManager>();

        services.TryAddComponentCollection<AboveBlockComponentCollection>();
        services.TryAddComponentCollection<AdminTagCollection>();
        services.TryAddComponentCollection<AnimatedSpriteComponentCollection>();
        services.TryAddComponentCollection<BlockPositionComponentCollection>();
        services.TryAddComponentCollection<CastBlockShadowsTagCollection>();
        services.TryAddComponentCollection<DrawableTagCollection>();
        services.TryAddComponentCollection<KinematicComponentCollection>();
        services.TryAddComponentCollection<MaterialComponentCollection>();
        services.TryAddComponentCollection<MaterialModifierComponentCollection>();
        services.TryAddComponentCollection<NameComponentCollection>();
        services.TryAddComponentCollection<OrientationComponentCollection>();
        services.TryAddComponentCollection<ParentComponentCollection>();
        services.TryAddComponentCollection<PlayerCharacterTagCollection>();
        services.TryAddComponentCollection<PointLightSourceComponentCollection>();
    }

    private static void AddComponentIndexers(IServiceCollection services)
    {
        services.TryAddSingleton<EntityHierarchyIndexer>();
        services.TryAddSingleton<PositionComponentIndexer>();
        services.TryAddSingleton<MovingComponentIndexer>();
        services.TryAddSingleton<PlayerPositionEventFilter>();
        services.TryAddSingleton<PlayerNameEventFilter>();
        services.TryAddSingleton<PlayerNameComponentIndexer>();
        services.TryAddSingleton<BlockGridPositionIndexer>();
        services.TryAddSingleton<NonBlockPositionEventFilter>();
        services.TryAddSingleton<NonBlockWorldSegmentIndexer>();
        services.TryAddSingleton<BlockWorldSegmentIndexer>();
        services.TryAddSingleton<BlockTemplateNameComponentFilter>();
        services.TryAddSingleton<BlockTemplateNameComponentIndexer>();
    }

    private static void AddComponentValidators(IServiceCollection services)
    {
        services.TryAddSingleton<NameComponentValidator>();
        services.TryAddSingleton<PointLightComponentValidator>();
    }

    private static void AddConfiguration(IServiceCollection services)
    {
        services.TryAddSingleton<IWorldManagementConfiguration, WorldManagementConfiguration>();
    }

    private static void AddEntities(IServiceCollection services)
    {
        services.TryAddSingleton<EntityManager>();
        services.TryAddSingleton<EntityNotifier>();
        services.TryAddSingleton<EntityTable>();
        services.TryAddSingleton<EntityDefinitionProcessor>();
        services.TryAddSingleton<EntityDefinitionGenerator>();
    }

    private static void AddEvents(IServiceCollection services)
    {
        services.TryAddTransient<EventCommunicator>();
        services.TryAddSingleton<EventAdapterManager>();
        services.TryAddSingleton<EventDescriptions>();
        services.TryAddSingleton<IEventLoop, MainEventLoop>();
        services.TryAddSingleton<IEventSender, EventSender>();

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IEventAdapter, ConsoleEventAdapter>());
    }

    private static void AddEventValidators(IServiceCollection services)
    {
        services.TryAddSingleton<EntityDefinitionValidator>();
        services.TryAddSingleton<BlockAddEventDetailsValidator>();
        services.TryAddSingleton<ChatEventDetailsValidator>();
        services.TryAddSingleton<EntityDefinitionEventDetailsValidator>();
        services.TryAddSingleton<EntityDesyncEventDetailsValidator>();
        services.TryAddSingleton<EntityEventDetailsValidator>();
        services.TryAddSingleton<EntityGridPositionEventDetailsValidator>();
        services.TryAddSingleton<GlobalChatEventDetailsValidator>();
        services.TryAddSingleton<GridPositionEventDetailsValidator>();
        services.TryAddSingleton<LocalChatEventDetailsValidator>();
        services.TryAddSingleton<MoveEventDetailsValidator>();
        services.TryAddSingleton<NullEventDetailsValidator>();
        services.TryAddSingleton<RequestMoveEventDetailsValidator>();
        services.TryAddSingleton<SystemChatEventDetailsValidator>();
        services.TryAddSingleton<TemplateEntityDefinitionEventDetailsValidator>();
        services.TryAddSingleton<WorldSegmentSubscriptionEventDetailsValidator>();
    }
}