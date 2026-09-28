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
using System.Diagnostics;
using System.IO;
using Sovereign.WorldGen.Biomes;
using Sovereign.WorldGen.Caves;
using Sovereign.WorldGen.Decorations;
using Sovereign.WorldGen.Hydrology;
using Sovereign.WorldGen.Noise;
using Sovereign.WorldGen.Output;
using Sovereign.WorldGen.Terrain;

namespace Sovereign.WorldGen;

/// <summary>
///     Sequences the terrain, hydrology, biome, cave, and preview stages of a world
///     generation plan.
/// </summary>
public interface IWorldGenPipeline
{
    /// <summary>
    ///     Runs the full world generation plan pipeline.
    /// </summary>
    /// <param name="profile">Validated world generation profile.</param>
    /// <param name="profileName">Name of the profile.</param>
    /// <param name="seed">Root world seed.</param>
    /// <param name="originX">World X coordinate of the footprint origin.</param>
    /// <param name="originY">World Y coordinate of the footprint origin.</param>
    /// <param name="previewPath">Absolute path to which the preview PNG is written.</param>
    /// <param name="progress">Optional callback invoked at stage boundaries with the stage name.</param>
    /// <returns>The completed plan.</returns>
    WorldGenPlan Plan(WorldGenProfile profile, string profileName, ulong seed, int originX, int originY,
        string previewPath, Action<string>? progress);
}

/// <summary>
///     Runs the terrain, hydrology, biome, cave, and preview stages of a world generation
///     plan in sequence. All stages derive their sub-seeds deterministically from the root
///     seed; the same root seed and profile always produce a byte-identical heightmap,
///     biome map, material assignment, cave map, decoration list, and preview PNGs.
/// </summary>
public sealed class WorldGenPipeline : IWorldGenPipeline
{
    /// <summary>
    ///     Total cells in a plan footprint.
    /// </summary>
    /// <param name="profile">World generation profile.</param>
    /// <returns>Cell count.</returns>
    private static long CellCount(WorldGenProfile profile)
    {
        return (long)profile.Width * profile.Height;
    }

