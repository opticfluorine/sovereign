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

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SDL2;
using Sovereign.ClientCore.Configuration;
using Sovereign.ClientCore.Systems.ClientState;
using Sovereign.ClientCore.Systems.Inventory;

namespace Sovereign.ClientCore.Systems.Input;

/// <summary>
///     Parses the configured client keyboard bindings into SDL keycodes.
/// </summary>
public class Keybindings
{
    private const string SectionName = "KeybindingsOptions";

    /// <summary>
    ///     Maps each assigned keycode to the configuration path of the first binding that claimed it,
    ///     used to warn about duplicate assignments.
    /// </summary>
    private readonly Dictionary<SDL.SDL_Keycode, string> keyOwners = new();

    private readonly ILogger<Keybindings> logger;

    /// <summary>
    ///     Key bound to the ShowNetworkDebug global shortcut.
    /// </summary>
    public SDL.SDL_Keycode ShowNetworkDebug { get; }

    /// <summary>
    ///     Key bound to the ShowImGuiDebugLog global shortcut.
    /// </summary>
    public SDL.SDL_Keycode ShowImGuiDebugLog { get; }

    /// <summary>
    ///     Key bound to the ShowImGuiDemo global shortcut.
    /// </summary>
    public SDL.SDL_Keycode ShowImGuiDemo { get; }

    /// <summary>
    ///     Key bound to the ShowImGuiIdStackTool global shortcut.
    /// </summary>
    public SDL.SDL_Keycode ShowImGuiIdStackTool { get; }

    /// <summary>
    ///     Key bound to the ShowImGuiMetrics global shortcut.
    /// </summary>
    public SDL.SDL_Keycode ShowImGuiMetrics { get; }

    /// <summary>
    ///     Key bound to the DebugFrame global shortcut.
    /// </summary>
    public SDL.SDL_Keycode DebugFrame { get; }

    /// <summary>
    ///     Key bound to the ShowResourceEditor global shortcut.
    /// </summary>
    public SDL.SDL_Keycode ShowResourceEditor { get; }

    /// <summary>
    ///     Key bound to the ShowInventory in-game shortcut.
    /// </summary>
    public SDL.SDL_Keycode ShowInventory { get; }

    /// <summary>
    ///     Key bound to the ShowChat in-game shortcut.
    /// </summary>
    public SDL.SDL_Keycode ShowChat { get; }

    /// <summary>
    ///     Key bound to the ShowInGameMenu in-game shortcut.
    /// </summary>
    public SDL.SDL_Keycode ShowInGameMenu { get; }

    /// <summary>
    ///     Key bound to the ShowPlayerDebug in-game shortcut.
    /// </summary>
    public SDL.SDL_Keycode ShowPlayerDebug { get; }

    /// <summary>
    ///     Key bound to the ShowEntityDebug in-game shortcut.
    /// </summary>
    public SDL.SDL_Keycode ShowEntityDebug { get; }

    /// <summary>
    ///     Key bound to the ShowRendererDebug in-game shortcut.
    /// </summary>
    public SDL.SDL_Keycode ShowRendererDebug { get; }

    /// <summary>
    ///     Key bound to the ShowTemplateEntityEditor in-game shortcut.
    /// </summary>
    public SDL.SDL_Keycode ShowTemplateEntityEditor { get; }

    /// <summary>
    ///     Key bound to the world edit mode in-game shortcut.
    /// </summary>
    public SDL.SDL_Keycode ToggleWorldEditMode { get; }

    /// <summary>
    ///     Key bound to jumping.
    /// </summary>
    public SDL.SDL_Keycode Jump { get; }

    /// <summary>
    ///     Key bound to interaction.
    /// </summary>
    public SDL.SDL_Keycode Interact { get; }

    /// <summary>
    ///     Key bound to picking up the item under the player.
    /// </summary>
    public SDL.SDL_Keycode PickUpItem { get; }

    /// <summary>
    ///     Keys bound to upward movement; any key in the list activates the action.
    /// </summary>
    public IReadOnlyList<SDL.SDL_Keycode> MoveUp { get; }

    /// <summary>
    ///     Keys bound to downward movement; any key in the list activates the action.
    /// </summary>
    public IReadOnlyList<SDL.SDL_Keycode> MoveDown { get; }

