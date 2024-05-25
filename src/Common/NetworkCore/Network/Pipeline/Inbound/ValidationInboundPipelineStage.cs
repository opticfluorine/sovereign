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
using Sovereign.EngineCore.Events.Details.Validators;
using Sovereign.NetworkCore.Network.Infrastructure;

namespace Sovereign.NetworkCore.Network.Pipeline.Inbound;

/// <summary>
///     Inbound pipeline stage that validates received events.
/// </summary>
public class ValidationInboundPipelineStage : IInboundPipelineStage
{
    private readonly TemplateEntityDefinitionEventDetailsValidator templateValidator;
    private readonly Dictionary<EventId, IEventDetailsValidator> validators;

    public ValidationInboundPipelineStage(NullEventDetailsValidator nullValidator,
        EntityDefinitionEventDetailsValidator entityDefinitionValidator,
        WorldSegmentSubscriptionEventDetailsValidator worldSegmentSubscriptionValidator,
        MoveEventDetailsValidator moveValidator, RequestMoveEventDetailsValidator requestMoveValidator,
        EntityGridPositionEventDetailsValidator entityGridPositionValidator,
        EntityDesyncEventDetailsValidator entityDesyncValidator,
        EntityEventDetailsValidator entityValidator,
        ChatEventDetailsValidator chatValidator,
        LocalChatEventDetailsValidator localChatValidator,
        GlobalChatEventDetailsValidator globalChatValidator,
        SystemChatEventDetailsValidator systemChatValidator,
        TemplateEntityDefinitionEventDetailsValidator templateValidator)
    {
        this.templateValidator = templateValidator;
        validators = new Dictionary<EventId, IEventDetailsValidator>
        {
            { EventId.Core_Ping_Ping, nullValidator },
            { EventId.Core_Ping_Pong, nullValidator },
            { EventId.Core_WorldManagement_Subscribe, worldSegmentSubscriptionValidator },
            { EventId.Core_WorldManagement_Unsubscribe, worldSegmentSubscriptionValidator },
            { EventId.Client_EntitySynchronization_Sync, entityDefinitionValidator },
            { EventId.Client_EntitySynchronization_Desync, entityDesyncValidator },
            { EventId.Client_EntitySynchronization_SyncTemplate, templateValidator },
            { EventId.Core_Movement_Move, moveValidator },
            { EventId.Core_Movement_RequestMove, requestMoveValidator },
            { EventId.Core_WorldManagement_EntityLeaveWorldSegment, entityGridPositionValidator },
            { EventId.Core_Network_Logout, entityValidator },
            { EventId.Core_Chat_Send, chatValidator },
            { EventId.Core_Chat_Local, localChatValidator },
            { EventId.Core_Chat_Global, globalChatValidator },
            { EventId.Core_Chat_System, systemChatValidator },
            { EventId.Server_TemplateEntity_Update, entityDefinitionValidator }
        };
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    public int Priority => int.MinValue + 1;
    public IInboundPipelineStage? NextStage { get; set; }

    public void ProcessEvent(Event ev, NetworkConnection connection)
    {
        if (validators.TryGetValue(ev.EventId, out var validator))
        {
            if (validator.IsValid(ev.EventDetails))
                NextStage?.ProcessEvent(ev, connection);
            else
                Logger.ErrorFormat("Received invalid details for event ID {0} from connection ID {1}.",
                    ev.EventId, connection.Id);
        }
        else
        {
            Logger.ErrorFormat("No validator found for event ID {0}, rejecting event.", ev.EventId);
        }
    }
}