    /// <summary>
    ///     Runs the full world generation plan pipeline.
    /// </summary>
    /// <param name="profile">Validated world generation profile.</param>
    /// <param name="profileName">Name of the profile.</param>
    /// <param name="seed">Root world seed.</param>
    /// <param name="originX">World X coordinate of the footprint origin.</param>
    /// <param name="originY">World Y coordinate of the footprint origin.</param>
    /// <param name="previewPath">Absolute path to which the preview PNG is written.</param>
    /// <param name="progress">Optional callback invoked at stage boundaries with the stage name.</param>
    /// <returns>The completed plan.</returns>
    public WorldGenPlan Plan(WorldGenProfile profile, string profileName, ulong seed, int originX,
        int originY, string previewPath, Action<string>? progress)
    {
        var total = Stopwatch.StartNew();
        var terrainClock = new Stopwatch();
        var hydrologyClock = new Stopwatch();
        var biomesClock = new Stopwatch();
        var cavesClock = new Stopwatch();
        var previewClock = new Stopwatch();

        var landCells = 0;
        int riverCount;
        int lakeCount;
        int longestStraightRiverRun;

        Report(progress, "Terrain: sampling noise fields");
        terrainClock.Start();
        var fields = new TerrainFieldStack().Sample(profile.Width, profile.Height,
            SubSeed(seed, "TerrainFields"), profile.Terrain);
        var continentalness = new ContinentalnessStage().Apply(fields, profile.Width,
            profile.Height, profile.Terrain);
        terrainClock.Stop();

        Report(progress, "Terrain: shaping surface");
        terrainClock.Start();
        var shape = new TerrainShapeStage().Apply(fields, continentalness, profile, SubSeed(seed, "TerrainShape"));
        terrainClock.Stop();

        var map = shape.Map;
        for (var y = 0; y < profile.Height; ++y)
        {
            for (var x = 0; x < profile.Width; ++x)
            {
                if (!map.IsOcean[x, y]) ++landCells;
            }
        }

        Report(progress, "Hydrology: filling depressions");
        hydrologyClock.Start();
        var filled = (int[,])map.Heights.Clone();
        DepressionFill.Fill(filled);
        var sills = (int[,])map.Heights.Clone();
        DepressionFill.Fill(sills, useEpsilon: false);
        hydrologyClock.Stop();

        Report(progress, "Hydrology: routing flow");
        hydrologyClock.Start();
        var routing = new FlowRouter(SubSeed(seed, "FlowRouting")).Route(filled);
        hydrologyClock.Stop();

        Report(progress, "Hydrology: extracting rivers");
        hydrologyClock.Start();
        var extraction = new RiverExtractor(SubSeed(seed, "RiverExtraction"))
            .Extract(map, filled, sills, routing, profile);
        riverCount = extraction.RiverCount;
        lakeCount = extraction.LakeCount;
        longestStraightRiverRun = extraction.LongestStraightRiverRun;
        hydrologyClock.Stop();

        BiomeMap? biomeMap = null;
        ColumnMaterials? materials = null;
        IReadOnlyList<DecorationPlacement> decorations = new List<DecorationPlacement>();
        CaveStageResult? caves = null;

        if (profile.Biomes is { } biomes)
        {
            Report(progress, "Biomes: classifying biomes");
            biomesClock.Start();
            biomeMap = new BiomeStage().Apply(map, continentalness, profile, SubSeed(seed, "Biomes"));

            Report(progress, "Biomes: assigning materials");
            materials = new MaterialStage().Apply(map, biomeMap, biomes, SubSeed(seed, "MaterialJitter"));
            biomesClock.Stop();
        }

        // Caves run after the biome stages and before decoration placement so that
        // decorations can keep clear of surface mouths.
        if (profile.CaveLevels is { Count: > 0 })
        {
            Report(progress, "Caves: generating levels");
            cavesClock.Start();
            caves = new CaveStage().Apply(profile, map, biomeMap, seed);
            cavesClock.Stop();
        }

        if (biomeMap is not null && profile.Biomes is { } biomePlacement)
        {
            Report(progress, "Biomes: placing decorations");
            biomesClock.Start();
            decorations = new DecorationPlacer(SubSeed(seed, "Decorations"))
                .Apply(map, biomeMap, biomePlacement, caves?.Map.Mouths).Placements;
            biomesClock.Stop();
        }

        Report(progress, "Rendering preview");
        previewClock.Start();
        var preview = new PreviewRenderer().Render(map, continentalness, profile, biomeMap,
            caves?.Map.Mouths);
        PngWriter.WritePng(previewPath, preview.Width, preview.Height, preview.Pixels);

        var cavePreviewPaths = new List<string>();
        if (caves is not null)
        {
            var cavePreviews = new CavePreviewRenderer().Render(caves.Map);
            for (var i = 0; i < cavePreviews.Count; ++i)
            {
                var path = CavePreviewPath(previewPath, i + 1);
                PngWriter.WritePng(path, cavePreviews[i].Width, cavePreviews[i].Height,
                    cavePreviews[i].Pixels);
                cavePreviewPaths.Add(path);
            }
        }

        previewClock.Stop();

        total.Stop();

        var statistics = new PlanStatistics
        {
            Width = profile.Width,
            Height = profile.Height,
            LandCells = landCells,
            WaterCells = (int)(CellCount(profile) - landCells),
            RiverCount = riverCount,
            LakeCount = lakeCount,
            LongestStraightRiverRun = longestStraightRiverRun,
            TerrainMs = terrainClock.ElapsedMilliseconds,
            HydrologyMs = hydrologyClock.ElapsedMilliseconds,
            BiomesMs = biomesClock.ElapsedMilliseconds,
            CavesMs = cavesClock.ElapsedMilliseconds,
            PreviewMs = previewClock.ElapsedMilliseconds,
            TotalMs = total.ElapsedMilliseconds,
            BiomePercentages = biomeMap is null ? null : BiomePercentages(biomeMap),
            DecorationCounts = biomeMap is null ? null : DecorationCounts(decorations),
            DecorationsTotal = decorations.Count,
            Caves = caves?.Stats
        };

        return new WorldGenPlan
        {
            Seed = seed,
            ProfileName = profileName,
            OriginX = originX,
            OriginY = originY,
            Terrain = map,
            Biomes = biomeMap,
            Materials = materials,
            Decorations = biomeMap is null ? null : decorations,
            Caves = caves?.Map,
            CavePreviewPaths = cavePreviewPaths,
            Statistics = statistics,
            PreviewPath = previewPath
        };
    }

