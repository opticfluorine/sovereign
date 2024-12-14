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

using System;
using System.Globalization;
using System.Numerics;
using ImGuiNET;
using Sovereign.ClientCore.Systems.Camera;
using Sovereign.ClientCore.Systems.Perspective;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Entities;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui.Debug;

/// <summary>
///     Entity debug window.
/// </summary>
public class EntityDebugGui
{
    private readonly AboveBlockComponentCollection aboveBlocks;
    private readonly AnimatedSpriteComponentCollection animatedSprites;
    private readonly BlockPositionComponentCollection blockPositions;
    private readonly CameraServices cameraServices;
    private readonly CastBlockShadowsTagCollection castBlockShadows;
    private readonly DrawableTagCollection drawables;
    private readonly EntityTable entityTable;
    private readonly KinematicsComponentCollection kinematics;
    private readonly MaterialModifierComponentCollection materialModifiers;
    private readonly MaterialComponentCollection materials;
    private readonly NameComponentCollection names;
    private readonly OrientationComponentCollection orientations;
    private readonly ParentComponentCollection parents;
    private readonly PerspectiveServices perspectiveServices;
    private readonly PointLightSourceComponentCollection pointLightSources;
    private string entityIdInput = "";

    public EntityDebugGui(AboveBlockComponentCollection aboveBlocks, AnimatedSpriteComponentCollection animatedSprites,
        DrawableTagCollection drawables, MaterialComponentCollection materials,
        MaterialModifierComponentCollection materialModifiers, NameComponentCollection names,
        OrientationComponentCollection orientations, ParentComponentCollection parents,
        KinematicsComponentCollection kinematics,
        BlockPositionComponentCollection blockPositions,
        CastBlockShadowsTagCollection castBlockShadows,
        EntityTable entityTable,
        CameraServices cameraServices,
        PerspectiveServices perspectiveServices,
        PointLightSourceComponentCollection pointLightSources)
    {
        this.aboveBlocks = aboveBlocks;
        this.animatedSprites = animatedSprites;
        this.drawables = drawables;
        this.materials = materials;
        this.materialModifiers = materialModifiers;
        this.names = names;
        this.orientations = orientations;
        this.parents = parents;
        this.kinematics = kinematics;
        this.blockPositions = blockPositions;
        this.castBlockShadows = castBlockShadows;
        this.entityTable = entityTable;
        this.cameraServices = cameraServices;
        this.perspectiveServices = perspectiveServices;
        this.pointLightSources = pointLightSources;
    }

