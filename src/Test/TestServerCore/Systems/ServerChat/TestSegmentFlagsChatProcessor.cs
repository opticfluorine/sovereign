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
using System.Numerics;
using Microsoft.Extensions.Logging.Abstractions;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Player;
using Sovereign.EngineCore.Systems.Data;
using Sovereign.EngineCore.World;
using Sovereign.ServerCore.Systems.Persistence;
using Sovereign.ServerCore.Systems.WorldManagement;
using Sovereign.ServerCore.TestSupport;
using Xunit;

namespace Sovereign.ServerCore.Systems.ServerChat;

/// <summary>
///     Unit tests for SegmentFlagsChatProcessor.
/// </summary>
public class TestSegmentFlagsChatProcessor
{
    private const ulong AdminId = 0x10;
    private const ulong NonAdminId = 0x11;

    private readonly AdminTagCollection admins;
    private readonly FakeDataServices dataServices = new();
    private readonly FakeEventSender eventSender = new();
    private readonly KinematicsComponentCollection kinematics;
    private readonly PlayerFlagsComponentCollection playerFlags;
    private readonly SegmentFlagsChatProcessor processor;
    private readonly SegmentFlagManager segmentFlags = new();

    public TestSegmentFlagsChatProcessor()
    {
        var entityTable = new EntityTable();
        var componentManager = new ComponentManager(new EntityNotifier());
        admins = new AdminTagCollection(entityTable, componentManager);
        playerFlags = new PlayerFlagsComponentCollection(entityTable, componentManager);
        kinematics = new KinematicsComponentCollection(entityTable, componentManager);
        var names = new NameComponentCollection(entityTable, componentManager);

        dataServices.SetGlobal(GlobalConfigKeys.MinimumZ, "-1024");
        dataServices.SetGlobal(GlobalConfigKeys.MaximumZ, "1024");

        // The admin is positioned in segment (0, 0, 0).
        admins.TagEntity(AdminId);
        admins.ApplyComponentUpdates();
        kinematics.AddOrUpdateComponent(AdminId, new Kinematics { Position = new Vector3(1, 1, 1) });

        var playerRoleCheck = new PlayerRoleCheck(admins, playerFlags);
        var internalController = new ServerChatInternalController(eventSender);
        processor = new SegmentFlagsChatProcessor(segmentFlags, new WorldSegmentResolver(), kinematics,
            dataServices, new PersistenceController(), internalController, eventSender, playerRoleCheck,
            new LoggingUtil(names), NullLogger<SegmentFlagsChatProcessor>.Instance);
    }

    [Fact]
    public void NonAdmin_Rejected_NoEventPublished()
    {
        processor.ProcessChat("segflag", "add here Pvp", NonAdminId);

        Assert.Empty(FlagEvents());
        Assert.Contains("Only admins", LastSystemMessage());
    }

    [Fact]
    public void AddHere_PublishesSetForSenderSegment()
    {
        processor.ProcessChat("segflag", "add here Pvp", AdminId);

        var details = SingleFlagDetails();
        Assert.Equal(new GridPosition(0, 0, 0), details.MinSegment);
        Assert.Equal(new GridPosition(0, 0, 0), details.MaxSegment);
        Assert.Equal(WorldSegmentFlag.Pvp, details.Flag);
        Assert.True(details.Set);
    }

    [Fact]
    public void RemoveHere_PublishesClearForSenderSegment()
    {
        processor.ProcessChat("segflag", "remove here Pvp", AdminId);

        var details = SingleFlagDetails();
        Assert.Equal(WorldSegmentFlag.Pvp, details.Flag);
        Assert.False(details.Set);
    }

    [Fact]
    public void FourIntegerBox_CoversAllZ()
    {
        processor.ProcessChat("segflag", "add 1 2 3 4 Pvp", AdminId);

        var details = SingleFlagDetails();
        Assert.Equal(new GridPosition(1, 2, -32), details.MinSegment);
        Assert.Equal(new GridPosition(3, 4, 32), details.MaxSegment);
    }

