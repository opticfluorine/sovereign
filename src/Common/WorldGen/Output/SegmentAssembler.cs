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
using System.IO;
using System.Threading;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Configuration;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Network;
using Sovereign.EngineCore.Systems.WorldManagement;
using Sovereign.EngineCore.World;
using Sovereign.WorldGen.Biomes;
using Sovereign.WorldGen.Caves;
using Sovereign.WorldGen.Decorations;
using Sovereign.WorldGen.Terrain;

namespace Sovereign.WorldGen.Output;

/// <summary>
///     Counts of the outputs written by one segment assembly run.
/// </summary>
public sealed class AssemblyResult
{
    /// <summary>
    ///     Number of segment blobs written.
    /// </summary>
    public required int SegmentCount { get; init; }

    /// <summary>
    ///     Number of staged decorations written.
    /// </summary>
    public required int DecorationCount { get; init; }
}

/// <summary>
///     Converts the plan data of a completed world generation plan into per-segment
///     <see cref="WorldSegmentBlockData" /> blobs and staged decoration rows, streamed to
///     the plan's staging directory. The blobs use the same homogeneous-plane format that
///     <c>WorldSegmentBlockDataGenerator</c> produces at runtime, so committed worlds load
///     identically to editor-built ones.
/// </summary>
public sealed class SegmentAssembler
{
    private const int SegmentLength = (int)WorldConstants.SegmentLength;
    private const int CellsPerPlane = SegmentLength * SegmentLength;

    private readonly WorldGenProfile profile;
    private readonly TerrainMap terrain;
    private readonly ColumnMaterials? materials;
    private readonly CaveMap? caves;
    private readonly IReadOnlyList<DecorationPlacement> decorations;
    private readonly WorldGenResolvedTemplates templates;
    private readonly int originX;
    private readonly int originY;
    private readonly string stagingDirectory;

    /// <summary>
    ///     Block template IDs by palette index; index 0 is unused and stands for air.
    /// </summary>
    private readonly List<ulong> palette = new() { 0 };

    /// <summary>
    ///     Palette index by template name.
    /// </summary>
    private readonly Dictionary<string, byte> paletteIndexByName = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     Palette index of the water block template.
    /// </summary>
    private readonly byte waterIndex;

    /// <summary>
    ///     Palette index of each Z level from the rock floor to the terrain surface maximum.
    /// </summary>
    private readonly byte[] stoneByZ = null!;

    /// <summary>
    ///     Palette index of the bedrock template.
    /// </summary>
    private readonly byte bedrockIndex;

    /// <summary>
    ///     Creates a segment assembler for a completed plan.
    /// </summary>
    /// <param name="profile">Validated profile of the plan.</param>
    /// <param name="terrain">Terrain surface map of the plan.</param>
    /// <param name="materials">Per-column material assignment, or null if the profile has
    ///     no biomes section.</param>
    /// <param name="caves">Carved cave map, or null if the profile has no cave levels.</param>
    /// <param name="decorations">Decoration placements, or null if the profile has no
    ///     biomes section.</param>
    /// <param name="templates">Profile template names resolved against the live template
    ///     entity set.</param>
    /// <param name="originX">World X coordinate of footprint-local cell (0, 0).</param>
    /// <param name="originY">World Y coordinate of footprint-local cell (0, 0).</param>
    /// <param name="stagingDirectory">Absolute path of the plan's staging directory.</param>
    public SegmentAssembler(WorldGenProfile profile, TerrainMap terrain,
        ColumnMaterials? materials, CaveMap? caves,
        IReadOnlyList<DecorationPlacement>? decorations, WorldGenResolvedTemplates templates,
        int originX, int originY, string stagingDirectory)
    {
        this.profile = profile;
        this.terrain = terrain;
        this.materials = materials;
        this.caves = caves;
        this.decorations = decorations ?? new List<DecorationPlacement>();
        this.templates = templates;
        this.originX = originX;
        this.originY = originY;
        this.stagingDirectory = stagingDirectory;

        waterIndex = PaletteIndexOf("Water");
        bedrockIndex = PaletteIndexOf(profile.BedrockTemplate);
        stoneByZ = BuildStoneByZ();
    }