    /// <summary>
    ///     Renders the entity debug window.
    /// </summary>
    public void Render()
    {
        var fontSize = ImGui.GetFontSize();
        ImGui.SetNextWindowSize(fontSize * new Vector2(26.0f, 28.0f), ImGuiCond.Once);
        if (!ImGui.Begin("Entity Debug")) return;

        if (ImGui.BeginTabBar("entityDebugTabs", ImGuiTabBarFlags.None))
        {
            ulong entityId = 0;
            var valid = false;

            if (ImGui.BeginTabItem("By ID"))
            {
                ImGui.InputText("Entity ID", ref entityIdInput, 16, ImGuiInputTextFlags.CharsHexadecimal);
                valid = ulong.TryParse(entityIdInput, NumberStyles.HexNumber, null, out entityId);
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("By Mouse Hover"))
            {
                var mousePosWorld = cameraServices.GetMousePositionWorldCoordinates();
                valid = perspectiveServices.TryGetHighestCoveringEntity(mousePosWorld, out entityId);

                ImGui.Text($"Hovered Position: {mousePosWorld}");
                if (valid) ImGui.Text($"Hovered Entity ID: {entityId:x16}");
                else ImGui.TextColored(new Vector4(0.7f), "No entity hovered");
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
            ImGui.Separator();

            if (valid)
            {
                if (ImGui.BeginTable("entityInfo", 2))
                {
                    AddValueRow("In EntityTable:", entityTable.Exists(entityId));
                    AddComponentRow("AboveBlock:", entityId, aboveBlocks, x => $"{x:X}");
                    AddComponentRow("AnimatedSprite:", entityId, animatedSprites);
                    AddComponentRow("Drawable:", entityId, drawables);
                    AddComponentRow("Material:", entityId, materials);
                    AddComponentRow("Material Modifier:", entityId, materialModifiers);
                    AddComponentRow("Name:", entityId, names);
                    AddComponentRow("Orientation:", entityId, orientations);
                    AddComponentRow("Parent:", entityId, parents, x => $"{x:X}");
                    AddComponentRow("Position:", entityId, kinematics, x => CleanVec3ToString(x.Position));
                    AddComponentRow("Velocity:", entityId, kinematics, x => CleanVec3ToString(x.Velocity));
                    AddComponentRow("Block Position:", entityId, blockPositions);
                    AddComponentRow("Cast Block Shadows:", entityId, castBlockShadows);
                    AddCompoundRows("Point Light Source:", entityId, pointLightSources,
                        pls =>
                        {
                            AddValueRow("PLS Radius:", pls.Radius);
                            AddValueRow("PLS Intensity:", pls.Intensity);
                            AddValueRow("PLS Color:", CleanVec3ToString(pls.Color));
                            AddValueRow("PLS Pos Offset:", CleanVec3ToString(pls.PositionOffset));
                        });
                    ImGui.EndTable();
                }
            }
            else
            {
                ImGui.Text("Entity not found.");
            }
        }

        ImGui.End();
    }

    /// <summary>
    ///     Adds a component-backed data row to the player entity debug table.
    /// </summary>
    /// <param name="label">Label.</param>
    /// <param name="value">Value.</param>
    /// <typeparam name="T">Value type.</typeparam>
    private void AddValueRow<T>(string label, T value)
        where T : notnull
    {
        ImGui.TableNextColumn();
        ImGui.Text(label);
        ImGui.TableNextColumn();
        ImGui.Text(value.ToString());
    }

    /// <summary>
    ///     Adds a component-backed data row to the player entity debug table.
    /// </summary>
    /// <param name="label">Label.</param>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="components">Backing component collection.</param>
    /// <typeparam name="T">Component value type.</typeparam>
    private void AddComponentRow<T>(string label, ulong entityId, BaseComponentCollection<T> components)
        where T : notnull
    {
        AddComponentRow(label, entityId, components, x => x.ToString());
    }

    /// <summary>
    ///     Adds a component-backed data row to the player entity debug table.
    /// </summary>
    /// <param name="label">Label.</param>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="components">Backing component collection.</param>
    /// <param name="transform">Data transform to apply to the component.</param>
    /// <typeparam name="T">Component value type.</typeparam>
    private void AddComponentRow<T>(string label, ulong entityId, BaseComponentCollection<T> components,
        Func<T, string?> transform)
        where T : notnull
    {
        ImGui.TableNextColumn();
        ImGui.Text(label);
        ImGui.TableNextColumn();
        if (components.HasComponentForEntity(entityId))
            ImGui.Text(transform(components[entityId]));
        else
            ImGui.Text("[No Data]");
    }

    /// <summary>
    ///     Adds one or more component-backed data rows according to a condition, invoking the
    ///     specified callback to add the remaining rows if the condition is true.
    /// </summary>
    /// <param name="label">Label.</param>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="components">Backing component collection.</param>
    /// <param name="callbackIfFound">Callback to call if the component is present.</param>
    private void AddCompoundRows<T>(string label, ulong entityId, BaseComponentCollection<T> components,
        Action<T> callbackIfFound)
        where T : notnull
    {
        ImGui.TableNextColumn();
        ImGui.Text(label);
        ImGui.TableNextColumn();
        var hasComponent = components.HasComponentForEntity(entityId);
        ImGui.Text(hasComponent ? "Yes" : "No");
        if (hasComponent) callbackIfFound.Invoke(components[entityId]);
    }

    /// <summary>
    ///     Nicely formats a vector.
    /// </summary>
    /// <param name="v">Vector.</param>
    /// <returns>Nicely formatted vector.</returns>
    private string CleanVec3ToString(Vector3 v)
    {
        return $"({v.X,8:F3}, {v.Y,8:F3}, {v.Z,8:F3})";
    }
}