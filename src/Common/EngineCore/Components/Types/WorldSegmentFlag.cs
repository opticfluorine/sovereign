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

using System;
using Sovereign.EngineUtil.Attributes;

namespace Sovereign.EngineCore.Components.Types;

/// <summary>
///     Bitwise flags that apply to a world segment.
/// </summary>
/// <remarks>
///     New segment-scoped features are added here; each flag's semantics are enforced at its
///     point of use (combat enforces Pvp, and so on).
/// </remarks>
[Flags]
[Scriptable]
[ScriptableEnum]
public enum WorldSegmentFlag
{
    /// <summary>
    ///     No flags.
    /// </summary>
    None = 0,

    /// <summary>
    ///     Player-initiated damage between players is allowed in the segment.
    /// </summary>
    Pvp = 0x1
}
