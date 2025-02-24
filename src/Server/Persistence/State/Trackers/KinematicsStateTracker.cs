﻿/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Numerics;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;
using Sovereign.Persistence.Entities;

namespace Sovereign.Persistence.State.Trackers;

/// <summary>
///     State tracker for the Position component.
/// </summary>
public sealed class KinematicsStateTracker : BaseStateTracker<Kinematics>
{
    public KinematicsStateTracker(KinematicsComponentCollection kinematics,
        EntityMapper entityMapper, StateManager stateManager)
        : base(kinematics, new Kinematics { Position = Vector3.Zero, Velocity = Vector3.Zero }, entityMapper,
            stateManager)
    {
    }

    protected override void OnStateUpdate(ref StateUpdate<Kinematics> update)
    {
        stateManager.FrontBuffer.UpdatePosition(ref update);
    }
}