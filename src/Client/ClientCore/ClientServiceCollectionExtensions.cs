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
using Sovereign.ClientCore.Components;
using Sovereign.ClientCore.Components.Indexers;
using Sovereign.ClientCore.Configuration;
using Sovereign.ClientCore.Entities;
using Sovereign.ClientCore.Events;
using Sovereign.ClientCore.Logging;
using Sovereign.ClientCore.Network;
using Sovereign.ClientCore.Network.Infrastructure;
using Sovereign.ClientCore.Network.Pipeline.Inbound;
using Sovereign.ClientCore.Network.Pipeline.Outbound;
using Sovereign.ClientCore.Network.Rest;
using Sovereign.ClientCore.Rendering;
using Sovereign.ClientCore.Rendering.Components.Indexers;
using Sovereign.ClientCore.Rendering.Configuration;
using Sovereign.ClientCore.Rendering.Display;
using Sovereign.ClientCore.Rendering.Gui;
using Sovereign.ClientCore.Rendering.Gui.Debug;
using Sovereign.ClientCore.Rendering.Materials;
using Sovereign.ClientCore.Rendering.Scenes;
using Sovereign.ClientCore.Rendering.Scenes.Game;
using Sovereign.ClientCore.Rendering.Scenes.Game.Gui;
using Sovereign.ClientCore.Rendering.Scenes.Game.Gui.Debug;
using Sovereign.ClientCore.Rendering.Scenes.Game.Gui.ResourceEditor;
using Sovereign.ClientCore.Rendering.Scenes.Game.Gui.TemplateEditor;
using Sovereign.ClientCore.Rendering.Scenes.Game.Gui.WorldEditor;
using Sovereign.ClientCore.Rendering.Scenes.Game.World;
using Sovereign.ClientCore.Rendering.Scenes.MainMenu;
using Sovereign.ClientCore.Rendering.Scenes.Update;
using Sovereign.ClientCore.Rendering.Sprites;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.ClientCore.Rendering.Sprites.Atlas;
using Sovereign.ClientCore.Rendering.Sprites.TileSprites;
using Sovereign.ClientCore.Resources;
using Sovereign.ClientCore.Systems.Block.Caches;
using Sovereign.ClientCore.Systems.Camera;
using Sovereign.ClientCore.Systems.ClientChat;
using Sovereign.ClientCore.Systems.ClientNetwork;
using Sovereign.ClientCore.Systems.ClientState;
using Sovereign.ClientCore.Systems.ClientWorldEdit;
using Sovereign.ClientCore.Systems.Dialogue;
using Sovereign.ClientCore.Systems.EntityAnimation;
using Sovereign.ClientCore.Systems.EntitySynchronization;
using Sovereign.ClientCore.Systems.Input;
using Sovereign.ClientCore.Systems.Movement;
using Sovereign.ClientCore.Systems.Network;
using Sovereign.ClientCore.Systems.Perspective;
using Sovereign.ClientCore.Timing;
using Sovereign.ClientCore.Updater;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Configuration;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Main;
using Sovereign.EngineCore.Resources;
using Sovereign.EngineCore.Systems;
using Sovereign.EngineCore.Systems.Movement;
using Sovereign.EngineCore.Timing;
using Sovereign.NetworkCore.Network.Infrastructure;
using Sovereign.NetworkCore.Network.Pipeline.Inbound;
using Sovereign.NetworkCore.Network.Pipeline.Outbound;
using Sovereign.NetworkCore.Systems.Network;

namespace Sovereign.ClientCore;

