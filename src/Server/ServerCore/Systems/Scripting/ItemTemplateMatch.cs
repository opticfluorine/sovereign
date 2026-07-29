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

using Sovereign.EngineUtil.Attributes;

namespace Sovereign.ServerCore.Systems.Scripting;

/// <summary>
///     A single fuzzy item-template-name match, as returned by
/// <see cref="ItemsScripting.FindByFuzzyName" />.
/// </summary>
[Scriptable]
public struct ItemTemplateMatch
{
    /// <summary>
    ///     Required explicit parameterless constructor (structs with field
    ///     initializers must declare one explicitly).
    /// </summary>
    public ItemTemplateMatch()
    {
        Name = string.Empty;
    }

    /// <summary>
    ///     Entity ID of the matched item template.
    /// </summary>
    [ScriptableField]
    public ulong EntityId { get; set; }

    /// <summary>
    ///     Matched item template name.
    /// </summary>
    [ScriptableField]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Similarity score in [0, 1] (1.0 = exact match).
    /// </summary>
    [ScriptableField]
    public float Score { get; set; }
}