    /// <summary>
    ///     Keys bound to leftward movement; any key in the list activates the action.
    /// </summary>
    public IReadOnlyList<SDL.SDL_Keycode> MoveLeft { get; }

    /// <summary>
    ///     Keys bound to rightward movement; any key in the list activates the action.
    /// </summary>
    public IReadOnlyList<SDL.SDL_Keycode> MoveRight { get; }

    /// <summary>
    ///     Keys that, while held, cause a click on an inventory item to drop the item.
    /// </summary>
    public IReadOnlyList<SDL.SDL_Keycode> DropItemModifier { get; }

    /// <summary>
    ///     Keys that, while held, cause the mouse wheel to vary the world editor Z offset.
    /// </summary>
    public IReadOnlyList<SDL.SDL_Keycode> ScrollZOffsetModifier { get; }

    /// <summary>
    ///     Keys that, while held, cause the mouse wheel to vary the world editor pen width.
    /// </summary>
    public IReadOnlyList<SDL.SDL_Keycode> ScrollPenWidthModifier { get; }

    /// <summary>
    ///     Hotbar slot keys in slot order; the key at index i selects hotbar slot i.
    /// </summary>
    public IReadOnlyList<SDL.SDL_Keycode> HotbarSlotKeys { get; }

    public Keybindings(IOptions<KeybindingsOptions> options, ILogger<Keybindings> logger)
    {
        this.logger = logger;

        var globalShortcuts = options.Value.GlobalShortcuts;
        var inGameShortcuts = options.Value.InGameShortcuts;
        var inGameActions = options.Value.InGameActions;

        ShowNetworkDebug = BindSingle(OptionPath("GlobalShortcuts", nameof(globalShortcuts.ShowNetworkDebug)),
            globalShortcuts.ShowNetworkDebug);
        ShowImGuiDebugLog = BindSingle(OptionPath("GlobalShortcuts", nameof(globalShortcuts.ShowImGuiDebugLog)),
            globalShortcuts.ShowImGuiDebugLog);
        ShowImGuiDemo = BindSingle(OptionPath("GlobalShortcuts", nameof(globalShortcuts.ShowImGuiDemo)),
            globalShortcuts.ShowImGuiDemo);
        ShowImGuiIdStackTool = BindSingle(OptionPath("GlobalShortcuts", nameof(globalShortcuts.ShowImGuiIdStackTool)),
            globalShortcuts.ShowImGuiIdStackTool);
        ShowImGuiMetrics = BindSingle(OptionPath("GlobalShortcuts", nameof(globalShortcuts.ShowImGuiMetrics)),
            globalShortcuts.ShowImGuiMetrics);
        DebugFrame = BindSingle(OptionPath("GlobalShortcuts", nameof(globalShortcuts.DebugFrame)),
            globalShortcuts.DebugFrame);
        ShowResourceEditor = BindSingle(OptionPath("GlobalShortcuts", nameof(globalShortcuts.ShowResourceEditor)),
            globalShortcuts.ShowResourceEditor);

        ShowInventory = BindSingle(OptionPath("InGameShortcuts", nameof(inGameShortcuts.ShowInventory)),
            inGameShortcuts.ShowInventory);
        ShowChat = BindSingle(OptionPath("InGameShortcuts", nameof(inGameShortcuts.ShowChat)),
            inGameShortcuts.ShowChat);
        ShowInGameMenu = BindSingle(OptionPath("InGameShortcuts", nameof(inGameShortcuts.ShowInGameMenu)),
            inGameShortcuts.ShowInGameMenu);
        ShowPlayerDebug = BindSingle(OptionPath("InGameShortcuts", nameof(inGameShortcuts.ShowPlayerDebug)),
            inGameShortcuts.ShowPlayerDebug);
        ShowEntityDebug = BindSingle(OptionPath("InGameShortcuts", nameof(inGameShortcuts.ShowEntityDebug)),
            inGameShortcuts.ShowEntityDebug);
        ShowRendererDebug = BindSingle(OptionPath("InGameShortcuts", nameof(inGameShortcuts.ShowRendererDebug)),
            inGameShortcuts.ShowRendererDebug);
        ShowTemplateEntityEditor =
            BindSingle(OptionPath("InGameShortcuts", nameof(inGameShortcuts.ShowTemplateEntityEditor)),
                inGameShortcuts.ShowTemplateEntityEditor);
        ToggleWorldEditMode = BindSingle(OptionPath("InGameShortcuts", nameof(inGameShortcuts.ToggleWorldEditMode)),
            inGameShortcuts.ToggleWorldEditMode);

        MoveUp = BindList(OptionPath("InGameActions", nameof(inGameActions.MoveUp)), inGameActions.MoveUp);
        MoveDown = BindList(OptionPath("InGameActions", nameof(inGameActions.MoveDown)), inGameActions.MoveDown);
        MoveLeft = BindList(OptionPath("InGameActions", nameof(inGameActions.MoveLeft)), inGameActions.MoveLeft);
        MoveRight = BindList(OptionPath("InGameActions", nameof(inGameActions.MoveRight)), inGameActions.MoveRight);
        Jump = BindSingle(OptionPath("InGameActions", nameof(inGameActions.Jump)), inGameActions.Jump);
        Interact = BindSingle(OptionPath("InGameActions", nameof(inGameActions.Interact)), inGameActions.Interact);
        PickUpItem = BindSingle(OptionPath("InGameActions", nameof(inGameActions.PickUpItem)),
            inGameActions.PickUpItem);
        DropItemModifier = BindList(OptionPath("InGameActions", nameof(inGameActions.DropItemModifier)),
            inGameActions.DropItemModifier);
        ScrollZOffsetModifier = BindList(OptionPath("InGameActions", nameof(inGameActions.ScrollZOffsetModifier)),
            inGameActions.ScrollZOffsetModifier);
        ScrollPenWidthModifier = BindList(OptionPath("InGameActions", nameof(inGameActions.ScrollPenWidthModifier)),
            inGameActions.ScrollPenWidthModifier);
        HotbarSlotKeys = ParseHotbarSlots(inGameActions.HotbarSlots);
    }

