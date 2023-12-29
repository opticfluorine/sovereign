/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
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

using System;
using Castle.Core.Logging;

namespace Sovereign.EngineCore.Events;

/// <summary>
///     Event adapter that produces quit events in response to SIGINT.
/// </summary>
public sealed class ConsoleEventAdapter : IEventAdapter
{
    /// <summary>
    ///     Whether the server has been requested to stop from the console.
    /// </summary>
    private bool stopRequested;

    public ConsoleEventAdapter(EventAdapterManager adapterManager)
    {
        Console.CancelKeyPress += Console_CancelKeyPress;

        adapterManager.RegisterEventAdapter(this);
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    public void PrepareEvents()
    {
    }

    public bool PollEvent(out Event? ev)
    {
        if (stopRequested)
        {
            Logger.Info("Received SIGINT, shutting down.");

            Console.CancelKeyPress -= Console_CancelKeyPress;
            stopRequested = false;

            ev = new Event(EventId.Core_Quit);
            return true;
        }

        ev = null;
        return false;
    }

    /// <summary>
    ///     Called when Ctrl+C is pressed at the console.
    /// </summary>
    /// <param name="sender">Not used.</param>
    /// <param name="e">Event.</param>
    private void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {
        e.Cancel = true;
        stopRequested = true;
    }
}