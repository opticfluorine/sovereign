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
using Sovereign.EngineCore.Configuration;
using Sovereign.EngineCore.Entities;
using Sovereign.ServerCore.Configuration;
using Sovereign.ServerCore.Entities;

namespace Sovereign.ServerCore;

/// <summary>
///     Top-level service registration manager for Sovereign.ServerCore.
/// </summary>
public static class ServerServiceCollectionExtensions
{
    public static IServiceCollection AddSovereignServer(this IServiceCollection services)
    {
        AddServerImplementations(services);

        return services;
    }

    private static void AddServerImplementations(IServiceCollection services)
    {
        services.TryAddSingleton<IEntityFactory, ServerEntityFactory>();
        services.TryAddSingleton<IEngineConfiguration, ServerEngineConfiguration>();
    }
}