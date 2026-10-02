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

using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using EventId = Sovereign.EngineCore.Events.EventId;

namespace Sovereign.ServerCore.Systems.WorldGeneration;

/// <summary>
///     Public API for sending requests to the WorldGeneration system, consumed by the chat
///     command processor. Requests are delivered asynchronously as events.
/// </summary>
public class WorldGenerationController
{
    /// <summary>
    ///     Requests a new world generation plan. Replies are sent to the requesting entity.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="seed">World generation seed.</param>
    /// <param name="profileName">World generation profile name, or null for the default profile.</param>
    /// <param name="origin">Origin of the generated world region, or null for the default origin.</param>
    /// <param name="senderEntityId">Entity to reply to.</param>
    public void Plan(IEventSender eventSender, ulong seed, string? profileName, GridPosition? origin, 
        ulong senderEntityId)
    {
        var details = new WorldGenPlanEventDetails
        {
            Seed = seed,
            ProfileName = profileName,
            Origin = origin,
            SenderEntityId = senderEntityId
        };
        eventSender.SendEvent(new Event(EventId.Server_WorldGen_Plan, details));
    }

    /// <summary>
    ///     Requests that the staged world generation plan be committed to the world in
    ///     batched, idempotent transactions.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="seed">Seed to confirm, or null to confirm the staged seed.</param>
    /// <param name="force">Whether to proceed despite subscribed players.</param>
    /// <param name="senderEntityId">Entity to reply to.</param>
    public void Commit(IEventSender eventSender, ulong? seed, bool force, ulong senderEntityId)
    {
        var details = new WorldGenCommitEventDetails
        {
            Seed = seed,
            Force = force,
            SenderEntityId = senderEntityId
        };
        eventSender.SendEvent(new Event(EventId.Server_WorldGen_Commit, details));
    }

    /// <summary>
    ///     Requests that the staged world generation plan be committed, replacing a
    ///     registered world whose footprint it fully contains.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="stagedSeed">Seed of the staged plan.</param>
    /// <param name="oldSeed">Seed of the registered world to replace.</param>
    /// <param name="force">Whether to proceed despite subscribed players.</param>
    /// <param name="senderEntityId">Entity to reply to.</param>
    public void Replace(IEventSender eventSender, ulong stagedSeed, ulong oldSeed, bool force, ulong senderEntityId)
    {
        var details = new WorldGenReplaceEventDetails
        {
            StagedSeed = stagedSeed,
            OldSeed = oldSeed,
            Force = force,
            SenderEntityId = senderEntityId
        };
        eventSender.SendEvent(new Event(EventId.Server_WorldGen_Replace, details));
    }

    /// <summary>
    ///     Requests that the in-progress world generation job be aborted. Aborting is
    ///     cooperative: the running pipeline or commit writer observes the cancellation at
    ///     stage boundaries or per batch and unwinds to Idle.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    public void Abort(IEventSender eventSender)
    {
        eventSender.SendEvent(new Event(EventId.Server_WorldGen_Abort));
    }
}
