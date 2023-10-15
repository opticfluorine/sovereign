/*
 * Sovereign Engine
 * Copyright (c) 2022 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using MessagePack;

namespace Sovereign.EngineCore.Systems.WorldManagement;

/// <summary>
///     Encodes material and modifier IDs in a tuple.
///     First member is material ID, second is modifier ID.
/// </summary>
[MessagePackObject]
public sealed class BlockMaterialData
{
    /// <summary>
    ///     Material ID.
    /// </summary>
    [Key(0)] public int MaterialId;

    /// <summary>
    ///     Material modifier ID.
    /// </summary>
    [Key(1)] public int ModifierId;

    public override bool Equals(object obj)
    {
        return obj is BlockMaterialData data &&
               MaterialId == data.MaterialId &&
               ModifierId == data.ModifierId;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(MaterialId, ModifierId);
    }
}

/// <summary>
///     Encodes block data at a specific position along the x axis.
/// </summary>
[MessagePackObject]
public sealed class LinePositionedBlockData
{
    /// <summary>
    ///     Block material data.
    /// </summary>
    [Key(1)] public BlockMaterialData MaterialData;

    /// <summary>
    ///     X offset from segment origin.
    /// </summary>
    [Key(0)] public byte OffsetX;
}

/// <summary>
///     Encodes block data for a single horizontal (x) line of a world segment.
/// </summary>
[MessagePackObject]
public sealed class WorldSegmentBlockDataLine
{
    /// <summary>
    ///     Non-default block data along this line.
    ///     The absence of a block at a given position indicates that that position
    ///     is occupied by a block of the default material for this depth plane.
    /// </summary>
    [Key(1)] public List<LinePositionedBlockData> BlockData;

    /// <summary>
    ///     Y offset relative to segment origin of this line within its depth plane.
    /// </summary>
    [Key(0)] public byte OffsetY;
}

/// <summary>
///     Encodes block data for a single depth (xy) plane of a world segment.
/// </summary>
[MessagePackObject]
public sealed class WorldSegmentBlockDataPlane
{
    /// <summary>
    ///     Lines within this plane containing one or more non-default blocks.
    ///     The absence of a line for a given Y offset indicates that all positions
    ///     on that line are occupied with blocks of the default material.
    /// </summary>
    [Key(1)] public List<WorldSegmentBlockDataLine> Lines;

    /// <summary>
    ///     Z offset relative to segment origin of this depth plane.
    /// </summary>
    [Key(0)] public byte OffsetZ;
}

/// <summary>
///     Encodes block data for a world segment for sending from server to client.
/// </summary>
[MessagePackObject]
public sealed class WorldSegmentBlockData
{
    /// <summary>
    ///     Depth planes containing non-default blocks within this world segment.
    ///     The absence of a plane indicates that it is entirely comprised of the
    ///     default block for that plane.
    /// </summary>
    [Key(1)] public List<WorldSegmentBlockDataPlane> DataPlanes;

    /// <summary>
    ///     Default materials for unspecified blocks in each depth plane,
    ///     indexed by z offset from segment origin.
    /// </summary>
    [Key(0)] public BlockMaterialData[] DefaultMaterialsPerPlane;
}