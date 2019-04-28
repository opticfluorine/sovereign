/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
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

using Castle.Core.Logging;
using System;

namespace Sovereign.EngineCore.Events
{

    /// <summary>
    /// Event adapter that produces quit events in response to SIGINT.
    /// </summary>
    public sealed class ConsoleEventAdapter : IEventAdapter
    {
        
        public ILogger Logger { private get; set; } = NullLogger.Instance;

        /// <summary>
        /// Whether the server has been requested to stop from the console.
        /// </summary>
        private bool StopRequested = false;

        public ConsoleEventAdapter()
        {
            Console.CancelKeyPress += Console_CancelKeyPress;
        }

        public void PrepareEvents()
        {
        }

        public bool PollEvent(out Event ev)
        {
            if (StopRequested)
            {
                Logger.Info("Received SIGINT, shutting down.");

                Console.CancelKeyPress -= Console_CancelKeyPress;
                StopRequested = false;

                ev = new Event(EventId.Core_Quit);
                return true;
            }

            ev = null;
            return false;
        }
        
        /// <summary>
        /// Called when Ctrl+C is pressed at the console.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Event.</param>
        private void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            StopRequested = true;
        }

    }

}
