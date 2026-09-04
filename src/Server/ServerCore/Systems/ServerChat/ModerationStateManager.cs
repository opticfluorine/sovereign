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
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Timing;
using Sovereign.EngineUtil.Collections;

namespace Sovereign.ServerCore.Systems.ServerChat;

/// <summary>
///     Tracks timed chat mutes for the ServerChat system.
/// </summary>
/// <remarks>
///     All methods must be invoked from the ServerChat system executor thread.
/// </remarks>
public class ModerationStateManager
{
    /// <summary>
    ///     Number of TimeSpan ticks per microsecond of system time.
    /// </summary>
    private const long TicksPerMicrosecond = TimeSpan.TicksPerMillisecond / 1000;

    private readonly Dictionary<ulong, ulong> allChatMuteExpiries = new();
    private readonly BinaryHeap<MuteExpiry> expiryHeap =
        new(BinaryHeap<MuteExpiry>.DEFAULT_SIZE, Comparer<MuteExpiry>.Create(CompareMuteExpiries));
    private readonly ServerChatInternalController internalController;
    private readonly Dictionary<ulong, ulong> globalMuteExpiries = new();
    private readonly ISystemTimer timer;

    public ModerationStateManager(ServerChatInternalController internalController, ISystemTimer timer)
    {
        this.internalController = internalController;
        this.timer = timer;
    }

    /// <summary>
    ///     Mutes the given player from the given chat scope for the given duration.
    ///     Muting an already-muted player supersedes the previous expiry.
    /// </summary>
    /// <param name="playerEntityId">Player entity ID.</param>
    /// <param name="scope">Chat scope to mute.</param>
    /// <param name="duration">Mute duration.</param>
    public void Mute(ulong playerEntityId, ChatMuteScope scope, TimeSpan duration)
    {
        var expiry = timer.GetTime() + (ulong)(duration.Ticks / TicksPerMicrosecond);
        GetExpiries(scope)[playerEntityId] = expiry;
        expiryHeap.Push(new MuteExpiry(expiry, playerEntityId, scope));
        internalController.SendMuteAdded(playerEntityId, scope, expiry);
    }

    /// <summary>
    ///     Removes the mute for the given player and chat scope if present.
    /// </summary>
    /// <param name="playerEntityId">Player entity ID.</param>
    /// <param name="scope">Chat scope to unmute.</param>
    /// <returns>true if an active mute was removed, false if the player was not muted.</returns>
    public bool Unmute(ulong playerEntityId, ChatMuteScope scope)
    {
        if (!GetExpiries(scope).Remove(playerEntityId)) return false;
        internalController.SendMuteRemoved(playerEntityId, scope, 0);
        return true;
    }

    /// <summary>
    ///     Checks whether the given player is currently muted from the given chat scope.
    /// </summary>
    /// <param name="playerEntityId">Player entity ID.</param>
    /// <param name="scope">Chat scope to check.</param>
    /// <returns>true if the player is muted, false otherwise.</returns>
    public bool IsMuted(ulong playerEntityId, ChatMuteScope scope)
    {
        return GetExpiries(scope).ContainsKey(playerEntityId);
    }

    /// <summary>
    ///     Gets a snapshot of all active mutes.
    /// </summary>
    /// <returns>Snapshot of active mutes.</returns>
    public IReadOnlyList<ActiveMute> GetActiveMutes()
    {
        var mutes = new List<ActiveMute>(globalMuteExpiries.Count + allChatMuteExpiries.Count);
        foreach (var kvp in globalMuteExpiries)
            mutes.Add(new ActiveMute(kvp.Key, ChatMuteScope.Global, kvp.Value));
        foreach (var kvp in allChatMuteExpiries)
            mutes.Add(new ActiveMute(kvp.Key, ChatMuteScope.All, kvp.Value));
        return mutes;
    }

    /// <summary>
    ///     Removes any mutes that have expired and sends the corresponding events.
    /// </summary>
    public void PurgeExpired()
    {
        var now = timer.GetTime();
        while (expiryHeap.Count > 0 && expiryHeap.Peek().ExpiryTime <= now)
        {
            var entry = expiryHeap.Pop();
            var expiries = GetExpiries(entry.Scope);
            if (expiries.TryGetValue(entry.EntityId, out var expiry) && expiry == entry.ExpiryTime)
            {
                expiries.Remove(entry.EntityId);
                internalController.SendMuteRemoved(entry.EntityId, entry.Scope, entry.ExpiryTime);
            }
        }
    }

    /// <summary>
    ///     Compares two mute expiry entries by expiry time, breaking ties by entity ID and scope.
    /// </summary>
    /// <param name="a">First entry.</param>
    /// <param name="b">Second entry.</param>
    /// <returns>Negative if a precedes b, positive if a follows b, zero if equal.</returns>
    private static int CompareMuteExpiries(MuteExpiry a, MuteExpiry b)
    {
        var cmp = a.ExpiryTime.CompareTo(b.ExpiryTime);
        if (cmp != 0) return cmp;
        cmp = a.EntityId.CompareTo(b.EntityId);
        return cmp != 0 ? cmp : a.Scope.CompareTo(b.Scope);
    }

    /// <summary>
    ///     Gets the mute expiry map for the given chat scope.
    /// </summary>
    /// <param name="scope">Chat scope.</param>
    /// <returns>Mute expiry map.</returns>
    private Dictionary<ulong, ulong> GetExpiries(ChatMuteScope scope)
    {
        return scope == ChatMuteScope.All ? allChatMuteExpiries : globalMuteExpiries;
    }

    /// <summary>
    ///     A single active mute.
    /// </summary>
    /// <param name="EntityId">Player entity ID.</param>
    /// <param name="Scope">Mute scope.</param>
    /// <param name="ExpiryTime">Absolute system time in microseconds at which the mute expires.</param>
    public readonly record struct ActiveMute(ulong EntityId, ChatMuteScope Scope, ulong ExpiryTime);

    /// <summary>
    ///     Expiry heap entry for a mute.
    /// </summary>
    /// <param name="ExpiryTime">Absolute system time in microseconds at which the mute expires.</param>
    /// <param name="EntityId">Player entity ID.</param>
    /// <param name="Scope">Mute scope.</param>
    private readonly record struct MuteExpiry(ulong ExpiryTime, ulong EntityId, ChatMuteScope Scope);
}
