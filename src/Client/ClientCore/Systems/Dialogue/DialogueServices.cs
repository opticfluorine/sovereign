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

using System.Diagnostics.CodeAnalysis;

namespace Sovereign.ClientCore.Systems.Dialogue;

public interface IDialogueServices
{
    /// <summary>
    ///     Tries to get the current dialogue.
    /// </summary>
    /// <param name="subject">Subject (e.g. who is talking).</param>
    /// <param name="message">Message.</param>
    /// <param name="charsShown">Number of characters to currently display.</param>
    /// <returns>true if a dialogue is active, false otherwise.</returns>
    bool TryGetDialogue([NotNullWhen(true)] out string? subject,
        [NotNullWhen(true)] out string? message, out int charsShown);
}

/// <summary>
///     Implementation of IDialogueServices.
/// </summary>
internal class DialogueServices(DialogueQueue dialogueQueue) : IDialogueServices
{
    public bool TryGetDialogue([NotNullWhen(true)] out string? subject, [NotNullWhen(true)] out string? message,
        out int charsShown)
    {
        return dialogueQueue.TryGetDialogue(out subject, out message, out charsShown);
    }
}