    /// <summary>
    ///     Gets the key bound to the given global shortcut state flag.
    /// </summary>
    /// <param name="flag">Global shortcut state flag.</param>
    /// <returns>Bound key, or SDLK_UNKNOWN if the flag has no binding.</returns>
    public SDL.SDL_Keycode GlobalShortcutKey(ClientStateFlag flag)
    {
        return flag switch
        {
            ClientStateFlag.ShowNetworkDebug => ShowNetworkDebug,
            ClientStateFlag.ShowImGuiDebugLog => ShowImGuiDebugLog,
            ClientStateFlag.ShowImGuiDemo => ShowImGuiDemo,
            ClientStateFlag.ShowImGuiIdStackTool => ShowImGuiIdStackTool,
            ClientStateFlag.ShowImGuiMetrics => ShowImGuiMetrics,
            ClientStateFlag.DebugFrame => DebugFrame,
            ClientStateFlag.ShowResourceEditor => ShowResourceEditor,
            _ => SDL.SDL_Keycode.SDLK_UNKNOWN
        };
    }

    /// <summary>
    ///     Gets the key bound to the given in-game shortcut state flag.
    /// </summary>
    /// <param name="flag">In-game shortcut state flag.</param>
    /// <returns>Bound key, or SDLK_UNKNOWN if the flag has no binding.</returns>
    public SDL.SDL_Keycode InGameShortcutKey(ClientStateFlag flag)
    {
        return flag switch
        {
            ClientStateFlag.ShowInventory => ShowInventory,
            ClientStateFlag.ShowChat => ShowChat,
            ClientStateFlag.ShowInGameMenu => ShowInGameMenu,
            ClientStateFlag.ShowPlayerDebug => ShowPlayerDebug,
            ClientStateFlag.ShowEntityDebug => ShowEntityDebug,
            ClientStateFlag.ShowRendererDebug => ShowRendererDebug,
            ClientStateFlag.ShowTemplateEntityEditor => ShowTemplateEntityEditor,
            ClientStateFlag.WorldEditMode => ToggleWorldEditMode,
            _ => SDL.SDL_Keycode.SDLK_UNKNOWN
        };
    }

