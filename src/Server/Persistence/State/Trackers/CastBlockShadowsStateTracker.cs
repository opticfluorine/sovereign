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

using Sovereign.EngineCore.Components;
using Sovereign.Persistence.Entities;

namespace Sovereign.Persistence.State.Trackers;

/// <summary>
///     Persistence state tracker for the CastBlockShadows tag.
/// </summary>
public class CastBlockShadowsStateTracker : BaseStateTracker<bool>
{
    public CastBlockShadowsStateTracker(CastBlockShadowsTagCollection castBlockShadows,
        EntityMapper entityMapper, StateManager stateManager)
        : base(castBlockShadows, false, entityMapper, stateManager)
    {
    }

    protected override void OnStateUpdate(ref StateUpdate<bool> update)
    {
        stateManager.FrontBuffer.UpdateCastBlockShadows(ref update);
    }
}