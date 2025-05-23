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
using System.Numerics;
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Systems;
using Sovereign.EngineUtil.Types;
using EventId = Sovereign.EngineCore.Events.EventId;

namespace Sovereign.ClientCore.Systems.ClientChat;

/// <summary>
///     System that manages client-side chat operations.
/// </summary>
public class ClientChatSystem : ISystem
{
    private readonly ChatHistoryManager chatHistoryManager;
    private readonly IEventLoop eventLoop;
    private readonly ILogger<ClientChatSystem> logger;
    private readonly LoggingUtil loggingUtil;

    public ClientChatSystem(IEventLoop eventLoop, EventCommunicator eventCommunicator,
        ChatHistoryManager chatHistoryManager, LoggingUtil loggingUtil, ILogger<ClientChatSystem> logger)
    {
        this.eventLoop = eventLoop;
        this.chatHistoryManager = chatHistoryManager;
        this.loggingUtil = loggingUtil;
        this.logger = logger;
        EventCommunicator = eventCommunicator;

        eventLoop.RegisterSystem(this);
    }

    public EventCommunicator EventCommunicator { get; }

    public ISet<EventId> EventIdsOfInterest { get; } = new HashSet<EventId>
    {
        EventId.Core_Chat_Local,
        EventId.Core_Chat_Global,
        EventId.Core_Chat_System,
        EventId.Core_Chat_Generic,
        EventId.Client_Network_ConnectionLost,
        EventId.Core_Network_Logout,
        EventId.Client_Network_AboutToSelectPlayer
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
                        logger.LogWarning("Received local chat without details.");
                        break;
                    }

                    OnLocalChat(details);
                }
                    break;

                case EventId.Core_Chat_Global:
                {
                    if (ev.EventDetails is not GlobalChatEventDetails details)
                    {
                        logger.LogWarning("Received global chat without details.");
                        break;
                    }

                    OnGlobalChat(details);
                }
                    break;

                case EventId.Core_Chat_System:
                {
                    if (ev.EventDetails is not SystemChatEventDetails details)
                    {
                        logger.LogWarning("Received system chat without details.");
                        break;
                    }

                    OnSystemChat(details);
                }
                    break;

                case EventId.Core_Chat_Generic:
                {
                    if (ev.EventDetails is not GenericChatEventDetails details)
                    {
                        logger.LogWarning("Received generic chat without details.");
                        break;
                    }

                    OnGenericChat(details);
                }
                    break;

                case EventId.Client_Network_ConnectionLost:
                case EventId.Core_Network_Logout:
                case EventId.Client_Network_AboutToSelectPlayer:
                    chatHistoryManager.Clear();
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
        logger.LogInformation("[Local] {Player}: {Message}", loggingUtil.FormatEntity(details.SenderEntityId),
            details.Message);
        chatHistoryManager.AddChat(ChatType.Local, details.Message, details.SenderEntityId);
    }

    /// <summary>
    ///     Called when a global chat message is received.
    /// </summary>
    /// <param name="details">Details.</param>
    private void OnGlobalChat(GlobalChatEventDetails details)
    {
        logger.LogInformation("[Global] {PlayerName}: {Message}", details.SenderName, details.Message);
        chatHistoryManager.AddChat(ChatType.Global, details.Message, details.SenderName);
    }

    /// <summary>
    ///     Called when a system message is received.
    /// </summary>
    /// <param name="details">Details.</param>
    private void OnSystemChat(SystemChatEventDetails details)
    {
        logger.LogInformation("[System] {Message}", details.Message);
        chatHistoryManager.AddChat(ChatType.System, details.Message);
    }

    /// <summary>
    ///     Called when a generic message is received.
    /// </summary>
    /// <param name="details">Details.</param>
    private void OnGenericChat(GenericChatEventDetails details)
    {
        logger.LogInformation("[Message] {Message}", details.Message);
        chatHistoryManager.AddChat(details.Message, new Vector4(ColorUtil.UnpackColorRgb(details.Color), 1.0f));
    }
}