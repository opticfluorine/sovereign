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
using Sovereign.EngineCore.Configuration;
using Sovereign.EngineCore.Timing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.EngineCore.Events
{

    /// <summary>
    /// 
    /// </summary>
    public class EventTimedAction : ITimedAction
    {

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        public ulong Interval => engineConfiguration.EventTickInterval;

        /// <summary>
        /// Event loop.
        /// </summary>
        private readonly IEventLoop eventLoop;

        /// <summary>
        /// Engine configuration.
        /// </summary>
        private readonly IEngineConfiguration engineConfiguration;

        public EventTimedAction(IEventLoop eventLoop, IEngineConfiguration engineConfiguration)
        {
            this.eventLoop = eventLoop;
            this.engineConfiguration = engineConfiguration;
        }

        public void Invoke(ulong triggerTime)
        {
            eventLoop.UpdateSystemTime(triggerTime);
        }

    }

}
