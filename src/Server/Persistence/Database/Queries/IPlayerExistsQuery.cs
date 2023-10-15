// Sovereign Engine
// Copyright (c) 2023 opticfluorine
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

namespace Sovereign.Persistence.Database.Queries;

/// <summary>
///     Query that checks for the existence of a player character with the given name.
/// </summary>
public interface IPlayerExistsQuery
{
    /// <summary>
    ///     Determines whether a player character with the given name exists in the database.
    /// </summary>
    /// <param name="name">Player character name.</param>
    /// <returns>true if the player exists, false otherwise.</returns>
    bool PlayerExists(string name);
}