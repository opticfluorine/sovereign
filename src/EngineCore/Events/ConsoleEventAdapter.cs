/*
 * Sovereign Engine
 * Copyright (c) 2020 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
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

        public ConsoleEventAdapter(EventAdapterManager adapterManager)
        {
            Console.CancelKeyPress += Console_CancelKeyPress;

            adapterManager.RegisterEventAdapter(this);
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
