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

using System.Collections.Generic;
using Sovereign.EngineCore.Components.Types;

namespace Sovereign.EngineCore.Events.Details;

/// <summary>
///     Event details for adding a batch of blocks at once.
/// </summary>
public sealed class BlockAddBatchEventDetails : IEventDetails
{
    /// <summary>
    ///     Blocks to be added.
    /// </summary>
    public List<BlockRecord> BlockRecords = new();

    /// <summary>
    ///     If true, indicates the blocks are being loaded rather than created.
    /// </summary>
    public bool IsLoad;

    /// <summary>
    ///     If true, indicates this is the data for a full world segment and the
    ///     corresponding load event should be published after processing.
    /// </summary>
    public bool IsWorldSegment;

    /// <summary>
    ///     World segment index for this data. Only set if IsWorldSegment is true.
    /// </summary>
    public GridPosition SegmentIndex = GridPosition.Zero;
}