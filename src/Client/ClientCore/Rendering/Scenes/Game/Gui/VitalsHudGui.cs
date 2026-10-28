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
using System.Numerics;
using Hexa.NET.ImGui;
using Sovereign.ClientCore.Rendering.Gui;
using Sovereign.ClientCore.Systems.ClientState;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui;

/// <summary>
///     HUD window showing the local player's health, stamina, and mana as three
///     stacked progress bars in the top-left corner of the screen.
/// </summary>
/// <remarks>
///     Each bar is drawn from a per-vital dynamic texture that is created lazily on
///     first render and re-uploaded only when the quantized fill fraction changes.
/// </remarks>
public sealed class VitalsHudGui(
    GuiExtensions guiExtensions,
    ClientStateServices stateServices,
    GuiFontAtlas fontAtlas,
    IDynamicTextureManager dynamicTextureManager,
    HealthComponentCollection healths,
    StaminaComponentCollection staminas,
    ManaComponentCollection manas)
{
    /// <summary>Distance in pixels from the top-left corner of the screen.</summary>
    private const float WindowMarginPixels = 12.0f;

    /// <summary>Vertical spacing in pixels between adjacent bars.</summary>
    private const float BarSpacingPixels = 4.0f;

    /// <summary>Bar width in pixels.</summary>
    private const float BarWidthPixels = 200.0f;

    /// <summary>Largest fraction of the display width a bar may occupy.</summary>
    private const float MaxBarWidthFraction = 0.25f;

    /// <summary>Smallest bar height in pixels.</summary>
    private const float MinBarHeightPixels = 14.0f;

    /// <summary>Inset in pixels between the bar edge and the numeric label.</summary>
    private const float LabelInsetPixels = 3.0f;

    /// <summary>Bar texture width in pixels.</summary>
    private const int TextureWidth = 256;

    /// <summary>Bar texture height in pixels.</summary>
    private const int TextureHeight = 12;

    /// <summary>Dynamic texture handle used before a texture is created.</summary>
    private const int NoTexture = -1;

    /// <summary>Color of the numeric labels overlaid on the bars.</summary>
    private static readonly Vector4 LabelColor = new(0.95f, 0.95f, 0.95f, 1.0f);

    /// <summary>Fill color for the health bar.</summary>
    private static readonly VitalsBarColor HealthFillColor = VitalsBarPixels.MakeFillColor(200, 40, 40);

    /// <summary>Fill color for the stamina bar.</summary>
    private static readonly VitalsBarColor StaminaFillColor = VitalsBarPixels.MakeFillColor(220, 170, 40);

    /// <summary>Fill color for the mana bar.</summary>
    private static readonly VitalsBarColor ManaFillColor = VitalsBarPixels.MakeFillColor(60, 110, 220);

    /// <summary>Per-vital texture state, in display order: health, stamina, mana.</summary>
    private readonly VitalBarState[] bars = [new VitalBarState(), new VitalBarState(), new VitalBarState()];

    /// <summary>
    ///     Reusable pixel buffer for texture uploads. Uploaded only when the quantized
    ///     fill fraction of a bar changes, since texture uploads cost bandwidth even
    ///     when the bar is visually idle.
    /// </summary>
    private readonly byte[] pixelBuffer = new byte[TextureWidth * TextureHeight * 4];

    /// <summary>
    ///     Renders the vitals HUD. Does nothing while the local player is not logged in.
    /// </summary>
    public void Render()
    {
        if (!stateServices.TryGetSelectedPlayer(out var playerId)) return;

        var displaySize = ImGui.GetIO().DisplaySize;
        var barWidth = MathF.Min(BarWidthPixels, displaySize.X * MaxBarWidthFraction);
        var barHeight = GetBarHeight();

        ImGui.SetNextWindowPos(new Vector2(WindowMarginPixels, WindowMarginPixels), ImGuiCond.Always);
        ImGui.SetNextWindowSize(new Vector2(barWidth, 3.0f * barHeight + 2.0f * BarSpacingPixels));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);

        if (ImGui.Begin("VitalsHUD", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar |
                                      ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoScrollbar |
                                      ImGuiWindowFlags.NoBackground))
        {
            DrawBar(bars[0], 0, playerId, barWidth, barHeight, healths, HealthFillColor);
            DrawBar(bars[1], 1, playerId, barWidth, barHeight, staminas, StaminaFillColor);
            DrawBar(bars[2], 2, playerId, barWidth, barHeight, manas, ManaFillColor);
        }

        ImGui.PopStyleVar();
        ImGui.End();
    }

    /// <summary>
    ///     Draws one vital bar and its numeric label.
    /// </summary>
    /// <param name="bar">Texture state for the bar.</param>
    /// <param name="row">Zero-based row index counting from the top.</param>
    /// <param name="playerId">Local player entity ID.</param>
    /// <param name="barWidth">Bar draw width in pixels.</param>
    /// <param name="barHeight">Bar draw height in pixels.</param>
    /// <param name="vitals">Backing component collection.</param>
    /// <param name="fillColor">Fill color for the bar.</param>
    private void DrawBar(VitalBarState bar, int row, ulong playerId, float barWidth, float barHeight,
        BaseComponentCollection<Vital> vitals, VitalsBarColor fillColor)
    {
        if (!vitals.HasComponentForEntity(playerId)) return;
        var vital = vitals[playerId];

        EnsureTexture(bar);
        var fraction = vital.MaxValue > 0
            ? Math.Clamp((float)vital.Value / vital.MaxValue, 0.0f, 1.0f)
            : 0.0f;
        UpdateTexture(bar, fraction, fillColor);

        var yPos = row * (barHeight + BarSpacingPixels);
        ImGui.SetCursorPos(new Vector2(0.0f, yPos));
        if (!guiExtensions.Image(bar.Handle, new Vector2(barWidth, barHeight)))
        {
            // The handle is no longer valid; drop it so the texture is lazily re-created next frame.
            bar.Handle = NoTexture;
            return;
        }

        DrawLabel(barHeight, yPos, vital);
    }

    /// <summary>
    ///     Draws the numeric value label overlaid on a bar.
    /// </summary>
    /// <param name="barHeight">Bar draw height in pixels.</param>
    /// <param name="yPos">Top of the bar in window coordinates.</param>
    /// <param name="vital">Vital being displayed.</param>
    private void DrawLabel(float barHeight, float yPos, Vital vital)
    {
        ImGui.PushFont(fontAtlas.ItemLabelFont);
        var labelHeight = ImGui.GetFontSize();
        ImGui.SetCursorPos(new Vector2(LabelInsetPixels, yPos + 0.5f * (barHeight - labelHeight)));
        ImGui.TextColored(LabelColor, $"{vital.Value} / {vital.MaxValue}");
        ImGui.PopFont();
    }

    /// <summary>
    ///     Creates the dynamic texture for a bar on first use.
    /// </summary>
    /// <param name="bar">Texture state for the bar.</param>
    private void EnsureTexture(VitalBarState bar)
    {
        if (bar.Handle != NoTexture) return;

        bar.Handle = dynamicTextureManager.AddTexture(TextureWidth, TextureHeight,
            DynamicTextureSampling.Point);
        bar.QuantizedFraction = -1.0f;
    }

    /// <summary>
    ///     Updates the dynamic texture of a bar if the quantized fill fraction changed
    ///     since the last upload.
    /// </summary>
    /// <param name="bar">Texture state for the bar.</param>
    /// <param name="fraction">Current fill fraction in [0, 1].</param>
    /// <param name="fillColor">Fill color for the bar.</param>
    private void UpdateTexture(VitalBarState bar, float fraction, VitalsBarColor fillColor)
    {
        var quantizedFraction = VitalsBarPixels.QuantizeFraction(TextureWidth, fraction);
        if (quantizedFraction == bar.QuantizedFraction) return;

        VitalsBarPixels.FillBar(pixelBuffer, TextureWidth, TextureHeight, fraction, fillColor);
        dynamicTextureManager.UpdateTexture(bar.Handle, pixelBuffer);
        bar.QuantizedFraction = quantizedFraction;
    }

    /// <summary>
    ///     Gets the bar draw height in pixels, sized to fit the numeric label.
    /// </summary>
    /// <returns>Bar draw height in pixels.</returns>
    private float GetBarHeight()
    {
        ImGui.PushFont(fontAtlas.ItemLabelFont);
        var labelHeight = ImGui.GetFontSize();
        ImGui.PopFont();
        return MathF.Max(MinBarHeightPixels, labelHeight + 2.0f * LabelInsetPixels);
    }

    /// <summary>
    ///     Cached dynamic texture state for one vital bar.
    /// </summary>
    private sealed class VitalBarState
    {
        /// <summary>Dynamic texture handle, or <see cref="NoTexture"/> before creation.</summary>
        public int Handle = NoTexture;

        /// <summary>Last uploaded fill fraction, quantized to the texture resolution.</summary>
        public float QuantizedFraction = -1.0f;
    }
}
