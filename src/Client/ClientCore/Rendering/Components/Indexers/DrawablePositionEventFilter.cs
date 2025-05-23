﻿/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
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

using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Components.Types;

namespace Sovereign.ClientCore.Rendering.Components.Indexers;

/// <summary>
///     Event source that filters position events to exclude non-drawable entities.
/// </summary>
public sealed class DrawablePositionEventFilter : BaseComponentEventFilter<Kinematics>
{
    private readonly DrawableTagCollection drawableCollection;

    public DrawablePositionEventFilter(KinematicsComponentCollection kinematicsCollection,
        DrawableTagCollection drawableCollection)
        : base(kinematicsCollection, kinematicsCollection)
    {
        this.drawableCollection = drawableCollection;
    }

    protected override bool ShouldAccept(ulong entityId)
    {
        return drawableCollection.HasTagForEntity(entityId, true)
               || drawableCollection.HasPendingTagForEntity(entityId);
    }
}