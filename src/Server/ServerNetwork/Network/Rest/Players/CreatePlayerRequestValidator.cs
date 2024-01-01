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

using System.Diagnostics.CodeAnalysis;
using Sovereign.EngineCore.Components.Validators;
using Sovereign.NetworkCore.Network.Rest.Data;

namespace Sovereign.ServerNetwork.Network.Rest.Players;

/// <summary>
///     Provides validation for create player requests.
/// </summary>
public class CreatePlayerRequestValidator
{
    private readonly NameComponentValidator nameValidator;

    public CreatePlayerRequestValidator(NameComponentValidator nameValidator)
    {
        this.nameValidator = nameValidator;
    }

    /// <summary>
    ///     Checks the validity of a create player request.
    /// </summary>
    /// <param name="request">Request.</param>
    /// <returns>true if valid, false otherwise.</returns>
    public bool IsValid([NotNullWhen(true)] CreatePlayerRequest? request)
    {
        return request != null && nameValidator.IsValid(request.PlayerName);
    }
}