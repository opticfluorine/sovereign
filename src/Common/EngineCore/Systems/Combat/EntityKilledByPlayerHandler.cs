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

using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Logging;

namespace Sovereign.EngineCore.Systems.Combat;

/// <summary>
///     Handles kill credit when a player kills another entity.
/// </summary>
public class EntityKilledByPlayerHandler(ILogger<EntityKilledByPlayerHandler> logger, LoggingUtil loggingUtil)
{
    /// <summary>
    ///     Credits the kill of a slain entity to a player.
    /// </summary>
    /// <param name="victimEntityId">Entity ID of the slain entity.</param>
    /// <param name="killerPlayerEntityId">Entity ID of the player credited with the kill.</param>
    public void HandleEntityKilledByPlayer(ulong victimEntityId, ulong killerPlayerEntityId)
    {
        logger.LogInformation("{Victim} was killed by {Killer}.",
            loggingUtil.FormatEntity(victimEntityId), loggingUtil.FormatEntity(killerPlayerEntityId));
    }
}
