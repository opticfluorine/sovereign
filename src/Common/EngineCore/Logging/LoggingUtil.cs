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

namespace Sovereign.EngineCore.Logging;

public class LoggingUtil
{
    private readonly NameComponentCollection names;

    public LoggingUtil(NameComponentCollection names)
    {
        this.names = names;
    }

    /// <summary>
    ///     Formats an entity using its name or ID depending on what information is available.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <returns>Formatted string.</returns>
    public string FormatEntity(ulong entityId)
    {
        return names.HasComponentForEntity(entityId) ? names[entityId] : $"Entity ID {entityId}";
    }
}