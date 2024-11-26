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

namespace Sovereign.EngineCore.Components;

/// <summary>
///     Extensions to IServiceCollection for component collection services.
/// </summary>
public static class ComponentsServiceCollectionExtensions
{
    /// <summary>
    ///     Registers the given component collection as a singleton service
    ///     if not already registered.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <typeparam name="T">Component collection type.</typeparam>
    public static void TryAddComponentCollection<T>(this IServiceCollection services)
        where T : class, IComponentUpdater
    {
        services.TryAddSingleton<T>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IComponentUpdater, T>());
    }
}