/// <summary>
///     Manages service registration for Sovereign.ClientCore.
/// </summary>
public static class ClientServiceCollectionExtensions
{
    /// <summary>
    ///     Binds configuration options for the client.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configuration">Configuration.</param>
    /// <returns>Service collection.</returns>
    public static IServiceCollection AddSovereignClientOptions(this IServiceCollection services,
        IConfigurationManager configuration)
    {
        services.Configure<ConnectionOptions>(
            configuration.GetSection($"Sovereign:{nameof(ConnectionOptions)}"));
        services.Configure<AutoUpdaterOptions>(
            configuration.GetSection($"Sovereign:{nameof(AutoUpdaterOptions)}"));
        services.Configure<DisplayOptions>(
            configuration.GetSection($"Sovereign:{nameof(DisplayOptions)}"));
        services.Configure<RendererOptions>(
            configuration.GetSection($"Sovereign:{nameof(RendererOptions)}"));
        services.Configure<DayNightOptions>(
            configuration.GetSection($"Sovereign:{nameof(DayNightOptions)}"));

        return services;
    }

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
        AddInboundPipeline(services);
        AddComponents(services);
        AddEntities(services);
        AddClientNetwork(services);
        AddRendering(services);
        AddGui(services);
        AddResources(services);
        AddScenes(services);
        AddSprites(services);
        AddSystems(services);
        AddUpdater(services);

