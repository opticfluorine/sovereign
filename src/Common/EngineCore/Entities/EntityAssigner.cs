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

namespace Sovereign.EngineCore.Entities;

/// <summary>
///     Responsible for assigning entity IDs within a block.
/// </summary>
/// This class is not thread-safe. Consider using one or more unique
/// assigners for each ISystem.
public class EntityAssigner
{
    /// <summary>
    ///     First block ID (upper 32 bits) for persisted entity IDs.
    /// </summary>
    public const ulong PersistedBlock = 0x7FFF0000;

    /// <summary>
    ///     First volatile entity ID.
    /// </summary>
    public const ulong FirstVolatileId = 0;

    /// <summary>
    ///     Block ID shifted to the upper dword
    /// </summary>
    private readonly ulong shiftBlock;

    /// <summary>
    ///     Counter for the local segment of the id.
    /// </summary>
    private ulong localIdCounter;

    /// <summary>
    ///     Creates an assigner for the given block.
    /// </summary>
    /// <param name="block">Block.</param>
    public EntityAssigner(ulong block)
    {
        Block = block;
        shiftBlock = block << 32;
    }

    /// <summary>
    ///     Entity block assigned to this assigner.
    /// </summary>
    public ulong Block { get; private set; }

    /// <summary>
    ///     Gets the next ID from this assigner.
    /// </summary>
    /// <returns>Next ID.</returns>
    public ulong GetNextId()
    {
        return shiftBlock | localIdCounter++;
    }
}