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
using Sovereign.Persistence.Accounts;
using Sovereign.Persistence.Data;
using Sovereign.Persistence.Database;
using Sovereign.Persistence.Entities;
using Sovereign.Persistence.Players;
using Sovereign.Persistence.State;
using Sovereign.Persistence.State.Trackers;
using Sovereign.Persistence.Systems.Persistence;

namespace Sovereign.Persistence;

/// <summary>
///     Manages service registration for Sovereign.Persistence.
/// </summary>
public static class PersistenceServiceCollectionExtensions
{
    /// <summary>
    ///     Adds the services for Sovereign.Persistence.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <returns>Service collection.</returns>
    public static IServiceCollection AddSovereignPersistence(this IServiceCollection services)
    {
        AddAccounts(services);
        AddData(services);
        AddDatabase(services);
        AddEntities(services);
        AddPlayers(services);
        AddState(services);
        AddPersistenceSystem(services);

        return services;
    }

    private static void AddAccounts(IServiceCollection services)
    {
        services.TryAddSingleton<PersistenceAccountServices>();
    }

    private static void AddData(IServiceCollection services)
    {
        services.TryAddSingleton<GlobalKeyValueProcessor>();
    }

    private static void AddDatabase(IServiceCollection services)
    {
        services.TryAddSingleton<PersistenceProviderManager>();
    }

    private static void AddEntities(IServiceCollection services)
    {
        services.TryAddSingleton<EntityMapper>();
        services.TryAddSingleton<EntityProcessor>();
    }

    private static void AddPlayers(IServiceCollection services)
    {
        services.TryAddSingleton<PersistencePlayerServices>();
    }

    private static void AddState(IServiceCollection services)
    {
        services.TryAddSingleton<StateManager>();
        services.TryAddSingleton<AccountStateTracker>();
        services.TryAddSingleton<TrackerManager>();
        services.TryAddSingleton<KinematicsStateTracker>();
        services.TryAddSingleton<MaterialStateTracker>();
        services.TryAddSingleton<MaterialModifierStateTracker>();
        services.TryAddSingleton<PlayerCharacterStateTracker>();
        services.TryAddSingleton<NameStateTracker>();
        services.TryAddSingleton<ParentStateTracker>();
        services.TryAddSingleton<DrawableStateTracker>();
        services.TryAddSingleton<AnimatedSpriteStateTracker>();
        services.TryAddSingleton<OrientationStateTracker>();
        services.TryAddSingleton<AdminStateTracker>();
        services.TryAddSingleton<TemplateStateTracker>();
        services.TryAddSingleton<CastBlockShadowsStateTracker>();
        services.TryAddSingleton<PointLightSourceStateTracker>();
        services.TryAddSingleton<CastShadowsStateTracker>();
        services.TryAddSingleton<PhysicsStateTracker>();
        services.TryAddSingleton<BoundingBoxStateTracker>();
        services.TryAddSingleton<EntityTypeStateTracker>();
    }

    private static void AddPersistenceSystem(IServiceCollection services)
    {
        services.TryAddSingleton<DatabaseValidator>();
        services.TryAddSingleton<PersistenceEventHandler>();
        services.TryAddSingleton<PersistenceScheduler>();
        services.TryAddSingleton<PersistenceSynchronizer>();
        services.TryAddSingleton<PersistenceEntityRetriever>();
        services.TryAddSingleton<PersistenceRangeRetriever>();
        services.TryAddSingleton<PersistenceInternalController>();
        services.TryAddSingleton<WorldSegmentPersister>();

        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISystem, PersistenceSystem>());
    }
}