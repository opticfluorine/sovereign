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

using System.Collections.Generic;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Network;
using Sovereign.EngineCore.Systems.WorldManagement;
using Sovereign.EngineCore.World;
using Sovereign.ServerCore.Systems.WorldManagement;
using Sovereign.WorldGen;
using Sovereign.WorldGen.Biomes;
using Sovereign.WorldGen.Caves;
using Sovereign.WorldGen.Decorations;
using Sovereign.WorldGen.Output;
using Sovereign.WorldGen.Terrain;
using Xunit;
using Microsoft.Extensions.Logging.Abstractions;

namespace TestWorldGen;

/// <summary>
///     Tests of the segment assembler: byte identity with the runtime block data
///     generator, cave carve correctness, and water integrity.
/// </summary>
public class TestSegmentAssembler : IDisposable
{
    /// <summary>
    ///     Root directory of the test staging area.
    /// </summary>
    private readonly string stagingRoot;

    public TestSegmentAssembler()
    {
        stagingRoot = Path.Combine(Path.GetTempPath(), "worldgen-assembler-test",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(stagingRoot);
    }

    public void Dispose()
    {
        Directory.Delete(stagingRoot, true);
    }

    [Fact]
    public void AssembleSegment_MatchesRuntimeGeneratorByteForByte()
    {
        var profile = SmallProfile();
        var terrain = UniformTerrain(profile, height: 20);
        var materials = UniformMaterials(profile, "Grass", "Dirt", depth: 3);
        var resolved = Resolved("Bedrock", "Water", "Granite", "Grass", "Dirt");
        var assembler = new SegmentAssembler(profile, terrain, materials, null, null, resolved,
            0, 0, StagingDir("identity"));
        assembler.Assemble();

        // Decode the blob for a fully underground segment, rebuild its blocks as block
        // entities in real component collections, and regenerate with the runtime
        // generator. The blobs must be byte-identical.
        var segmentIndex = new GridPosition { X = 0, Y = 0, Z = -1 };
        var data = ReadSegmentBlob(StagingDir("identity"), segmentIndex);

        var world = new BlockWorldFixture();
        foreach (var (position, templateId) in DecodeTemplateBlocks(data, segmentIndex))
        {
            world.AddBlock(position, templateId);
        }

        world.Commit();
        var regenerated = world.Generator.Create(segmentIndex);

        Assert.Equal(MessageConfig.SerializeMsgPack(data),
            MessageConfig.SerializeMsgPack(regenerated));
    }

    [Fact]
    public void Assemble_CoversFootprintTimesZRange()
    {
        var profile = SmallProfile(width: 96, height: 64);
        var terrain = UniformTerrain(profile, height: 20);
        var assembler = new SegmentAssembler(profile, terrain, null, null, null,
            TestResolvedTemplates.ForProfile(profile), 0, 0, StagingDir("coverage"));
        assembler.Assemble();

        var segments = Directory.GetFiles(Path.Combine(StagingDir("coverage"), "segments"),
            "*.bin");
        // 96x64 columns span 3x2 segments in x/y and 3 segments in z (bedrock -64..surface 28).
        Assert.Equal(3 * 2 * 3, segments.Length);
    }

    [Fact]
    public void AssembleSegment_OriginAppliedToBlobCoordinates()
    {
        var profile = SmallProfile();
        var terrain = UniformTerrain(profile, height: 20);
        var assembler = new SegmentAssembler(profile, terrain, null, null, null,
            TestResolvedTemplates.ForProfile(profile), 32, -32, StagingDir("origin"));
        assembler.Assemble();

        // With origin (32, -32), the footprint covers x segments 1..2 and y segments -1..0.
        var path = Path.Combine(StagingDir("origin"), "segments", "1_-1_-1.bin");
        Assert.True(File.Exists(path), "Segment blob should be named by world segment index.");
        var data = MessageConfig.DeserializeMsgPack<WorldSegmentBlockData>(File.ReadAllBytes(path));
        Assert.NotEmpty(data.DefaultsPerPlane);
    }

    [Fact]
    public void AssembleSegment_CarvesCavesShaftsAndMouths()
    {
        var profile = SmallProfile();
        var terrain = UniformTerrain(profile, height: 20);
        var caves = HandBuiltCaveMap(profile);
        var assembler = new SegmentAssembler(profile, terrain, null, caves, null,
            TestResolvedTemplates.ForProfile(profile), 0, 0, StagingDir("caves"));
        assembler.Assemble();

        var cells = ReadSegmentCells(StagingDir("caves"),
            new GridPosition { X = 0, Y = 0, Z = -1 });

        // Open level cells are air above the floor and solid at the floor.
        Assert.Equal(Air, cells[(10, 10, -15)]);
        Assert.Equal(Air, cells[(10, 10, -14)]);
        Assert.NotEqual(Air, cells[(10, 10, -16)]);

        // Shaft step columns are solid below each step with headroom above.
        Assert.NotEqual(Air, cells[(12, 11, -21)]);
        Assert.Equal(Air, cells[(12, 11, -20)]);
        Assert.Equal(Air, cells[(12, 11, -19)]);

        // The shaft center is carved open between the floors.
        Assert.Equal(Air, cells[(12, 12, -18)]);
        Assert.Equal(Air, cells[(12, 12, -17)]);

        // Mouth columns carve through to the surface: the shaft is open down to the
        // level floor, which itself stays solid.
        Assert.Equal(Air, cells[(20, 20, -15)]);
        Assert.NotEqual(Air, cells[(20, 20, -16)]);

        var surfaceCells = ReadSegmentCells(StagingDir("caves"),
            new GridPosition { X = 0, Y = 0, Z = 0 });

        // The mouth removes the surface block itself and stays open to the sky.
        Assert.Equal(Air, surfaceCells[(20, 20, 20)]);
        Assert.Equal(Air, surfaceCells[(20, 20, 0)]);

        // Columns away from the mouth keep their surface block.
        Assert.NotEqual(Air, surfaceCells[(10, 10, 20)]);
    }

    [Fact]
    public void AssembleSegment_ShaftRingStaysSolid()
    {
        var profile = SmallProfile();
        var terrain = UniformTerrain(profile, height: 20);
        var caves = HandBuiltCaveMap(profile);
        var assembler = new SegmentAssembler(profile, terrain, null, caves, null,
            TestResolvedTemplates.ForProfile(profile), 0, 0, StagingDir("ring"));
        assembler.Assemble();

        var segmentIndex = new GridPosition { X = 0, Y = 0, Z = -1 };
        var cells = ReadSegmentCells(StagingDir("ring"), segmentIndex);

        // Ring columns of the shaft are re-solidified across the shaft Z range even
        // though they lie inside the level's open band.
        Assert.NotEqual(Air, cells[(14, 12, -15)]);
        Assert.NotEqual(Air, cells[(14, 12, -14)]);
        Assert.NotEqual(Air, cells[(14, 11, -14)]);
    }

    [Fact]
    public void AssembleSegment_WaterIntegrity()
    {
        var profile = SmallProfile();
        var terrain = UniformTerrain(profile, height: 20);
        terrain.IsOcean[2, 2] = true;
        terrain.Heights[2, 2] = 2;
        terrain.IsLake[3, 3] = true;
        terrain.Heights[3, 3] = 5;
        terrain.LakeSurfaceZ[3, 3] = 10;
        terrain.IsRiver[4, 4] = true;
        terrain.Heights[4, 4] = 18;

        var materials = UniformMaterials(profile, "Grass", "Dirt", depth: 3);
        materials.SurfaceTemplate[2, 2] = "Gravel";
        materials.SurfaceTemplate[3, 3] = "Sand";
        materials.SurfaceTemplate[4, 4] = "Sand";

        var assembler = new SegmentAssembler(profile, terrain, materials, null, null,
            Resolved("Bedrock", "Water", "Granite", "Gravel", "Sand", "Grass", "Dirt"),
            0, 0, StagingDir("water"));
        assembler.Assemble();

        var segmentIndex = new GridPosition { X = 0, Y = 0, Z = 0 };
        var cells = ReadSegmentCells(StagingDir("water"), segmentIndex);
        var waterId = cells[(2, 2, 12)];

        // Ocean column: water from sea level down to the floor; floor block below.
        Assert.Equal(waterId, cells[(2, 2, 3)]);
        Assert.Equal(waterId, cells[(2, 2, 12)]);
        Assert.NotEqual(Air, cells[(2, 2, 2)]);
        Assert.NotEqual(waterId, cells[(2, 2, 2)]);

        // Lake column: water from the lake surface Z down to the floor.
        Assert.Equal(waterId, cells[(3, 3, 6)]);
        Assert.Equal(waterId, cells[(3, 3, 10)]);
        Assert.Equal(Air, cells[(3, 3, 11)]);

        // River column: a shallow water channel above the carved floor.
        Assert.Equal(waterId, cells[(4, 4, 19)]);
        Assert.Equal(waterId, cells[(4, 4, 20)]);
        Assert.Equal(Air, cells[(4, 4, 21)]);
    }

    [Fact]
    public void Assemble_NoWaterUnderCaves()
    {
        var profile = SmallProfile();
        var terrain = UniformTerrain(profile, height: 20);
        terrain.IsLake[5, 5] = true;
        terrain.Heights[5, 5] = 5;
        terrain.LakeSurfaceZ[5, 5] = 10;

        // A cave level directly under the lake that is excluded from carving by the
        // water-proximity rule: the water column must survive assembly untouched.
        var level = new CaveLevelMap
        {
            Width = profile.Width,
            Height = profile.Height,
            BaseFloorZ = -16,
            Headroom = 2,
            Open = new bool[profile.Width, profile.Height],
            FloorZ = Filled(profile, -16),
            CarveHeight = Filled(profile, (byte)2),
            Worley = new byte[profile.Width, profile.Height],
            WaterExcluded = new bool[profile.Width, profile.Height]
        };
        level.WaterExcluded[5, 5] = true;
        var caves = new CaveMap
        {
            Width = profile.Width,
            Height = profile.Height,
            Levels = new List<CaveLevelMap> { level },
            Shafts = new List<CaveShaft>(),
            MouthList = new List<CaveMouth>()
        };

        var materials = UniformMaterials(profile, "Grass", "Dirt", depth: 3);
        materials.SurfaceTemplate[5, 5] = "Sand";
        var assembler = new SegmentAssembler(profile, terrain, materials, caves, null,
            Resolved("Bedrock", "Water", "Granite", "Sand", "Grass", "Dirt"),
            0, 0, StagingDir("perched"));
        assembler.Assemble();

        var waterCells = ReadSegmentCells(StagingDir("perched"), new GridPosition
        {
            X = 0,
            Y = 0,
            Z = 0
        });
        Assert.NotEqual(Air, waterCells[(5, 5, 6)]);

        var caveCells = ReadSegmentCells(StagingDir("perched"), new GridPosition
        {
            X = 0,
            Y = 0,
            Z = -1
        });
        // The excluded cell is not carved: it stays stone from the bands.
        Assert.NotEqual(Air, caveCells[(5, 5, -15)]);
    }

    [Fact]
    public void Assemble_StagesDecorationsWithAbsoluteCoordinates()
    {
        var profile = SmallProfile();
        var terrain = UniformTerrain(profile, height: 20);
        var decorations = new List<DecorationPlacement>
        {
            new()
            {
                TemplateName = "Oak Tree", X = 1, Y = 2, Z = 21, Biome = BiomeId.Grassland,
                PoolIndex = 0
            },
            new()
            {
                TemplateName = "Boulder", X = 40, Y = 50, Z = 21, Biome = BiomeId.Grassland,
                PoolIndex = 1
            }
        };
        var resolved = Resolved("Water", "Bedrock", "Granite",
            ("Oak Tree", 0x7FFE000000000011UL), ("Boulder", 0x7FFE000000000014UL));
        var assembler = new SegmentAssembler(profile, terrain, null, null, decorations,
            resolved, 100, -60, StagingDir("decorations"));
        assembler.Assemble();

        var rows = MessageConfig.DeserializeMsgPack<List<StagedDecoration>>(
            File.ReadAllBytes(Path.Combine(StagingDir("decorations"), "decorations.bin")))!;

        Assert.Equal(2, rows.Count);
        Assert.Equal(0x7FFE000000000011UL, rows[0].TemplateEntityId);
        Assert.Equal(101f, rows[0].X);
        Assert.Equal(-58f, rows[0].Y);
        Assert.Equal(21f, rows[0].Z);
        Assert.Equal(140f, rows[1].X);
        Assert.Equal(-10f, rows[1].Y);
    }

    /// <summary>
    ///     Sentinel marking an air (absent) cell in decoded segment data.
    /// </summary>
    private const ulong Air = ulong.MaxValue;

    /// <summary>
    ///     Builds a resolved template set from explicit names, assigning sequential
    ///     placeholder template entity IDs.
    /// </summary>
    /// <param name="names">Template names, or name-ID pairs.</param>
    /// <returns>Resolved template set.</returns>
    private static WorldGenResolvedTemplates Resolved(params object[] names)
    {
        var resolved = new List<(string Name, ulong TemplateEntityId)>();
        ulong nextId = 0x7FFE000000000000;
        foreach (var name in names)
        {
            switch (name)
            {
                case string n:
                    resolved.Add((n, nextId));
                    break;
                case (string n, ulong id):
                    resolved.Add((n, id));
                    break;
            }

            ++nextId;
        }

        return new WorldGenResolvedTemplates(resolved);
    }

    /// <summary>
    ///     Creates a small single-band profile.
    /// </summary>
    /// <param name="width">Width in blocks.</param>
    /// <param name="height">Height in blocks.</param>
    /// <returns>Profile.</returns>
    private static WorldGenProfile SmallProfile(int width = 64, int height = 64)
    {
        return new WorldGenProfile
        {
            Width = width,
            Height = height,
            SeaLevelZ = 12,
            SurfaceMaxZ = 28,
            RockFloorZ = -63,
            BedrockZ = -64,
            BedrockTemplate = "Bedrock",
            StoneBands = new List<StoneBand>
            {
                new() { FromZ = -63, ToZ = -1, Template = "Granite" }
            }
        };
    }

    /// <summary>
    ///     Creates a uniform terrain map at the given surface height.
    /// </summary>
    /// <param name="profile">Profile.</param>
    /// <param name="height">Surface height of every column.</param>
    /// <returns>Terrain map.</returns>
    private static TerrainMap UniformTerrain(WorldGenProfile profile, int height)
    {
        var map = new TerrainMap
        {
            Width = profile.Width,
            Height = profile.Height,
            Heights = new int[profile.Width, profile.Height],
            IsOcean = new bool[profile.Width, profile.Height],
            IsCliff = new bool[profile.Width, profile.Height],
            IsBeach = new bool[profile.Width, profile.Height],
            IsRiver = new bool[profile.Width, profile.Height],
            RiverWidth = new int[profile.Width, profile.Height],
            IsBank = new bool[profile.Width, profile.Height],
            IsLake = new bool[profile.Width, profile.Height],
            LakeSurfaceZ = new int[profile.Width, profile.Height]
        };
        for (var y = 0; y < profile.Height; ++y)
        for (var x = 0; x < profile.Width; ++x)
        {
            map.Heights[x, y] = height;
        }

        return map;
    }

    /// <summary>
    ///     Creates a uniform material assignment.
    /// </summary>
    /// <param name="profile">Profile.</param>
    /// <param name="surface">Surface template name.</param>
    /// <param name="subSurface">Subsurface template name.</param>
    /// <param name="depth">Subsurface depth.</param>
    /// <returns>Material assignment.</returns>
    private static ColumnMaterials UniformMaterials(WorldGenProfile profile, string surface,
        string subSurface, int depth)
    {
        var materials = new ColumnMaterials
        {
            Width = profile.Width,
            Height = profile.Height,
            SurfaceTemplate = new string[profile.Width, profile.Height],
            SubSurfaceTemplate = new string[profile.Width, profile.Height],
            SubSurfaceDepth = new int[profile.Width, profile.Height],
            SurfaceModifier = new byte[profile.Width, profile.Height]
        };
        for (var y = 0; y < profile.Height; ++y)
        for (var x = 0; x < profile.Width; ++x)
        {
            materials.SurfaceTemplate[x, y] = surface;
            materials.SubSurfaceTemplate[x, y] = subSurface;
            materials.SubSurfaceDepth[x, y] = depth;
        }

        return materials;
    }

    /// <summary>
    ///     Creates a cave map with one level, one shaft, and one mouth at hand-chosen
    ///     coordinates.
    /// </summary>
    /// <param name="profile">Profile.</param>
    /// <returns>Cave map.</returns>
    private static CaveMap HandBuiltCaveMap(WorldGenProfile profile)
    {
        var level = new CaveLevelMap
        {
            Width = profile.Width,
            Height = profile.Height,
            BaseFloorZ = -16,
            Headroom = 2,
            Open = new bool[profile.Width, profile.Height],
            FloorZ = Filled(profile, -16),
            CarveHeight = Filled(profile, (byte)2),
            Worley = new byte[profile.Width, profile.Height],
            WaterExcluded = new bool[profile.Width, profile.Height]
        };

        // A row of open cells including the shaft site and the ring columns.
        level.Open[10, 10] = true;
        level.Open[11, 10] = true;
        level.Open[12, 10] = true;
        level.Open[13, 10] = true;
        level.Open[14, 10] = true;
        level.Open[10, 11] = true;
        level.Open[11, 11] = true;
        level.Open[12, 11] = true;
        level.Open[13, 11] = true;
        level.Open[14, 11] = true;
        level.Open[10, 12] = true;
        level.Open[11, 12] = true;
        level.Open[12, 12] = true;
        level.Open[13, 12] = true;
        level.Open[14, 12] = true;

        var shaft = new CaveShaft
        {
            UpperLevel = 0,
            LowerLevel = 0,
            CenterX = 12,
            CenterY = 12,
            LowerFloorZ = -22,
            UpperFloorZ = -16,
            Headroom = 2,
            Columns = new List<CaveShaftColumn>
            {
                new()
                {
                    X = 12, Y = 11,
                    StepZs = new[] { -21, -20, -19, -18, -17 }
                }
            },
            Ring = new List<(int X, int Y)>
            {
                (14, 12), (14, 11), (14, 13), (11, 13)
            }
        };

        var mouth = new CaveMouth
        {
            X = 20,
            Y = 20,
            SurfaceZ = 20,
            FloorZ = -16,
            Headroom = 2,
            Columns = new List<CaveShaftColumn>()
        };

        return new CaveMap
        {
            Width = profile.Width,
            Height = profile.Height,
            Levels = new List<CaveLevelMap> { level },
            Shafts = new List<CaveShaft> { shaft },
            MouthList = new List<CaveMouth> { mouth }
        };
    }

    /// <summary>
    ///     Creates a constant-value array over a profile.
    /// </summary>
    /// <param name="profile">Profile.</param>
    /// <param name="value">Value.</param>
    /// <typeparam name="T">Array type.</typeparam>
    /// <returns>Filled array indexed [x, y].</returns>
    private static T[,] Filled<T>(WorldGenProfile profile, T value)
    {
        var array = new T[profile.Width, profile.Height];
        for (var y = 0; y < profile.Height; ++y)
        for (var x = 0; x < profile.Width; ++x)
        {
            array[x, y] = value;
        }

        return array;
    }

    /// <summary>
    ///     Creates a staging directory beneath the test root.
    /// </summary>
    /// <param name="name">Directory name.</param>
    /// <returns>Absolute path.</returns>
    private string StagingDir(string name)
    {
        return Path.Combine(stagingRoot, name);
    }

    /// <summary>
    ///     Reads and deserializes one staged segment blob.
    /// </summary>
    /// <param name="stagingDirectory">Staging directory.</param>
    /// <param name="segmentIndex">World segment index.</param>
    /// <returns>Deserialized block data.</returns>
    private static WorldSegmentBlockData ReadSegmentBlob(string stagingDirectory,
        GridPosition segmentIndex)
    {
        var path = Path.Combine(stagingDirectory, "segments",
            $"{segmentIndex.X}_{segmentIndex.Y}_{segmentIndex.Z}.bin");
        return MessageConfig.DeserializeMsgPack<WorldSegmentBlockData>(File.ReadAllBytes(path))!;
    }

    /// <summary>
    ///     Decodes a segment blob to a per-cell map using the assembler's inverse: absent
    ///     cells with an air default and explicit air cells map to <see cref="Air" />.
    /// </summary>
    /// <param name="stagingDirectory">Staging directory.</param>
    /// <param name="segmentIndex">World segment index.</param>
    /// <returns>Cell map keyed by world coordinates, values template entity IDs.</returns>
    private static Dictionary<(int X, int Y, int Z), ulong> ReadSegmentCells(
        string stagingDirectory, GridPosition segmentIndex)
    {
        var data = ReadSegmentBlob(stagingDirectory, segmentIndex);
        var cells = new Dictionary<(int X, int Y, int Z), ulong>();
        var baseX = segmentIndex.X * 32;
        var baseY = segmentIndex.Y * 32;
        var baseZ = segmentIndex.Z * 32;

        for (var z = 0; z < 32; ++z)
        {
            var defaultBlock = data.DefaultsPerPlane[z];
            for (var y = 0; y < 32; ++y)
            for (var x = 0; x < 32; ++x)
            {
                cells[(baseX + x, baseY + y, baseZ + z)] =
                    defaultBlock.BlockType == BlockDataType.Air
                        ? Air
                        : EntityConstants.FirstTemplateEntityId + defaultBlock.TemplateIdOffset;
            }
        }

        foreach (var plane in data.DataPlanes)
        foreach (var line in plane.Lines)
        foreach (var entry in line.BlockData)
        {
            var position = (baseX + entry.OffsetX, baseY + line.OffsetY, baseZ + plane.OffsetZ);
            cells[position] = entry.Data.BlockType == BlockDataType.Air
                ? Air
                : EntityConstants.FirstTemplateEntityId + entry.Data.TemplateIdOffset;
        }

        return cells;
    }

    /// <summary>
    ///     Enumerates the template blocks of a segment blob in plane scan order for
    ///     insertion into component collections.
    /// </summary>
    /// <param name="data">Segment block data.</param>
    /// <param name="segmentIndex">World segment index.</param>
    /// <returns>Positions and template entity IDs in (y, x) scan order per plane.</returns>
    private static IEnumerable<(GridPosition Position, ulong TemplateEntityId)>
        DecodeTemplateBlocks(WorldSegmentBlockData data, GridPosition segmentIndex)
    {
        var occupied = new HashSet<(int X, int Y, int Z)>();
        foreach (var plane in data.DataPlanes)
        foreach (var line in plane.Lines)
        foreach (var entry in line.BlockData)
        {
            occupied.Add((entry.OffsetX, line.OffsetY, plane.OffsetZ));
        }

        for (var dz = 0; dz < 32; ++dz)
        {
            var defaultBlock = data.DefaultsPerPlane[dz];
            for (var dy = 0; dy < 32; ++dy)
            for (var dx = 0; dx < 32; ++dx)
            {
                BlockData block;
                if (occupied.Contains((dx, dy, dz)))
                {
                    block = FindBlock(data, dz, dy, dx);
                }
                else
                {
                    block = defaultBlock;
                }

                if (block.BlockType == BlockDataType.Air) continue;

                yield return (new GridPosition
                {
                    X = segmentIndex.X * 32 + dx,
                    Y = segmentIndex.Y * 32 + dy,
                    Z = segmentIndex.Z * 32 + dz
                }, EntityConstants.FirstTemplateEntityId + block.TemplateIdOffset);
            }
        }
    }

    /// <summary>
    ///     Finds the block entry at a position within a plane.
    /// </summary>
    /// <param name="data">Segment block data.</param>
    /// <param name="dz">Z offset.</param>
    /// <param name="dy">Y offset.</param>
    /// <param name="dx">X offset.</param>
    /// <returns>Block data at the position.</returns>
    private static BlockData FindBlock(WorldSegmentBlockData data, int dz, int dy, int dx)
    {
        foreach (var plane in data.DataPlanes)
        {
            if (plane.OffsetZ != dz) continue;
            foreach (var line in plane.Lines)
            {
                if (line.OffsetY != dy) continue;
                foreach (var entry in line.BlockData)
                {
                    if (entry.OffsetX == dx) return entry.Data;
                }
            }
        }

        throw new KeyNotFoundException($"No block entry at ({dx}, {dy}, {dz}).");
    }

    /// <summary>
    ///     Fixture holding the real component collections and runtime block data generator
    ///     for byte-identity checks.
    /// </summary>
    private sealed class BlockWorldFixture
    {
        private readonly EntityTable entityTable = new();
        private readonly ComponentManager componentManager;
        private readonly BlockPositionComponentCollection blockPositions;
        private readonly BlockTileComponentCollection blockTiles;
        private readonly WorldSegmentResolver resolver = new();
        private readonly BlockWorldSegmentIndexer blockIndexer;
        private ulong nextBlockEntityId = EntityConstants.FirstBlockEntityId;

        public BlockWorldFixture()
        {
            componentManager = new ComponentManager(new EntityNotifier());
            blockPositions = new BlockPositionComponentCollection(entityTable, componentManager);
            blockTiles = new BlockTileComponentCollection(entityTable, componentManager);
            blockIndexer = new BlockWorldSegmentIndexer(blockPositions, resolver);
            Generator = new WorldSegmentBlockDataGenerator(resolver, entityTable, blockIndexer,
                blockPositions, NullLogger<WorldSegmentBlockDataGenerator>.Instance);
        }

        /// <summary>
        ///     The runtime block data generator.
        /// </summary>
        public WorldSegmentBlockDataGenerator Generator { get; }

        /// <summary>
        ///     Commits all pending entity and component updates so that indexers and the
        ///     generator observe them.
        /// </summary>
        public void Commit()
        {
            entityTable.UpdateAllEntities();
            componentManager.UpdateAllComponents();
        }

        /// <summary>
        ///     Adds one block entity at the given position with the given template.
        /// </summary>
        /// <param name="position">Block position.</param>
        /// <param name="templateEntityId">Template entity ID.</param>
        public void AddBlock(GridPosition position, ulong templateEntityId)
        {
            var entityId = nextBlockEntityId++;
            entityTable.Add(entityId, templateEntityId, isBlock: true, isLoad: false,
                isPersisted: false);
            blockPositions.AddComponent(entityId, position);
            blockTiles.AddComponent(entityId, new BlockTile { FrontFaceId = 0, TopFaceId = 0 });
        }

    }
}
