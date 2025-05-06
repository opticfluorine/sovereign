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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Components.Validators;
using Sovereign.EngineCore.Configuration;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details.Validators;
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Main;
using Sovereign.EngineCore.Performance;
using Sovereign.EngineCore.Player;
using Sovereign.EngineCore.Systems;
using Sovereign.EngineCore.Systems.Block;
using Sovereign.EngineCore.Systems.Data;
using Sovereign.EngineCore.Systems.Movement;
using Sovereign.EngineCore.Systems.Performance;
using Sovereign.EngineCore.Systems.WorldManagement;
using Sovereign.EngineCore.Timing;
using Sovereign.EngineCore.World;

namespace Sovereign.EngineCore;

/// <summary>
///     Top-level service registration manager for Sovereign.EngineCore.
/// </summary>
public static class CoreServiceCollectionExtensions
{
    /// <summary>
    ///     Configures configuration bindings for Sovereign.EngineCore.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configuration">Configuration.</param>
    /// <returns></returns>
    public static IServiceCollection AddSovereignCoreOptions(this IServiceCollection services,
        IConfigurationManager configuration)
    {
        services.Configure<DebugOptions>(
            configuration.GetSection($"Sovereign:{nameof(DebugOptions)}"));

        return services;
    }

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
        AddEntities(services);
        AddEvents(services);
        AddEventValidators(services);
        AddLogging(services);
        AddMain(services);
        AddPerformance(services);
        AddPlayer(services);
        AddSystems(services);
        AddTiming(services);
        AddWorld(services);

        return services;
    }

    /// <summary>
    ///     Adds background services for Sovereign.EngineCore.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <returns>Service collection.</returns>
    public static IServiceCollection AddSovereignCoreHostedServices(this IServiceCollection services)
    {
        services.AddHostedService<SystemManager>();
        services.AddHostedService<EngineService>();
        services.AddHostedService<EventLoggerService>();

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
        services.TryAddComponentCollection<KinematicsComponentCollection>();
        services.TryAddComponentCollection<MaterialComponentCollection>();
        services.TryAddComponentCollection<MaterialModifierComponentCollection>();
        services.TryAddComponentCollection<NameComponentCollection>();
        services.TryAddComponentCollection<OrientationComponentCollection>();
        services.TryAddComponentCollection<ParentComponentCollection>();
        services.TryAddComponentCollection<PlayerCharacterTagCollection>();
        services.TryAddComponentCollection<PointLightSourceComponentCollection>();
        services.TryAddComponentCollection<PhysicsTagCollection>();
        services.TryAddComponentCollection<BoundingBoxComponentCollection>();
        services.TryAddComponentCollection<CastShadowsComponentCollection>();
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
        services.TryAddSingleton<ShadowComponentValidator>();
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
        services.TryAddSingleton<IEventLoop, MainEventLoop>();
        services.TryAddSingleton<IEventSender, EventSender>();
        services.TryAddSingleton<ConsoleEventAdapter>();
        services.TryAddSingleton<EventLogger>();
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
        services.TryAddSingleton<GenericChatEventDetailsValidator>();
        services.TryAddSingleton<TeleportNoticeEventDetailsValidator>();
    }

    private static void AddLogging(IServiceCollection services)
    {
        services.TryAddSingleton<LoggingUtil>();
    }

    private static void AddMain(IServiceCollection services)
    {
        services.TryAddTransient<FatalErrorHandler>();
        services.TryAddSingleton<CoreController>();
    }

    private static void AddPerformance(IServiceCollection services)
    {
        services.TryAddSingleton<EventLatencyPerformanceMonitor>();
    }

    private static void AddPlayer(IServiceCollection services)
    {
        services.TryAddSingleton<PlayerRoleCheck>();
    }

    private static void AddSystems(IServiceCollection services)
    {
        services.TryAddTransient<SystemExecutor>();

        AddBlockSystem(services);
        AddDataSystem(services);
        AddMovementSystem(services);
        AddPerformanceSystem(services);
        AddWorldManagementSystem(services);
    }

    private static void AddBlockSystem(IServiceCollection services)
    {
        services.TryAddSingleton<BlockEventHandler>();
        services.TryAddSingleton<BlockController>();
        services.TryAddSingleton<BlockManager>();
        services.TryAddSingleton<IBlockServices, BlockServices>();
        services.TryAddSingleton<BlockNoticeProcessor>();
        services.TryAddSingleton<BlockGridTracker>();
        services.TryAddSingleton<BlockInternalController>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISystem, BlockSystem>());
    }

    private static void AddDataSystem(IServiceCollection services)
    {
        services.TryAddSingleton<IDataServices, DataServices>();
        services.TryAddSingleton<IDataController, DataController>();
        services.TryAddSingleton<DataInternalController>();
        services.TryAddSingleton<GlobalKeyValueStore>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISystem, DataSystem>());
    }

    private static void AddMovementSystem(IServiceCollection services)
    {
        services.TryAddSingleton<MovementEventHandler>();
        services.TryAddSingleton<MovementController>();
        services.TryAddSingleton<MovementManager>();
        services.TryAddSingleton<MovementInternalController>();
        services.TryAddSingleton<CollisionMeshFactory>();
        services.TryAddSingleton<CollisionMeshManager>();
        services.TryAddSingleton<PhysicsProcessor>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISystem, MovementSystem>());
    }

    private static void AddPerformanceSystem(IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISystem, PerformanceSystem>());
    }

    private static void AddWorldManagementSystem(IServiceCollection services)
    {
        services.TryAddSingleton<WorldSegmentBlockDataLoader>();
        services.TryAddSingleton<CoreWorldManagementController>();
    }

    private static void AddTiming(IServiceCollection services)
    {
        services.TryAddSingleton<TimeManager>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ITimedAction, EventTimedAction>());
    }

    private static void AddWorld(IServiceCollection services)
    {
        services.TryAddSingleton<WorldSegmentResolver>();
    }
}