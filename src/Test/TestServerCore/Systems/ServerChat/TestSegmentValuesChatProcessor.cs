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
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Player;
using Sovereign.EngineCore.Systems.Data;
using Sovereign.ServerCore.TestSupport;
using Xunit;

namespace Sovereign.ServerCore.Systems.ServerChat;

/// <summary>
///     Unit tests for SegmentValuesChatProcessor.
/// </summary>
public class TestSegmentValuesChatProcessor
{
    private const ulong AdminId = 0x10;
    private const ulong NonAdminId = 0x11;

    private static readonly GridPosition Segment = new(1, 2, 3);

    private readonly FakeDataServices dataServices = new();
    private readonly FakeEventSender eventSender = new();
    private readonly SegmentValuesChatProcessor processor;
    private readonly RecordingSegmentKeyValueController controller = new();

    public TestSegmentValuesChatProcessor()
    {
        var entityTable = new EntityTable();
        var componentManager = new ComponentManager(new EntityNotifier());
        var admins = new AdminTagCollection(entityTable, componentManager);
        var playerFlags = new PlayerFlagsComponentCollection(entityTable, componentManager);
        var names = new NameComponentCollection(entityTable, componentManager);

        admins.TagEntity(AdminId);
        admins.ApplyComponentUpdates();

        var playerRoleCheck = new PlayerRoleCheck(admins, playerFlags);
        var internalController = new ServerChatInternalController(eventSender);
        processor = new SegmentValuesChatProcessor(controller, dataServices, internalController, playerRoleCheck,
            new LoggingUtil(names), NullLogger<SegmentValuesChatProcessor>.Instance);
    }

    [Fact]
    public void NonAdmin_Rejected_NoControllerCall()
    {
        processor.ProcessChat("segvalue", "set 1 2 3 Owner 5", NonAdminId);

        Assert.Empty(controller.SetValues);
        Assert.Contains("Only admins", LastSystemMessage());
    }

    [Fact]
    public void Get_ExistingKey_PrintsTypeAndValue()
    {
        controller.Seed(Segment, "Owner", DataValue.FromEntityId(0x7));

        processor.ProcessChat("segvalue", "get 1 2 3 Owner", AdminId);

        // EntityId values display in canonical hex: 16 lowercase digits.
        Assert.Equal("EntityId 0000000000000007", LastSystemMessage());
    }

    [Fact]
    public void Get_MissingKey_ReportsNotSet()
    {
        controller.MarkLoaded(Segment);

        processor.ProcessChat("segvalue", "get 1 2 3 Owner", AdminId);

        Assert.Contains("not set", LastSystemMessage());
    }

    [Fact]
    public void Set_WritesTypedValue()
    {
        processor.ProcessChat("segvalue", "set 1 2 3 Owner 42 Int", AdminId);

        var (segment, key, value) = Assert.Single(controller.SetValues);
        Assert.Equal(Segment, segment);
        Assert.Equal("Owner", key);
        Assert.Equal(DataValueType.Int, value.Type);
        Assert.Equal(42L, value.AsInt());
    }

    [Fact]
    public void Set_DefaultTypeIsString()
    {
        processor.ProcessChat("segvalue", "set 1 2 3 Owner hello", AdminId);

        var (_, _, value) = Assert.Single(controller.SetValues);
        Assert.Equal(DataValueType.String, value.Type);
    }

    [Theory]
    [InlineData("set 1 2 3 Owner 5 Bogus")]
    [InlineData("set 1 2 3 Owner notanint Int")]
    [InlineData("set 1 2 3 Owner")]
    public void Set_InvalidValueOrType_UsageNoControllerCall(string message)
    {
        processor.ProcessChat("segvalue", message, AdminId);

        Assert.Empty(controller.SetValues);
        Assert.Contains("Usage", LastSystemMessage());
    }

    [Fact]
    public void Set_ReadOnlyKey_RejectedNoControllerCall()
    {
        processor.ProcessChat("segvalue", "set 1 2 3 __reserved 5", AdminId);

        Assert.Empty(controller.SetValues);
        Assert.Contains("reserved", LastSystemMessage());
    }

    [Fact]
    public void Clear_RemovesKey()
    {
        processor.ProcessChat("segvalue", "clear 1 2 3 Owner", AdminId);

        var (segment, key) = Assert.Single(controller.RemovedKeys);
        Assert.Equal(Segment, segment);
        Assert.Equal("Owner", key);
    }

    [Fact]
    public void Clear_ReadOnlyKey_RejectedNoControllerCall()
    {
        processor.ProcessChat("segvalue", "clear 1 2 3 __reserved", AdminId);

        Assert.Empty(controller.RemovedKeys);
        Assert.Contains("reserved", LastSystemMessage());
    }

    [Fact]
    public void BadCoordinates_RejectedNoControllerCall()
    {
        processor.ProcessChat("segvalue", "set a b c Owner 5", AdminId);

        Assert.Empty(controller.SetValues);
        Assert.Contains("integers", LastSystemMessage());
    }

    [Fact]
    public void UnknownSubcommand_ShowsUsage()
    {
        processor.ProcessChat("segvalue", "frobnicate", AdminId);

        Assert.Contains("Usage", LastSystemMessage());
    }

    /// <summary>
    ///     Gets all system messages sent to players, in order.
    /// </summary>
    /// <returns>System messages.</returns>
    private List<string> SystemMessages()
    {
        return eventSender.SentEvents
            .Where(ev => ev.EventId == EventId.Core_Chat_System)
            .Select(ev => ((SystemChatEventDetails)ev.EventDetails!).Message)
            .ToList();
    }

    /// <summary>
    ///     Gets the most recent system message sent to a player.
    /// </summary>
    /// <returns>System message.</returns>
    private string LastSystemMessage()
    {
        var messages = SystemMessages();
        Assert.NotEmpty(messages);
        return messages[^1];
    }
}
