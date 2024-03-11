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
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Systems.Block.Components;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Debug;

/// <summary>
///     Entity debug window.
/// </summary>
public class EntityDebugGui
{
    private readonly AboveBlockComponentCollection aboveBlocks;
    private readonly AnimatedSpriteComponentCollection animatedSprites;
    private readonly DrawableTagCollection drawables;
    private readonly EntityTable entityTable;
    private readonly MaterialModifierComponentCollection materialModifiers;
    private readonly MaterialComponentCollection materials;
    private readonly NameComponentCollection names;
    private readonly OrientationComponentCollection orientations;
    private readonly ParentComponentCollection parents;
    private readonly PositionComponentCollection positions;
    private readonly VelocityComponentCollection velocities;
    private string entityIdInput = "";

    public EntityDebugGui(AboveBlockComponentCollection aboveBlocks, AnimatedSpriteComponentCollection animatedSprites,
        DrawableTagCollection drawables, MaterialComponentCollection materials,
        MaterialModifierComponentCollection materialModifiers, NameComponentCollection names,
        OrientationComponentCollection orientations, ParentComponentCollection parents,
        PositionComponentCollection positions,
        VelocityComponentCollection velocities, EntityTable entityTable)
    {
        this.aboveBlocks = aboveBlocks;
        this.animatedSprites = animatedSprites;
        this.drawables = drawables;
        this.materials = materials;
        this.materialModifiers = materialModifiers;
        this.names = names;
        this.orientations = orientations;
        this.parents = parents;
        this.positions = positions;
        this.velocities = velocities;
        this.entityTable = entityTable;
    }

    /// <summary>
    ///     Renders the entity debug window.
    /// </summary>
    public void Render()
    {
        if (!ImGui.Begin("Entity Debug")) return;

        ImGui.InputText("Entity ID", ref entityIdInput, 16, ImGuiInputTextFlags.CharsHexadecimal);
        ImGui.Separator();

        if (ulong.TryParse(entityIdInput, NumberStyles.HexNumber, null, out var entityId))
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
                AddComponentRow("Position:", entityId, positions, CleanVec3ToString);
                AddComponentRow("Velocity:", entityId, velocities, CleanVec3ToString);
                ImGui.EndTable();
            }
        }
        else
        {
            ImGui.Text("Entity not found.");
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
    ///     Nicely formats a vector.
    /// </summary>
    /// <param name="v">Vector.</param>
    /// <returns>Nicely formatted vector.</returns>
    private string CleanVec3ToString(Vector3 v)
    {
        return $"({v.X,8:F3}, {v.Y,8:F3}, {v.Z,8:F3})";
    }
}