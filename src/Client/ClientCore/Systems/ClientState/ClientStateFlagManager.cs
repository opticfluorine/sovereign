// Sovereign Engine
// Copyright (c) 2024 opticfluorine
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

using System.Collections.Concurrent;

namespace Sovereign.ClientCore.Systems.ClientState;

/// <summary>
///     Manages the set of client state flags and their values.
/// </summary>
public class ClientStateFlagManager
{
    private readonly ConcurrentDictionary<ClientStateFlag, bool> flags = new();

    /// <summary>
    ///     Gets the current value of a state flag, initializing the flag to false if it hasn't already been set.
    /// </summary>
    /// <param name="flag">Flag.</param>
    /// <returns>Flag value.</returns>
    public bool GetStateFlagValue(ClientStateFlag flag)
    {
        flags.TryAdd(flag, false);
        return flags[flag];
    }

    /// <summary>
    ///     Sets the value of a state flag.
    /// </summary>
    /// <param name="flag">Flag.</param>
    /// <param name="value">Flag value.</param>
    public void SetStateFlagValue(ClientStateFlag flag, bool value)
    {
        flags[flag] = value;
    }

    /// <summary>
    ///     Resets all flags.
    /// </summary>
    public void ResetFlags()
    {
        foreach (var flag in flags.Keys) flags[flag] = false;
    }
}