    /// <summary>
    ///     Inclusive Z range of the world segments spanned by the profile, from the segment
    ///     containing the bedrock layer to the segment containing the terrain surface maximum.
    /// </summary>
    /// <param name="profile">World generation profile.</param>
    /// <returns>Segment Z range.</returns>
    public static (int MinSegmentZ, int MaxSegmentZ) SegmentZRange(WorldGenProfile profile)
    {
        return (profile.BedrockZ >> 5, profile.SurfaceMaxZ >> 5);
    }

    /// <summary>
    ///     Assembles every segment of the plan footprint and the staged decoration rows,
    ///     writing them to the staging directory.
    /// </summary>
    /// <param name="progress">Optional callback invoked with a progress description.</param>
    /// <returns>Counts of the written outputs.</returns>
    public AssemblyResult Assemble(Action<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var segmentsDirectory = EnsureSegmentsDirectory();
        var (minSegmentZ, maxSegmentZ) = SegmentZRange(profile);
        var (minSegmentX, maxSegmentX) = SegmentRange(profile.Width, originX);
        var (minSegmentY, maxSegmentY) = SegmentRange(profile.Height, originY);
        var segmentZLevels = maxSegmentZ - minSegmentZ + 1;

        progress?.Invoke("Assembly: writing segment blobs (0%)");
        var segmentCount = 0;
        var totalSegments = 0L;
        for (var sz = minSegmentZ; sz <= maxSegmentZ; ++sz)
        {
            totalSegments += (long)(maxSegmentX - minSegmentX + 1)
                             * (maxSegmentY - minSegmentY + 1);
        }

        for (var sz = minSegmentZ; sz <= maxSegmentZ; ++sz)
        {
            cancellationToken.ThrowIfCancellationRequested();
            for (var sy = minSegmentY; sy <= maxSegmentY; ++sy)
            {
                for (var sx = minSegmentX; sx <= maxSegmentX; ++sx)
                {
                    var data = AssembleSegment(sx, sy, sz);
                    var path = Path.Combine(segmentsDirectory, $"{sx}_{sy}_{sz}.bin");
                    File.WriteAllBytes(path, MessageConfig.SerializeMsgPack(data));
                    ++segmentCount;
                }
            }

            progress?.Invoke($"Assembly: writing segment blobs " +
                             $"{(sz - minSegmentZ + 1) * 100 / segmentZLevels}%");
        }

        cancellationToken.ThrowIfCancellationRequested();
        progress?.Invoke($"Assembly: staging decorations ({segmentCount * 100 / Math.Max(totalSegments, 1)}%)");
        var decorationCount = StageDecorations();
        progress?.Invoke("Assembly: complete (100%)");

        return new AssemblyResult { SegmentCount = segmentCount, DecorationCount = decorationCount };
    }

    /// <summary>
    ///     Assembles the block data of a single world segment from the plan's column model.
    /// </summary>
    /// <param name="segmentX">World segment X index.</param>
    /// <param name="segmentY">World segment Y index.</param>
    /// <param name="segmentZ">World segment Z index.</param>
    /// <returns>Assembled block data.</returns>
    public WorldSegmentBlockData AssembleSegment(int segmentX, int segmentY, int segmentZ)
    {
        var grid = new byte[SegmentLength * SegmentLength * SegmentLength];
        FillStone(segmentX, segmentY, segmentZ, grid);
        CarveCaves(segmentX, segmentY, segmentZ, grid);
        FillWater(segmentX, segmentY, segmentZ, grid);
        return BakePlanes(grid, segmentZ);
    }

    /// <summary>
    ///     Stages the decoration rows of the plan to the staging directory with the plan
    ///     origin applied. Positions are absolute world coordinates.
    /// </summary>
    /// <returns>Number of staged decorations.</returns>
    private int StageDecorations()
    {
        var rows = new List<StagedDecoration>(decorations.Count);
        foreach (var placement in decorations)
        {
            if (!templates.TryGetId(placement.TemplateName, out var templateId)) continue;

            rows.Add(new StagedDecoration
            {
                TemplateEntityId = templateId,
                X = originX + placement.X,
                Y = originY + placement.Y,
                Z = placement.Z
            });
        }

        var path = Path.Combine(stagingDirectory, "decorations.bin");
        File.WriteAllBytes(path, MessageConfig.SerializeMsgPack(rows));
        return rows.Count;
    }

