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

using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Entities;
using Sovereign.Persistence.Entities;

namespace Sovereign.Persistence.State.Trackers;

/// <summary>
///     State tracker for Physics tag.
/// </summary>
public class PhysicsStateTracker : BaseStateTracker<bool>
{
    public PhysicsStateTracker(PhysicsTagCollection physics, ExistingEntitySet existingEntitySet,
        StateManager stateManager,
        EntityTable entityTable)
        : base(physics, false, existingEntitySet, stateManager, entityTable)
    {
    }

    protected override void OnStateUpdate(ref StateUpdate<bool> update)
    {
        stateManager.FrontBuffer.UpdatePhysics(ref update);
    }
}