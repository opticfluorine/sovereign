// Sovereign Engine
// Copyright (c) 2026 opticfluorine
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

using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Entities;

namespace Sovereign.ClientCore.Components;

/// <summary>
///     Collection for the active animation phase override component.
/// </summary>
public class ActiveAnimationPhaseComponentCollection : BaseComponentCollection<ActiveAnimationPhase>
{
    private const int InitialSize = 8192;

    /// <summary>
    ///     Creates a new active animation phase component collection.
    /// </summary>
    /// <param name="entityTable">Entity table.</param>
    /// <param name="componentManager">Component manager.</param>
    public ActiveAnimationPhaseComponentCollection(EntityTable entityTable, ComponentManager componentManager)
        : base(entityTable, componentManager, InitialSize, ClientComponentOperators.ActiveAnimationPhaseOperators,
            ComponentType.ActiveAnimationPhase)
    {
    }
}
