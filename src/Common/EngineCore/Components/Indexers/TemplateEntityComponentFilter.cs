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

using Sovereign.EngineCore.Entities;

namespace Sovereign.EngineCore.Components.Indexers;

/// <summary>
///     Component filter that selects for template entities.
/// </summary>
/// <typeparam name="T"></typeparam>
public class TemplateEntityComponentFilter<T> : BaseComponentEventFilter<T> where T : notnull
{
    public TemplateEntityComponentFilter(BaseComponentCollection<T> components, IComponentEventSource<T> eventSource)
        : base(components, eventSource)
    {
    }

    protected override bool ShouldAccept(ulong entityId)
    {
        return entityId is >= EntityConstants.FirstTemplateEntityId and <= EntityConstants.LastTemplateEntityId;
    }
}