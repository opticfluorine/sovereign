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
using Sovereign.EngineCore.Components.Types;

namespace Sovereign.ServerCore.Systems.WorldGeneration;

/// <summary>
///     Public API for sending requests to the WorldGeneration system, consumed by the chat
///     command processor.
/// </summary>
public class WorldGenerationController
{
    private readonly WorldGenPlanJobRunner planRunner;

    public WorldGenerationController(WorldGenPlanJobRunner planRunner)
    {
        this.planRunner = planRunner;
    }

    /// <summary>
    ///     Requests a new world generation plan. The job slot must be Idle; profile loading,
    ///     validation, and job startup happen synchronously, and the plan itself is computed on
    ///     a background task. Replies are sent to the requesting entity.
    /// </summary>
    /// <param name="seed">World generation seed.</param>
    /// <param name="profileName">World generation profile name, or null for the default profile.</param>
    /// <param name="origin">Origin of the generated world region, or null for the default origin.</param>
    /// <param name="senderEntityId">Entity to reply to.</param>
    public void Plan(ulong seed, string? profileName, GridPosition? origin, ulong senderEntityId)
    {
        planRunner.BeginPlan(seed, profileName, origin, senderEntityId);
    }

    /// <summary>
    ///     Requests that the pending world generation plan be committed to the world.
    /// </summary>
    /// <param name="seed">Seed to confirm, or null to confirm the planned seed.</param>
    /// <exception cref="NotImplementedException">World generation commit is implemented in a
    /// later worldgen card.</exception>
    public void Commit(ulong? seed)
    {
        throw new NotImplementedException("worldgen commit is implemented in a later worldgen card");
    }

    /// <summary>
    ///     Requests that any in-progress world generation job be aborted. Aborting a running
    ///     pipeline is not yet implemented; the request is always rejected.
    /// </summary>
    /// <exception cref="NotImplementedException">World generation abort is implemented in a
    /// later worldgen card.</exception>
    public void Abort()
    {
        throw new NotImplementedException("abort is not yet implemented");
    }
}
