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

namespace Sovereign.ClientCore.Systems.ClientState;

/// <summary>
///     Manages state for integrated debugging tools.
/// </summary>
/// <remarks>
///     Since this is a non-stable debugging API with minimal coupling anyway, it is intended for direct access
///     from other systems rather than being exported through ClientStateServices/ClientStateController.
/// </remarks>
public sealed class DebugState
{
    /// <summary>
    ///     Whether to enable the Z layer cap for renderer debugging.
    /// </summary>
    public bool EnableZLayerCap { get; set; }

    /// <summary>
    ///     Z offset from player position to use for the Z layer cap. Only meaningful if EnableZLayerCap is true.
    /// </summary>
    public int ZLayerCapOffset { get; set; }

    /// <summary>
    ///     Whether to inhibit rendering of block front faces.
    /// </summary>
    public bool InhibitBlockFrontFaces { get; set; }

    /// <summary>
    ///     Whether to inhibit rendering of block top faces.
    /// </summary>
    public bool InhibitBlockTopFaces { get; set; }

    /// <summary>
    ///     Whether to inhibit rendering of non-block sprites.
    /// </summary>
    public bool InhibitNonBlocks { get; set; }

    /// <summary>
    ///     Whether to log tile sprite contexts when updating the tile sprite cache.
    /// </summary>
    public bool LogTileSpriteContexts { get; set; }
}