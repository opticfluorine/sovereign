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
using System.Numerics;
using ImGuiNET;
using Sovereign.ClientCore.Systems.ClientState;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Systems.Block.Components;
using Sovereign.EngineCore.Systems.Block.Components.Indexers;
using Sovereign.EngineCore.Systems.Player.Components;
using Sovereign.EngineCore.World;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui.Debug;

/// <summary>
///     Debug window for player information.
/// </summary>
public class PlayerDebugGui
{
    private readonly AnimatedSpriteComponentCollection animatedSprites;
    private readonly BlockGridPositionIndexer blocks;
    private readonly DrawableTagCollection drawables;
    private readonly MaterialModifierComponentCollection materialModifiers;
    private readonly MaterialComponentCollection materials;
    private readonly NameComponentCollection names;
    private readonly OrientationComponentCollection orientations;
    private readonly PlayerCharacterTagCollection players;
    private readonly PositionComponentCollection positions;
    private readonly ClientStateServices stateServices;
    private readonly VelocityComponentCollection velocities;
    private readonly WorldSegmentResolver worldSegmentResolver;

    public PlayerDebugGui(ClientStateServices stateServices, NameComponentCollection names,
        PositionComponentCollection positions, VelocityComponentCollection velocities,
        AnimatedSpriteComponentCollection animatedSprites, WorldSegmentResolver worldSegmentResolver,
        DrawableTagCollection drawables, PlayerCharacterTagCollection players,
        OrientationComponentCollection orientations, BlockGridPositionIndexer blocks,
        MaterialComponentCollection materials, MaterialModifierComponentCollection materialModifiers)
    {
        this.stateServices = stateServices;
        this.names = names;
        this.positions = positions;
        this.velocities = velocities;
        this.animatedSprites = animatedSprites;
        this.worldSegmentResolver = worldSegmentResolver;
        this.drawables = drawables;
        this.players = players;
        this.orientations = orientations;
        this.blocks = blocks;
        this.materials = materials;
        this.materialModifiers = materialModifiers;
    }

    /// <summary>
    ///     Renders the player debug window.
    /// </summary>
    public void Render()
    {
        ImGui.SetNextWindowSize(Vector2.Zero);
        if (!ImGui.Begin("Player Debug")) return;

        if (stateServices.TryGetSelectedPlayer(out var playerEntityId))
        {
            if (ImGui.BeginTable("debugPlayerInfo", 2))
            {
                AddValueRow("Player ID:", $"{playerEntityId:X}");
                AddComponentRow("Player Name:", playerEntityId, names);
                AddComponentRow("Player Character:", playerEntityId, players);
                AddComponentRow("Drawable:", playerEntityId, drawables);
                AddComponentRow("AnimatedSprite:", playerEntityId, animatedSprites);
                AddComponentRow("Position:", playerEntityId, positions, CleanVec3ToString);
                AddComponentRow("Velocity:", playerEntityId, velocities, CleanVec3ToString);
                AddComponentRow("Orientation:", playerEntityId, orientations);
                AddComponentRow("World Segment:", playerEntityId, positions,
                    x => worldSegmentResolver.GetWorldSegmentForPosition(x).ToString());

                AddBelowBlockInfo(playerEntityId);

                ImGui.EndTable();
            }
        }
        else
        {
            ImGui.Text("Player is not logged in.");
        }

        ImGui.End();
    }

    /// <summary>
    ///     Adds inforamtion about the block below the player, if any.
    /// </summary>
    /// <param name="playerEntityId">Player entity ID.</param>
    private void AddBelowBlockInfo(ulong playerEntityId)
    {
        if (!positions.HasComponentForEntity(playerEntityId)) return;

        var blockPos = new GridPosition(positions[playerEntityId]);
        var blocksBelow = blocks.GetEntitiesAtPosition(blockPos);
        if (blocksBelow is null)
        {
            AddValueRow("Below Block ID:", "[None]");
            return;
        }

        foreach (var block in blocksBelow)
        {
            AddValueRow("Below Block ID:", $"{block:X}");
            AddComponentRow("Below Block Material:", block, materials);
            AddComponentRow("Below Block Material Mod:", block, materialModifiers);
        }
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
    /// <param name="playerEntityId">Player entity ID.</param>
    /// <param name="components">Backing component collection.</param>
    /// <typeparam name="T">Component value type.</typeparam>
    private void AddComponentRow<T>(string label, ulong playerEntityId, BaseComponentCollection<T> components)
        where T : notnull
    {
        AddComponentRow(label, playerEntityId, components, x => x.ToString());
    }

    /// <summary>
    ///     Adds a component-backed data row to the player entity debug table.
    /// </summary>
    /// <param name="label">Label.</param>
    /// <param name="playerEntityId">Player entity ID.</param>
    /// <param name="components">Backing component collection.</param>
    /// <param name="transform">Data transform to apply to the component.</param>
    /// <typeparam name="T">Component value type.</typeparam>
    private void AddComponentRow<T>(string label, ulong playerEntityId, BaseComponentCollection<T> components,
        Func<T, string?> transform)
        where T : notnull
    {
        ImGui.TableNextColumn();
        ImGui.Text(label);
        ImGui.TableNextColumn();
        if (components.HasComponentForEntity(playerEntityId))
            ImGui.Text(transform(components[playerEntityId]));
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