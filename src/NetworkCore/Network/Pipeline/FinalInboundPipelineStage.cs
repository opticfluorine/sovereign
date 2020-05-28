﻿/*
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

using System;
using System.Collections.Generic;
using System.Text;
using Sovereign.EngineCore.Events;
using Sovereign.NetworkCore.Network.Infrastructure;
using Sovereign.NetworkCore.Network.Service;

namespace Sovereign.NetworkCore.Network.Pipeline
{

    /// <summary>
    /// Final stage of the inbound network pipeline.
    /// </summary>
    public sealed class FinalInboundPipelineStage : IInboundPipelineStage
    {
        private readonly ReceivedEventQueue queue;

        public int Priority => int.MaxValue;

        public IInboundPipelineStage NextStage { get; set; }

        public FinalInboundPipelineStage(ReceivedEventQueue queue)
        {
            this.queue = queue;
        }

        public void ProcessEvent(Event ev, NetworkConnection connection)
        {
            queue.ReceivedEvents.Enqueue(ev);
        }
    }

}
