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

namespace Engine8.EngineCore.Systems
{

    /// <summary>
    /// Interface implemented by all Systems in the ECS framework.
    /// </summary>
    public interface ISystem
    {

        /// <summary>
        /// Called to initialize the system.
        /// </summary>
        /// <param name="manager">System manager that owns the system.</param>
        void InitializeSystem(SystemManager manager);

        /// <summary>
        /// Called to clean up the system.
        /// </summary>
        void CleanupSystem();

        /// <summary>
        /// Called from the main loop to update the system.
        /// An update is a non-rendering step.
        /// </summary>
        /// <param name="systemTime">System time in microseconds.</param>
        void DoUpdate(ulong systemTime);

        /// <summary>
        /// Called from the main loop to perform rendering logic.
        /// </summary>
        /// <param name="timeSinceUpdate">Time (us) since the last state update.</param>
        void DoRender(ulong timeSinceUpdate);

    }

}
