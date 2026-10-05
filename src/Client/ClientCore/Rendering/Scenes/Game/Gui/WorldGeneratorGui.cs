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
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using Sovereign.ClientCore.Rendering.Gui;
using Sovereign.WorldGen;
using Sovereign.WorldGen.Layout;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui;

/// <summary>
///     Admin GUI for configuring the world generator before submitting world generation
///     commands to the server.
/// </summary>
public sealed class WorldGeneratorGui
{
    private const int DimensionStep = 32;
    private const int MaxDimension = 16384;
    private const int MaxTemplateNameLength = 64;

    /// <summary>
    ///     Longest allowed side of a generated preview image in pixels.
    /// </summary>
    private const int PreviewMaxDimension = 1024;

    /// <summary>
    ///     Name reported for preview plans run by the world generator GUI.
    /// </summary>
    private const string PreviewProfileName = "preview";

    /// <summary>
    ///     Name of the preview staging directory root below the system temporary path.
    /// </summary>
    private const string PreviewScratchDirectoryName = "sovereign-worldgen-preview";

    private readonly IWorldGenPipeline pipeline;
    private readonly WorldGenTemplateResolver templateResolver;
    private readonly GuiExtensions gui;
    private readonly IDynamicTextureManager textureManager;
    private readonly ILogger<WorldGeneratorGui> logger;

    /// <summary>
    ///     Profile under edit; not yet submitted to the server.
    /// </summary>
    private readonly WorldGenProfile profile = NewDefaultProfile();

    private int seed;

    /// <summary>
    ///     Signature of the profile and seed at the time of the last scheduled preview run.
    /// </summary>
    private string? previewSignature;

    /// <summary>
    ///     Most recently scheduled preview run, or null before the first schedule.
    /// </summary>
    private PreviewRun? previewRun;

    /// <summary>
    ///     Background task running the most recently scheduled preview plan.
    /// </summary>
    private Task? previewTask;

    /// <summary>
    ///     Error raised while scheduling the most recent preview run, if any.
    /// </summary>
    private string? scheduleError;

    /// <summary>
    ///     Handle of the dynamic texture displaying the terrain preview, or 0 for none.
    /// </summary>
    private int textureHandle;

    /// <summary>
    ///     Width of the dynamic texture in pixels.
    /// </summary>
    private uint textureWidth;

    /// <summary>
    ///     Height of the dynamic texture in pixels.
    /// </summary>
    private uint textureHeight;

    /// <summary>
    ///     Whether the dynamic texture has been populated by a completed preview run.
    /// </summary>
    private bool textureReady;

    public WorldGeneratorGui(IWorldGenPipeline pipeline, WorldGenTemplateResolver templateResolver,
        GuiExtensions gui, IDynamicTextureManager textureManager,
        ILogger<WorldGeneratorGui> logger)
    {
        this.pipeline = pipeline;
        this.templateResolver = templateResolver;
        this.gui = gui;
        this.textureManager = textureManager;
        this.logger = logger;
    }

    /// <summary>
    ///     Renders the world generator GUI.
    /// </summary>
    public void Render()
    {
        var fontSize = ImGui.GetFontSize();
        ImGui.SetNextWindowSize(fontSize * new Vector2(84.0f, 44.0f), ImGuiCond.Once);
        if (!ImGui.Begin("World Generator")) return;

        UpdatePreview();

        if (ImGui.BeginTable("layout", 3, ImGuiTableFlags.Resizable))
        {
            ImGui.TableSetupColumn("##instancing", ImGuiTableColumnFlags.WidthStretch, 1.0f);
            ImGui.TableSetupColumn("##preview", ImGuiTableColumnFlags.WidthStretch, 1.8f);
            ImGui.TableSetupColumn("##parameters", ImGuiTableColumnFlags.WidthStretch, 1.4f);
            ImGui.TableNextRow();

            ImGui.TableNextColumn();
            if (ImGui.BeginChild("##instancing", Vector2.Zero, ImGuiChildFlags.None,
                    ImGuiWindowFlags.HorizontalScrollbar))
            {
                RenderSeedControl(fontSize);
                RenderWorldSection(fontSize);
                RenderLayoutSection(fontSize);
            }

            ImGui.EndChild();

            ImGui.TableNextColumn();
            RenderPreviewSection();

            ImGui.TableNextColumn();
            if (ImGui.BeginChild("##parameters", Vector2.Zero, ImGuiChildFlags.None,
                    ImGuiWindowFlags.HorizontalScrollbar))
            {
                RenderStoneBandsSection(fontSize);
                RenderCaveLevelsSection(fontSize);
                RenderRiversSection(fontSize);
                RenderCavesSection(fontSize);
                RenderTerrainSection(fontSize);
            }

            ImGui.EndChild();

            ImGui.EndTable();
        }

        ImGui.End();
    }

