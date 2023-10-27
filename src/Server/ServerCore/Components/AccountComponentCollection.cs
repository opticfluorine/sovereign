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
using Sovereign.EngineCore.Components;

namespace Sovereign.ServerCore.Components;

/// <summary>
///     Component that links an entity to a specific account. Normally used for player character entities.
/// </summary>
public class AccountComponentCollection : BaseComponentCollection<Guid>
{
    /// <summary>
    ///     Default size of collection.
    /// </summary>
    private const int InitialSize = 512;

    public AccountComponentCollection(ComponentManager componentManager) : base(
        componentManager, InitialSize, ComponentOperators.GuidOperators, ComponentType.Account)
    {
    }
}