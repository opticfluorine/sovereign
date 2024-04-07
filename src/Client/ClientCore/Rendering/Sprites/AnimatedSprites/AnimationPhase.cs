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

namespace Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;

/// <summary>
///     Animation phases.
/// </summary>
public enum AnimationPhase
{
    /// <summary>
    ///     Animation phase when the entity is stationary and not performing actions.
    ///     Also a fallback for any other phases which are not specified.
    /// </summary>
    Default,

    /// <summary>
    ///     Animation phase when the entity is moving.
    /// </summary>
    Moving
}