    /// <summary>
    ///     Renders the seed input control.
    /// </summary>
    private void RenderSeedControl(float fontSize)
    {
        InputInt("Seed", seed, v => seed = v, fontSize * 7.0f);
    }

    /// <summary>
    ///     Renders the world dimensions and Z level controls.
    /// </summary>
    private void RenderWorldSection(float fontSize)
    {
        if (!ImGui.CollapsingHeader("World", ImGuiTreeNodeFlags.DefaultOpen)) return;

        InputInt("Width", profile.Width, v => profile.Width = ClampDimension(v),
            fontSize * 7.0f, DimensionStep, DimensionStep * 10);
        ImGui.SameLine();
        InputInt("Height", profile.Height, v => profile.Height = ClampDimension(v),
            fontSize * 7.0f, DimensionStep, DimensionStep * 10);
        ImGui.TextDisabled("Dimensions must be positive multiples of 32 (max 16384).");

        InputInt("Sea Level Z", profile.SeaLevelZ, v => profile.SeaLevelZ = v, fontSize * 7.0f);
        InputInt("Surface Max Z", profile.SurfaceMaxZ, v => profile.SurfaceMaxZ = v, fontSize * 7.0f);
        InputInt("Rock Floor Z", profile.RockFloorZ, v => profile.RockFloorZ = v, fontSize * 7.0f);
        InputInt("Bedrock Z", profile.BedrockZ, v => profile.BedrockZ = v, fontSize * 7.0f);

        var bedrockTemplate = profile.BedrockTemplate;
        ImGui.SetNextItemWidth(fontSize * 16.0f);
        if (ImGui.InputText("Bedrock Template", ref bedrockTemplate, MaxTemplateNameLength + 1))
            profile.BedrockTemplate = bedrockTemplate;
    }

    /// <summary>
    ///     Renders the layout options editor.
    /// </summary>
    private void RenderLayoutSection(float fontSize)
    {
        if (!ImGui.CollapsingHeader("Layout", ImGuiTreeNodeFlags.DefaultOpen)) return;

        var enabled = profile.Layout is not null;
        if (ImGui.Checkbox("Enable Layout", ref enabled))
            profile.Layout = enabled ? NewDefaultLayout() : null;
        if (profile.Layout is not { } layout) return;

        InputFloat("Strength", layout.Strength, v => layout.Strength = v, fontSize * 5.0f);
        RenderConnectivityCombo(fontSize, layout);
        RenderPresetCombo(fontSize, layout);
        if (layout.Preset is not null)
        {
            ImGui.TextDisabled("Preset overrides explicit anchors.");
            return;
        }

        InputInt("Anchor Count", layout.AnchorCount, v => layout.AnchorCount = v, fontSize * 5.0f);
        RenderAnchorList(fontSize, layout);
    }

    /// <summary>
    ///     Renders the layout connectivity combo box.
    /// </summary>
    private static void RenderConnectivityCombo(float fontSize, LayoutOptions layout)
    {
        var connectivity = layout.Connectivity;
        ImGui.SetNextItemWidth(fontSize * 7.0f);
        if (!ImGui.BeginCombo("Connectivity", connectivity.ToString())) return;

        foreach (var value in Enum.GetValues<LayoutConnectivity>())
        {
            if (ImGui.Selectable(value.ToString(), value == connectivity))
                layout.Connectivity = value;
        }

        ImGui.EndCombo();
    }

    /// <summary>
    ///     Renders the layout preset combo box.
    /// </summary>
    private static void RenderPresetCombo(float fontSize, LayoutOptions layout)
    {
        const string customLabel = "Custom";
        var presets = LayoutPresets.Names;
        var currentIndex = 0;
        for (var i = 0; i < presets.Count; ++i)
        {
            if (presets[i] == layout.Preset) currentIndex = i + 1;
        }

        ImGui.SetNextItemWidth(fontSize * 9.0f);
        if (!ImGui.BeginCombo("Preset", currentIndex == 0 ? customLabel : presets[currentIndex - 1]))
            return;

        if (ImGui.Selectable(customLabel, currentIndex == 0)) layout.Preset = null;
        for (var i = 0; i < presets.Count; ++i)
        {
            if (ImGui.Selectable(presets[i], currentIndex == i + 1))
                layout.Preset = presets[i];
        }

        ImGui.EndCombo();
    }

