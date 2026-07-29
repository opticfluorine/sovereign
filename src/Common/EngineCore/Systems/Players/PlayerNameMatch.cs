// Sovereign Engine
// Copyright (c) 2025 opticfluorine
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

using Sovereign.EngineUtil.Attributes;

namespace Sovereign.EngineCore.Systems.Players;

/// <summary>
///     A single fuzzy player-name match, as returned by
///     <see cref="PlayersScripting.FindByFuzzyName" />.
/// </summary>
[Scriptable]
public struct PlayerNameMatch
{
    /// <summary>
    ///     Required explicit parameterless constructor (structs with field
    ///     initializers must declare one explicitly).
    /// </summary>
    public PlayerNameMatch()
    {
        Name = string.Empty;
    }

    /// <summary>
    ///     Entity ID of the matched online player.
    /// </summary>
    [ScriptableField]
    public ulong EntityId { get; set; }

    /// <summary>
    ///     Matched player name.
    /// </summary>
    [ScriptableField]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Similarity score in [0, 1] (1.0 = exact match).
    /// </summary>
    [ScriptableField]
    public float Score { get; set; }
}