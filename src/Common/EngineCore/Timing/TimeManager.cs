/*
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

using System.Collections.Generic;

namespace Sovereign.EngineCore.Timing;

/// <summary>
///     Manages the system time and invokes timed actions on the
///     main thread as needed.
/// </summary>
public class TimeManager
{
    /// <summary>
    ///     Last tick number (floor(system time / interval)) of each action
    ///     in the timedActions list.
    /// </summary>
    private readonly ulong[] lastTickNumbers;

    /// <summary>
    ///     System timer.
    /// </summary>
    private readonly ISystemTimer systemTimer;

    /// <summary>
    ///     All registered ITimedActions.
    /// </summary>
    private readonly IList<ITimedAction> timedActions;

    public TimeManager(ISystemTimer systemTimer, IList<ITimedAction> timedActions)
    {
        /* Dependency injection. */
        this.systemTimer = systemTimer;
        this.timedActions = timedActions;

        /* Start tracking previous ticks. */
        lastTickNumbers = new ulong[timedActions.Count];
        for (var i = 0; i < timedActions.Count; ++i) lastTickNumbers[i] = 0;
    }

    /// <summary>
    ///     Advances the TimeManager to the latest system time, executing any timed actions
    ///     as needed. This should only be called from the main thread.
    /// </summary>
    public void AdvanceTime()
    {
        /* Check all of the actions. */
        for (var i = 0; i < timedActions.Count; ++i) CheckSingleAction(i);
    }

    /// <summary>
    ///     Checks a single timed action to see if it needs to be executed again.
    /// </summary>
    /// <param name="index">Action index.</param>
    private void CheckSingleAction(int index)
    {
        var action = timedActions[index];
        var lastTick = lastTickNumbers[index];
        var currentTime = systemTimer.GetTime();

        /* Have we advanced to the next tick? */
        var currentTick = currentTime / action.Interval;
        if (currentTick > lastTick)
        {
            /* Execute the action. */
            var tickTime = currentTick * action.Interval;
            action.Invoke(tickTime);

            /* Update the tick tracker. */
            lastTickNumbers[index] = currentTick;
        }
    }
}