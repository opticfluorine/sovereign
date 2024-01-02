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

namespace Sovereign.NetworkCore.Network.Rest.Data;

/// <summary>
///     Describes basic information for a single player character.
/// </summary>
public class PlayerInfo
{
    /// <summary>
    ///     Player character entity ID.
    /// </summary>
    public ulong Id { get; set; }

    /// <summary>
    ///     Player character name.
    /// </summary>
    public string? Name { get; set; }
}