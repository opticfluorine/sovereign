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

using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems;
using Sovereign.EngineCore.Systems.WorldManagement;
using System;
using System.Collections.Generic;

namespace Sovereign.ClientCore.Systems.TestContent
{

    /// <summary>
    /// System responsible for supplying content for early testing.
    /// </summary>
    /// <remarks>
    /// This will be removed in the future.
    /// </remarks>
    public sealed class TestContentSystem : ISystem, IDisposable
    {
        private readonly IEventLoop eventLoop;

        public EventCommunicator EventCommunicator { get; private set; }

        private readonly IEventSender eventSender;
        private readonly WorldManagementController worldManagementController;

        public ISet<EventId> EventIdsOfInterest => new HashSet<EventId>()
        {
            /* no events */
        };

        public int WorkloadEstimate => 0;

        public TestContentSystem(IEventLoop eventLoop,
            EventCommunicator eventCommunicator,
            IEventSender eventSender,
            WorldManagementController worldManagementController)
        {
            this.eventLoop = eventLoop;
            EventCommunicator = eventCommunicator;
            this.eventSender = eventSender;
            this.worldManagementController = worldManagementController;

            eventLoop.RegisterSystem(this);
        }

        public void Initialize()
        {
            /* Load some test world segments. */
            for (int i = -1; i < 2; ++i)
            {
                for (int j = -1; j < 2; ++j)
                {
                    worldManagementController.LoadSegment(eventSender,
                        new GridPosition(i, j, 0));
                }
            }
        }

        public void Cleanup()
        {

        }

        public int ExecuteOnce()
        {
            /* No action. */
            return 0;
        }

        public void Dispose()
        {
            eventLoop.UnregisterSystem(this);
        }

    }

}
