// Sovereign Engine
// Copyright (c) 2025 opticfluorine
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using EventId = Sovereign.EngineCore.Events.EventId;

namespace Sovereign.EngineCore.Systems.Data;

/// <summary>
///     System responsible for managing non-component game data.
/// </summary>
internal class DataSystem : ISystem
{
    private readonly GlobalKeyValueStore globalStore;
    private readonly ILogger<DataSystem> logger;

    public DataSystem(EventCommunicator eventCommunicator, IEventLoop eventLoop,
        ILogger<DataSystem> logger, GlobalKeyValueStore globalStore)
    {
        this.logger = logger;
        this.globalStore = globalStore;
        EventCommunicator = eventCommunicator;
        eventLoop.RegisterSystem(this);
    }

    public EventCommunicator EventCommunicator { get; }

    public ISet<EventId> EventIdsOfInterest { get; } = new HashSet<EventId>
    {
        EventId.Core_Data_SetGlobal,
        EventId.Core_Data_RemoveGlobal
    };

    public int WorkloadEstimate => 40;

    public void Initialize()
    {
    }

    public void Cleanup()
    {
    }

    public int ExecuteOnce()
    {
        var processed = 0;

        while (EventCommunicator.GetIncomingEvent(out var ev))
        {
            processed++;

            switch (ev.EventId)
            {
                case EventId.Core_Data_SetGlobal:
                {
                    if (ev.EventDetails is not KeyValueEventDetails details)
                    {
                        logger.LogError("Bad event details for SetGlobal.");
                        break;
                    }

                    globalStore.SetGlobal(details.Key, details.Value);
                    break;
                }

                case EventId.Core_Data_RemoveGlobal:
                {
                    if (ev.EventDetails is not StringEventDetails details)
                    {
                        logger.LogError("Bad event details for RemoveGlobal.");
                        break;
                    }

                    globalStore.RemoveGlobal(details.Value);
                    break;
                }

                default:
                    logger.LogError("Unhandled event in DataSystem: {EventId}", ev.EventId);
                    break;
            }
        }

        return processed;
    }
}