    /// <summary>
    ///     Fills the solid column material of every cell of a segment from the surface,
    ///     subsurface, stone band, and bedrock model.
    /// </summary>
    /// <param name="segmentX">World segment X index.</param>
    /// <param name="segmentY">World segment Y index.</param>
    /// <param name="segmentZ">World segment Z index.</param>
    /// <param name="grid">Cell material grid to fill, indexed by plane-major offsets.</param>
    private void FillStone(int segmentX, int segmentY, int segmentZ, byte[] grid)
    {
        var baseX = segmentX * SegmentLength;
        var baseY = segmentY * SegmentLength;
        var baseZ = segmentZ * SegmentLength;
        var localOriginX = baseX - originX;
        var localOriginY = baseY - originY;
        var zStart = Math.Max(baseZ, profile.BedrockZ);
        var zEnd = Math.Min(baseZ + SegmentLength - 1, profile.SurfaceMaxZ);

        for (var dy = 0; dy < SegmentLength; ++dy)
        {
            var ly = localOriginY + dy;
            if (ly < 0 || ly >= profile.Height) continue;

            for (var dx = 0; dx < SegmentLength; ++dx)
            {
                var lx = localOriginX + dx;
                if (lx < 0 || lx >= profile.Width) continue;

                for (var z = zStart; z <= zEnd; ++z)
                {
                    grid[GridIndex(dx, dy, z - baseZ)] = MaterialOf(lx, ly, z);
                }
            }
        }
    }

    /// <summary>
    ///     Determines the solid block material of a column cell from the column model.
    /// </summary>
    /// <param name="lx">Footprint-local X coordinate.</param>
    /// <param name="ly">Footprint-local Y coordinate.</param>
    /// <param name="z">World Z coordinate.</param>
    /// <returns>Palette index of the solid material.</returns>
    private byte MaterialOf(int lx, int ly, int z)
    {
        if (z == profile.BedrockZ) return bedrockIndex;

        var h = terrain.Heights[lx, ly];
        if (z > h) return 0;

        if (z == h) return SurfaceIndexOf(lx, ly);
        if (materials is { } mats)
        {
            var depth = mats.SubSurfaceDepth[lx, ly];
            if (z > h - depth) return PaletteIndexOf(mats.SubSurfaceTemplate[lx, ly]);
        }

        return stoneByZ[Math.Clamp(z - profile.RockFloorZ, 0, stoneByZ.Length - 1)];
    }

    /// <summary>
    ///     Determines the surface material index of a column.
    /// </summary>
    /// <param name="lx">Footprint-local X coordinate.</param>
    /// <param name="ly">Footprint-local Y coordinate.</param>
    /// <returns>Palette index of the surface material.</returns>
    private byte SurfaceIndexOf(int lx, int ly)
    {
        return materials is { } mats
            ? PaletteIndexOf(mats.SurfaceTemplate[lx, ly])
            : stoneByZ[profile.SurfaceMaxZ - profile.RockFloorZ];
    }

    /// <summary>
    ///     Carves cave air into a segment: open level cells, shaft steps and centers, and
    ///     surface mouths. Ring columns of shafts are kept solid across the shaft Z range
    ///     so that a shaft crossing a chamber wall merges cleanly. Bedrock is never carved.
    /// </summary>
    /// <param name="segmentX">World segment X index.</param>
    /// <param name="segmentY">World segment Y index.</param>
    /// <param name="segmentZ">World segment Z index.</param>
    /// <param name="grid">Cell material grid to carve.</param>
    private void CarveCaves(int segmentX, int segmentY, int segmentZ, byte[] grid)
    {
        if (caves is null) return;

        var baseX = segmentX * SegmentLength;
        var baseY = segmentY * SegmentLength;
        var baseZ = segmentZ * SegmentLength;
        var localOriginX = baseX - originX;
        var localOriginY = baseY - originY;

        foreach (var level in caves.Levels)
        {
            var lyStart = Math.Max(0, localOriginY);
            var lyEnd = Math.Min(profile.Height - 1, localOriginY + SegmentLength - 1);
            var lxStart = Math.Max(0, localOriginX);
            var lxEnd = Math.Min(profile.Width - 1, localOriginX + SegmentLength - 1);
            for (var ly = lyStart; ly <= lyEnd; ++ly)
            {
                for (var lx = lxStart; lx <= lxEnd; ++lx)
                {
                    if (!level.Open[lx, ly]) continue;
                    Carve(grid, localOriginX, localOriginY, baseZ, lx, ly,
                        level.FloorZ[lx, ly] + 1,
                        level.FloorZ[lx, ly] + level.CarveHeight[lx, ly]);
                }
            }
        }

        foreach (var shaft in caves.Shafts)
        {
            foreach (var (rx, ry) in shaft.Ring)
            {
                // Ring columns stay solid across the shaft Z range: undo any level carving.
                if (rx < localOriginX || rx >= localOriginX + SegmentLength
                                       || ry < localOriginY || ry >= localOriginY + SegmentLength)
                {
                    continue;
                }

                for (var z = Math.Max(shaft.LowerFloorZ, baseZ);
                     z <= Math.Min(shaft.RingTopZ, baseZ + SegmentLength - 1);
                     ++z)
                {
                    if (z == profile.BedrockZ) continue;
                    grid[GridIndex(rx - localOriginX, ry - localOriginY, z - baseZ)] =
                        MaterialOf(rx, ry, z);
                }
            }

            Carve(grid, localOriginX, localOriginY, baseZ, shaft.CenterX, shaft.CenterY,
                shaft.LowerFloorZ + 1, shaft.UpperFloorZ + shaft.Headroom);

            foreach (var column in shaft.Columns)
            {
                foreach (var stepZ in column.StepZs)
                {
                    Carve(grid, localOriginX, localOriginY, baseZ, column.X, column.Y,
                        stepZ + 1, stepZ + shaft.Headroom);
                }
            }
        }

        foreach (var mouth in caves.MouthList)
        {
            for (var my = mouth.Y; my <= mouth.Y + 1; ++my)
            {
                for (var mx = mouth.X; mx <= mouth.X + 1; ++mx)
                {
                    Carve(grid, localOriginX, localOriginY, baseZ, mx, my,
                        mouth.FloorZ + 1, mouth.SurfaceZ);
                }
            }
        }
    }

