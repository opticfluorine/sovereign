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
using System.Diagnostics;
using Sovereign.WorldGen.Hydrology;
using Sovereign.WorldGen.Noise;
using Sovereign.WorldGen.Output;
using Sovereign.WorldGen.Terrain;

namespace Sovereign.WorldGen;

/// <summary>
///     Sequences the terrain, hydrology, and preview stages of a world generation plan.
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
///     Runs the terrain, hydrology, and preview stages of a world generation plan in sequence.
///     All stages derive their sub-seeds deterministically from the root seed; the same root
///     seed and profile always produce a byte-identical heightmap and preview PNG.
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
    public WorldGenPlan Plan(WorldGenProfile profile, string profileName, ulong seed, int originX, int originY,
        string previewPath, Action<string>? progress)
    {
        var total = Stopwatch.StartNew();
        var terrainClock = new Stopwatch();
        var hydrologyClock = new Stopwatch();
        var previewClock = new Stopwatch();

        var landCells = 0;
        int riverCount;
        int lakeCount;

        Report(progress, "Terrain: sampling noise fields");
        terrainClock.Start();
        var fields = new TerrainFieldStack().Sample(profile.Width, profile.Height,
            SubSeed(seed, "TerrainFields"));
        var continentalness = new ContinentalnessStage().Apply(fields, profile.Width, profile.Height);
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
        hydrologyClock.Stop();

        Report(progress, "Rendering preview");
        previewClock.Start();
        var preview = new PreviewRenderer().Render(map, continentalness, profile);
        PngWriter.WritePng(previewPath, preview.Width, preview.Height, preview.Pixels);
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
            TerrainMs = terrainClock.ElapsedMilliseconds,
            HydrologyMs = hydrologyClock.ElapsedMilliseconds,
            PreviewMs = previewClock.ElapsedMilliseconds,
            TotalMs = total.ElapsedMilliseconds
        };

        return new WorldGenPlan
        {
            Seed = seed,
            ProfileName = profileName,
            OriginX = originX,
            OriginY = originY,
            Terrain = map,
            Statistics = statistics,
            PreviewPath = previewPath
        };
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