    /// <summary>
    ///     Renders the layout anchor list editor.
    /// </summary>
    private static void RenderAnchorList(float fontSize, LayoutOptions layout)
    {
        layout.Anchors ??= [];
        var anchors = layout.Anchors;
        int? removeIndex = null;
        for (var i = 0; i < anchors.Count; ++i)
        {
            var anchor = anchors[i];
            ImGui.PushID(i);
            if (ImGui.TreeNodeEx($"Anchor {i}", ImGuiTreeNodeFlags.DefaultOpen))
            {
                InputFloat("X", anchor.X, v => anchor.X = v, fontSize * 4.0f);
                ImGui.SameLine();
                InputFloat("Y", anchor.Y, v => anchor.Y = v, fontSize * 4.0f);
                InputFloat("Radius", anchor.Radius, v => anchor.Radius = v, fontSize * 4.0f);
                ImGui.SameLine();
                InputFloat("Weight", anchor.Weight, v => anchor.Weight = v, fontSize * 4.0f);
                InputFloat("Jitter", anchor.Jitter, v => anchor.Jitter = v, fontSize * 4.0f);
                InputFloat("Mountain Bias", anchor.MountainBias, v => anchor.MountainBias = v,
                    fontSize * 4.0f);
                InputFloat("Temperature Bias", anchor.TemperatureBias, v => anchor.TemperatureBias = v,
                    fontSize * 4.0f);
                InputFloat("Moisture Bias", anchor.MoistureBias, v => anchor.MoistureBias = v,
                    fontSize * 4.0f);
                InputFloat("Roughness Bias", anchor.RoughnessBias, v => anchor.RoughnessBias = v,
                    fontSize * 4.0f);
                if (ImGui.SmallButton("Remove")) removeIndex = i;
                ImGui.TreePop();
            }

            ImGui.PopID();
        }

        if (removeIndex is { } index) anchors.RemoveAt(index);

        if (ImGui.Button("Add Anchor")) anchors.Add(new LayoutAnchor());
    }

    /// <summary>
    ///     Renders the stone band editor.
    /// </summary>
    private void RenderStoneBandsSection(float fontSize)
    {
        if (!ImGui.CollapsingHeader("Stone Bands", ImGuiTreeNodeFlags.DefaultOpen)) return;

        var bands = profile.StoneBands;
        for (var i = bands.Count - 1; i >= 0; --i)
        {
            var band = bands[i];
            ImGui.PushID(i);

            InputInt("From Z", band.FromZ, v => band.FromZ = v, fontSize * 6.0f);
            ImGui.SameLine();
            InputInt("To Z", band.ToZ, v => band.ToZ = v, fontSize * 6.0f);
            ImGui.SameLine();
            var template = band.Template;
            ImGui.SetNextItemWidth(fontSize * 12.0f);
            if (ImGui.InputText("Template", ref template, MaxTemplateNameLength + 1))
                band.Template = template;
            ImGui.SameLine();
            if (ImGui.SmallButton("Remove")) bands.RemoveAt(i);

            ImGui.PopID();
        }

        if (ImGui.Button("Add Stone Band"))
            bands.Add(new StoneBand { FromZ = -1, ToZ = -1, Template = "" });
    }

    /// <summary>
    ///     Renders the cave level editor.
    /// </summary>
    private void RenderCaveLevelsSection(float fontSize)
    {
        if (!ImGui.CollapsingHeader("Cave Levels", ImGuiTreeNodeFlags.DefaultOpen)) return;

        var enabled = profile.CaveLevels is not null;
        if (ImGui.Checkbox("Enable Cave Levels", ref enabled))
            profile.CaveLevels = enabled ? NewDefaultCaveLevels() : null;
        if (profile.CaveLevels is not { } caveLevels) return;

        for (var i = caveLevels.Count - 1; i >= 0; --i)
        {
            var caveLevel = caveLevels[i];
            ImGui.PushID(i);

            InputInt("Floor Z", caveLevel.FloorZ, v => caveLevel.FloorZ = v, fontSize * 6.0f);
            ImGui.SameLine();
            InputInt("Headroom", caveLevel.Headroom, v => caveLevel.Headroom = v, fontSize * 6.0f);
            ImGui.SameLine();
            if (ImGui.SmallButton("Remove")) caveLevels.RemoveAt(i);

            ImGui.PopID();
        }

        if (ImGui.Button("Add Cave Level"))
            caveLevels.Add(new CaveLevel { FloorZ = -1, Headroom = 2 });
    }