        return services;
    }

    private static void AddEvents(IServiceCollection services)
    {
        services.TryAddSingleton<SdlEventAdapter>();
    }

    private static void AddClientImplementations(IServiceCollection services)
    {
        services.TryAddSingleton<IErrorHandler, ErrorHandler>();
        services.TryAddSingleton<IEntityFactory, ClientEntityFactory>();
        services.TryAddSingleton<IEngineConfiguration, ClientEngineConfiguration>();
        services.TryAddSingleton<IResourcePathBuilder, ClientResourcePathBuilder>();
        services.TryAddSingleton<ISystemTimer, SdlSystemTimer>();
        services.TryAddSingleton<ClientNetworkManager>();
        services.TryAddSingleton<INetworkManager>(x => x.GetService<ClientNetworkManager>()!);
        services
            .TryAddSingleton<IConnectionMappingOutboundPipelineStage, ClientConnectionMappingOutboundPipelineStage>();
        services.TryAddSingleton<IOutboundEventSet, ClientOutboundEventSet>();
        services.TryAddSingleton<IMovementNotifier, ClientMovementNotifier>();
    }

    private static void AddMain(IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IMainLoopAction, RenderingMainLoopAction>());
    }

    private static void AddInboundPipeline(IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor
            .Singleton<IInboundPipelineStage, ClientAllowedEventsInboundPipelineStage>());
    }

    private static void AddComponents(IServiceCollection services)
    {
        services.TryAddComponentCollection<AnimationPhaseComponentCollection>();

        services.TryAddSingleton<BlockTemplateEntityFilter>();
        services.TryAddSingleton<BlockTemplateEntityIndexer>();
        services.TryAddSingleton<NpcTemplateEntityFilter>();
        services.TryAddSingleton<NpcTemplateEntityIndexer>();
    }

    private static void AddEntities(IServiceCollection services)
    {
        services.TryAddSingleton<TemplateEntityDataLoader>();
    }

    private static void AddClientNetwork(IServiceCollection services)
    {
        services.TryAddSingleton<RestClient>();
        services.TryAddSingleton<AuthenticationClient>();
        services.TryAddSingleton<EntityDataClient>();
        services.TryAddSingleton<PlayerManagementClient>();
        services.TryAddSingleton<RegistrationClient>();
        services.TryAddSingleton<ScriptInfoClient>();
        services.TryAddSingleton<TemplateEntityDataClient>();
        services.TryAddSingleton<WorldSegmentDataClient>();
        services.TryAddSingleton<ClientNetworkInternalController>();
        services.TryAddSingleton<INetworkClient, NetworkClient>();
    }

    private static void AddRendering(IServiceCollection services)
    {
        services.TryAddSingleton<DrawablePositionEventFilter>();
        services.TryAddSingleton<DrawablePositionComponentIndexer>();
        services.TryAddSingleton<AdapterSelector>();
        services.TryAddSingleton<DisplayModeSelector>();
        services.TryAddSingleton<DisplayViewport>();
        services.TryAddSingleton<IDisplayModeEnumerator, SDLDisplayModeEnumerator>();
        services.TryAddSingleton<MainDisplay>();
        services.TryAddSingleton<RenderingManager>();
        services.TryAddSingleton<RenderingResourceManager>();
        services.TryAddSingleton<DrawableLookup>();
    }

    private static void AddGui(IServiceCollection services)
    {
        services.TryAddSingleton<CommonGuiManager>();
        services.TryAddSingleton<GuiFontAtlas>();
        services.TryAddSingleton<GuiExtensions>();
        services.TryAddSingleton<GuiTextureMapper>();
        services.TryAddSingleton<GuiComponentEditors>();
        services.TryAddSingleton<OverlayGui>();
        services.TryAddSingleton<NetworkDebugGui>();
        services.TryAddSingleton<DialogueGui>();
    }

    private static void AddResources(IServiceCollection services)
    {
        services.TryAddSingleton<MaterialManager>();
        services.TryAddSingleton<MaterialDefinitionsValidator>();
        services.TryAddSingleton<MaterialDefinitionsLoader>();
    }

    private static void AddScenes(IServiceCollection services)
    {
        services.TryAddSingleton<SceneManager>();
        services.TryAddSingleton<RenderCamera>();

        services.TryAddSingleton<GameScene>();
        services.TryAddSingleton<GlobalLightTable>();
        services.TryAddSingleton<InGameMenuGui>();
        services.TryAddSingleton<ChatGui>();
        services.TryAddSingleton<GameGui>();
        services.TryAddSingleton<PlayerDebugGui>();
        services.TryAddSingleton<EntityDebugGui>();
        services.TryAddSingleton<ResourceEditorGui>();
        services.TryAddSingleton<SpriteEditorTab>();
        services.TryAddSingleton<AnimatedSpriteEditorTab>();
        services.TryAddSingleton<TileSpriteEditorTab>();
        services.TryAddSingleton<MaterialEditorTab>();
        services.TryAddSingleton<SpriteSelectorPopup>();
        services.TryAddSingleton<AnimatedSpriteSelectorPopup>();
        services.TryAddSingleton<TileSpriteSelectorPopup>();
        services.TryAddSingleton<MaterialSelectorPopup>();
        services.TryAddSingleton<GenerateAnimatedSpritesPopup>();
        services.TryAddSingleton<SpritesheetSelector>();
        services.TryAddSingleton<TemplateEditorGui>();
        services.TryAddSingleton<BlockTemplateEditorTab>();
        services.TryAddSingleton<NpcTemplateEditorTab>();
        services.TryAddSingleton<BasicInformationControlGroup>();
        services.TryAddSingleton<AppearanceControlGroup>();
        services.TryAddSingleton<BehaviorControlGroup>();
        services.TryAddSingleton<EntityDataControlGroup>();
        services.TryAddSingleton<TemplateEditorInternalController>();
        services.TryAddSingleton<WorldEditorGui>();
        services.TryAddSingleton<BlockWorldEditorGui>();
        services.TryAddSingleton<NpcWorldEditorGui>();
        services.TryAddSingleton<WorldVertexSequencer>();
        services.TryAddSingleton<WorldLayerGrouper>();
        services.TryAddSingleton<WorldLayerVertexSequencer>();
        services.TryAddSingleton<WorldEntityRetriever>();
        services.TryAddSingleton<WorldRangeSelector>();
        services.TryAddSingleton<WorldSpriteSequencer>();
        services.TryAddSingleton<LightSourceTable>();
        services.TryAddSingleton<NonBlockShadowPlanner>();

        services.TryAddSingleton<MainMenuScene>();
        services.TryAddSingleton<StartupGui>();
        services.TryAddSingleton<LoginGui>();
        services.TryAddSingleton<RegistrationGui>();
        services.TryAddSingleton<PlayerSelectionGui>();
        services.TryAddSingleton<CreatePlayerGui>();
        services.TryAddSingleton<ConnectionLostGui>();

        services.TryAddSingleton<UpdateScene>();
        services.TryAddSingleton<UpdaterGui>();
    }

    private static void AddSprites(IServiceCollection services)
    {
        services.TryAddSingleton<SurfaceLoader>();
        services.TryAddSingleton<SpriteSheetFactory>();
        services.TryAddSingleton<SpriteSheetManager>();
        services.TryAddSingleton<SpriteSheetDefinitionLoader>();
        services.TryAddSingleton<SpriteSheetDefinitionValidator>();
        services.TryAddSingleton<SpriteManager>();
        services.TryAddSingleton<SpriteDefinitionsLoader>();
        services.TryAddSingleton<SpriteDefinitionsValidator>();
        services.TryAddSingleton<SpriteDefinitionsGenerator>();

        services.TryAddSingleton<AnimatedSpriteDefinitionsLoader>();
        services.TryAddSingleton<AnimatedSpriteDefinitionsValidator>();
        services.TryAddSingleton<AnimatedSpriteManager>();

        services.TryAddSingleton<TextureAtlasManager>();
        services.TryAddSingleton<AtlasMap>();

        services.TryAddSingleton<TileSpriteDefinitionsLoader>();
        services.TryAddSingleton<TileSpriteDefinitionsValidator>();
        services.TryAddSingleton<TileSpriteManager>();
    }

    private static void AddSystems(IServiceCollection services)
    {
        services.TryAddSingleton<IBlockAnimatedSpriteCache, BlockAnimatedSpriteCache>();

        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISystem, CameraSystem>());
        services.TryAddSingleton<CameraManager>();
        services.TryAddSingleton<CameraEventHandler>();
        services.TryAddSingleton<CameraServices>();

        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISystem, ClientChatSystem>());
        services.TryAddSingleton<ClientChatController>();
        services.TryAddSingleton<ChatHistoryManager>();
        services.TryAddSingleton<ClientChatServices>();

        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISystem, ClientNetworkSystem>());
        services.TryAddSingleton<ClientNetworkController>();
        services.TryAddSingleton<ClientNetworkEventHandler>();
        services.TryAddSingleton<ClientWorldSegmentSubscriptionManager>();

        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISystem, ClientStateSystem>());
        services.TryAddSingleton<ClientStateMachine>();
        services.TryAddSingleton<ClientStateServices>();
        services.TryAddSingleton<WorldEntryDetector>();
        services.TryAddSingleton<ClientStateController>();
        services.TryAddSingleton<ClientStateFlagManager>();
        services.TryAddSingleton<PlayerStateManager>();
        services.TryAddSingleton<MainMenuStateMachine>();
        services.TryAddSingleton<AutoUpdaterEndDetector>();

        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISystem, ClientWorldEditSystem>());
        services.TryAddSingleton<ClientWorldEditState>();
        services.TryAddSingleton<ClientWorldEditServices>();
        services.TryAddSingleton<ClientWorldEditController>();
        services.TryAddSingleton<ClientWorldEditInputHandler>();
        services.TryAddSingleton<ClientWorldEditInternalController>();
        services.TryAddSingleton<BlockToolHandler>();
        services.TryAddSingleton<NpcToolHandler>();
        services.TryAddSingleton<ItemToolHandler>();

        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISystem, DialogueSystem>());
        services.TryAddSingleton<DialogueQueue>();
        services.TryAddSingleton<IDialogueServices, DialogueServices>();

        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISystem, EntityAnimationSystem>());
        services.TryAddSingleton<AnimationPhaseStateMachine>();

        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISystem, EntitySynchronizationSystem>());
        services.TryAddSingleton<ClientEntityUnloader>();

        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISystem, InputSystem>());
        services.TryAddSingleton<KeyboardEventHandler>();
        services.TryAddSingleton<KeyboardState>();
        services.TryAddSingleton<MouseEventHandler>();
        services.TryAddSingleton<MouseState>();
        services.TryAddSingleton<PlayerInputMovementMapper>();
        services.TryAddSingleton<GlobalKeyboardShortcuts>();
        services.TryAddSingleton<InGameKeyboardShortcuts>();
        services.TryAddSingleton<InputServices>();
        services.TryAddSingleton<InputInternalController>();
        services.TryAddSingleton<NullInputHandler>();
        services.TryAddSingleton<InGameInputHandler>();
        services.TryAddSingleton<PlayerInteractionHandler>();

        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISystem, PerspectiveSystem>());
        services.TryAddSingleton<PerspectiveLineManager>();
        services.TryAddSingleton<IPerspectiveServices, PerspectiveServices>();
        services.TryAddSingleton<IPerspectiveController, PerspectiveController>();
        services.TryAddSingleton<OverheadTransparency>();
        services.TryAddSingleton<OverheadBlockGraphManager>();
    }

    private static void AddUpdater(IServiceCollection services)
    {
        services.TryAddSingleton<AutoUpdater>();
    }
}