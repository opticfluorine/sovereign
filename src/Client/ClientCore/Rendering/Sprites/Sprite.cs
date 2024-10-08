﻿/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Text.Json.Serialization;

namespace Sovereign.ClientCore.Rendering.Sprites;

/// <summary>
///     Points to a single sprite.
/// </summary>
public sealed class Sprite
{
    /// <summary>
    ///     Sprite ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///     Spritesheet filename.
    /// </summary>
    public string SpritesheetName { get; set; } = "";

    /// <summary>
    ///     Row containing the sprite.
    /// </summary>
    public int Row { get; set; }

    /// <summary>
    ///     Column containing the sprite.
    /// </summary>
    public int Column { get; set; }

    /// <summary>
    ///     Flag indicating that the sprite is opaque (i.e. has no transparency).
    /// </summary>
    [JsonIgnore]
    public bool Opaque { get; set; }
}