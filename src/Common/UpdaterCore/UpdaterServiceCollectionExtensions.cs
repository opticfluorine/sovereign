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
using Sovereign.UpdaterCore.Updater;

namespace Sovereign.UpdaterCore;

/// <summary>
///     Service registration manager for Sovereign.UpdaterCore.
/// </summary>
public static class UpdaterServiceCollectionExtensions
{
    /// <summary>
    ///     Adds services from Sovereign.UpdaterCore.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <returns>Service collection.</returns>
    public static IServiceCollection AddSovereignUpdaterCore(this IServiceCollection services)
    {
        AddUpdater(services);

        return services;
    }

    private static void AddUpdater(IServiceCollection services)
    {
        services.TryAddSingleton<UpdaterResourceSetBuilder>();
        services.TryAddSingleton<UpdaterHash>();
    }
}