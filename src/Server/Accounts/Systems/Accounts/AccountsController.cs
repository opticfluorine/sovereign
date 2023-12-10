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
using Sovereign.EngineCore.Events;
using Sovereign.ServerCore.Events;

namespace Sovereign.Accounts.Systems.Accounts;

/// <summary>
///     Controller API for the Accounts system.
/// </summary>
public class AccountsController
{
    /// <summary>
    ///     Selects a player character for use by an account that is logging in.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="accountId">Account ID.</param>
    /// <param name="playerCharacterEntityId">Player character entity ID.</param>
    /// <param name="newPlayer">Whether the selected player is a new player character.</param>
    public void SelectPlayer(IEventSender eventSender, Guid accountId, ulong playerCharacterEntityId, bool newPlayer)
    {
        var details = new SelectPlayerEventDetails
        {
            AccountId = accountId,
            PlayerCharacterEntityId = playerCharacterEntityId,
            NewPlayer = newPlayer
        };
        var ev = new Event(EventId.Server_Accounts_SelectPlayer, details);
        eventSender.SendEvent(ev);
    }
}