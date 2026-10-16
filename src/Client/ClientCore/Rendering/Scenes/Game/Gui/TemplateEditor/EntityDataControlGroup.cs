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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using Hexa.NET.ImGui;
using Sovereign.ClientCore.Network.Infrastructure;
using Sovereign.ClientCore.Rendering.Gui;
using Sovereign.ClientCore.Rendering.Scenes.Game.Gui.Controls;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Systems.Data;
using Sovereign.Scripting;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui.TemplateEditor;

/// <summary>
///     Editor control groups for properties related to entity key-value data.
/// </summary>
public class EntityDataControlGroup(
    GuiExtensions guiExtensions,
    EntityDataClient entityDataClient,
    ScriptInfoClient scriptInfoClient,
    NpcTemplateSelectorPopup npcTemplateSelectorPopup,
    ItemTemplateSelectorPopup itemTemplateSelectorPopup,
    NameComponentCollection names,
    EntityTypeComponentCollection entityTypes,
    AnimatedSpriteComponentCollection animatedSprites)
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
        EntityConstants.UnloadCallbackFunctionKey,
        EntityConstants.InteractScriptKey,
        EntityConstants.InteractFunctionKey
    };

    private readonly List<string> keysToRemove = new();
    private readonly List<EntityParameterHint> scriptParameters = new();
    private Dictionary<string, DataValueDto>? entityData = new();

    private bool entityDataLoaded;
    private string inputEntityAddedFunction = "";
    private string inputEntityAddedScript = "";
    private Dictionary<string, DataValueDto> inputEntityData = new();
    private string inputEntityLoadedFunction = "";
    private string inputEntityLoadedScript = "";
    private string inputEntityRemovedFunction = "";
    private string inputEntityRemovedScript = "";
    private string inputEntityUnloadedFunction = "";
    private string inputEntityUnloadedScript = "";
    private string inputInteractFunction = "";
    private string inputInteractScript = "";
    private string inputNewKey = "";
    private bool previousFrameEntityDataLoaded;
    private Dictionary<string, EntityDataRowEdit> rowEdits = new();

    private ulong selectedEntityId;
    private EntityType selectedEntityType;

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

    /// <summary>
    ///     Selects the entity being edited.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    public void SelectEntity(ulong entityId)
    {
        selectedEntityId = entityId;
        if (!entityTypes.TryGetValue(entityId, out selectedEntityType)) selectedEntityType = EntityType.Other;
        entityDataClient.RefreshEntity(selectedEntityId);

        inputNewKey = "";
        inputEntityData.Clear(); // will be filled by ResetEntityDataInputBuffers after async load
        rowEdits.Clear();
        previousFrameEntityDataLoaded = false;

        OnChangeScripts();
    }

    /// <summary>
    ///     Gets a copy of the input entity data.
    /// </summary>
    /// <returns>Input entity data.</returns>
    public Dictionary<string, DataValueDto> GetInputEntityData()
    {
        // Return a copy of the input data to avoid external modifications.
        var newEntityData = new Dictionary<string, DataValueDto>(inputEntityData);

        SetOrRemoveString(newEntityData, EntityConstants.AddCallbackScriptKey, inputEntityAddedScript);
        SetOrRemoveString(newEntityData, EntityConstants.AddCallbackFunctionKey, inputEntityAddedFunction);
        SetOrRemoveString(newEntityData, EntityConstants.LoadCallbackScriptKey, inputEntityLoadedScript);
        SetOrRemoveString(newEntityData, EntityConstants.LoadCallbackFunctionKey, inputEntityLoadedFunction);
        SetOrRemoveString(newEntityData, EntityConstants.RemoveCallbackScriptKey, inputEntityRemovedScript);
        SetOrRemoveString(newEntityData, EntityConstants.RemoveCallbackFunctionKey, inputEntityRemovedFunction);
        SetOrRemoveString(newEntityData, EntityConstants.UnloadCallbackScriptKey, inputEntityUnloadedScript);
        SetOrRemoveString(newEntityData, EntityConstants.UnloadCallbackFunctionKey, inputEntityUnloadedFunction);
        SetOrRemoveString(newEntityData, EntityConstants.InteractScriptKey, inputInteractScript);
        SetOrRemoveString(newEntityData, EntityConstants.InteractFunctionKey, inputInteractFunction);

        return newEntityData;
    }

    /// <summary>
    ///     Sets or removes a string-valued entry in the given dictionary.
    /// </summary>
    /// <param name="data">Data dictionary.</param>
    /// <param name="key">Key.</param>
    /// <param name="value">String value; an empty value removes the key.</param>
    private static void SetOrRemoveString(Dictionary<string, DataValueDto> data, string key, string value)
    {
        if (!string.IsNullOrEmpty(value))
            data[key] = DataValueDto.FromDataValue(DataValue.FromString(value));
        else
            data.Remove(key);
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

                    ImGui.TableNextColumn();
                    ImGui.Text("Interact:");
                    ImGui.TableNextColumn();
                    ImGui.TableNextColumn();
                    guiExtensions.ScriptFunctionSelector("int", ref inputInteractScript,
                        ref inputInteractFunction, out changed);
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
                npcTemplateSelectorPopup.Render();
                itemTemplateSelectorPopup.Render();

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
    /// <param name="hint">Parameter hint.</param>
    private void InputParameter(EntityParameterHint hint)
    {
        ImGui.TableNextColumn();
        ImGui.Text($"{hint.Name}:");
        if (ImGui.BeginItemTooltip())
        {
            ImGui.TextColored(new Vector4(0.7f, 0.7f, 0.7f, 1.0f), hint.Tooltip);
            ImGui.EndTooltip();
        }

        ImGui.TableNextColumn();
        var currentValue = inputEntityData.TryGetValue(hint.Name, out var dto) ? dto.Value : "";
        switch (hint.Type)
        {
            case EntityParameterType.Int:
                var intValue = int.TryParse(currentValue, NumberStyles.Integer, EntityConstants.EntityDataCulture,
                    out var i)
                    ? i
                    : 0;
                if (ImGui.InputInt($"##{hint.Name}", ref intValue))
                    inputEntityData[hint.Name] = DataValueDto.FromDataValue(DataValue.FromInt(intValue));
                break;

            case EntityParameterType.Float:
                var floatValue = float.TryParse(currentValue, NumberStyles.Float, EntityConstants.EntityDataCulture,
                    out var f)
                    ? f
                    : 0f;
                if (ImGui.InputFloat($"##{hint.Name}", ref floatValue))
                    inputEntityData[hint.Name] = DataValueDto.FromDataValue(DataValue.FromFloat(floatValue));
                break;

            case EntityParameterType.Bool:
                var boolValue = currentValue == "true";
                if (ImGui.Checkbox($"##{hint.Name}", ref boolValue))
                    inputEntityData[hint.Name] = DataValueDto.FromDataValue(DataValue.FromBool(boolValue));
                break;

            case EntityParameterType.NpcTemplate:
                ShowNpcTemplateSelect(hint, currentValue);
                break;

            case EntityParameterType.ItemTemplate:
                ShowItemTemplateSelect(hint, currentValue);
                break;

            case EntityParameterType.SameTypeTemplate:
                switch (selectedEntityType)
                {
                    case EntityType.Npc:
                        ShowNpcTemplateSelect(hint, currentValue);
                        break;

                    case EntityType.Item:
                        ShowItemTemplateSelect(hint, currentValue);
                        break;
                }

                break;

            case EntityParameterType.String:
            default:
                var stringValue = currentValue;
                if (ImGui.InputText($"##{hint.Name}", ref stringValue,
                        EntityConstants.MaxEntityDataValueLength + 1))
                    inputEntityData[hint.Name] = DataValueDto.FromDataValue(DataValue.FromString(stringValue));
                break;
        }
    }

    /// <summary>
    ///     Shows controls specific to NPC template selection.
    /// </summary>
    /// <param name="hint">Parameter hint.</param>
    /// <param name="currentValue">Current value as string.</param>
    private void ShowNpcTemplateSelect(EntityParameterHint hint, string currentValue)
    {
        if (!ulong.TryParse(currentValue, out var npcId)) npcId = 0;
        if (npcId > 0)
        {
            if (!animatedSprites.TryGetValue(npcId, out var animSpriteId)) animSpriteId = 0;

            if (!names.TryGetValue(npcId, out var name)) name = "[No Name]";
            var textSize = ImGui.CalcTextSize(name);
            var spriteSize =
                guiExtensions.CalcAnimatedSpriteSize(animSpriteId, Orientation.South, AnimationPhase.Default);

            guiExtensions.AnimatedSprite(animSpriteId, Orientation.South, AnimationPhase.Default);
            var shift = 0.5f * (spriteSize.Y - textSize.Y);
            ImGui.SameLine();
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + shift);
            ImGui.Text(name);
            ImGui.SameLine();
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + shift);
        }

        if (ImGui.Button("...")) npcTemplateSelectorPopup.Open(hint.Name);

        if (npcTemplateSelectorPopup.TryGetSelection(hint.Name, out var newNpcId))
        {
            if (newNpcId > 0)
                inputEntityData[hint.Name] = DataValueDto.FromDataValue(DataValue.FromEntityId(newNpcId));
            else
                inputEntityData.Remove(hint.Name);
        }
    }

    /// <summary>
    ///     Shows controls specific to item template selection.
    /// </summary>
    /// <param name="hint">Parameter hint.</param>
    /// <param name="currentValue">Current value as string.</param>
    private void ShowItemTemplateSelect(EntityParameterHint hint, string currentValue)
    {
        if (!ulong.TryParse(currentValue, out var npcId)) npcId = 0;
        if (npcId > 0)
        {
            if (!animatedSprites.TryGetValue(npcId, out var animSpriteId)) animSpriteId = 0;

            if (!names.TryGetValue(npcId, out var name)) name = "[No Name]";
            var textSize = ImGui.CalcTextSize(name);
            var spriteSize =
                guiExtensions.CalcAnimatedSpriteSize(animSpriteId, Orientation.South, AnimationPhase.Default);

            guiExtensions.AnimatedSprite(animSpriteId, Orientation.South, AnimationPhase.Default);
            var shift = 0.5f * (spriteSize.Y - textSize.Y);
            ImGui.SameLine();
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + shift);
            ImGui.Text(name);
            ImGui.SameLine();
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + shift);
        }

        if (ImGui.Button("...")) itemTemplateSelectorPopup.Open(hint.Name);

        if (itemTemplateSelectorPopup.TryGetSelection(hint.Name, out var newNpcId))
        {
            if (newNpcId > 0)
                inputEntityData[hint.Name] = DataValueDto.FromDataValue(DataValue.FromEntityId(newNpcId));
            else
                inputEntityData.Remove(hint.Name);
        }
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
                    if (callbackKeys.Contains(key) || scriptParameters.Select(p => p.Name).Contains(key)) continue;
                    InputKeyValue(key);
                }

                foreach (var key in keysToRemove)
                {
                    inputEntityData.Remove(key);
                    rowEdits.Remove(key);
                }

                // Last row is special for adding new keys.
                ImGui.TableNextColumn();
                var fontSize = ImGui.GetFontSize();
                ImGui.SetNextItemWidth(fontSize * 12.0f);
                ImGui.InputText("##newKey", ref inputNewKey, EntityConstants.MaxEntityDataKeyLength + 1);
                ImGui.SameLine();
                if (ImGui.Button("Add"))
                    if (inputNewKey.Length > 0 &&
                        inputEntityData.TryAdd(inputNewKey,
                            DataValueDto.FromDataValue(DataValue.FromString(string.Empty))))
                    {
                        rowEdits[inputNewKey] =
                            EntityDataRowEdit.FromDataValue(DataValue.FromString(string.Empty));
                        inputNewKey = "";
                    }

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
        if (!rowEdits.TryGetValue(key, out var row))
        {
            row = inputEntityData.TryGetValue(key, out var dto) && dto.TryToDataValue(out var value)
                ? EntityDataRowEdit.FromDataValue(value)
                : EntityDataRowEdit.FromDataValue(DataValue.FromString(string.Empty));
            rowEdits[key] = row;
        }

        var fontSize = ImGui.GetFontSize();
        ImGui.SetNextItemWidth(fontSize * 6.0f);
        var typeIndex = EntityDataRowEdit.IndexOf(row.Type);
        if (ImGui.Combo($"##type{key}", ref typeIndex, EntityDataRowEdit.TypeNames,
                EntityDataRowEdit.Types.Length))
            row.SetType(EntityDataRowEdit.Types[typeIndex]);
        ImGui.SameLine();

        var invalidColor = new Vector4(0.75f, 0.15f, 0.15f, 1.0f);
        if (!row.IsValid) ImGui.PushStyleColor(ImGuiCol.FrameBg, invalidColor);
        RenderRowInput(key, row);
        if (!row.IsValid)
        {
            ImGui.PopStyleColor();
            if (ImGui.IsItemHovered())
                if (ImGui.BeginTooltip())
                {
                    ImGui.Text($"Value is not valid for type {row.Type}.");
                    ImGui.EndTooltip();
                }
        }

        ImGui.SameLine();
        if (ImGui.Button($"Remove##{key}")) keysToRemove.Add(key);

        inputEntityData[key] = row.ToDto();
    }

    /// <summary>
    ///     Renders the type-specific input for a single key-value pair row.
    /// </summary>
    /// <param name="key">Key.</param>
    /// <param name="row">Row edit state.</param>
    private void RenderRowInput(string key, EntityDataRowEdit row)
    {
        var fontSize = ImGui.GetFontSize();
        switch (row.Type)
        {
            case DataValueType.Int:
                var intValue = int.TryParse(row.Text, NumberStyles.Integer, EntityConstants.EntityDataCulture,
                    out var i)
                    ? i
                    : 0;
                ImGui.SetNextItemWidth(fontSize * 12.0f);
                if (ImGui.InputInt($"##{key}", ref intValue))
                    row.SetText(intValue.ToString(EntityConstants.EntityDataCulture));
                break;

            case DataValueType.Float:
                var floatValue = float.TryParse(row.Text, NumberStyles.Float, EntityConstants.EntityDataCulture,
                    out var f)
                    ? f
                    : 0f;
                ImGui.SetNextItemWidth(fontSize * 12.0f);
                if (ImGui.InputFloat($"##{key}", ref floatValue))
                    row.SetText(floatValue.ToString(EntityConstants.EntityDataCulture));
                break;

            case DataValueType.Bool:
                var boolValue = row.Text == "true";
                if (ImGui.Checkbox($"##{key}", ref boolValue))
                    row.SetText(boolValue ? "true" : "false");
                break;

            case DataValueType.Vector3:
                RenderVector3Input(key, row, fontSize);
                break;

            case DataValueType.EntityId:
            case DataValueType.String:
            default:
                var textValue = row.Text;
                ImGui.SetNextItemWidth(fontSize * 12.0f);
                if (ImGui.InputText($"##{key}", ref textValue, EntityConstants.MaxEntityDataValueLength + 1))
                    row.SetText(textValue);
                break;
        }
    }

    /// <summary>
    ///     Renders the three-field input for a Vector3-valued row.
    /// </summary>
    /// <param name="key">Key.</param>
    /// <param name="row">Row edit state.</param>
    /// <param name="fontSize">Current font size.</param>
    private void RenderVector3Input(string key, EntityDataRowEdit row, float fontSize)
    {
        var parts = row.Text.Split(',');
        var x = parts.Length > 0 && float.TryParse(parts[0], NumberStyles.Float, EntityConstants.EntityDataCulture,
            out var px)
            ? px
            : 0f;
        var y = parts.Length > 1 && float.TryParse(parts[1], NumberStyles.Float, EntityConstants.EntityDataCulture,
            out var py)
            ? py
            : 0f;
        var z = parts.Length > 2 && float.TryParse(parts[2], NumberStyles.Float, EntityConstants.EntityDataCulture,
            out var pz)
            ? pz
            : 0f;

        var width = fontSize * 4.0f;
        ImGui.SetNextItemWidth(width);
        var changed = ImGui.InputFloat($"##{key}x", ref x);
        ImGui.SameLine();
        ImGui.SetNextItemWidth(width);
        changed |= ImGui.InputFloat($"##{key}y", ref y);
        ImGui.SameLine();
        ImGui.SetNextItemWidth(width);
        changed |= ImGui.InputFloat($"##{key}z", ref z);

        if (changed)
            row.SetText(string.Join(',',
                x.ToString(EntityConstants.EntityDataCulture),
                y.ToString(EntityConstants.EntityDataCulture),
                z.ToString(EntityConstants.EntityDataCulture)));
    }

    /// <summary>
    ///     Resets the input buffers for entity key-value data.
    /// </summary>
    private void ResetEntityDataInputBuffers()
    {
        // Reset the working dict.
        inputEntityData = new Dictionary<string, DataValueDto>(entityData!);

        // Rebuild per-row edit state, preserving rows that survived a refresh.
        var newRowEdits = new Dictionary<string, EntityDataRowEdit>();
        foreach (var (key, dto) in inputEntityData)
            newRowEdits[key] = rowEdits.TryGetValue(key, out var existing)
                ? existing
                : dto.TryToDataValue(out var value)
                    ? EntityDataRowEdit.FromDataValue(value)
                    : EntityDataRowEdit.FromDataValue(DataValue.FromString(dto.Value));
        rowEdits = newRowEdits;

        // Set up callbacks to match current state.
        inputEntityAddedFunction = GetStringValue(EntityConstants.AddCallbackFunctionKey);
        inputEntityAddedScript = GetStringValue(EntityConstants.AddCallbackScriptKey);
        inputEntityLoadedFunction = GetStringValue(EntityConstants.LoadCallbackFunctionKey);
        inputEntityLoadedScript = GetStringValue(EntityConstants.LoadCallbackScriptKey);
        inputEntityRemovedFunction = GetStringValue(EntityConstants.RemoveCallbackFunctionKey);
        inputEntityRemovedScript = GetStringValue(EntityConstants.RemoveCallbackScriptKey);
        inputEntityUnloadedFunction = GetStringValue(EntityConstants.UnloadCallbackFunctionKey);
        inputEntityUnloadedScript = GetStringValue(EntityConstants.UnloadCallbackScriptKey);
        inputInteractFunction = GetStringValue(EntityConstants.InteractFunctionKey);
        inputInteractScript = GetStringValue(EntityConstants.InteractScriptKey);

        OnChangeScripts();
    }

    /// <summary>
    ///     Gets the canonical text of a string-valued entity data key.
    /// </summary>
    /// <param name="key">Key.</param>
    /// <returns>Value text, or an empty string if the key is absent.</returns>
    private string GetStringValue(string key)
    {
        return entityData!.TryGetValue(key, out var dto) ? dto.Value : "";
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
        var parameters = new Dictionary<string, EntityParameterHint>();

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

            foreach (var param in function.EntityParameters)
                if (!parameters.ContainsKey(param.Name))
                    parameters[param.Name] = param;
        }

        scriptParameters.AddRange(parameters.Values);
        scriptParameters.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
    }
}