    /// <summary>
    ///     Renders the river options editor.
    /// </summary>
    private void RenderRiversSection(float fontSize)
    {
        if (!ImGui.CollapsingHeader("Rivers", ImGuiTreeNodeFlags.DefaultOpen)) return;

        var enabled = profile.Rivers is not null;
        if (ImGui.Checkbox("Enable Rivers", ref enabled))
            profile.Rivers = enabled ? NewDefaultRivers() : null;
        if (profile.Rivers is not { } rivers) return;

        InputInt("Max Count", rivers.MaxCount, v => rivers.MaxCount = v, fontSize * 7.0f);
        ImGui.SameLine();
        InputInt("Min Length", rivers.MinLength, v => rivers.MinLength = v, fontSize * 7.0f);
    }

    /// <summary>
    ///     Renders the cave options editor.
    /// </summary>
    private void RenderCavesSection(float fontSize)
    {
        if (!ImGui.CollapsingHeader("Caves", ImGuiTreeNodeFlags.DefaultOpen)) return;

        var enabled = profile.Caves is not null;
        if (ImGui.Checkbox("Enable Caves", ref enabled))
            profile.Caves = enabled ? NewDefaultCaves() : null;
        if (profile.Caves is not { } caves) return;

        InputInt("Shafts per Level Pair", caves.ShaftsPerLevelPair, v => caves.ShaftsPerLevelPair = v,
            fontSize * 7.0f);
        ImGui.SameLine();
        InputInt("Surface Mouths", caves.SurfaceMouths, v => caves.SurfaceMouths = v, fontSize * 7.0f);
        InputDouble("Porosity", caves.Porosity, v => caves.Porosity = v, fontSize * 7.0f);
        ImGui.SameLine();
        InputInt("Max Mouth Depth Z", caves.MaxMouthDepthZ, v => caves.MaxMouthDepthZ = v, fontSize * 7.0f);
        InputInt("Min Tunnel Width", caves.MinTunnelWidth, v => caves.MinTunnelWidth = v, fontSize * 7.0f);
        ImGui.SameLine();
        InputInt("Mouth Min Land Distance", caves.MouthMinLandDistance, v => caves.MouthMinLandDistance = v,
            fontSize * 7.0f);
    }

    /// <summary>
    ///     Renders the terrain tuning options editor.
    /// </summary>
    private void RenderTerrainSection(float fontSize)
    {
        if (!ImGui.CollapsingHeader("Terrain")) return;

        var width = fontSize * 8.0f;
        var terrain = profile.Terrain;
        InputFloat("Continentalness Wavelength Factor", terrain.ContinentalnessWavelengthFactor,
            v => terrain.ContinentalnessWavelengthFactor = v, width);
        InputInt("Continentalness Octaves", terrain.ContinentalnessOctaves,
            v => terrain.ContinentalnessOctaves = v, width);
        InputFloat("Warp Amplitude Inner", terrain.WarpAmplitudeInner,
            v => terrain.WarpAmplitudeInner = v, width);
        InputFloat("Warp Amplitude Outer", terrain.WarpAmplitudeOuter,
            v => terrain.WarpAmplitudeOuter = v, width);
        InputFloat("Ocean Threshold", terrain.Thresholds.Ocean,
            v => terrain.Thresholds.Ocean = v, width);
        InputFloat("Coast Threshold", terrain.Thresholds.Coast,
            v => terrain.Thresholds.Coast = v, width);
        InputFloat("Inland Threshold", terrain.Thresholds.Inland,
            v => terrain.Thresholds.Inland = v, width);
        InputInt("Max Straight River Run", terrain.MaxStraightRiverRun,
            v => terrain.MaxStraightRiverRun = v, width);
    }

