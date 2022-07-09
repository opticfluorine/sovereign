/*
 * Sovereign Engine
 * Copyright (c) 2022 opticfluorine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using MessagePack;

namespace Sovereign.EngineCore.Systems.WorldManagement;

/// <summary>
/// Encodes material and modifier IDs in a tuple.
/// First member is material ID, second is modifier ID.
/// </summary>
[MessagePackObject]
public sealed class BlockMaterialData
{

    /// <summary>
    /// Material ID.
    /// </summary>
    [Key(0)]
    int MaterialId;

    /// <summary>
    /// Material modifier ID.
    /// </summary>
    [Key(1)]
    int ModifierId;

}

/// <summary>
/// Encodes block data at a specific position along the x axis.
/// </summary>
[MessagePackObject]
public sealed class LinePositionedBlockData
{

    /// <summary>
    /// X offset from segment origin.
    /// </summary>
    [Key(0)]
    public byte OffsetX;

    /// <summary>
    /// Block material data.
    /// </summary>
    [Key(1)]
    public BlockMaterialData MaterialData;

}

/// <summary>
/// Encodes block data for a single horizontal (x) line of a world segment.
/// </summary>
[MessagePackObject]
public sealed class WorldSegmentBlockDataLine
{

    /// <summary>
    /// Y offset relative to segment origin of this line within its depth plane.
    /// </summary>
    [Key(0)]
    public byte OffsetY;

    /// <summary>
    /// Non-default block data along this line.
    /// The absence of a block at a given position indicates that that position
    /// is occupied by a block of the default material for this depth plane.
    /// </summary>
    [Key(1)]
    public IList<LinePositionedBlockData> BlockData;

}

/// <summary>
/// Encodes block data for a single depth (xy) plane of a world segment.
/// </summary>
[MessagePackObject]
public sealed class WorldSegmentBlockDataPlane
{

    /// <summary>
    /// Z offset relative to segment origin of this depth plane.
    /// </summary>
    [Key(0)]
    public byte OffsetZ;

    /// <summary>
    /// Lines within this plane containing one or more non-default blocks.
    /// The absence of a line for a given Y offset indicates that all positions
    /// on that line are occupied with blocks of the default material.
    /// </summary>
    [Key(1)]
    public IList<WorldSegmentBlockDataLine> Lines;

}

/// <summary>
/// Encodes block data for a world segment for sending from server to client.
/// </summary>
[MessagePackObject]
public sealed class WorldSegmentBlockData
{

    /// <summary>
    /// Default materials for unspecified blocks in each depth plane, 
    /// indexed by z offset from segment origin.
    /// </summary>
    [Key(0)]
    public BlockMaterialData[] DefaultMaterialsPerPlane;

    /// <summary>
    /// Depth planes containing non-default blocks within this world segment.
    /// The absence of a plane indicates that it is entirely comprised of the
    /// default block for that plane.
    /// </summary>
    [Key(1)]
    public IList<WorldSegmentBlockDataPlane> DataPlanes;

}
