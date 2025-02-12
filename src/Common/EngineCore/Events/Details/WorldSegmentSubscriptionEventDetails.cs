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

using MessagePack;
using Sovereign.EngineCore.Components.Types;

namespace Sovereign.EngineCore.Events.Details;

[MessagePackObject]
public class WorldSegmentSubscriptionEventDetails : IEventDetails
{
    /// <summary>
    ///     Entity ID of the subscribing player.
    /// </summary>
    [IgnoreMember] public ulong EntityId;

    /// <summary>
    ///     Segment index of the subscribed world segment.
    /// </summary>
    [Key(0)] public GridPosition SegmentIndex;
}