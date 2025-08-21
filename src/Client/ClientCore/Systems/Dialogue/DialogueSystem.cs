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
using Sovereign.EngineCore.Systems;
using EventId = Sovereign.EngineCore.Events.EventId;

namespace Sovereign.ClientCore.Systems.Dialogue;

/// <summary>
///     System responsible for handling in-game dialogue.
/// </summary>
internal class DialogueSystem : ISystem
{
    private readonly DialogueQueue dialogueQueue;
    private readonly ILogger<DialogueSystem> logger;

    public DialogueSystem(EventCommunicator eventCommunicator, IEventLoop mainLoop, ILogger<DialogueSystem> logger,
        DialogueQueue dialogueQueue)
    {
        this.logger = logger;
        this.dialogueQueue = dialogueQueue;
        EventCommunicator = eventCommunicator;

        mainLoop.RegisterSystem(this);
    }

    public EventCommunicator EventCommunicator { get; }

    public ISet<EventId> EventIdsOfInterest { get; } = new HashSet<EventId>
    {
        EventId.Client_Dialogue_Enqueue,
        EventId.Client_Dialogue_Advance,
        EventId.Core_Network_Logout
    };

    public int WorkloadEstimate => 5;

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
                case EventId.Client_Dialogue_Enqueue:
                {
                    if (ev.EventDetails is not DialogueEventDetails details)
                    {
                        logger.LogError("Received Enqueue event without details.");
                        break;
                    }

                    dialogueQueue.Enqueue(details.Subject, details.Message, details.ProfileSpriteId);
                    break;
                }

                case EventId.Client_Dialogue_Advance:
                    dialogueQueue.Advance();
                    break;

                case EventId.Core_Network_Logout:
                    dialogueQueue.Clear();
                    break;
            }
        }

        return eventCount;
    }
}