// Sovereign Engine
// Copyright (c) 2026 opticfluorine
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
using Sovereign.WorldGen;

namespace Sovereign.ServerCore.Systems.WorldGeneration;

/// <summary>
///     Service registrations for the WorldGeneration system.
/// </summary>
public static class WorldGenerationServiceCollectionExtensions
{
    /// <summary>
    ///     Adds services for the WorldGeneration system.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <returns>Service collection.</returns>
    public static IServiceCollection AddWorldGenerationSystem(this IServiceCollection services)
    {
        services.TryAddSingleton<WorldGenerationSystem>();
        services.TryAddSingleton<WorldGenerationServices>();
        services.TryAddSingleton<WorldGenerationController>();
        services.TryAddSingleton<WorldGenChatCommandHandler>();
        services.TryAddSingleton<ProfileLoader>();
        services.TryAddSingleton<ProfileValidator>();

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<ISystem, WorldGenerationSystem>(s =>
                s.GetRequiredService<WorldGenerationSystem>()));

        return services;
    }
}