    /// <summary>
    ///     Carves an air column into a segment grid at a footprint-local column, clipped to
    ///     the segment window and the footprint bounds. Bedrock is never carved.
    /// </summary>
    /// <param name="grid">Cell material grid to carve.</param>
    /// <param name="localOriginX">Footprint-local X offset of the segment origin.</param>
    /// <param name="localOriginY">Footprint-local Y offset of the segment origin.</param>
    /// <param name="baseZ">World Z of the segment base plane.</param>
    /// <param name="lx">Footprint-local X coordinate of the column.</param>
    /// <param name="ly">Footprint-local Y coordinate of the column.</param>
    /// <param name="fromZ">Lowest Z to carve.</param>
    /// <param name="toZ">Highest Z to carve.</param>
    private void Carve(byte[] grid, int localOriginX, int localOriginY, int baseZ,
        int lx, int ly, int fromZ, int toZ)
    {
        var dx = lx - localOriginX;
        var dy = ly - localOriginY;
        if (dx < 0 || dx >= SegmentLength || dy < 0 || dy >= SegmentLength) return;
        if (lx < 0 || lx >= profile.Width || ly < 0 || ly >= profile.Height) return;

        var from = Math.Max(fromZ, baseZ);
        var to = Math.Min(toZ, baseZ + SegmentLength - 1);
        for (var z = from; z <= to; ++z)
        {
            if (z == profile.BedrockZ) continue;
            grid[GridIndex(dx, dy, z - baseZ)] = 0;
        }
    }

    /// <summary>
    ///     Fills water into a segment: ocean and shelf columns from sea level down to the
    ///     floor, lakes from their surface Z, and river channels down from the carved
    ///     surface.
    /// </summary>
    /// <param name="segmentX">World segment X index.</param>
    /// <param name="segmentY">World segment Y index.</param>
    /// <param name="segmentZ">World segment Z index.</param>
    /// <param name="grid">Cell material grid to fill.</param>
    private void FillWater(int segmentX, int segmentY, int segmentZ, byte[] grid)
    {
        var baseX = segmentX * SegmentLength;
        var baseY = segmentY * SegmentLength;
        var baseZ = segmentZ * SegmentLength;
        var localOriginX = baseX - originX;
        var localOriginY = baseY - originY;

        for (var dy = 0; dy < SegmentLength; ++dy)
        {
            var ly = localOriginY + dy;
            if (ly < 0 || ly >= profile.Height) continue;

            for (var dx = 0; dx < SegmentLength; ++dx)
            {
                var lx = localOriginX + dx;
                if (lx < 0 || lx >= profile.Width) continue;

                var waterTop = WaterTopZ(lx, ly);
                if (waterTop < 0) continue;

                var floorZ = terrain.Heights[lx, ly] + 1;
                var from = Math.Max(floorZ, baseZ);
                var to = Math.Min(waterTop, baseZ + SegmentLength - 1);
                for (var z = from; z <= to; ++z)
                {
                    grid[GridIndex(dx, dy, z - baseZ)] = waterIndex;
                }
            }
        }
    }

