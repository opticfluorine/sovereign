/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.EngineCore.Timing
{

    /// <summary>
    /// Manages the system time and invokes timed actions on the
    /// main thread as needed.
    /// </summary>
    public class TimeManager
    {

        /// <summary>
        /// System timer.
        /// </summary>
        private readonly ISystemTimer systemTimer;

        /// <summary>
        /// All registered ITimedActions.
        /// </summary>
        private readonly IList<ITimedAction> timedActions;

        /// <summary>
        /// Last tick number (floor(system time / interval)) of each action
        /// in the timedActions list.
        /// </summary>
        private readonly ulong[] lastTickNumbers;

        public TimeManager(ISystemTimer systemTimer, IList<ITimedAction> timedActions)
        {
            /* Dependency injection. */
            this.systemTimer = systemTimer;
            this.timedActions = timedActions;

            /* Start tracking previous ticks. */
            lastTickNumbers = new ulong[timedActions.Count];
            for (int i = 0; i < timedActions.Count; ++i)
            {
                lastTickNumbers[i] = 0;
            }
        }

        /// <summary>
        /// Advances the TimeManager to the latest system time, executing any timed actions
        /// as needed. This should only be called from the main thread.
        /// </summary>
        public void AdvanceTime()
        {
            /* Check all of the actions. */
            for (int i = 0; i < timedActions.Count; ++i)
            {
                CheckSingleAction(i);
            }
        }

        /// <summary>
        /// Checks a single timed action to see if it needs to be executed again.
        /// </summary>
        /// <param name="index">Action index.</param>
        private void CheckSingleAction(int index)
        {
            ITimedAction action = timedActions[index];
            ulong lastTick = lastTickNumbers[index];
            ulong currentTime = systemTimer.GetTime();

            /* Have we advanced to the next tick? */
            ulong currentTick = currentTime / action.Interval;
            if (currentTick > lastTick)
            {
                /* Execute the action. */
                ulong tickTime = currentTick * action.Interval;
                action.Invoke(tickTime);

                /* Update the tick tracker. */
                lastTickNumbers[index] = currentTick;
            }
        }

    }

}
