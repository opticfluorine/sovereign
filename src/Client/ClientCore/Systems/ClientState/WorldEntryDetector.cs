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

using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Timing;
using Sovereign.EngineUtil.Numerics;

namespace Sovereign.ClientCore.Systems.ClientState;

/// <summary>
///     Detects the state change associated with player world entry after player selection.
/// </summary>
/// <remarks>
///     The state change detector works by looking for the completion of the world segment loads
///     that occur immediately following player selection. The detector waits for a minimum number
///     of world segment subscribe events to be received before checking for the number of successful
///     loads to match; this helps to avoid rare false positives where a very fast initial load of a single
///     world segment causes an early detection.
/// </remarks>
public class WorldEntryDetector
{
    /// <summary>
    ///     Minimum subscribe count before declaring a detection.
    /// </summary>
    private const int MinSubscribeCount = 27;

    /// <summary>
    ///     Amount of time to delay after detection before triggering transition.
    ///     The delay is used to provide a "smoother" transition with less graphical artifacts
    ///     caused by late loading (e.g. the tiling engine warming up).
    /// </summary>
    private const ulong TransitionDelay = (ulong)(1.0f * UnitConversions.SToUs);

    private readonly ILogger<WorldEntryDetector> logger;

    private readonly ClientStateMachine stateMachine;
    private readonly ISystemTimer systemTimer;
    private bool isEntryScheduled;

    private bool isPlayerSelected;
    private int loadCount;
    private ulong scheduledTransitionTime;
    private int subscribeCount;

    public WorldEntryDetector(ClientStateMachine stateMachine, ISystemTimer systemTimer,
        ILogger<WorldEntryDetector> logger)
    {
        this.stateMachine = stateMachine;
        this.systemTimer = systemTimer;
        this.logger = logger;
    }

    /// <summary>
    ///     Called when a new login is made to the server. This resets the detector.
    /// </summary>
    public void OnLogin()
    {
        // Reset detector state.
        isPlayerSelected = false;
        subscribeCount = 0;
        loadCount = 0;
        isEntryScheduled = false;
        scheduledTransitionTime = 0;
    }

    /// <summary>
    ///     Called when the player character has been selected.
    /// </summary>
    public void OnPlayerSelected()
    {
        isPlayerSelected = true;
        CheckForDetection();
    }

    /// <summary>
    ///     Called for each tick during the main menu state.
    /// </summary>
    public void OnTick()
    {
        if (!isEntryScheduled || systemTimer.GetTime() < scheduledTransitionTime) return;

        isEntryScheduled = false;
        scheduledTransitionTime = 0;
        if (!stateMachine.TryTransition(MainClientState.InGame))
            logger.LogError("Unexpectedly failed to transition to in-game state.");
    }

    /// <summary>
    ///     Called when a world segment has been subscribed.
    /// </summary>
    public void OnSegmentSubscribe()
    {
        subscribeCount++;
        CheckForDetection();
    }

    /// <summary>
    ///     Called when a world segment has been loaded.
    /// </summary>
    public void OnSegmentLoaded()
    {
        loadCount++;
        CheckForDetection();
    }

    /// <summary>
    ///     Checks whether world entry has been detected, driving a state change if so.
    /// </summary>
    private void CheckForDetection()
    {
        if (stateMachine.State != MainClientState.MainMenu) return;
        if (!isPlayerSelected) return;
        if (subscribeCount < MinSubscribeCount) return;
        if (subscribeCount != loadCount) return;

        // Detection.
        logger.LogInformation("Sufficient world data loaded, transitioning to in-game.");
        scheduledTransitionTime = systemTimer.GetTime() + TransitionDelay;
        isEntryScheduled = true;
    }
}