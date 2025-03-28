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
using Sovereign.EngineCore.Configuration;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Resources;
using Sovereign.EngineCore.Systems;
using Sovereign.EngineCore.Systems.Movement;
using Sovereign.EngineCore.Timing;
using Sovereign.Scripting.Lua;
using Sovereign.ServerCore.Components;
using Sovereign.ServerCore.Configuration;
using Sovereign.ServerCore.Entities;
using Sovereign.ServerCore.Logging;
using Sovereign.ServerCore.Resources;
using Sovereign.ServerCore.Systems.Movement;
using Sovereign.ServerCore.Systems.Persistence;
using Sovereign.ServerCore.Systems.Scripting;
using Sovereign.ServerCore.Systems.ServerChat;
using Sovereign.ServerCore.Systems.ServerManagement;
using Sovereign.ServerCore.Systems.TemplateEntity;
using Sovereign.ServerCore.Systems.WorldEdit;
using Sovereign.ServerCore.Systems.WorldManagement;
using Sovereign.ServerCore.Timing;

namespace Sovereign.ServerCore;

/// <summary>
///     Top-level service registration manager for Sovereign.ServerCore.
/// </summary>
public static class ServerServiceCollectionExtensions
{
    /// <summary>
    ///     Adds services for Sovereign.ServerCore.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <returns>Service collection.</returns>
    public static IServiceCollection AddSovereignServer(this IServiceCollection services)
    {
        AddComponents(services);
        AddServerImplementations(services);
        AddConfiguration(services);
        AddPersistenceSystem(services);
        AddServerChatSystem(services);
        AddServerManagementSystem(services);
        AddTemplateEntitySystem(services);
        AddWorldEditSystem(services);
        AddWorldManagementSystem(services);
        AddScriptingSystem(services);

        return services;
    }

    private static void AddComponents(IServiceCollection services)
    {
        services.TryAddComponentCollection<AccountComponentCollection>();
    }

    private static void AddServerImplementations(IServiceCollection services)
    {
        services.TryAddSingleton<IEntityFactory, ServerEntityFactory>();
        services.TryAddSingleton<IEngineConfiguration, ServerEngineConfiguration>();
        services.TryAddSingleton<IErrorHandler, ServerErrorHandler>();
        services.TryAddSingleton<IResourcePathBuilder, ServerResourcePathBuilder>();
        services.TryAddSingleton<ISystemTimer, ServerSystemTimer>();
        services.TryAddSingleton<IMovementNotifier, ServerMovementNotifier>();
    }

    private static void AddConfiguration(IServiceCollection services)
    {
        services.TryAddSingleton<IServerConfigurationManager, ServerConfigurationManager>();
    }

    private static void AddPersistenceSystem(IServiceCollection services)
    {
        services.TryAddSingleton<PersistenceController>();
    }

    private static void AddServerChatSystem(IServiceCollection services)
    {
        services.TryAddSingleton<ChatRouter>();
        services.TryAddSingleton<ServerChatInternalController>();
        services.TryAddSingleton<ChatHelpManager>();
        services.TryAddSingleton<ServerChatScripting>();

        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISystem, ServerChatSystem>());

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IChatProcessor, GlobalChatProcessor>());
    }

    private static void AddServerManagementSystem(IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISystem, ServerManagementSystem>());
    }

    private static void AddTemplateEntitySystem(IServiceCollection services)
    {
        services.TryAddSingleton<TemplateEntityDataGenerator>();
        services.TryAddSingleton<TemplateEntityServices>();
        services.TryAddSingleton<TemplateEntityInternalController>();
        services.TryAddSingleton<TemplateEntityManager>();

        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISystem, TemplateEntitySystem>());
    }

    private static void AddWorldEditSystem(IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISystem, WorldEditSystem>());
    }

    private static void AddWorldManagementSystem(IServiceCollection services)
    {
        services.TryAddSingleton<WorldManagementEventHandler>();
        services.TryAddSingleton<WorldSegmentRegistry>();
        services.TryAddSingleton<WorldSegmentBlockDataManager>();
        services.TryAddSingleton<WorldSegmentBlockDataGenerator>();
        services.TryAddSingleton<WorldSegmentActivationManager>();
        services.TryAddSingleton<WorldSegmentSubscriptionManager>();
        services.TryAddSingleton<WorldManagementInternalController>();
        services.TryAddSingleton<WorldSegmentSynchronizationManager>();
        services.TryAddSingleton<EntitySynchronizer>();
        services.TryAddSingleton<WorldManagementServices>();
        services.TryAddSingleton<WorldSegmentChangeMonitor>();
        services.TryAddSingleton<WorldManagementController>();

        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISystem, WorldManagementSystem>());
    }

    private static void AddScriptingSystem(IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISystem, ScriptingSystem>());
        services.TryAddSingleton<ScriptLoader>();
        services.TryAddSingleton<ScriptManager>();
        services.TryAddSingleton<ScriptingCallbackManager>();
        services.TryAddSingleton<ScriptingServices>();
        services.TryAddSingleton<ScriptingController>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ILuaLibrary, ScriptingLuaLibrary>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ILuaLibrary, ServerEntityBuilderLuaLibrary>());
    }
}