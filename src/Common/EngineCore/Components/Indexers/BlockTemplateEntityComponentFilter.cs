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

using Sovereign.EngineCore.Systems.Block;

namespace Sovereign.EngineCore.Components.Indexers;

/// <summary>
///     Event filter that only accepts events from block template entities.
/// </summary>
/// <typeparam name="T">Component value type.</typeparam>
public class BlockTemplateEntityComponentFilter<T> : TemplateEntityComponentFilter<T> where T : notnull
{
    private readonly BlockServices blockServices;

    public BlockTemplateEntityComponentFilter(BaseComponentCollection<T> components,
        IComponentEventSource<T> eventSource,
        BlockServices blockServices)
        : base(components, eventSource)
    {
        this.blockServices = blockServices;
    }

    protected override bool ShouldAccept(ulong entityId)
    {
        return base.ShouldAccept(entityId) && blockServices.IsEntityBlock(entityId, true);
    }
}