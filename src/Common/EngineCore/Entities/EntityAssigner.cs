/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Threading;

namespace Sovereign.EngineCore.Entities;

/// <summary>
///     Responsible for assigning entity IDs to new entities.
/// </summary>
public sealed class EntityAssigner
{
    private ulong nextEntityId;

    /// <summary>
    ///     Next free entity ID.
    /// </summary>
    public ulong NextEntityId
    {
        get => nextEntityId;
        set => Interlocked.Exchange(ref nextEntityId, value);
    }

    /// <summary>
    ///     Gets the next ID from this assigner.
    /// </summary>
    /// <returns>Next ID.</returns>
    public ulong GetNextId()
    {
        return Interlocked.Increment(ref nextEntityId);
    }
}