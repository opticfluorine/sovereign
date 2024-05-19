// Sovereign Engine
// Copyright (c) 2024 opticfluorine
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
using Castle.Core.Logging;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems;

namespace Sovereign.ServerCore.Systems.TemplateEntity;

/// <summary>
///     System responsible for managing server-side template entities.
/// </summary>
public class TemplateEntitySystem : ISystem
{
    private readonly TemplateEntityDataGenerator dataGenerator;

    public TemplateEntitySystem(IEventLoop eventLoop, EventCommunicator eventCommunicator,
        TemplateEntityDataGenerator dataGenerator)
    {
        this.dataGenerator = dataGenerator;
        EventCommunicator = eventCommunicator;
        eventLoop.RegisterSystem(this);
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    public EventCommunicator EventCommunicator { get; }

    public ISet<EventId> EventIdsOfInterest { get; } = new HashSet<EventId>
    {
        EventId.Server_TemplateEntity_CreateNew,
        EventId.Server_TemplateEntity_Update
    };

    public int WorkloadEstimate => 10;

    public void Initialize()
    {
    }

    public void Cleanup()
    {
    }

    public int ExecuteOnce()
    {
        var eventCount = 0;
        while (EventCommunicator.GetIncomingEvent(out var ev))
        {
            eventCount++;
            switch (ev.EventId)
            {
                case EventId.Server_TemplateEntity_CreateNew:
                    break;

                case EventId.Server_TemplateEntity_Update:
                    break;

                default:
                    Logger.Error("Unhandled event in TemplateEntitySystem.");
                    break;
            }

            // Processing any event will invalidate the current template entity data for synchronization.
            dataGenerator.OnTemplatesChanged();
        }

        return eventCount;
    }
}