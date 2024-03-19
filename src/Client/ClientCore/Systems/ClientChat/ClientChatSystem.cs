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
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Systems;

namespace Sovereign.ClientCore.Systems.ClientChat;

/// <summary>
///     System that manages client-side chat operations.
/// </summary>
public class ClientChatSystem : ISystem
{
    private readonly ChatHistoryManager chatHistoryManager;
    private readonly IEventLoop eventLoop;
    private readonly LoggingUtil loggingUtil;

    public ClientChatSystem(IEventLoop eventLoop, EventCommunicator eventCommunicator,
        ChatHistoryManager chatHistoryManager, LoggingUtil loggingUtil)
    {
        this.eventLoop = eventLoop;
        this.chatHistoryManager = chatHistoryManager;
        this.loggingUtil = loggingUtil;
        EventCommunicator = eventCommunicator;

        eventLoop.RegisterSystem(this);
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    public EventCommunicator EventCommunicator { get; }

    public ISet<EventId> EventIdsOfInterest { get; } = new HashSet<EventId>
    {
        EventId.Core_Chat_Local,
        EventId.Core_Chat_Global,
        EventId.Core_Chat_System
    };

    public int WorkloadEstimate => 20;

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
                case EventId.Core_Chat_Local:
                {
                    if (ev.EventDetails is not LocalChatEventDetails details)
                    {
                        Logger.Warn("Received local chat without details.");
                        break;
                    }

                    OnLocalChat(details);
                }
                    break;

                case EventId.Core_Chat_Global:
                {
                    if (ev.EventDetails is not GlobalChatEventDetails details)
                    {
                        Logger.Warn("Received global chat without details.");
                        break;
                    }

                    OnGlobalChat(details);
                }
                    break;

                case EventId.Core_Chat_System:
                {
                    if (ev.EventDetails is not SystemChatEventDetails details)
                    {
                        Logger.Warn("Received system chat without details.");
                        break;
                    }

                    OnSystemChat(details);
                }
                    break;
            }
        }

        return processed;
    }

    /// <summary>
    ///     Called when a local chat message is received.
    /// </summary>
    /// <param name="details">Details.</param>
    private void OnLocalChat(LocalChatEventDetails details)
    {
        Logger.InfoFormat("[Local] {0}: {1}", loggingUtil.FormatEntity(details.SenderEntityId), details.Message);
        chatHistoryManager.AddChat(ChatType.Local, details.Message, details.SenderEntityId);
    }

    /// <summary>
    ///     Called when a global chat message is received.
    /// </summary>
    /// <param name="details">Details.</param>
    private void OnGlobalChat(GlobalChatEventDetails details)
    {
        Logger.InfoFormat("[Global] {0}: {1}", details.SenderName, details.Message);
        chatHistoryManager.AddChat(ChatType.Global, details.Message, details.SenderName);
    }

    /// <summary>
    ///     Called when a system message is received.
    /// </summary>
    /// <param name="details">Details.</param>
    private void OnSystemChat(SystemChatEventDetails details)
    {
        Logger.InfoFormat("[System] {0}", details.Message);
        chatHistoryManager.AddChat(ChatType.System, details.Message);
    }
}