/*
 * Sovereign Engine
 * Copyright (c) 2020 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using Sovereign.EngineCore.Systems.Movement.Components;
using Sovereign.Persistence.Entities;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Sovereign.Persistence.State.Trackers
{

    /// <summary>
    /// State tracker for the Position component.
    /// </summary>
    public sealed class PositionStateTracker : BaseStateTracker<Vector3>
    {

        public PositionStateTracker(PositionComponentCollection positions,
            EntityMapper entityMapper, StateManager stateManager)
            : base(positions, Vector3.Zero, entityMapper, stateManager)
        {

        }

        protected override void OnStateUpdate(ref StateUpdate<Vector3> update)
        {
            stateManager.FrontBuffer.UpdatePosition(ref update);
        }

    }
}
