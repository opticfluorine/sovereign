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

using System;
using Microsoft.Extensions.Logging;
using Sovereign.ClientCore.Systems.Input;

namespace Sovereign.ClientCore.Systems.ClientWorldEdit;

/// <summary>
///     Handles user inputs for the world editor.
/// </summary>
public class ClientWorldEditInputHandler
{
    /// <summary>
    ///     Mouse button for drawing.
    /// </summary>
    private const MouseButton DrawButton = MouseButton.Left;

    /// <summary>
    ///     Mouse button for erasing.
    /// </summary>
    private const MouseButton EraseButton = MouseButton.Right;

    private readonly IWorldEditToolHandler blockToolHandler;

    private readonly InputServices inputServices;

    private readonly IWorldEditToolHandler itemToolHandler;
    private readonly ILogger<ClientWorldEditInputHandler> logger;

    private readonly IWorldEditToolHandler npcToolHandler;
    private readonly ClientWorldEditState userState;

    /// <summary>
    ///     Current internal editor state.
    /// </summary>
    private EditState currentState = EditState.Idle;


    public ClientWorldEditInputHandler(
        InputServices inputServices,
        ILogger<ClientWorldEditInputHandler> logger,
        ClientWorldEditState userState,
        BlockToolHandler blockToolHandler,
        NpcToolHandler npcToolHandler,
        ItemToolHandler itemToolHandler)
    {
        this.inputServices = inputServices;
        this.logger = logger;
        this.userState = userState;
        this.blockToolHandler = blockToolHandler;
        this.npcToolHandler = npcToolHandler;
        this.itemToolHandler = itemToolHandler;
    }

    /// <summary>
    ///     Active tool handler.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the world editor is in an invalid state.</exception>
    private IWorldEditToolHandler ToolHandler => userState.WorldEditTool switch
    {
        WorldEditTool.Block => blockToolHandler,
        WorldEditTool.Npc => npcToolHandler,
        WorldEditTool.Item => itemToolHandler,
        _ => throw new ArgumentOutOfRangeException(nameof(userState.WorldEditTool), "Invalid world edit tool selected.")
    };

    /// <summary>
    ///     Handles input processing once per tick.
    /// </summary>
    public void OnTick()
    {
        UpdateState();
        switch (currentState)
        {
            case EditState.Draw:
                ToolHandler.ProcessDraw();
                break;

            case EditState.Erase:
                ToolHandler.ProcessErase();
                break;
        }
    }

    /// <summary>
    ///     Updates the internal editor state based on the latest mouse button inputs.
    /// </summary>
    private void UpdateState()
    {
        switch (currentState)
        {
            case EditState.Idle:
                if (inputServices.IsMouseButtonDown(DrawButton))
                {
                    logger.LogDebug("Enter Draw state.");
                    currentState = EditState.Draw;
                }
                else if (inputServices.IsMouseButtonDown(EraseButton))
                {
                    logger.LogDebug("Enter Erase state.");
                    currentState = EditState.Erase;
                }

                break;

            case EditState.Draw:
                if (inputServices.IsMouseButtonDown(DrawButton))
                {
                    logger.LogDebug("Enter Draw state.");
                    currentState = EditState.Draw;
                }

                break;

            case EditState.Erase:
                if (!inputServices.IsMouseButtonDown(EraseButton)) ResetToIdle();
                break;
        }
    }

    /// <summary>
    ///     Resets the internal editor state to idle.
    /// </summary>
    private void ResetToIdle()
    {
        currentState = EditState.Idle;
        ToolHandler.Reset();
    }

    /// <summary>
    ///     Possible editor states.
    /// </summary>
    private enum EditState
    {
        /// <summary>
        ///     Editor is idle.
        /// </summary>
        Idle,

        /// <summary>
        ///     Editor is drawing where the mouse is dragged.
        /// </summary>
        Draw,

        /// <summary>
        ///     Editor is erasing where the mouse is dragged.
        /// </summary>
        Erase
    }
}