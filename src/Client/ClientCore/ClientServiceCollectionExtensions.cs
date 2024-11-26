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
using Sovereign.ClientCore.Configuration;
using Sovereign.ClientCore.Entities;
using Sovereign.ClientCore.Events;
using Sovereign.ClientCore.Logging;
using Sovereign.ClientCore.Rendering;
using Sovereign.ClientCore.Resources;
using Sovereign.ClientCore.Timing;
using Sovereign.EngineCore.Configuration;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Main;
using Sovereign.EngineCore.Resources;
using Sovereign.EngineCore.Timing;

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
    }

    private static void AddMain(IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IMainLoopAction, RenderingMainLoopAction>());
    }
}