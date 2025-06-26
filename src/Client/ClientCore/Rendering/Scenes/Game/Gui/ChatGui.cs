// Sovereign Engine
// Copyright (c) 2024 opticfluorine
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
using Microsoft.Extensions.Logging;
using Sovereign.ClientCore.Rendering.Gui;
using Sovereign.ClientCore.Systems.ClientChat;
using Sovereign.EngineCore.Events;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui;

/// <summary>
///     GUI for chat.
/// </summary>
public class ChatGui
{
    private static readonly Vector2 WindowRelPos = new(30.0f, 38.0f);
    private static readonly Vector2 TableSize = new(50.0f, 12.0f);
    private readonly ClientChatController chatController;
    private readonly ClientChatServices chatServices;
    private readonly IEventSender eventSender;
    private readonly ILogger<ChatGui> logger;
    private string input = "";
    private int lastHistoryCount;
    private bool lockScrollToBottom = true;

    public ChatGui(IEventSender eventSender, ClientChatServices chatServices, ClientChatController chatController,
        ILogger<ChatGui> logger)
    {
        this.eventSender = eventSender;
        this.chatServices = chatServices;
        this.chatController = chatController;
        this.logger = logger;
    }

    /// <summary>
    ///     Renders the chat window.
    /// </summary>
    public void Render()
    {
        var io = ImGui.GetIO();
        var fontSize = ImGui.GetFontSize();

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, fontSize * new Vector2(0.222f, 0.0f));

        ImGui.SetNextWindowSize(Vector2.Zero, ImGuiCond.Once);
        ImGui.SetNextWindowPos(fontSize * WindowRelPos, ImGuiCond.Once, new Vector2(0.5f));
        if (ImGui.Begin("Chat", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoCollapse))
        {
            var tableDims = fontSize * TableSize;
            if (ImGui.BeginTable("chat", 1, ImGuiTableFlags.ScrollX | ImGuiTableFlags.ScrollY, tableDims))
            {
                // Draw chat entries.
                foreach (var entry in chatServices.ChatHistory) RenderChatEntry(entry);

                // If scrolled all the way down, keep scrolling down with new messages.
                if (lastHistoryCount == chatServices.ChatHistory.Count)
                    lockScrollToBottom = Math.Abs(ImGui.GetScrollY() - ImGui.GetScrollMaxY()) < 1e-3;
                if (lockScrollToBottom) ImGui.SetScrollHereY(1.0f);
                lastHistoryCount = chatServices.ChatHistory.Count;

                ImGui.EndTable();
            }

            ImGui.PushItemWidth(tableDims.X);
            if (ImGui.IsWindowFocused()) ImGui.SetKeyboardFocusHere();

            if (GuiWorkarounds.InputTextEnterReturns("##chatInput", ref input, 128)) OnSubmit();
            ImGui.PopItemWidth();
            ImGui.End();
        }

        ImGui.PopStyleVar();
    }

    /// <summary>
    ///     Renders a single entry from the chat history.
    /// </summary>
    /// <param name="entry">Chat history entry.</param>
    private void RenderChatEntry(ChatHistoryEntry entry)
    {
        ImGui.TableNextColumn();
        ImGui.TextColored(entry.Color, entry.Message);
    }

    /// <summary>
    ///     Called when the player submits a chat message.
    /// </summary>
    private unsafe void OnSubmit()
    {
        // If no input, remove focus from the window.
        if (input.Length == 0)
        {
            ImGui.SetWindowFocus((byte*)null);
            return;
        }

        // Send message.
        chatController.SendChat(eventSender, input);
        input = "";
    }
}