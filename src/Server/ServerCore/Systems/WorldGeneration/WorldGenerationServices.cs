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

namespace Sovereign.ServerCore.Systems.WorldGeneration;

/// <summary>
///     Provides a public API for accessing world generation job state.
/// </summary>
public class WorldGenerationServices
{
    private readonly WorldGenerationSystem system;

    public WorldGenerationServices(WorldGenerationSystem system)
    {
        this.system = system;
    }

    /// <summary>
    ///     Gets the status of the single world generation job slot.
    /// </summary>
    public WorldGenerationJobStatus JobStatus => system.JobStatus;

    /// <summary>
    ///     Gets the last human-readable status message reported for the job slot.
    /// </summary>
    public string? LastStatusMessage => system.LastStatusMessage;
}
