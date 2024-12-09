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

using System;
using Sovereign.EngineUtil.Attributes;

namespace Sovereign.EngineCore.Events.Details;

/// <summary>
///     Event details for player selection events.
/// </summary>
[Scriptable]
public class SelectPlayerEventDetails : IEventDetails
{
    /// <summary>
    ///     Account ID.
    /// </summary>
    public Guid AccountId { get; set; }

    /// <summary>
    ///     Player character entity ID.
    /// </summary>
    [ScriptableOrder(1)]
    public ulong PlayerCharacterEntityId { get; set; }

    /// <summary>
    ///     Flag indicating whether the player character is new (true = new).
    /// </summary>
    [ScriptableOrder(2)]
    public bool NewPlayer { get; set; }
}