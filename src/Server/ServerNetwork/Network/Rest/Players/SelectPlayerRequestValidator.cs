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
using Sovereign.NetworkCore.Network.Rest.Data;
using Sovereign.Persistence.Players;

namespace Sovereign.ServerNetwork.Network.Rest.Players;

/// <summary>
///     Provides validation for select player requests.
/// </summary>
public class SelectPlayerRequestValidator
{
    private readonly PersistencePlayerServices playerServices;

    public SelectPlayerRequestValidator(PersistencePlayerServices playerServices)
    {
        this.playerServices = playerServices;
    }

    /// <summary>
    ///     Checks the validity of a select player request.
    /// </summary>
    /// <param name="request">Request.</param>
    /// <param name="accountId">Account ID.</param>
    /// <returns>true if valid, false otherwise.</returns>
    public bool IsValid(SelectPlayerRequest request, Guid accountId)
    {
        // Check whether the entity ID corresponds to a player belonging to the account.
        return playerServices.ValidatePlayerAccountPair(request.PlayerEntityId, accountId);
    }
}