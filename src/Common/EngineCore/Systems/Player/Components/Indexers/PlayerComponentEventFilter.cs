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

using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Indexers;

namespace Sovereign.EngineCore.Systems.Player.Components.Indexers;

/// <summary>
///     Base component filter type that restricts events to those generated from player
///     entities. Note that this only passes events from the player entity itself, not
///     from any child entity.
/// </summary>
/// <typeparam name="T">Component value type.</typeparam>
public class PlayerComponentEventFilter<T> : BaseComponentEventFilter<T>
{
    private readonly PlayerCharacterTagCollection playerTags;

    public PlayerComponentEventFilter(PlayerCharacterTagCollection playerTags,
        BaseComponentCollection<T> components, IComponentEventSource<T> eventSource)
        : base(components, eventSource)
    {
        this.playerTags = playerTags;
    }

    protected override bool ShouldAccept(ulong entityId)
    {
        return playerTags.HasTagForEntity(entityId);
    }
}