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
///     Public API for the WorldGeneration system, consumed by the chat command processor.
/// </summary>
public class WorldGenerationController
{
    private readonly WorldGenerationServices services;

    public WorldGenerationController(WorldGenerationServices services)
    {
        this.services = services;
    }

    /// <summary>
    ///     Gets a string describing the current world generation job status.
    /// </summary>
    /// <returns>Current job status string, e.g. "Idle".</returns>
    public string Status()
    {
        return services.JobStatus.ToString();
    }

    /// <summary>
    ///     Requests a new world generation plan.
    /// </summary>
    /// <param name="seed">World generation seed.</param>
    /// <param name="profileName">World generation profile name, or null for the default profile.</param>
    /// <param name="origin">Origin of the generated world region, or null for the default origin.</param>
    /// <exception cref="NotImplementedException">World generation planning is implemented in a
    /// later worldgen card.</exception>
    public void Plan(ulong seed, string? profileName, GridPosition? origin)
    {
        throw new NotImplementedException("worldgen plan is implemented in a later worldgen card");
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
    ///     Requests that any in-progress world generation job be aborted.
    /// </summary>
    /// <exception cref="NotImplementedException">World generation abort is implemented in a
    /// later worldgen card.</exception>
    public void Abort()
    {
        throw new NotImplementedException("worldgen abort is implemented in a later worldgen card");
    }
}
