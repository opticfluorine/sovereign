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

namespace Sovereign.EngineCore.Components.Indexers;

/// <summary>
///     Filters name-related events to include only those for player character entities.
/// </summary>
public class PlayerNameEventFilter : PlayerComponentEventFilter<string>
{
    public PlayerNameEventFilter(PlayerCharacterTagCollection players, NameComponentCollection names)
        : base(players, names, names)
    {
    }
}