    /// <summary>
    ///     Schedules preview generation when the profile or seed changes, and publishes the
    ///     result of a completed preview run to its dynamic texture.
    /// </summary>
    private void UpdatePreview()
    {
        var signature = ComputeSignature();
        if (signature != previewSignature)
        {
            previewSignature = signature;
            SchedulePreview();
        }

        if (previewTask is not { IsCompleted: true }) return;
        if (previewRun is not { PlanCompleted: true, Published: false } run) return;

        run.Published = true;
        textureManager.UpdateTexture(textureHandle, run.PreviewBuffer);
        textureReady = true;
    }

    /// <summary>
    ///     Cancels any in-flight preview run and schedules a new one from the current
    ///     profile and seed.
    /// </summary>
    private void SchedulePreview()
    {
        previewRun?.Cts.Cancel();
        scheduleError = null;
        textureReady = false;

        PreviewRun run;
        try
        {
            run = CreateRun();
        }
        catch (Exception e)
        {
            scheduleError = e.Message;
            logger.LogError(e, "World generator preview could not be scheduled.");
            return;
        }

        RecreateTextureIfNeeded(run);
        previewRun = run;
        previewTask = RunPreview(run, previewTask);
    }

    /// <summary>
    ///     Creates the state of one preview run from the current profile and seed.
    /// </summary>
    /// <returns>The new preview run.</returns>
    private PreviewRun CreateRun()
    {
        // The pipeline reads the profile snapshot on a background task, so the snapshot
        // must be isolated from later edits made by the GUI.
        var snapshot = CloneProfile(profile);
        snapshot.Preview = new PreviewOptions { MaxDimension = PreviewMaxDimension };
        var resolvedTemplates = templateResolver.Resolve(snapshot, PreviewProfileName);

        var bufferLength = pipeline.PreviewBufferLength(snapshot);
        var caveLevelCount = snapshot.CaveLevels?.Count ?? 0;
        return new PreviewRun
        {
            Profile = snapshot,
            Seed = unchecked((ulong)seed),
            Templates = resolvedTemplates,
            StagingDirectory = NewStagingDirectory(),
            PreviewBuffer = new byte[bufferLength],
            CavePreviewBuffers = caveLevelCount > 0
                ? Enumerable.Range(0, caveLevelCount)
                    .Select(_ => new Memory<byte>(new byte[bufferLength]))
                    .ToArray()
                : null,
            Cts = new CancellationTokenSource()
        };
    }

    /// <summary>
    ///     Replaces the preview texture when a run's preview dimensions differ from the
    ///     current texture dimensions; otherwise the existing texture is updated in place.
    /// </summary>
    /// <param name="run">Scheduled preview run.</param>
    private void RecreateTextureIfNeeded(PreviewRun run)
    {
        var (width, height) = PreviewDimensions(run.Profile);
        if (textureHandle != 0 && textureWidth == (uint)width && textureHeight == (uint)height)
            return;

        if (textureHandle != 0) textureManager.RemoveTexture(textureHandle);
        textureHandle = textureManager.AddTexture((uint)width, (uint)height,
            DynamicTextureSampling.Linear);
        textureWidth = (uint)width;
        textureHeight = (uint)height;
    }

    /// <summary>
    ///     Runs the pipeline for a preview run on a background task that first waits for
    ///     the superseded preview task to unwind.
    /// </summary>
    /// <param name="run">Preview run to execute.</param>
    /// <param name="previousTask">Task of the superseded preview run, if any.</param>
    /// <returns>The background task.</returns>
    private Task RunPreview(PreviewRun run, Task? previousTask)
    {
        var token = run.Cts.Token;
        return Task.Run(async () =>
        {
            try
            {
                if (previousTask is not null)
                {
                    try
                    {
                        await previousTask;
                    }
                    catch
                    {
                        // The superseded run failed or was canceled; its error, if any, is
                        // already reported through its own run state.
                    }
                }

                token.ThrowIfCancellationRequested();
                Directory.CreateDirectory(run.StagingDirectory);
                pipeline.Plan(run.Profile, PreviewProfileName, run.Seed, 0, 0, run.PreviewBuffer,
                    run.CavePreviewBuffers, run.StagingDirectory, run.Templates, null, token);
                run.PlanCompleted = true;
            }
            catch (OperationCanceledException)
            {
                // The run was superseded by a newer preview request.
            }
            catch (Exception e)
            {
                logger.LogError(e, "World generator preview failed.");
                run.ErrorMessage = e.Message;
            }
            finally
            {
                try
                {
                    Directory.Delete(run.StagingDirectory, true);
                }
                catch (Exception e)
                {
                    logger.LogWarning(e, "Failed to remove preview staging directory {Directory}.",
                        run.StagingDirectory);
                }
            }
        });
    }

