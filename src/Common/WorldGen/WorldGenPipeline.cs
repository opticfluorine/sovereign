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
using System.Linq;
using System.Text;
using System.Threading;
using Sovereign.WorldGen.Biomes;
using Sovereign.WorldGen.Caves;
using Sovereign.WorldGen.Decorations;
using Sovereign.WorldGen.Hydrology;
using Sovereign.WorldGen.Layout;
using Sovereign.WorldGen.Noise;
using Sovereign.WorldGen.Output;
using Sovereign.WorldGen.Terrain;

namespace Sovereign.WorldGen;

/// <summary>
///     Sequences the terrain, hydrology, biome, cave, preview, and assembly stages of a
///     world generation plan.
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
    /// <param name="stagingDirectory">Absolute path of the plan's staging directory, to
    ///     which the segment blobs and staged decorations are written.</param>
    /// <param name="resolvedTemplates">Profile template names resolved against the live
    ///     template entity set.</param>
    /// <param name="progress">Optional callback invoked at stage boundaries with the stage name.</param>
    /// <param name="cancellationToken">Token observed at stage and sub-stage boundaries;
    ///     cancellation unwinds through <see cref="OperationCanceledException" />.</param>
    /// <returns>The completed plan.</returns>
    WorldGenPlan Plan(WorldGenProfile profile, string profileName, ulong seed, int originX,
        int originY, string previewPath, string stagingDirectory,
        WorldGenResolvedTemplates resolvedTemplates, Action<string>? progress,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Runs the full world generation plan pipeline, writing raw preview pixels into
    ///     caller-provided buffers instead of encoding PNG files.
    /// </summary>
    /// <param name="profile">Validated world generation profile.</param>
    /// <param name="profileName">Name of the profile.</param>
    /// <param name="seed">Root world seed.</param>
    /// <param name="originX">World X coordinate of the footprint origin.</param>
    /// <param name="originY">World Y coordinate of the footprint origin.</param>
    /// <param name="previewBuffer">Caller-owned buffer receiving the preview's raw pixels
    ///     as packed RGB triples in row-major order, top row first.</param>
    /// <param name="cavePreviewBuffers">Caller-owned buffers receiving one cave level
    ///     preview's raw pixels each in level order, or null when the profile has no cave
    ///     levels.</param>
    /// <param name="stagingDirectory">Absolute path of the plan's staging directory, to
    ///     which the segment blobs and staged decorations are written.</param>
    /// <param name="resolvedTemplates">Profile template names resolved against the live
    ///     template entity set.</param>
    /// <param name="progress">Optional callback invoked at stage boundaries with the stage name.</param>
    /// <param name="cancellationToken">Token observed at stage and sub-stage boundaries;
    ///     cancellation unwinds through <see cref="OperationCanceledException" />.</param>
    /// <returns>The completed plan, whose preview paths are empty in this mode.</returns>
    /// <remarks>
    ///     Each buffer must hold <c>width * height * 3</c> bytes for its rendered preview,
    ///     whose dimensions are the profile footprint downscaled by
    ///     <c>max(1, ceiling(longSide / maxDimension))</c> using the profile's preview max
    ///     dimension clamped to [256, 8192]. One buffer per cave level is required when the
    ///     profile enables cave levels; otherwise an exception is thrown.
    /// </remarks>
    WorldGenPlan Plan(WorldGenProfile profile, string profileName, ulong seed, int originX,
        int originY, Span<byte> previewBuffer, Memory<byte>[]? cavePreviewBuffers,
        string stagingDirectory, WorldGenResolvedTemplates resolvedTemplates,
        Action<string>? progress, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Computes the number of bytes required for a single preview buffer.
    /// </summary>
    /// <param name="profile">World generation profile.</param>
    /// <returns>Number of bytes required for the main preview buffer and for each cave
    ///     level preview buffer, which are always the same size.</returns>
    int PreviewBufferLength(WorldGenProfile profile);
}

/// <summary>
///     Runs the terrain, hydrology, biome, cave, preview, and assembly stages of a world
///     generation plan in sequence. All stages derive their sub-seeds deterministically
///     from the root seed; the same root seed and profile always produce a byte-identical
///     heightmap, biome map, material assignment, cave map, decoration list, preview PNGs,
///     and staged segment blobs.
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
    /// <param name="stagingDirectory">Absolute path of the plan's staging directory, to
    ///     which the segment blobs and staged decorations are written.</param>
    /// <param name="resolvedTemplates">Profile template names resolved against the live
    ///     template entity set.</param>
    /// <param name="progress">Optional callback invoked at stage boundaries with the stage name.</param>
    /// <param name="cancellationToken">Token observed at stage and sub-stage boundaries.</param>
    /// <returns>The completed plan.</returns>
    public WorldGenPlan Plan(WorldGenProfile profile, string profileName, ulong seed, int originX,
        int originY, string previewPath, string stagingDirectory,
        WorldGenResolvedTemplates resolvedTemplates, Action<string>? progress,
        CancellationToken cancellationToken = default)
    {
        return PlanCore(profile, profileName, seed, originX, originY, previewPath, default,
            null, stagingDirectory, resolvedTemplates, progress,
            cancellationToken);
    }

    /// <summary>
    ///     Runs the full world generation plan pipeline, writing raw preview pixels into
    ///     caller-provided buffers instead of encoding PNG files.
    /// </summary>
    /// <param name="profile">Validated world generation profile.</param>
    /// <param name="profileName">Name of the profile.</param>
    /// <param name="seed">Root world seed.</param>
    /// <param name="originX">World X coordinate of the footprint origin.</param>
    /// <param name="originY">World Y coordinate of the footprint origin.</param>
    /// <param name="previewBuffer">Caller-owned buffer receiving the preview's raw pixels
    ///     as packed RGB triples in row-major order, top row first.</param>
    /// <param name="cavePreviewBuffers">Caller-owned buffers receiving one cave level
    ///     preview's raw pixels each in level order, or null when the profile has no cave
    ///     levels.</param>
    /// <param name="stagingDirectory">Absolute path of the plan's staging directory, to
    ///     which the segment blobs and staged decorations are written.</param>
    /// <param name="resolvedTemplates">Profile template names resolved against the live
    ///     template entity set.</param>
    /// <param name="progress">Optional callback invoked at stage boundaries with the stage name.</param>
    /// <param name="cancellationToken">Token observed at stage and sub-stage boundaries.</param>
    /// <returns>The completed plan, whose preview paths are empty in this mode.</returns>
    /// <remarks>
    ///     Each buffer must hold <c>width * height * 3</c> bytes for its rendered preview,
    ///     whose dimensions are the profile footprint downscaled by
    ///     <c>max(1, ceiling(longSide / maxDimension))</c> using the profile's preview max
    ///     dimension clamped to [256, 8192]. One buffer per cave level is required when the
    ///     profile enables cave levels; otherwise an exception is thrown.
    /// </remarks>
    public WorldGenPlan Plan(WorldGenProfile profile, string profileName, ulong seed, int originX,
        int originY, Span<byte> previewBuffer, Memory<byte>[]? cavePreviewBuffers,
        string stagingDirectory, WorldGenResolvedTemplates resolvedTemplates,
        Action<string>? progress, CancellationToken cancellationToken = default)
    {
        return PlanCore(profile, profileName, seed, originX, originY, null, previewBuffer,
            cavePreviewBuffers, stagingDirectory, resolvedTemplates, progress,
            cancellationToken);
    }

    /// <summary>
    ///     Computes the number of bytes required for a single preview buffer.
    /// </summary>
    /// <param name="profile">World generation profile.</param>
    /// <returns>Number of bytes required for the main preview buffer and for each cave
    ///     level preview buffer, which are always the same size.</returns>
    public int PreviewBufferLength(WorldGenProfile profile)
    {
        var cap = Math.Clamp(PreviewOptions.EffectiveMaxDimension(profile),
            PreviewOptions.MinMaxDimension, PreviewOptions.MaxMaxDimension);
        var longSide = Math.Max(profile.Width, profile.Height);
        var factor = longSide <= cap ? 1 : (longSide + cap - 1) / cap;
        var width = (profile.Width + factor - 1) / factor;
        var height = (profile.Height + factor - 1) / factor;
        return width * height * 3;
    }

    /// <summary>
    ///     Runs the full world generation plan pipeline, encoding PNG previews when a
    ///     preview path is given and otherwise writing raw preview pixels into caller
    ///     buffers.
    /// </summary>
    /// <param name="profile">Validated world generation profile.</param>
    /// <param name="profileName">Name of the profile.</param>
    /// <param name="seed">Root world seed.</param>
    /// <param name="originX">World X coordinate of the footprint origin.</param>
    /// <param name="originY">World Y coordinate of the footprint origin.</param>
    /// <param name="previewPath">Absolute path to which the preview PNGs are written, or
    ///     null to write raw preview pixels into the caller's buffers.</param>
    /// <param name="previewBuffer">Buffer receiving the preview's raw pixels as packed RGB
    ///     triples in row-major order, top row first.</param>
    /// <param name="cavePreviewBuffers">Buffers receiving one cave level preview's raw
    ///     pixels each in level order, or null.</param>
    /// <param name="stagingDirectory">Absolute path of the plan's staging directory, to
    ///     which the segment blobs and staged decorations are written.</param>
    /// <param name="resolvedTemplates">Profile template names resolved against the live
    ///     template entity set.</param>
    /// <param name="progress">Optional callback invoked at stage boundaries with the stage name.</param>
    /// <param name="cancellationToken">Token observed at stage and sub-stage boundaries.</param>
    /// <returns>The completed plan.</returns>
    private WorldGenPlan PlanCore(WorldGenProfile profile, string profileName, ulong seed,
        int originX, int originY, string? previewPath, Span<byte> previewBuffer,
        Memory<byte>[]? cavePreviewBuffers, string stagingDirectory,
        WorldGenResolvedTemplates resolvedTemplates, Action<string>? progress,
        CancellationToken cancellationToken)
    {
        var total = Stopwatch.StartNew();
        var terrainClock = new Stopwatch();
        var hydrologyClock = new Stopwatch();
        var biomesClock = new Stopwatch();
        var cavesClock = new Stopwatch();
        var previewClock = new Stopwatch();
        var assemblyClock = new Stopwatch();

        var landCells = 0;
        int riverCount;
        int lakeCount;
        int longestStraightRiverRun;

        LayoutFields? layout = null;
        LayoutReport? layoutReport = null;

        Report(progress, "Terrain: sampling noise fields");
        terrainClock.Start();
        TerrainFields fields;
        ContinentalnessResult continentalness;
        TerrainShapeResult shape;
        if (profile.Layout is { IsActive: true } layoutOptions)
        {
            (fields, continentalness, shape, layout, layoutReport) =
                RunLayoutTerrain(profile, seed, layoutOptions, progress, cancellationToken);
        }
        else
        {
            fields = new TerrainFieldStack().Sample(profile.Width, profile.Height,
                SubSeed(seed, "TerrainFields"), profile.Terrain);
            cancellationToken.ThrowIfCancellationRequested();
            continentalness = new ContinentalnessStage().Apply(fields, profile.Width,
                profile.Height, profile.Terrain);
            cancellationToken.ThrowIfCancellationRequested();

            Report(progress, "Terrain: shaping surface (0%)");
            shape = new TerrainShapeStage().Apply(fields, continentalness, profile,
                SubSeed(seed, "TerrainShape"), null, null, cancellationToken);
            Report(progress, "Terrain: shaping surface (100%)");
        }

        terrainClock.Stop();
        cancellationToken.ThrowIfCancellationRequested();

        var map = shape.Map;
        for (var y = 0; y < profile.Height; ++y)
        {
            for (var x = 0; x < profile.Width; ++x)
            {
                if (!map.IsOcean[x, y]) ++landCells;
            }
        }

        Report(progress, "Hydrology: filling depressions (0%)");
        hydrologyClock.Start();
        var filled = (int[,])map.Heights.Clone();
        DepressionFill.Fill(filled, useEpsilon: true, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        Report(progress, "Hydrology: filling depressions (50%)");
        var sills = (int[,])map.Heights.Clone();
        DepressionFill.Fill(sills, useEpsilon: false, cancellationToken);
        Report(progress, "Hydrology: filling depressions (100%)");
        hydrologyClock.Stop();
        cancellationToken.ThrowIfCancellationRequested();

        Report(progress, "Hydrology: routing flow");
        hydrologyClock.Start();
        var routing = new FlowRouter(SubSeed(seed, "FlowRouting")).Route(filled,
            cancellationToken);
        hydrologyClock.Stop();
        cancellationToken.ThrowIfCancellationRequested();

        Report(progress, "Hydrology: extracting rivers");
        hydrologyClock.Start();
        var extraction = new RiverExtractor(SubSeed(seed, "RiverExtraction"))
            .Extract(map, filled, sills, routing, profile, cancellationToken);
        riverCount = extraction.RiverCount;
        lakeCount = extraction.LakeCount;
        longestStraightRiverRun = extraction.LongestStraightRiverRun;
        hydrologyClock.Stop();
        cancellationToken.ThrowIfCancellationRequested();

        BiomeMap? biomeMap = null;
        ColumnMaterials? materials = null;
        IReadOnlyList<DecorationPlacement> decorations = new List<DecorationPlacement>();
        CaveStageResult? caves = null;

        if (profile.Biomes is { } biomes)
        {
            Report(progress, "Biomes: classifying biomes");
            biomesClock.Start();
            biomeMap = new BiomeStage().Apply(map, continentalness, profile,
                SubSeed(seed, "Biomes"), layout, cancellationToken);

            Report(progress, "Biomes: assigning materials");
            materials = new MaterialStage().Apply(map, biomeMap, biomes,
                SubSeed(seed, "MaterialJitter"), cancellationToken);
            biomesClock.Stop();
            cancellationToken.ThrowIfCancellationRequested();
        }

        // Caves run after the biome stages and before decoration placement so that
        // decorations can keep clear of surface mouths.
        if (profile.CaveLevels is { Count: > 0 })
        {
            Report(progress, "Caves: generating levels");
            cavesClock.Start();
            caves = new CaveStage().Apply(profile, map, biomeMap, seed, cancellationToken);
            cavesClock.Stop();
            cancellationToken.ThrowIfCancellationRequested();
        }

        if (biomeMap is not null && profile.Biomes is { } biomePlacement)
        {
            Report(progress, "Biomes: placing decorations");
            biomesClock.Start();
            decorations = new DecorationPlacer(SubSeed(seed, "Decorations"))
                .Apply(map, biomeMap, biomePlacement, caves?.Map.Mouths,
                    cancellationToken).Placements;
            biomesClock.Stop();
            cancellationToken.ThrowIfCancellationRequested();
        }

        Report(progress, "Rendering preview (0%)");
        previewClock.Start();
        var maxDimension = PreviewOptions.EffectiveMaxDimension(profile);
        var showAnchorOverlay = PreviewOptions.EffectiveShowAnchorOverlay(profile);
        var preview = new PreviewRenderer().Render(map, continentalness, profile, maxDimension,
            biomeMap, caves?.Map.Mouths, layout, layoutReport, showAnchorOverlay);
        cancellationToken.ThrowIfCancellationRequested();
        Report(progress, "Rendering preview (85%)");
        if (previewPath is { } path)
        {
            PngWriter.WritePng(path, preview.Width, preview.Height, preview.Pixels);
        }
        else
        {
            WritePreviewPixels(previewBuffer, preview);
        }

        var cavePreviewPaths = new List<string>();
        if (caves is not null)
        {
            var cavePreviews = new CavePreviewRenderer().Render(caves.Map, maxDimension);
            var cavePreviewCount = cavePreviewBuffers?.Length ?? 0;
            if (previewPath is null && cavePreviewCount < cavePreviews.Count)
            {
                throw new ArgumentException(
                    $"World generation with cave levels requires {cavePreviews.Count} cave preview buffers, but {cavePreviewCount} were provided.");
            }

            for (var i = 0; i < cavePreviews.Count; ++i)
            {
                Report(progress, $"Rendering preview (85% + cave level {i + 1})");
                if (previewPath is { } previewFilePath)
                {
                    var cavePath = CavePreviewPath(previewFilePath, i + 1);
                    PngWriter.WritePng(cavePath, cavePreviews[i].Width, cavePreviews[i].Height,
                        cavePreviews[i].Pixels);
                    cavePreviewPaths.Add(cavePath);
                }
                else
                {
                    WritePreviewPixels(cavePreviewBuffers![i].Span, cavePreviews[i]);
                }
            }
        }

        Report(progress, "Rendering preview (100%)");
        previewClock.Stop();
        cancellationToken.ThrowIfCancellationRequested();

        Report(progress, "Assembling segments (0%)");
        assemblyClock.Start();
        var assembler = new SegmentAssembler(profile, map, materials, caves?.Map,
            biomeMap is null ? null : decorations, resolvedTemplates, originX, originY,
            stagingDirectory);
        var assembly = assembler.Assemble(progress, cancellationToken);
        assemblyClock.Stop();

        total.Stop();
        cancellationToken.ThrowIfCancellationRequested();

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
            AssemblyMs = assemblyClock.ElapsedMilliseconds,
            TotalMs = total.ElapsedMilliseconds,
            BiomePercentages = biomeMap is null ? null : BiomePercentages(biomeMap),
            DecorationCounts = biomeMap is null ? null : DecorationCounts(decorations),
            DecorationsTotal = decorations.Count,
            Layout = layoutReport,
            Caves = caves?.Stats
        };

        return new WorldGenPlan
        {
            Seed = seed,
            ProfileName = profileName,
            Profile = profile,
            OriginX = originX,
            OriginY = originY,
            ResolvedTemplates = resolvedTemplates,
            StagingDirectory = stagingDirectory,
            Terrain = map,
            Biomes = biomeMap,
            Materials = materials,
            Decorations = biomeMap is null ? null : decorations,
            Caves = caves?.Map,
            CavePreviewPaths = cavePreviewPaths,
            Statistics = statistics,
            PreviewPath = previewPath ?? ""
        };
    }

    /// <summary>
    ///     Number of layout resample attempts after the initial attempt.
    /// </summary>
    private const int LayoutResampleAttempts = 2;

    private const float LayoutForcedJitter = 0.02f;

    /// <summary>
    ///     Runs the terrain stages with a layout mask, resampling the anchor jitter when the
    ///     layout is not fully honored. Attempts are ranked by distinct matched count, then
    ///     total matched count, then mass-count proximity; the best attempt is accepted with a
    ///     warning, unless strict connectivity is configured, in which case a failed layout
    ///     raises <see cref="LayoutValidationException" />.
    /// </summary>
    /// <param name="profile">World generation profile.</param>
    /// <param name="seed">Root world seed.</param>
    /// <param name="options">Active layout options.</param>
    /// <param name="progress">Optional progress callback.</param>
    /// <param name="cancellationToken">Token observed at stage boundaries.</param>
    /// <returns>Terrain fields, continentalness, shaped terrain, layout fields, and report.</returns>
    private static (TerrainFields Fields, ContinentalnessResult Continentalness,
        TerrainShapeResult Shape, LayoutFields Layout, LayoutReport Report) RunLayoutTerrain(
        WorldGenProfile profile, ulong seed, LayoutOptions options, Action<string>? progress,
        CancellationToken cancellationToken)
    {
        var warnings = new List<string>();
        var maskStage = new LayoutMaskStage();
        var validator = new LayoutValidator();
        var connectivity = options.Connectivity;

        TerrainFields bestFields = null!;
        ContinentalnessResult bestContinentalness = null!;
        TerrainShapeResult bestShape = null!;
        LayoutFields bestLayout = null!;
        LayoutValidationResult? bestValidation = null;
        IReadOnlyList<ResolvedAnchor>? previousAnchors = null;
        var attempts = 0;

        for (var attempt = 0; attempt <= LayoutResampleAttempts; ++attempt)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var layoutSeed = attempt == 0
                ? SeedDerivation.DeriveSubSeed(seed, "Layout")
                : SeedDerivation.DeriveSubSeed(seed, $"Layout.Resample{attempt}");
            var forcedJitter = attempt == 0 ? 0f : LayoutForcedJitter;
            var layout = maskStage.Build(profile.Width, profile.Height, options, layoutSeed,
                forcedJitter);

            // A forced jitter that resolved to the same anchors as the previous attempt
            // produces identical terrain, so skip the remaining attempts.
            if (attempt > 0
                && LayoutMaskStage.AnchorsEquivalent(layout.Anchors, previousAnchors))
            {
                break;
            }

            previousAnchors = layout.Anchors;
            ++attempts;

            Report(progress, attempt == 0
                ? "Terrain: sampling noise fields"
                : $"Terrain: resampling layout (attempt {attempt + 1})");
            var fields = new TerrainFieldStack().Sample(profile.Width, profile.Height,
                SubSeed(seed, "TerrainFields"), profile.Terrain, layout);
            cancellationToken.ThrowIfCancellationRequested();
            var continentalness = new ContinentalnessStage().Apply(fields, profile.Width,
                profile.Height, profile.Terrain);
            cancellationToken.ThrowIfCancellationRequested();

            Report(progress, "Terrain: shaping surface (0%)");
            var shape = new TerrainShapeStage().Apply(fields, continentalness, profile,
                SubSeed(seed, "TerrainShape"), layout, layout.Anchors, cancellationToken);
            Report(progress, "Terrain: shaping surface (100%)");

            var validation = validator.Validate(shape.Map, layout.Anchors, warnings, connectivity);
            if (bestValidation is null
                || LayoutValidator.CompareAttempts(validation, bestValidation) > 0)
            {
                bestFields = fields;
                bestContinentalness = continentalness;
                bestShape = shape;
                bestLayout = layout;
                bestValidation = validation;
            }

            if (validation.Success) break;
        }

        bestValidation!.Report.AttemptCount = attempts;

        if (!bestValidation.Success)
        {
            var message = FormatLayoutFailure(bestValidation, attempts);
            if (connectivity == LayoutConnectivity.Strict)
            {
                throw new LayoutValidationException(message);
            }

            warnings.Add(message);
        }

        return (bestFields, bestContinentalness, bestShape, bestLayout, bestValidation.Report);
    }

    /// <summary>
    ///     Formats a consolidated layout failure message naming the failure kind, the involved
    ///     anchor indices, and any shared mass centroids.
    /// </summary>
    /// <param name="validation">Failing validation result.</param>
    /// <param name="attempts">Number of attempts made.</param>
    /// <returns>Formatted failure message.</returns>
    private static string FormatLayoutFailure(LayoutValidationResult validation, int attempts)
    {
        var report = validation.Report;
        var builder = new StringBuilder();
        builder.Append($"layout validation failed after {attempts} attempt(s); ");

        if (report.SharedMassGroups.Count > 0)
        {
            builder.Append("anchors sharing a mass: ");
            for (var i = 0; i < report.SharedMassGroups.Count; ++i)
            {
                if (i > 0) builder.Append("; ");
                var group = report.SharedMassGroups[i];
                var centroid = report.Anchors[group[0]];
                builder.Append(string.Join(", ", group.Select(index => $"#{index + 1}")));
                builder.Append($" -> mass ({centroid.CentroidX:F2}, {centroid.CentroidY:F2})");
            }

            builder.Append('.');
        }

        var unmatched = new List<string>();
        foreach (var anchor in report.Anchors)
        {
            if (anchor.Weight >= LayoutValidator.SignificantWeight && !anchor.Matched
                && !anchor.SharedMass)
            {
                unmatched.Add($"#{anchor.Index + 1} ({anchor.X:F2}, {anchor.Y:F2})");
            }
        }

        if (unmatched.Count > 0)
        {
            if (report.SharedMassGroups.Count > 0) builder.Append(' ');
            builder.Append($"unmatched anchors: {string.Join(", ", unmatched)}.");
        }

        return builder.ToString();
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
    ///     Copies a rendered preview's raw pixels into a caller-owned buffer.
    /// </summary>
    /// <param name="buffer">Destination buffer.</param>
    /// <param name="preview">Rendered preview image.</param>
    private static void WritePreviewPixels(Span<byte> buffer, PreviewImage preview)
    {
        var length = preview.Width * preview.Height * 3;
        if (buffer.Length < length)
        {
            throw new ArgumentException(
                $"Preview buffer is too small: {length} bytes required, but the buffer holds {buffer.Length} bytes.");
        }

        preview.Pixels.AsSpan(0, length).CopyTo(buffer);
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
