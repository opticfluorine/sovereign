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

using Engine8.EngineCore.Timing;
using Xunit;
using System.Threading;

namespace Engine8.ClientCore.Timing
{

    /// <summary>
    /// Unit tests for the SFMLSystemTimer class.
    /// </summary>
    public class TestSFMLSystemTimer
    {

        /// <summary>
        /// Tests that the GetTime() method increases with successive calls.
        /// </summary>
        [Fact]
        public void TestGetTime()
        {
            /* Create a system timer. */
            ISystemTimer systemTimer = new SDLSystemTimer();

            /* Sample the system time in microseconds. */
            ulong t0 = systemTimer.GetTime();

            /* Sleep to ensure that at least 1000 us pass. */
            Thread.Sleep(1);

            /* Sample the system time again. */
            ulong t1 = systemTimer.GetTime();

            /* Assert that at least 1000 us have passed. */
            Assert.True(t1 - t0 > 1000);
        }

    }
}