    /// <summary>
    ///     Determines the water surface Z of a column, or -1 if the column holds no water.
    /// </summary>
    /// <param name="lx">Footprint-local X coordinate.</param>
    /// <param name="ly">Footprint-local Y coordinate.</param>
    /// <returns>Water surface Z, or -1.</returns>
    private int WaterTopZ(int lx, int ly)
    {
        if (terrain.IsOcean[lx, ly]) return profile.SeaLevelZ;
        if (terrain.IsLake[lx, ly]) return terrain.LakeSurfaceZ[lx, ly];
        if (terrain.IsRiver[lx, ly]) return terrain.Heights[lx, ly] + RiverChannelDepth;
        return -1;
    }

    /// <summary>
    ///     Depth in blocks of the water column carried by a river channel above its carved
    ///     floor.
    /// </summary>
    private const int RiverChannelDepth = 2;

    /// <summary>
    ///     Bakes the cell grid of a segment into the homogeneous-plane block data format,
    ///     matching the output of the runtime <c>WorldSegmentBlockDataGenerator</c>: each
    ///     depth plane carries its most frequent material as the plane default with sparse
    ///     exceptions, and planes with a non-air default synthesize explicit air cells.
    /// </summary>
    /// <param name="grid">Cell material grid of the segment.</param>
    /// <param name="segmentZ">World segment Z index.</param>
    /// <returns>Baked block data.</returns>
    private WorldSegmentBlockData BakePlanes(byte[] grid, int segmentZ)
    {
        var data = new WorldSegmentBlockData
        {
            DefaultsPerPlane = new BlockData[SegmentLength],
            DataPlanes = new List<WorldSegmentBlockDataPlane>()
        };

        for (var dz = 0; dz < SegmentLength; ++dz)
        {
            BakePlane(grid, dz, data);
        }

        return data;
    }

    /// <summary>
    ///     Bakes one depth plane of a segment grid into its plane default and sparse
    ///     exception lines.
    /// </summary>
    /// <param name="cells">Cell material grid of the segment.</param>
    /// <param name="dz">Depth offset of the plane within the segment.</param>
    /// <param name="target">Block data to fill.</param>
    private void BakePlane(byte[] cells, int dz, WorldSegmentBlockData target)
    {
        var airCount = CellsPerPlane;
        var leadingCount = 0;
        var defaultBlock = new BlockData { BlockType = BlockDataType.Air };
        var counts = new Dictionary<BlockData, int>();
        var blocks = new List<(int Dx, int Dy, BlockData Data)>();

        var planeBase = dz * CellsPerPlane;
        for (var dy = 0; dy < SegmentLength; ++dy)
        {
            for (var dx = 0; dx < SegmentLength; ++dx)
            {
                var paletteIndex = cells[planeBase + dy * SegmentLength + dx];
                if (paletteIndex == 0) continue;

                --airCount;
                var blockData = new BlockData
                {
                    BlockType = BlockDataType.Template,
                    TemplateIdOffset = palette[paletteIndex] - EntityConstants.FirstTemplateEntityId
                };
                blocks.Add((dx, dy, blockData));

                var count = counts.GetValueOrDefault(blockData) + 1;
                counts[blockData] = count;
                if (count > leadingCount)
                {
                    leadingCount = count;
                    defaultBlock = blockData;
                }
            }
        }

        if (airCount > leadingCount)
        {
            defaultBlock = new BlockData { BlockType = BlockDataType.Air };
        }

        target.DefaultsPerPlane[dz] = defaultBlock;
        if (blocks.Count == 0 && defaultBlock.BlockType == BlockDataType.Air) return;

        var plane = new WorldSegmentBlockDataPlane { OffsetZ = (byte)dz };
        var lines = new WorldSegmentBlockDataLine?[SegmentLength];

        foreach (var (dx, dy, block) in blocks)
        {
            if (!block.Equals(defaultBlock)) AddPlaneBlock(plane, lines, dx, dy, block);
        }

        if (defaultBlock.BlockType != BlockDataType.Air)
        {
            // Synthesize explicit air wherever the plane default is solid.
            var filled = new bool[CellsPerPlane];
            foreach (var (dx, dy, _) in blocks) filled[dy * SegmentLength + dx] = true;

            var air = new BlockData { BlockType = BlockDataType.Air };
            for (var dy = 0; dy < SegmentLength; ++dy)
            {
                for (var dx = 0; dx < SegmentLength; ++dx)
                {
                    if (!filled[dy * SegmentLength + dx]) AddPlaneBlock(plane, lines, dx, dy, air);
                }
            }
        }

        if (plane.Lines.Count > 0) target.DataPlanes.Add(plane);
    }

