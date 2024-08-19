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

using Castle.Core.Logging;
using Sovereign.ClientCore.Systems.Camera;
using Sovereign.ClientCore.Systems.Input;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Events;

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

    private readonly CameraServices cameraServices;
    private readonly IEventSender eventSender;
    private readonly InputServices inputServices;
    private readonly ClientWorldEditInternalController internalController;
    private readonly ClientWorldEditState userState;

    /// <summary>
    ///     Current internal editor state.
    /// </summary>
    private EditState currentState = EditState.Idle;

    /// <summary>
    ///     Flag indicating whether the first block has been impacted by the current edit operation.
    /// </summary>
    private bool editStarted;

    /// <summary>
    ///     Block position last impacted by the current edit operation.
    /// </summary>
    private GridPosition lastPosition;

    public ClientWorldEditInputHandler(InputServices inputServices, ClientWorldEditState userState,
        CameraServices cameraServices, ClientWorldEditInternalController internalController, IEventSender eventSender)
    {
        this.inputServices = inputServices;
        this.userState = userState;
        this.cameraServices = cameraServices;
        this.internalController = internalController;
        this.eventSender = eventSender;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    /// <summary>
    ///     Handles input processing once per tick.
    /// </summary>
    public void OnTick()
    {
        UpdateState();
        switch (currentState)
        {
            case EditState.Draw:
                ProcessDraw();
                break;

            case EditState.Erase:
                ProcessErase();
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
                    Logger.Debug("Enter Draw state.");
                    currentState = EditState.Draw;
                }
                else if (inputServices.IsMouseButtonDown(EraseButton))
                {
                    Logger.Debug("Enter Erase state.");
                    currentState = EditState.Erase;
                }

                break;

            case EditState.Draw:
                if (!inputServices.IsMouseButtonDown(DrawButton)) Reset();
                break;

            case EditState.Erase:
                if (!inputServices.IsMouseButtonDown(EraseButton)) Reset();
                break;
        }
    }

    /// <summary>
    ///     Resets the internal state of the editor.
    /// </summary>
    private void Reset()
    {
        Logger.Debug("Entering Idle state.");
        currentState = EditState.Idle;
        editStarted = false;
    }

    /// <summary>
    ///     Processes latest inputs while in the Draw state.
    /// </summary>
    private void ProcessDraw()
    {
        var hoveredPos = GetHoveredBlockWithOffset();
        if (!editStarted || !lastPosition.Equals(hoveredPos))
        {
            editStarted = true;
            lastPosition = hoveredPos;
            internalController.SetBlock(eventSender, hoveredPos, userState.BlockTemplateId);
            Logger.DebugFormat("Draw block at {0}.", hoveredPos);
        }
    }

    /// <summary>
    ///     Processes latest inputs while in the Erase state.
    /// </summary>
    private void ProcessErase()
    {
        var hoveredPos = GetHoveredBlockWithOffset();
        if (!editStarted || !lastPosition.Equals(hoveredPos))
        {
            editStarted = true;
            lastPosition = hoveredPos;
            internalController.RemoveBlock(eventSender, hoveredPos);
        }
    }

    /// <summary>
    ///     Gets the block coordinate currently hovered by the mouse, taking Z offset into account.
    /// </summary>
    /// <returns>Hovered block coordinate.</returns>
    private GridPosition GetHoveredBlockWithOffset()
    {
        var hoverPos = cameraServices.GetMousePositionWorldCoordinates();
        var posWithOffset = hoverPos with { Y = hoverPos.Y - userState.ZOffset, Z = hoverPos.Z + userState.ZOffset };
        return (GridPosition)posWithOffset;
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