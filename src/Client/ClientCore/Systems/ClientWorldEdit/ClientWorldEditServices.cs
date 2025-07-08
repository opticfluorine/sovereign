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

namespace Sovereign.ClientCore.Systems.ClientWorldEdit;

/// <summary>
///     Public read API for the client-side world editor system.
/// </summary>
public class ClientWorldEditServices
{
    private readonly ClientWorldEditState state;

    public ClientWorldEditServices(ClientWorldEditState state)
    {
        this.state = state;
    }

    /// <summary>
    ///     Block template entity ID to use for world editing.
    /// </summary>
    public ulong BlockTemplateId => state.BlockTemplateId;

    /// <summary>
    ///     Z offset relative to camera to use for world editing.
    /// </summary>
    public int ZOffset => state.ZOffset;

    /// <summary>
    ///     Pen width in blocks to use for world editing.
    /// </summary>
    public int PenWidth => state.PenWidth;

    /// <summary>
    ///     Tool to use for world editing.
    /// </summary>
    public WorldEditTool WorldEditTool => state.WorldEditTool;

    /// <summary>
    ///     Whether world editing is snapped to grid.
    /// </summary>
    public bool SnapToGrid => state.SnapToGrid;

    /// <summary>
    ///     NPC template entity ID to use for world editing.
    /// </summary>
    public ulong NpcTemplateId => state.NpcTemplateId;
}