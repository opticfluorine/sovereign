// Sovereign Engine
// Copyright (c) 2025 opticfluorine
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

using System.Numerics;
using System.Text.RegularExpressions;
using Hexa.NET.ImGui;
using Sovereign.ClientCore.Rendering.Gui;
using Sovereign.ClientCore.Systems.Dialogue;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems.Dialogue;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui;

/// <summary>
///     GUI for in-game dialogue.
/// </summary>
public class DialogueGui(
    IDialogueServices dialogueServices,
    GuiFontAtlas fontAtlas,
    IEventSender eventSender,
    IDialogueController dialogueController)
{
    private const string DownArrow = "\ue02e";
    private const float RelX = 0.5f;
    private const float RelY = 0.75f;
    private const float BaseW = 32.0f;
    private const float BaseH = 14.0f;

    private string cachedMessage = string.Empty;
    private int charsCached;
    private bool wasOpenLastFrame;

    /// <summary>
    ///     Renders the dialogue GUI if any dialogue is active. Does nothing otherwise.
    /// </summary>
    public void Render()
    {
        if (!dialogueServices.TryGetDialogue(out var subject, out var message, out var charsShown)) return;
        if (charsShown != charsCached) UpdateMessageCache(message, charsShown);

        var io = ImGui.GetIO();
        var fontSize = ImGui.GetFontSize();
        var pos = new Vector2(RelX, RelY) * io.DisplaySize;
        var size = new Vector2(BaseW, BaseH) * fontSize;

        ImGui.SetNextWindowSize(size);
        ImGui.SetNextWindowPos(pos, ImGuiCond.Always, new Vector2(RelX));
        if (!ImGui.Begin("Dialogue",
                ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse |
                ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoResize))
        {
            wasOpenLastFrame = false;
            return;
        }

        try
        {
            ImGui.PushTextWrapPos(0.0f);

            // Subject.
            ImGui.PushFont(fontAtlas.DialogueSubjectFont);
            ImGui.Text(subject);
            ImGui.PopFont();

            // Message.
            ImGui.PushFont(fontAtlas.DialogueFont);
            ImGui.Text(cachedMessage);
            ImGui.PopFont();

            // Down arrow symbol at bottom of window.
            var arrowOffset = RelX * ImGui.CalcTextSize(DownArrow);
            ImGui.SetCursorPos(new Vector2(RelX * size.X, size.Y - fontSize) - arrowOffset);
            ImGui.Text(DownArrow);

            ImGui.SetNextFrameWantCaptureKeyboard(true);
            if (wasOpenLastFrame && (ImGui.IsKeyReleased(ImGuiKey.Enter) || ImGui.IsKeyReleased(ImGuiKey.E) ||
                                     ImGui.IsKeyReleased(ImGuiKey.Space)))
                dialogueController.AdvanceDialogue(eventSender);
        }
        finally
        {
            ImGui.End();
            wasOpenLastFrame = true;
        }
    }

    /// <summary>
    ///     Updates the cached version of the message to handle gradual reveal of the message.
    /// </summary>
    /// <param name="message">Full message.</param>
    /// <param name="charsShown">Number of characters to currently display.</param>
    private void UpdateMessageCache(string message, int charsShown)
    {
        charsCached = charsShown;
        if (charsShown == message.Length)
        {
            // No transformation needed if the full message is displayed.
            cachedMessage = message;
            return;
        }

        // If the message is not yet fully revealed, we need to modify the message to hide
        // the remaining portion of the message without changing the ultimate word wrapping
        // pattern. Otherwise, words can jump from one line to the next as the message is gradually
        // revealed. We do this by replacing each hidden non-whitespace character with three
        // non-breaking spaces (since a typical NBSP is ~ 0.25em - 0.33em). There may still be
        // some edge cases with line jumping, but in testing with random "lorem ipsum"s this
        // approach appears to work well.
        var pattern = @$"(?<=^.{{{charsShown}}}.*)\S";
        cachedMessage = Regex.Replace(message, pattern, "\u00A0\u00A0");
    }
}