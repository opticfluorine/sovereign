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

using Castle.Core.Logging;
using Sovereign.EngineCore.Configuration;
using Sovereign.EngineCore.Timing;

namespace Sovereign.EngineCore.Events;

/// <summary>
/// </summary>
public class EventTimedAction : ITimedAction
{
    /// <summary>
    ///     Engine configuration.
    /// </summary>
    private readonly IEngineConfiguration engineConfiguration;

    /// <summary>
    ///     Event loop.
    /// </summary>
    private readonly IEventLoop eventLoop;

    public EventTimedAction(IEventLoop eventLoop, IEngineConfiguration engineConfiguration)
    {
        this.eventLoop = eventLoop;
        this.engineConfiguration = engineConfiguration;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    public ulong Interval => engineConfiguration.EventTickInterval;

    public void Invoke(ulong triggerTime)
    {
        eventLoop.UpdateSystemTime(triggerTime);
    }
}