    /// <summary>
    ///     Parses and registers a single-key binding.
    /// </summary>
    /// <param name="optionPath">Configuration path of the option, for log messages.</param>
    /// <param name="keyName">Configured SDL key name.</param>
    /// <returns>Parsed keycode, possibly SDLK_UNKNOWN if the name is unrecognized.</returns>
    private SDL.SDL_Keycode BindSingle(string optionPath, string keyName)
    {
        var key = ParseKey(optionPath, keyName);
        NoteAssignment(optionPath, keyName, key);
        return key;
    }

    /// <summary>
    ///     Parses and registers a list-key binding.
    /// </summary>
    /// <param name="optionPath">Configuration path of the option, for log messages.</param>
    /// <param name="keyNames">Configured SDL key names.</param>
    /// <returns>Parsed keycodes, possibly including SDLK_UNKNOWN for unrecognized names.</returns>
    private IReadOnlyList<SDL.SDL_Keycode> BindList(string optionPath, List<string> keyNames)
    {
        var keys = new List<SDL.SDL_Keycode>(keyNames.Count);
        foreach (var keyName in keyNames)
        {
            var key = ParseKey(optionPath, keyName);
            NoteAssignment(optionPath, keyName, key);
            keys.Add(key);
        }

        return keys;
    }

    /// <summary>
    ///     Parses the hotbar slot keys, warning if the configured list does not match the hotbar slot count.
    /// </summary>
    /// <param name="keyNames">Configured SDL key names in slot order.</param>
    /// <returns>Parsed keycodes for the bound slots, in slot order.</returns>
    private IReadOnlyList<SDL.SDL_Keycode> ParseHotbarSlots(List<string> keyNames)
    {
        var optionPath = OptionPath("InGameActions", nameof(KeybindingsOptions.InGameActions.HotbarSlots));
        if (keyNames.Count != ClientInventoryConstants.HotbarSlotCount)
            logger.LogWarning(
                "{OptionPath} has {Count} entries but the hotbar has {SlotCount} slots; only the first {BoundCount} entries will be bound.",
                optionPath, keyNames.Count, ClientInventoryConstants.HotbarSlotCount,
                Math.Min(keyNames.Count, ClientInventoryConstants.HotbarSlotCount));

        return BindList(optionPath, keyNames.GetRange(0, Math.Min(keyNames.Count,
            ClientInventoryConstants.HotbarSlotCount)));
    }

    /// <summary>
    ///     Parses a single SDL key name.
    /// </summary>
    /// <param name="optionPath">Configuration path of the option, for log messages.</param>
    /// <param name="keyName">Configured SDL key name.</param>
    /// <returns>Parsed keycode, or SDLK_UNKNOWN if the name is unrecognized.</returns>
    private SDL.SDL_Keycode ParseKey(string optionPath, string keyName)
    {
        var key = SDL.SDL_GetKeyFromName(keyName);
        if (key == SDL.SDL_Keycode.SDLK_UNKNOWN)
            logger.LogWarning("{OptionPath}: unrecognized key name '{KeyName}'; binding disabled.",
                optionPath, keyName);

        return key;
    }

    /// <summary>
    ///     Records a keycode assignment and warns if the keycode is already assigned to another binding.
    /// </summary>
    /// <param name="optionPath">Configuration path of the option being assigned.</param>
    /// <param name="keyName">Configured SDL key name.</param>
    /// <param name="key">Parsed keycode.</param>
    private void NoteAssignment(string optionPath, string keyName, SDL.SDL_Keycode key)
    {
        if (key == SDL.SDL_Keycode.SDLK_UNKNOWN) return;

        if (keyOwners.TryGetValue(key, out var previousPath))
            logger.LogWarning(
                "Key '{KeyName}' is assigned to both {PreviousPath} and {OptionPath}; the last registered binding wins where applicable.",
                keyName, previousPath, optionPath);
        else
            keyOwners[key] = optionPath;
    }

    /// <summary>
    ///     Builds the configuration path of a keybinding option.
    /// </summary>
    /// <param name="subsection">Subsection of KeybindingsOptions.</param>
    /// <param name="optionName">Option name.</param>
    /// <returns>Configuration path relative to the Sovereign section.</returns>
    private static string OptionPath(string subsection, string optionName)
    {
        return $"{SectionName}:{subsection}:{optionName}";
    }
}
