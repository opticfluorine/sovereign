// Sovereign Engine
// Copyright (c) 2026 opticfluorine
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

namespace Sovereign.EngineCore.Events.Details;

/// <summary>
///     Details for invoking the interaction callback of a target entity with a tool entity.
///     Server-internal; never crosses the network.
/// </summary>
public sealed class ScriptingInteractEventDetails : IEventDetails
{
    /// <summary>
    ///     Item entity used as the tool.
    /// </summary>
    public ulong ToolEntityId { get; set; }

    /// <summary>
    ///     Entity on which the tool is used.
    /// </summary>
    public ulong TargetEntityId { get; set; }
}