    /// <summary>
    ///     Computes the change-detection signature of the current profile and seed.
    /// </summary>
    /// <returns>Signature string.</returns>
    private string ComputeSignature()
    {
        return JsonSerializer.Serialize(profile) + "|" + seed;
    }

    /// <summary>
    ///     Creates a deep copy of the given profile via a JSON round trip.
    /// </summary>
    /// <param name="source">Profile to copy.</param>
    /// <returns>Deep copy of the profile.</returns>
    private static WorldGenProfile CloneProfile(WorldGenProfile source)
    {
        return JsonSerializer.Deserialize<WorldGenProfile>(JsonSerializer.Serialize(source))!;
    }

    /// <summary>
    ///     Computes the dimensions of a profile's preview image: the footprint box-downscaled
    ///     by an equal factor on both axes so that the long side is at most the preview
    ///     maximum dimension.
    /// </summary>
    /// <param name="profile">World generation profile.</param>
    /// <returns>Preview width and height in pixels.</returns>
    private static (int Width, int Height) PreviewDimensions(WorldGenProfile profile)
    {
        var longSide = Math.Max(profile.Width, profile.Height);
        var factor = longSide <= PreviewMaxDimension
            ? 1
            : (longSide + PreviewMaxDimension - 1) / PreviewMaxDimension;
        return ((profile.Width + factor - 1) / factor, (profile.Height + factor - 1) / factor);
    }

    /// <summary>
    ///     Resolves a fresh staging directory path for one preview run below the system
    ///     temporary path.
    /// </summary>
    /// <returns>Absolute path of the staging directory.</returns>
    private static string NewStagingDirectory()
    {
        return Path.Combine(Path.GetTempPath(), PreviewScratchDirectoryName,
            "staging_" + Guid.NewGuid().ToString("N"));
    }

    /// <summary>
    ///     State of one background preview generation run.
    /// </summary>
    private sealed class PreviewRun
    {
        /// <summary>
        ///     Snapshot of the profile used by this run.
        /// </summary>
        public required WorldGenProfile Profile { get; init; }

        /// <summary>
        ///     Root world seed of this run.
        /// </summary>
        public required ulong Seed { get; init; }

        /// <summary>
        ///     Profile template names resolved against the live template entity set.
        /// </summary>
        public required WorldGenResolvedTemplates Templates { get; init; }

        /// <summary>
        ///     Staging directory receiving the run's segment blobs.
        /// </summary>
        public required string StagingDirectory { get; init; }

        /// <summary>
        ///     Buffer receiving the terrain preview as packed RGBA quads with alpha 255.
        /// </summary>
        public required byte[] PreviewBuffer { get; init; }

        /// <summary>
        ///     Buffers receiving one cave level preview each, or null when the profile has
        ///     no cave levels.
        /// </summary>
        public required Memory<byte>[]? CavePreviewBuffers { get; init; }

        /// <summary>
        ///     Cancellation source of this run.
        /// </summary>
        public required CancellationTokenSource Cts { get; init; }

        /// <summary>
        ///     Whether the pipeline plan completed successfully.
        /// </summary>
        public volatile bool PlanCompleted;

        /// <summary>
        ///     Whether the run's preview buffer has been published to the texture.
        /// </summary>
        public bool Published;

        /// <summary>
        ///     Error raised by the pipeline, if any.
        /// </summary>
        public volatile string? ErrorMessage;
    }

    /// <summary>
    ///     Renders the live preview section.
    /// </summary>
    private void RenderPreviewSection()
    {
        ImGui.SeparatorText("Preview");
        if (ImGui.BeginChild("##preview", Vector2.Zero, ImGuiChildFlags.Borders,
                ImGuiWindowFlags.HorizontalScrollbar))
        {
            if (scheduleError is { } scheduleMessage)
            {
                ImGui.TextWrapped(scheduleMessage);
            }
            else if (previewTask is not { IsCompleted: true } || !textureReady)
            {
                ImGui.TextDisabled("Loading...");
            }
            else if (previewRun is { ErrorMessage: { } errorMessage })
            {
                ImGui.TextWrapped(errorMessage);
            }
            else
            {
                gui.Image(textureHandle, PreviewDrawSize());
            }
        }
        ImGui.EndChild();
    }

