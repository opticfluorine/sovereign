// Sovereign Engine
// Copyright (c) 2025 opticfluorine
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

using System.Collections.Generic;
using System.Linq;
using Hexa.NET.ImGui;
using Sovereign.ClientCore.Network.Infrastructure;
using Sovereign.ClientCore.Rendering.Gui;
using Sovereign.EngineCore.Entities;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui.TemplateEditor;

/// <summary>
///     Editor control groups for properties related to entity key-value data.
/// </summary>
public class EntityDataControlGroup(
    GuiExtensions guiExtensions,
    EntityDataClient entityDataClient,
    ScriptInfoClient scriptInfoClient)
{
    private readonly List<string> callbackKeys = new()
    {
        EntityConstants.AddCallbackScriptKey,
        EntityConstants.AddCallbackFunctionKey,
        EntityConstants.LoadCallbackScriptKey,
        EntityConstants.LoadCallbackFunctionKey,
        EntityConstants.RemoveCallbackScriptKey,
        EntityConstants.RemoveCallbackFunctionKey,
        EntityConstants.UnloadCallbackScriptKey,
        EntityConstants.UnloadCallbackFunctionKey
    };

    private readonly List<string> keysToRemove = new();
    private readonly List<string> scriptParameters = new();
    private Dictionary<string, string>? entityData = new();

    private bool entityDataLoaded;
    private string inputEntityAddedFunction = "";
    private string inputEntityAddedScript = "";
    private Dictionary<string, string> inputEntityData = new();
    private string inputEntityLoadedFunction = "";
    private string inputEntityLoadedScript = "";
    private string inputEntityRemovedFunction = "";
    private string inputEntityRemovedScript = "";
    private string inputEntityUnloadedFunction = "";
    private string inputEntityUnloadedScript = "";
    private string inputNewKey = "";
    private bool previousFrameEntityDataLoaded;

    private ulong selectedEntityId;

    /// <summary>
    ///     Renders the entity data control groups. Must be called within a two-column table.
    /// </summary>
    public void Render()
    {
        entityDataLoaded = entityDataClient.TryGetCachedEntityData(selectedEntityId, out entityData);
        if (entityDataLoaded && !previousFrameEntityDataLoaded) ResetEntityDataInputBuffers();
        previousFrameEntityDataLoaded = entityDataLoaded;

        RenderScriptingSection();
        RenderScriptParameters();
        RenderEntityData();
    }

    public void SelectEntity(ulong entityId)
    {
        selectedEntityId = entityId;
        entityDataClient.RefreshEntity(selectedEntityId);

        inputNewKey = "";
        inputEntityData.Clear(); // will be filled by ResetEntityDataInputBuffers after async load
        previousFrameEntityDataLoaded = false;

        OnChangeScripts();
    }

    /// <summary>
    ///     Gets a copy of the input entity data.
    /// </summary>
    /// <returns>Input entity data.</returns>
    public Dictionary<string, string> GetInputEntityData()
    {
        // Return a copy of the input data to avoid external modifications.
        var newEntityData = new Dictionary<string, string>(inputEntityData);
        newEntityData[EntityConstants.AddCallbackScriptKey] = inputEntityAddedScript;
        newEntityData[EntityConstants.AddCallbackFunctionKey] = inputEntityAddedFunction;
        newEntityData[EntityConstants.LoadCallbackScriptKey] = inputEntityLoadedScript;
        newEntityData[EntityConstants.LoadCallbackFunctionKey] = inputEntityLoadedFunction;
        newEntityData[EntityConstants.RemoveCallbackScriptKey] = inputEntityRemovedScript;
        newEntityData[EntityConstants.RemoveCallbackFunctionKey] = inputEntityRemovedFunction;
        newEntityData[EntityConstants.UnloadCallbackScriptKey] = inputEntityUnloadedScript;
        newEntityData[EntityConstants.UnloadCallbackFunctionKey] = inputEntityUnloadedFunction;
        return newEntityData;
    }

    /// <summary>
    ///     Renders the Scripting section of the editor.
    /// </summary>
    private void RenderScriptingSection()
    {
        if (ImGui.CollapsingHeader("Scripting", ImGuiTreeNodeFlags.DefaultOpen))
            if (entityDataLoaded)
            {
                if (ImGui.BeginTable("Scripting", 3, ImGuiTableFlags.SizingFixedFit))
                {
                    ImGui.TableSetupColumn("Label");
                    ImGui.TableSetupColumn("Spacer", ImGuiTableColumnFlags.WidthStretch);

                    ImGui.TableNextColumn();
                    ImGui.Text("Entity Added:");
                    ImGui.TableNextColumn();
                    ImGui.TableNextColumn();
                    guiExtensions.ScriptFunctionSelector("add", ref inputEntityAddedScript,
                        ref inputEntityAddedFunction, out var changed);
                    if (changed) OnChangeScripts();

                    ImGui.TableNextColumn();
                    ImGui.Text("Entity Loaded:");
                    ImGui.TableNextColumn();
                    ImGui.TableNextColumn();
                    guiExtensions.ScriptFunctionSelector("load", ref inputEntityLoadedScript,
                        ref inputEntityLoadedFunction, out changed);
                    if (changed) OnChangeScripts();

                    ImGui.TableNextColumn();
                    ImGui.Text("Entity Removed:");
                    ImGui.TableNextColumn();
                    ImGui.TableNextColumn();
                    guiExtensions.ScriptFunctionSelector("rem", ref inputEntityRemovedScript,
                        ref inputEntityRemovedFunction, out changed);
                    if (changed) OnChangeScripts();

                    ImGui.TableNextColumn();
                    ImGui.Text("Entity Unloaded:");
                    ImGui.TableNextColumn();
                    ImGui.TableNextColumn();
                    guiExtensions.ScriptFunctionSelector("unl", ref inputEntityUnloadedScript,
                        ref inputEntityUnloadedFunction, out changed);
                    if (changed) OnChangeScripts();

                    ImGui.EndTable();
                }
            }
            else
            {
                RenderLoading("loadScripting");
            }
    }

    /// <summary>
    ///     Renders the Script Parameters section of the editor.
    /// </summary>
    private void RenderScriptParameters()
    {
        if (!ImGui.CollapsingHeader("Script Parameters", ImGuiTreeNodeFlags.DefaultOpen)) return;

        if (entityDataLoaded)
        {
            if (ImGui.BeginTable("ScriptParameters", 2, ImGuiTableFlags.SizingStretchProp))
            {
                ImGui.TableSetupColumn("Key", ImGuiTableColumnFlags.WidthFixed);

                foreach (var param in scriptParameters) InputParameter(param);

                ImGui.EndTable();
            }
        }
        else
        {
            RenderLoading("loadScriptParameters");
        }
    }

    /// <summary>
    ///     Renders a single parameter row in the scripting section.
    /// </summary>
    /// <param name="name">Parameter name (key).</param>
    private void InputParameter(string name)
    {
        ImGui.TableNextColumn();
        ImGui.Text($"{name}:");

        ImGui.TableNextColumn();
        var currentValue = inputEntityData.TryGetValue(name, out var value) ? value : "";
        ImGui.InputText($"##{name}", ref currentValue, EntityConstants.MaxEntityDataValueLength);
        inputEntityData[name] = currentValue;
    }

    /// <summary>
    ///     Renders the remaining entity data (i.e. not callbacks or script parameters).
    /// </summary>
    private void RenderEntityData()
    {
        if (!ImGui.CollapsingHeader("Entity Data", ImGuiTreeNodeFlags.DefaultOpen)) return;

        if (entityDataLoaded)
        {
            if (ImGui.BeginTable("EntityData", 2, ImGuiTableFlags.SizingFixedFit))
            {
                // Show a row for each key-value pair not already bound to another section.
                keysToRemove.Clear();
                foreach (var key in inputEntityData.Keys.Order())
                {
                    if (callbackKeys.Contains(key) || scriptParameters.Contains(key)) continue;
                    InputKeyValue(key);
                }

                foreach (var key in keysToRemove) inputEntityData.Remove(key);

                // Last row is special for adding new keys.
                ImGui.TableNextColumn();
                var fontSize = ImGui.GetFontSize();
                ImGui.SetNextItemWidth(fontSize * 12.0f);
                ImGui.InputText("##newKey", ref inputNewKey, EntityConstants.MaxEntityDataKeyLength);
                ImGui.SameLine();
                if (ImGui.Button("Add"))
                    if (inputNewKey.Length > 0 && inputEntityData.TryAdd(inputNewKey, ""))
                        inputNewKey = "";

                ImGui.EndTable();
            }
        }
        else
        {
            RenderLoading("loadEntityData");
        }
    }

    /// <summary>
    ///     Renders the inputs for a single key-value pair.
    /// </summary>
    /// <param name="key">Key.</param>
    private void InputKeyValue(string key)
    {
        ImGui.TableNextColumn();
        ImGui.Text($"{key}:");

        ImGui.TableNextColumn();
        var currentValue = inputEntityData.TryGetValue(key, out var value) ? value : "";
        var fontSize = ImGui.GetFontSize();
        ImGui.SetNextItemWidth(fontSize * 12.0f);
        ImGui.InputText($"##{key}", ref currentValue, EntityConstants.MaxEntityDataValueLength);
        inputEntityData[key] = currentValue;
        ImGui.SameLine();
        if (ImGui.Button($"Remove##{key}")) keysToRemove.Add(key);
    }

    /// <summary>
    ///     Resets the input buffers for entity key-value data.
    /// </summary>
    private void ResetEntityDataInputBuffers()
    {
        // Reset the working dict.
        inputEntityData = new Dictionary<string, string>(entityData!);

        // Set up callbacks to match current state.
        inputEntityAddedFunction = entityData!.TryGetValue(EntityConstants.AddCallbackFunctionKey, out var addFunction)
            ? addFunction
            : "";
        inputEntityAddedScript = entityData!.TryGetValue(EntityConstants.AddCallbackScriptKey, out var addScript)
            ? addScript
            : "";
        inputEntityLoadedFunction =
            entityData!.TryGetValue(EntityConstants.LoadCallbackFunctionKey, out var loadFunction)
                ? loadFunction
                : "";
        inputEntityLoadedScript = entityData!.TryGetValue(EntityConstants.LoadCallbackScriptKey, out var loadScript)
            ? loadScript
            : "";
        inputEntityRemovedFunction =
            entityData!.TryGetValue(EntityConstants.RemoveCallbackFunctionKey, out var removeFunction)
                ? removeFunction
                : "";
        inputEntityRemovedScript =
            entityData!.TryGetValue(EntityConstants.RemoveCallbackScriptKey, out var removeScript)
                ? removeScript
                : "";
        inputEntityUnloadedFunction =
            entityData!.TryGetValue(EntityConstants.UnloadCallbackFunctionKey, out var unloadFunction)
                ? unloadFunction
                : "";
        inputEntityUnloadedScript =
            entityData!.TryGetValue(EntityConstants.UnloadCallbackScriptKey, out var unloadScript)
                ? unloadScript
                : "";

        OnChangeScripts();
    }

    /// <summary>
    ///     Renders a loading state for a part of the editor.
    /// </summary>
    /// <param name="id">Unique ID for this widget.</param>
    private void RenderLoading(string id)
    {
        if (!ImGui.BeginTable(id, 3)) return;
        ImGui.TableSetupColumn("A", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn("B", ImGuiTableColumnFlags.WidthFixed);
        ImGui.TableSetupColumn("C", ImGuiTableColumnFlags.WidthStretch);

        ImGui.TableNextColumn();
        ImGui.TableNextColumn();
        ImGui.Text("Loading...");
        ImGui.TableNextColumn();

        ImGui.EndTable();
    }

    /// <summary>
    ///     Rebuilds the parameter hint cache whenever a script or function is changed.
    /// </summary>
    private void OnChangeScripts()
    {
        scriptParameters.Clear();
        var scriptInfo = scriptInfoClient.ScriptInfo;
        var parameters = new HashSet<string>();

        var scriptFunctionPairs = new List<(string, string)>
        {
            (inputEntityAddedScript, inputEntityAddedFunction),
            (inputEntityLoadedScript, inputEntityLoadedFunction),
            (inputEntityRemovedScript, inputEntityRemovedFunction),
            (inputEntityUnloadedScript, inputEntityUnloadedFunction)
        };
        foreach (var (scriptName, functionName) in scriptFunctionPairs)
        {
            var script = scriptInfo.Scripts.Find(s => s.Name == scriptName);
            if (script == null) continue;

            var function = script.Functions.Find(f => f.Name == functionName);
            if (function == null) continue;

            foreach (var param in function.EntityParameters) parameters.Add(param);
        }

        scriptParameters.AddRange(parameters);
        scriptParameters.Sort();
    }
}