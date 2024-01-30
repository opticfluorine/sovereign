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
using Sovereign.EngineCore.Systems.Block.Components.Indexers;
using Sovereign.EngineCore.World;

namespace Sovereign.EngineCore.Systems.WorldManagement.Components.Indexers;

/// <summary>
///     Indexer that tracks the world segment that each positioned non-block entity
///     is located in. Note that only the positioned entities are tracked here,
///     not their children (if any).
/// </summary>
public class NonBlockWorldSegmentIndexer : BaseWorldSegmentIndexer
{
    public NonBlockWorldSegmentIndexer(PositionComponentCollection positions,
        NonBlockPositionEventFilter filter, WorldSegmentResolver resolver)
        : base(positions, filter, resolver)
    {
    }
}