    [Fact]
    public void SixIntegerBox_ParsesThreeDimensionalBox()
    {
        processor.ProcessChat("segflag", "add 1 2 3 4 5 6 Pvp", AdminId);

        var details = SingleFlagDetails();
        Assert.Equal(new GridPosition(1, 2, 3), details.MinSegment);
        Assert.Equal(new GridPosition(4, 5, 6), details.MaxSegment);
    }

    [Fact]
    public void SixIntegerBox_NormalizesCorners()
    {
        processor.ProcessChat("segflag", "add 4 5 6 1 2 3 Pvp", AdminId);

        var details = SingleFlagDetails();
        Assert.Equal(new GridPosition(1, 2, 3), details.MinSegment);
        Assert.Equal(new GridPosition(4, 5, 6), details.MaxSegment);
    }

    [Fact]
    public void FourIntegerBox_NormalizesCorners()
    {
        processor.ProcessChat("segflag", "add 3 4 1 2 Pvp", AdminId);

        var details = SingleFlagDetails();
        Assert.Equal(new GridPosition(1, 2, -32), details.MinSegment);
        Assert.Equal(new GridPosition(3, 4, 32), details.MaxSegment);
    }

    [Fact]
    public void FlagName_IsCaseInsensitive()
    {
        processor.ProcessChat("segflag", "add here pvp", AdminId);

        Assert.Equal(WorldSegmentFlag.Pvp, SingleFlagDetails().Flag);
    }

    [Theory]
    [InlineData("none")]
    [InlineData("unknown")]
    [InlineData("pvp,pvp")]
    [InlineData("3")]
    public void InvalidFlagName_Rejected_NoEventPublished(string flagName)
    {
        processor.ProcessChat("segflag", $"add here {flagName}", AdminId);

        Assert.Empty(FlagEvents());
        Assert.Contains("Unknown flag", LastSystemMessage());
    }

    [Fact]
    public void OversizedBox_Rejected_NoEventPublished()
    {
        processor.ProcessChat("segflag", "add 0 0 0 1000 1000 1000 Pvp", AdminId);

        Assert.Empty(FlagEvents());
        Assert.Contains("too large", LastSystemMessage());
    }

    [Fact]
    public void MissingFlag_Rejected_NoEventPublished()
    {
        processor.ProcessChat("segflag", "add here", AdminId);

        Assert.Empty(FlagEvents());
        Assert.Contains("Usage", LastSystemMessage());
    }

    [Fact]
    public void BadCoordinates_Rejected_NoEventPublished()
    {
        processor.ProcessChat("segflag", "add a b c d Pvp", AdminId);

        Assert.Empty(FlagEvents());
        Assert.Contains("integers", LastSystemMessage());
    }

    [Fact]
    public void ListWithoutArguments_UsesSenderSegment()
    {
        segmentFlags.SetFlagsInCache(new GridPosition(0, 0, 0), WorldSegmentFlag.Pvp);

        processor.ProcessChat("segflag", "list", AdminId);

        var messages = SystemMessages();
        Assert.Contains(messages, m => m.Contains("Pvp"));
    }

    [Fact]
    public void ListWithoutFlags_ReportsNoFlags()
    {
        processor.ProcessChat("segflag", "list", AdminId);

        Assert.Contains("No flags", LastSystemMessage());
    }

    /// <summary>
    ///     Gets the flag update events that were published.
    /// </summary>
    /// <returns>Flag update events.</returns>
    private List<Event> FlagEvents()
    {
        return eventSender.SentEvents
            .Where(ev => ev.EventId == EventId.Server_Persistence_UpdateSegmentFlags)
            .ToList();
    }

    /// <summary>
    ///     Gets the details of the single published flag update event.
    /// </summary>
    /// <returns>Event details.</returns>
    private SegmentFlagUpdateEventDetails SingleFlagDetails()
    {
        var ev = Assert.Single(FlagEvents());
        return Assert.IsType<SegmentFlagUpdateEventDetails>(ev.EventDetails);
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
