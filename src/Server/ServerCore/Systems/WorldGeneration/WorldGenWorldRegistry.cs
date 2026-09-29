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
using System.Text.Json;
using Sovereign.EngineCore.Components.Types;

namespace Sovereign.ServerCore.Systems.WorldGeneration;

/// <summary>
///     Relationship of a candidate commit footprint to a registered world footprint.
/// </summary>
public enum WorldGenFootprintRelation
{
    /// <summary>
    ///     The footprints do not overlap; touching edges are disjoint.
    /// </summary>
    Disjoint,

    /// <summary>
    ///     The candidate footprint fully contains the registered world.
    /// </summary>
    Contains,

    /// <summary>
    ///     The footprints partially overlap.
    /// </summary>
    Straddles
}

/// <summary>
///     One registered world in the worldgen world registry.
/// </summary>
public sealed class WorldGenRegistryEntry
{
    /// <summary>
    ///     Seed of the committed world.
    /// </summary>
    public required ulong Seed { get; init; }

    /// <summary>
    ///     Name of the profile used to generate the world.
    /// </summary>
    public required string Profile { get; init; }

    /// <summary>
    ///     World X coordinate of the footprint origin.
    /// </summary>
    public required int OriginX { get; init; }

    /// <summary>
    ///     World Y coordinate of the footprint origin.
    /// </summary>
    public required int OriginY { get; init; }

    /// <summary>
    ///     Width of the world in blocks.
    /// </summary>
    public required int Width { get; init; }

    /// <summary>
    ///     Height of the world in blocks.
    /// </summary>
    public required int Height { get; init; }

    /// <summary>
    ///     Lowest world segment Z index occupied by the world.
    /// </summary>
    public required int MinSegmentZ { get; init; }

    /// <summary>
    ///     Highest world segment Z index occupied by the world.
    /// </summary>
    public required int MaxSegmentZ { get; init; }

    /// <summary>
    ///     UTC time at which the world was committed.
    /// </summary>
    public required DateTime CommittedAtUtc { get; init; }

    /// <summary>
    ///     Serializes the entry to its registry JSON form.
    /// </summary>
    /// <returns>JSON string.</returns>
    public string ToJson()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }

    /// <summary>
    ///     Deserializes an entry from its registry JSON form.
    /// </summary>
    /// <param name="json">JSON string.</param>
    /// <returns>Deserialized entry, or null if the JSON is not a valid entry.</returns>
    public static WorldGenRegistryEntry? FromJson(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<WorldGenRegistryEntry>(json,
                new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }
        catch (JsonException)
        {
            return null;
        }
    }
}

/// <summary>
///     Store of committed world registry entries in the global key-value store under
///     <c>worldgen:world:&lt;n&gt;</c> keys. Many worlds may coexist; exactly one has the
///     default spawn, which the registry does not track.
/// </summary>
public interface IWorldGenWorldRegistryStore
{
    /// <summary>
    ///     Loads every registered world.
    /// </summary>
    /// <returns>Registered worlds, in key order. Malformed entries are skipped.</returns>
    IReadOnlyList<WorldGenRegistryEntry> LoadWorlds();

    /// <summary>
    ///     Appends a world to the registry.
    /// </summary>
    /// <param name="entry">World to register.</param>
    void AppendWorld(WorldGenRegistryEntry entry);
}

/// <summary>
///     Registry helper for classifying commit footprints against registered worlds.
/// </summary>
public static class WorldGenWorldRegistry
{
    /// <summary>
    ///     Key prefix of registry entries.
    /// </summary>
    public const string KeyPrefix = "worldgen:world:";

    /// <summary>
    ///     Classifies the relationship of a candidate commit footprint to a registered world.
    /// </summary>
    /// <param name="world">Registered world.</param>
    /// <param name="originX">World X coordinate of the candidate footprint origin.</param>
    /// <param name="originY">World Y coordinate of the candidate footprint origin.</param>
    /// <param name="width">Width of the candidate footprint in blocks.</param>
    /// <param name="height">Height of the candidate footprint in blocks.</param>
    /// <returns>Footprint relationship.</returns>
    public static WorldGenFootprintRelation Classify(WorldGenRegistryEntry world, int originX,
        int originY, int width, int height)
    {
        var overlapX = Math.Min(originX + width, world.OriginX + world.Width)
                       - Math.Max(originX, world.OriginX);
        var overlapY = Math.Min(originY + height, world.OriginY + world.Height)
                       - Math.Max(originY, world.OriginY);
        if (overlapX <= 0 || overlapY <= 0) return WorldGenFootprintRelation.Disjoint;

        var contains = originX <= world.OriginX && originY <= world.OriginY
                       && originX + width >= world.OriginX + world.Width
                       && originY + height >= world.OriginY + world.Height;
        return contains ? WorldGenFootprintRelation.Contains : WorldGenFootprintRelation.Straddles;
    }

    /// <summary>
    ///     Determines whether the given world segment lies within a footprint's segment box.
    /// </summary>
    /// <param name="segment">World segment index.</param>
    /// <param name="originX">World X coordinate of the footprint origin.</param>
    /// <param name="originY">World Y coordinate of the footprint origin.</param>
    /// <param name="width">Width of the footprint in blocks.</param>
    /// <param name="height">Height of the footprint in blocks.</param>
    /// <param name="minSegmentZ">Lowest footprint segment Z index.</param>
    /// <param name="maxSegmentZ">Highest footprint segment Z index.</param>
    /// <returns>true if the segment intersects the footprint, false otherwise.</returns>
    public static bool SegmentIntersectsFootprint(GridPosition segment, int originX, int originY,
        int width, int height, int minSegmentZ, int maxSegmentZ)
    {
        return segment.X >= (originX >> 5) && segment.X <= ((originX + width - 1) >> 5)
               && segment.Y >= (originY >> 5) && segment.Y <= ((originY + height - 1) >> 5)
               && segment.Z >= minSegmentZ && segment.Z <= maxSegmentZ;
    }
}
