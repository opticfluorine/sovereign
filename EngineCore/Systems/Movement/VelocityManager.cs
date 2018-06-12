/*
 * Engine8 Dynamic World MMORPG Engine
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

namespace Engine8.EngineCore.Systems.Movement
{

    /// <summary>
    /// Manages the velocities of all moving entities.
    /// </summary>
    public class VelocityManager
    {

        /// <summary>
        /// Internal state of an entity's velocity.
        /// </summary>
        private struct VelocityState
        {
            /// <summary>
            /// Unique identifier for each phase of motion.
            /// Used to filter out cancelled events.
            /// </summary>
            uint MovementPhase;

            /// <summary>
            /// Distance to move along x during each tick.
            /// </summary>
            float StepX;

            /// <summary>
            /// Distance to move along y during each tick.
            /// </summary>
            float StepY;
        }

        /// <summary>
        /// Active entity velocities that are generating movement events.
        /// </summary>
        private readonly Dictionary<ulong, VelocityState> activeVelocities
            = new Dictionary<ulong, VelocityState>();

        /// <summary>
        /// Used to assign unique movement phases.
        /// </summary>
        private uint PhaseCounter = 0;

        /// <summary>
        /// Sets the velocity of the given entity.
        /// </summary>
        /// <param name="entity">Entity ID.</param>
        /// <param name="dx">Relative velocity along x as a multiple of the entity base speed.</param>
        /// <param name="dy">Relative velocity along y as a multiple of the entity base speed.</param>
        public void SetVelocity(ulong entity, float dx, float dy)
        {

        }

        /// <summary>
        /// Stops movement of the given entity.
        /// </summary>
        /// <param name="entity">Entity ID.</param>
        public void StopMovement(ulong entity)
        {

        }

        /// <summary>
        /// Immediately moves the given entity.
        /// </summary>
        /// <param name="entity">Entity ID.</param>
        /// <param name="phase">Unique movement phase.</param>
        /// <param name="distanceX">Distance to move along x.</param>
        /// <param name="distanceY">Distance to move along y.</param>
        public void MoveOnce(ulong entity, uint phase, float distanceX, float distanceY)
        {

        }

    }

}
