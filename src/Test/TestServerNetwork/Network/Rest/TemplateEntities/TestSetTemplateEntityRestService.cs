// Sovereign Engine
// Copyright (c) 2026 opticfluorine
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
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Components.Validators;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Events.Details.Validators;
using Sovereign.EngineCore.Systems.Data;
using Sovereign.ServerCore.Systems.TemplateEntity;
using Xunit;

namespace Sovereign.ServerNetwork.Network.Rest.TemplateEntities;

/// <summary>
///     Tests for the template entity REST endpoint's typed entity data validation.
/// </summary>
public class TestSetTemplateEntityRestService
{
    private readonly FakeEventSender eventSender = new();

    private readonly SetTemplateEntityRestService service;

    public TestSetTemplateEntityRestService()
    {
        var validator = new EntityDefinitionValidator(new NameComponentValidator(),
            new ShadowComponentValidator(), new EntityTypeComponentValidator(),
            NullLogger<EntityDefinitionValidator>.Instance);
        service = new SetTemplateEntityRestService(validator, eventSender,
            NullLogger<SetTemplateEntityRestService>.Instance, new TemplateEntityController());
    }

    [Fact]
    public async Task SetTemplateEntityPost_ValidData_ReturnsOkAndSendsEvent()
    {
        var request = new KeyedEntityDefinitionEventDetails
        {
            EntityDefinition = CreateTemplateDefinition(),
            EntityKeyValuePairs = new Dictionary<string, DataValueDto>
            {
                ["k"] = DataValueDto.FromDataValue(DataValue.FromInt(7))
            }
        };

        var result = await service.SetTemplateEntityPost(request, new DefaultHttpContext());

        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(200, status.StatusCode);
        Assert.Single(eventSender.SentEvents);
    }

    [Fact]
    public async Task SetTemplateEntityPost_MalformedValue_Returns400NamingKey()
    {
        var request = new KeyedEntityDefinitionEventDetails
        {
            EntityDefinition = CreateTemplateDefinition(),
            EntityKeyValuePairs = new Dictionary<string, DataValueDto>
            {
                ["good"] = DataValueDto.FromDataValue(DataValue.FromInt(1)),
                ["bad"] = new DataValueDto { Type = "Int", Value = "not-a-number" }
            }
        };

        var result = await service.SetTemplateEntityPost(request, new DefaultHttpContext());

        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(400, status.StatusCode);
        var value = Assert.IsAssignableFrom<IValueHttpResult>(result).Value?.ToString();
        Assert.NotNull(value);
        Assert.Contains("bad", value);
        Assert.Empty(eventSender.SentEvents);
    }

    [Fact]
    public async Task SetTemplateEntityPost_UnknownTypeName_Returns400NamingKey()
    {
        var request = new KeyedEntityDefinitionEventDetails
        {
            EntityDefinition = CreateTemplateDefinition(),
            EntityKeyValuePairs = new Dictionary<string, DataValueDto>
            {
                ["weird"] = new DataValueDto { Type = "NotAType", Value = "1" }
            }
        };

        var result = await service.SetTemplateEntityPost(request, new DefaultHttpContext());

        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(400, status.StatusCode);
        var value = Assert.IsAssignableFrom<IValueHttpResult>(result).Value?.ToString();
        Assert.NotNull(value);
        Assert.Contains("weird", value);
        Assert.Empty(eventSender.SentEvents);
    }

    /// <summary>
    ///     Creates a minimal valid template entity definition.
    /// </summary>
    /// <returns>Entity definition.</returns>
    private static EntityDefinition CreateTemplateDefinition()
    {
        return new EntityDefinition
        {
            EntityId = EntityConstants.FirstTemplateEntityId,
            EntityType = EntityType.Other
        };
    }

    /// <summary>
    ///     Recording event sender that captures sent events.
    /// </summary>
    private sealed class FakeEventSender : IEventSender
    {
        /// <summary>
        ///     Captured events, in order.
        /// </summary>
        public List<Event> SentEvents { get; } = new();

        public void SendEvent(Event ev)
        {
            SentEvents.Add(ev);
        }

        public bool TryGetOutgoingEvent(out Event? ev)
        {
            ev = null;
            return false;
        }
    }
}
