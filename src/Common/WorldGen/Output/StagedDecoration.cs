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

using MessagePack;

namespace Sovereign.WorldGen.Output;

/// <summary>
///     One staged decoration row of a world generation plan. Positions are absolute
///     world coordinates; the plan origin is already applied.
/// </summary>
[MessagePackObject]
public sealed class StagedDecoration
{
    /// <summary>
    ///     Template entity ID of the decoration.
    /// </summary>
    [Key(0)]
    public ulong TemplateEntityId;

    /// <summary>
    ///     Absolute world X coordinate of the decoration.
    /// </summary>
    [Key(1)]
    public float X;

    /// <summary>
    ///     Absolute world Y coordinate of the decoration.
    /// </summary>
    [Key(2)]
    public float Y;

    /// <summary>
    ///     Absolute world Z coordinate of the decoration: the cell the decoration stands in.
    /// </summary>
    [Key(3)]
    public float Z;
}