    /// <summary>
    ///     Derives the preview path of one cave level from the main preview path. The
    ///     "preview_" filename prefix is replaced with "caves_" and the level number is
    ///     appended; other names fall back to a caves suffix.
    /// </summary>
    /// <param name="previewPath">Absolute path of the main preview image.</param>
    /// <param name="levelNumber">One-based cave level number.</param>
    /// <returns>Absolute path for the cave preview image.</returns>
    private static string CavePreviewPath(string previewPath, int levelNumber)
    {
        var directory = Path.GetDirectoryName(previewPath) ?? ".";
        var file = Path.GetFileNameWithoutExtension(previewPath);
        if (file.StartsWith("preview_", StringComparison.Ordinal))
        {
            file = "caves_" + file["preview_".Length..] + $"_{levelNumber}";
        }
        else
        {
            file = $"{file}_caves_{levelNumber}";
        }

        return Path.Combine(directory, file + ".png");
    }

    /// <summary>
    ///     Computes the fraction of the footprint classified as each biome.
    /// </summary>
    /// <param name="biomeMap">Classified biome map.</param>
    /// <returns>Biome fractions ordered by biome ID.</returns>
    private static IReadOnlyDictionary<BiomeId, double> BiomePercentages(BiomeMap biomeMap)
    {
        var counts = new int[Enum.GetValues<BiomeId>().Length];
        var total = (long)biomeMap.Width * biomeMap.Height;
        for (var y = 0; y < biomeMap.Height; ++y)
        {
            for (var x = 0; x < biomeMap.Width; ++x)
            {
                ++counts[(int)biomeMap.Biome[x, y]];
            }
        }

        var percentages = new SortedDictionary<BiomeId, double>();
        for (var i = 0; i < counts.Length; ++i)
        {
            if (counts[i] > 0) percentages[(BiomeId)i] = (double)counts[i] / total;
        }

        return percentages;
    }

    /// <summary>
    ///     Counts placed decorations per template name.
    /// </summary>
    /// <param name="decorations">Placed decorations.</param>
    /// <returns>Counts ordered by template name.</returns>
    private static IReadOnlyDictionary<string, int> DecorationCounts(
        IReadOnlyList<DecorationPlacement> decorations)
    {
        var counts = new SortedDictionary<string, int>();
        foreach (var placement in decorations)
        {
            counts[placement.TemplateName] = counts.GetValueOrDefault(placement.TemplateName) + 1;
        }

        return counts;
    }

    /// <summary>
    ///     Derives a stage sub-seed from the root seed.
    /// </summary>
    /// <param name="seed">Root world seed.</param>
    /// <param name="stageName">Stage name.</param>
    /// <returns>Stage sub-seed.</returns>
    private static ulong SubSeed(ulong seed, string stageName)
    {
        return SeedDerivation.DeriveSubSeed(seed, stageName);
    }

    /// <summary>
    ///     Reports progress to the callback, if any.
    /// </summary>
    /// <param name="progress">Progress callback.</param>
    /// <param name="stage">Stage name.</param>
    private static void Report(Action<string>? progress, string stage)
    {
        progress?.Invoke(stage);
    }
}
