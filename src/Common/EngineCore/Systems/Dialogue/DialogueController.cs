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

using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;

namespace Sovereign.EngineCore.Systems.Dialogue;

/// <summary>
///     Controller API for the Dialogue system.
/// </summary>
public interface IDialogueController
{
    /// <summary>
    ///     Adds dialogue for the given player.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="targetEntityId">Player to receive the dialogue.</param>
    /// <param name="subject">Dialogue subject (e.g. who is speaking).</param>
    /// <param name="message">Dialogue message.</param>
    void AddDialogue(IEventSender eventSender, ulong targetEntityId, string subject, string message);

    /// <summary>
    ///     Advances the current dialogue.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    void AdvanceDialogue(IEventSender eventSender);
}

/// <summary>
///     Implementation of IDialogueController.
/// </summary>
internal class DialogueController : IDialogueController
{
    public void AddDialogue(IEventSender eventSender, ulong targetEntityId, string subject, string message)
    {
        var details = new DialogueEventDetails
        {
            TargetEntityId = targetEntityId,
            Subject = subject,
            Message = message
        };
        var ev = new Event(EventId.Client_Dialogue_Enqueue, details);
        eventSender.SendEvent(ev);
    }

    public void AdvanceDialogue(IEventSender eventSender)
    {
        var ev = new Event(EventId.Client_Dialogue_Advance);
        eventSender.SendEvent(ev);
    }
}