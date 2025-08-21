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

using System.Threading;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems.Dialogue;
using Sovereign.EngineUtil.Attributes;

namespace Sovereign.ServerCore.Systems.Dialogue;

/// <summary>
///     Provides the "dialogue" Lua module for displaying dialogue to players.
/// </summary>
[ScriptableLibrary("dialogue")]
public class DialogueScripting(IDialogueController controller, IEventSender eventSender)
{
    private readonly Lock eventLock = new();

    /// <summary>
    ///     Scripting API for adding a dialogue item for the given player.
    /// </summary>
    /// <param name="targetEntityId">Player to receive the dialogue.</param>
    /// <param name="subject">Dialogue subject (e.g. who is speaking).</param>
    /// <param name="message">Dialogue message.</param>
    [ScriptableFunction("Show")]
    public void Show(ulong targetEntityId, string subject, string message)
    {
        lock (eventLock)
        {
            controller.AddDialogue(eventSender, targetEntityId, subject, message);
        }
    }

    /// <summary>
    ///     Scripting API for adding a dialogue item with profile sprite for the given player.
    /// </summary>
    /// <param name="targetEntityId">Player to receive the dialogue.</param>
    /// <param name="profileSpriteId">Profile sprite ID.</param>
    /// <param name="subject">Dialogue subject (e.g. who is speaking).</param>
    /// <param name="message">Dialogue message.</param>
    [ScriptableFunction("ShowProfile")]
    public void ShowProfile(ulong targetEntityId, int profileSpriteId, string subject, string message)
    {
        lock (eventLock)
        {
            controller.AddDialogue(eventSender, targetEntityId, subject, message, profileSpriteId);
        }
    }
}