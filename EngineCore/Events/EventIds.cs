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

namespace Sovereign.EngineCore.Events
{

    /// <summary>
    /// Enumerates the event ID numbers.
    /// </summary>
    public enum EventId
    {

        #region Core

        /// <summary>
        /// Unknown event type.
        /// 
        /// Associated details: None
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Quit event ID.
        /// 
        /// Associated details: None
        /// </summary>
        Core_Quit = 1,

        /// <summary>
        /// Attempted one-time relative movement of an entity.
        /// This is a private event of the MovementSystem.
        /// 
        /// Associated details: MoveOnceEventDetails
        /// </summary>
        Core_Move_Once = 100,

        /// <summary>
        /// Sets the velocity of an entity.
        /// 
        /// Associated details: SetMovementEventDetails
        /// </summary>
        Core_Set_Velocity = 101,

        /// <summary>
        /// Ends the continous movement, if any, of an entity.
        /// 
        /// Associate details: EntityDetails
        /// </summary>
        Core_End_Movement = 102,

        #endregion Core

        #region Client

        #region Client_Input

        /// <summary>
        /// Event sent when a key is pressed.
        /// 
        /// Associated details: KeyEventDetails
        /// </summary>
        Client_Input_KeyDown = 10000,

        /// <summary>
        /// Event sent when a key is released.
        /// 
        /// Associated details: KeyEventDetails
        /// </summary>
        Client_Input_KeyUp = 10001,

        #endregion

        #endregion Client

    }

}
