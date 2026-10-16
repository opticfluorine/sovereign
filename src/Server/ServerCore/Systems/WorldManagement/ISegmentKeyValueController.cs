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
using Sovereign.EngineCore.Systems.Data;

namespace Sovereign.ServerCore.Systems.WorldManagement;

/// <summary>
///     Read and write access to the world segment key-value cache.
/// </summary>
/// <remarks>
///     This is the only write door for world segment key-value data; Lua and admin commands both
///     go through it. Writes update the cache immediately (so a following read observes the write)
///     and enqueue a delta for the single persistence writer.
/// </remarks>
public interface ISegmentKeyValueController
{
    /// <summary>
    ///     Checks whether the given world segment's key-value data is loaded into the cache.
    /// </summary>
    /// <param name="segment">World segment index.</param>
    /// <returns>true if the segment is loaded, false otherwise.</returns>
    bool IsSegmentLoaded(GridPosition segment);

    /// <summary>
    ///     Gets the value stored for the given key of the given world segment.
    /// </summary>
    /// <param name="segment">World segment index.</param>
    /// <param name="key">Key.</param>
    /// <param name="value">Stored value. Only valid if the method returns true.</param>
    /// <returns>true if the value exists, false otherwise.</returns>
    bool TryGetValue(GridPosition segment, string key, out DataValue value);

    /// <summary>
    ///     Stores a key-value pair for the given world segment.
    /// </summary>
    /// <param name="segment">World segment index.</param>
    /// <param name="key">Key.</param>
    /// <param name="value">Value to store.</param>
    /// <returns>true if the value was stored, false if it was rejected.</returns>
    bool SetValue(GridPosition segment, string key, DataValue value);

    /// <summary>
    ///     Removes the value stored for the given key of the given world segment.
    /// </summary>
    /// <param name="segment">World segment index.</param>
    /// <param name="key">Key.</param>
    /// <returns>true if the key was removed, false if it was rejected.</returns>
    bool RemoveKey(GridPosition segment, string key);
}
