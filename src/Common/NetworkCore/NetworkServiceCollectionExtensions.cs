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
using Sovereign.EngineCore.Events;
using Sovereign.NetworkCore.Systems.Network;

namespace Sovereign.NetworkCore;

/// <summary>
///     Service registration manager for Sovereign.NetworkCore.
/// </summary>
public static class NetworkServiceCollectionExtensions
{
    public static IServiceCollection AddSovereignNetworkCore(this IServiceCollection services)
    {
        AddEvents(services);

        return services;
    }

    private static void AddEvents(IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IEventAdapter, NetworkEventAdapter>());
    }
}