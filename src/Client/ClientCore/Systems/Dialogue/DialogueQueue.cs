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

using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using Sovereign.ClientCore.Configuration;
using Sovereign.EngineCore.Timing;

namespace Sovereign.ClientCore.Systems.Dialogue;

/// <summary>
///     Manages the client-side dialogue queue and state.
/// </summary>
internal sealed class DialogueQueue(ISystemTimer systemTimer, IOptions<DisplayOptions> options)
{
    private readonly ulong MessageRate = 1000000UL / options.Value.TextSpeedCharsPerSecond;
    private readonly ConcurrentQueue<DialogueItem> queue = new();
    private ulong lastItemStartTime;

    /// <summary>
    ///     Tries to get the current dialogue.
    /// </summary>
    /// <param name="subject">Subject (e.g. who is talking).</param>
    /// <param name="message">Message.</param>
    /// <param name="profileSpriteId">Profile sprite ID. Negative means no profile sprite.</param>
    /// <param name="charsShown">Number of characters to currently display.</param>
    /// <returns>true if a dialogue is active, false otherwise.</returns>
    public bool TryGetDialogue([NotNullWhen(true)] out string? subject,
        [NotNullWhen(true)] out string? message, out int profileSpriteId, out int charsShown)
    {
        subject = null;
        message = null;
        profileSpriteId = 0;
        charsShown = 0;

        if (!queue.TryPeek(out var item)) return false;

        subject = item.Subject;
        message = item.Message;
        profileSpriteId = item.ProfileSpriteId;
        var timeElapsed = systemTimer.GetTime() - lastItemStartTime;
        charsShown = Math.Min((int)(timeElapsed / MessageRate), message.Length);
        return true;
    }

    /// <summary>
    ///     Enqueues a dialogue item.
    /// </summary>
    /// <param name="subject">Subject.</param>
    /// <param name="message">Message.</param>
    /// <param name="profileSpriteId">Profile sprite ID. Negative for no profile sprite.</param>
    public void Enqueue(string subject, string message, int profileSpriteId)
    {
        if (queue.IsEmpty) lastItemStartTime = systemTimer.GetTime();
        queue.Enqueue(new DialogueItem(subject, message, profileSpriteId));
    }

    /// <summary>
    ///     Advances the dialogue queue.
    /// </summary>
    public void Advance()
    {
        if (!TryGetDialogue(out _, out var message, out _, out var charsShown)) return;

        if (charsShown < message.Length)
        {
            // Display full message, but do not advance further.
            lastItemStartTime = 0UL;
            return;
        }

        // Otherwise move past the current item.
        queue.TryDequeue(out _);
        if (!queue.IsEmpty) lastItemStartTime = systemTimer.GetTime();
    }

    /// <summary>
    ///     Clears the dialogue queue.
    /// </summary>
    public void Clear()
    {
        queue.Clear();
    }

    /// <summary>
    ///     Immutable struct describing a single dialogue item.
    /// </summary>
    /// <param name="subject">Subject (e.g. who is talking).</param>
    /// <param name="message">Message.</param>
    private struct DialogueItem(string subject, string message, int profileSpriteId)
    {
        /// <summary>
        ///     Dialogue subject (e.g. who is talking).
        /// </summary>
        public string Subject { get; } = subject;

        /// <summary>
        ///     Dialogue message.
        /// </summary>
        public string Message { get; } = message;

        /// <summary>
        ///     Profile sprite ID. Negative means no profile sprite.
        /// </summary>
        public int ProfileSpriteId { get; } = profileSpriteId;
    }
}