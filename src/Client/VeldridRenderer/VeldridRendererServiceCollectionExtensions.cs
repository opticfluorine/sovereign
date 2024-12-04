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
using Sovereign.ClientCore.Rendering;
using Sovereign.ClientCore.Rendering.Configuration;
using Sovereign.VeldridRenderer.Rendering;
using Sovereign.VeldridRenderer.Rendering.Configuration;
using Sovereign.VeldridRenderer.Rendering.Gui;
using Sovereign.VeldridRenderer.Rendering.Resources;
using Sovereign.VeldridRenderer.Rendering.Scenes.Game;

namespace Sovereign.VeldridRenderer;

/// <summary>
///     Manages service registration for Sovereign.VeldridRenderer.
/// </summary>
public static class VeldridRendererServiceCollectionExtensions
{
    /// <summary>
    ///     Adds services for Sovereign.VeldridRenderer.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <returns>Service collection.</returns>
    public static IServiceCollection AddSovereignVeldridRenderer(this IServiceCollection services)
    {
        AddImplementations(services);
        AddRenderer(services);
        AddGui(services);
        AddResources(services);
        AddGameScene(services);

        return services;
    }

    private static void AddImplementations(IServiceCollection services)
    {
        services.TryAddSingleton<IAdapterEnumerator, VeldridAdapterEnumerator>();
        services.TryAddSingleton<IRenderer, Rendering.VeldridRenderer>();
    }

    private static void AddRenderer(IServiceCollection services)
    {
        services.TryAddSingleton<VeldridDevice>();
        services.TryAddSingleton<VeldridSceneConsumer>();
    }

    private static void AddGui(IServiceCollection services)
    {
        services.TryAddSingleton<GuiPipeline>();
        services.TryAddSingleton<GuiRenderer>();
        services.TryAddSingleton<GuiResourceManager>();
    }

    private static void AddResources(IServiceCollection services)
    {
        services.TryAddSingleton<VeldridResourceManager>();
    }

    private static void AddGameScene(IServiceCollection services)
    {
        services.TryAddSingleton<GameSceneRenderer>();
        services.TryAddSingleton<GameSceneConsumer>();
        services.TryAddSingleton<GameResourceManager>();
        services.TryAddSingleton<WorldRenderer>();
        services.TryAddSingleton<WorldPipeline>();
        services.TryAddSingleton<WorldVertexConstantsUpdater>();
        services.TryAddSingleton<WorldFragmentConstantsUpdater>();
        services.TryAddSingleton<LightingShaderConstantsUpdater>();
        services.TryAddSingleton<PointLightDepthMapRenderer>();
        services.TryAddSingleton<FullPointLightMapRenderer>();
    }
}