    /// <summary>
    ///     Appends one positioned block or explicit air entry to a plane, creating the line
    ///     on first use. Lines are added to the plane in ascending Y order.
    /// </summary>
    /// <param name="plane">Plane being built.</param>
    /// <param name="lines">Line lookup by Y offset.</param>
    /// <param name="dx">X offset within the plane.</param>
    /// <param name="dy">Y offset within the plane.</param>
    /// <param name="block">Block data to append.</param>
    private static void AddPlaneBlock(WorldSegmentBlockDataPlane plane,
        WorldSegmentBlockDataLine?[] lines, int dx, int dy, BlockData block)
    {
        var line = lines[dy] ??= new WorldSegmentBlockDataLine
        {
            OffsetY = (byte)dy,
            BlockData = new List<LinePositionedBlockData>()
        };
        if (line.BlockData.Count == 0) plane.Lines.Add(line);

        line.BlockData.Add(new LinePositionedBlockData
        {
            OffsetX = (byte)dx,
            Data = block
        });
    }

    /// <summary>
    ///     Builds the palette index of each Z level from the rock floor to the surface
    ///     maximum: the stone band containing the level, or the topmost band above the
    ///     bands' coverage.
    /// </summary>
    /// <returns>Palette indices indexed by <c>z - RockFloorZ</c>.</returns>
    private byte[] BuildStoneByZ()
    {
        var bands = profile.StoneBands;
        var topBand = PaletteIndexOf(bands[0].Template);
        var stone = new byte[profile.SurfaceMaxZ - profile.RockFloorZ + 1];
        for (var z = profile.RockFloorZ; z <= profile.SurfaceMaxZ; ++z)
        {
            var index = topBand;
            foreach (var band in bands)
            {
                if (z >= band.FromZ && z <= band.ToZ)
                {
                    index = PaletteIndexOf(band.Template);
                    break;
                }
            }

            stone[z - profile.RockFloorZ] = index;
        }

        return stone;
    }

    /// <summary>
    ///     Gets the palette index of a template name, registering the template on first use.
    /// </summary>
    /// <param name="name">Block template name.</param>
    /// <returns>Palette index.</returns>
    private byte PaletteIndexOf(string name)
    {
        if (paletteIndexByName.TryGetValue(name, out var index)) return index;

        if (!templates.TryGetId(name, out var templateId))
        {
            throw new InvalidOperationException(
                $"Template \"{name}\" was not resolved for the plan.");
        }

        index = (byte)palette.Count;
        palette.Add(templateId);
        paletteIndexByName[name] = index;
        return index;
    }

    /// <summary>
    ///     Computes the inclusive world segment index range covering a footprint dimension.
    /// </summary>
    /// <param name="dimension">Footprint dimension in blocks.</param>
    /// <param name="origin">World coordinate of footprint-local cell 0.</param>
    /// <returns>Inclusive segment index range.</returns>
    private static (int Min, int Max) SegmentRange(int dimension, int origin)
    {
        return (origin >> 5, (origin + dimension - 1) >> 5);
    }

    /// <summary>
    ///     Creates the segments directory of the staging directory.
    /// </summary>
    /// <returns>Absolute path of the segments directory.</returns>
    private string EnsureSegmentsDirectory()
    {
        var path = Path.Combine(stagingDirectory, "segments");
        Directory.CreateDirectory(path);
        return path;
    }

    /// <summary>
    ///     Computes the plane-major grid index of a cell.
    /// </summary>
    /// <param name="dx">X offset within the segment.</param>
    /// <param name="dy">Y offset within the segment.</param>
    /// <param name="dz">Z offset within the segment.</param>
    /// <returns>Grid index.</returns>
    private static int GridIndex(int dx, int dy, int dz)
    {
        return dz * CellsPerPlane + dy * SegmentLength + dx;
    }
}
