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

using System.Collections.Generic;
using MessagePack;

namespace Sovereign.EngineCore.Entities;

/// <summary>
///     Serializable data type that conveys the full set of template entities.
/// </summary>
[MessagePackObject]
public class TemplateEntityData
{
    /// <summary>
    ///     List of entity definiitions for all template entities.
    /// </summary>
    [Key(0)]
    public List<EntityDefinition> TemplateEntityDefinitions { get; set; } = new();
}