    /// <summary>
    ///     Computes the draw size of the preview image, scaled down to fit the available
    ///     region while preserving the aspect ratio.
    /// </summary>
    /// <returns>Draw size in pixels.</returns>
    private Vector2 PreviewDrawSize()
    {
        var avail = ImGui.GetContentRegionAvail();
        var scale = Math.Min(Math.Min(avail.X / textureWidth, avail.Y / textureHeight), 1.0f);
        if (!float.IsFinite(scale) || scale <= 0.0f) scale = 1.0f;
        return new Vector2(textureWidth * scale, textureHeight * scale);
    }

    /// <summary>
    ///     Renders an integer input control and applies any edited value.
    /// </summary>
    private static void InputInt(string label, int value, Action<int> setValue, float itemWidth,
        int step = 1, int stepFast = 8)
    {
        ImGui.SetNextItemWidth(itemWidth);
        if (ImGui.InputInt(label, ref value, step, stepFast)) setValue(value);
    }

    /// <summary>
    ///     Renders a float input control and applies any edited value.
    /// </summary>
    private static void InputFloat(string label, float value, Action<float> setValue, float itemWidth)
    {
        ImGui.SetNextItemWidth(itemWidth);
        if (ImGui.InputFloat(label, ref value)) setValue(value);
    }

    /// <summary>
    ///     Renders a double input control and applies any edited value.
    /// </summary>
    private static void InputDouble(string label, double value, Action<double> setValue, float itemWidth)
    {
        ImGui.SetNextItemWidth(itemWidth);
        if (ImGui.InputDouble(label, ref value)) setValue(value);
    }

    /// <summary>
    ///     Clamps a world dimension to the allowed range and snaps it to the dimension grid.
    /// </summary>
    private static int ClampDimension(int value)
    {
        return Math.Clamp(value, DimensionStep, MaxDimension) / DimensionStep * DimensionStep;
    }

    /// <summary>
    ///     Creates a profile with the shipped default generation parameters.
    /// </summary>
    private static WorldGenProfile NewDefaultProfile()
    {
        return new WorldGenProfile
        {
            Width = 2048,
            Height = 2048,
            SeaLevelZ = 12,
            SurfaceMaxZ = 28,
            RockFloorZ = -63,
            BedrockZ = -64,
            BedrockTemplate = "Bedrock",
            StoneBands = NewDefaultStoneBands(),
            CaveLevels = NewDefaultCaveLevels(),
            Rivers = NewDefaultRivers(),
            Caves = NewDefaultCaves(),
            Terrain = new TerrainOptions()
        };
    }

    /// <summary>
    ///     Creates the default stone bands.
    /// </summary>
    private static List<StoneBand> NewDefaultStoneBands()
    {
        return
        [
            new StoneBand { FromZ = -16, ToZ = -1, Template = "Shale" },
            new StoneBand { FromZ = -40, ToZ = -17, Template = "Granite" },
            new StoneBand { FromZ = -63, ToZ = -41, Template = "Basalt" }
        ];
    }

    /// <summary>
    ///     Creates the default cave levels.
    /// </summary>
    private static List<CaveLevel> NewDefaultCaveLevels()
    {
        return
        [
            new CaveLevel { FloorZ = -16, Headroom = 2 },
            new CaveLevel { FloorZ = -32, Headroom = 2 },
            new CaveLevel { FloorZ = -48, Headroom = 2 }
        ];
    }

    /// <summary>
    ///     Creates the default river options.
    /// </summary>
    private static RiverOptions NewDefaultRivers()
    {
        return new RiverOptions { MaxCount = 40, MinLength = 64 };
    }

    /// <summary>
    ///     Creates the default cave options.
    /// </summary>
    private static CaveOptions NewDefaultCaves()
    {
        return new CaveOptions
        {
            ShaftsPerLevelPair = 3,
            SurfaceMouths = 2,
            Porosity = 0.28,
            MaxMouthDepthZ = 48
        };
    }

    /// <summary>
    ///     Creates the default layout options.
    /// </summary>
    private static LayoutOptions NewDefaultLayout()
    {
        return new LayoutOptions
        {
            Strength = 1.0f,
            Preset = "continents3"
        };
    }
}
