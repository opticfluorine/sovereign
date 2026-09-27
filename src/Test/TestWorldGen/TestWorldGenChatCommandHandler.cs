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

using System;
using Microsoft.Extensions.Logging.Abstractions;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.ServerCore.Systems.ServerChat;
using Sovereign.ServerCore.Systems.WorldGeneration;
using Xunit;

namespace TestWorldGen;

/// <summary>
///     Unit tests for <see cref="WorldGenChatCommandHandler" />.
/// </summary>
public class TestWorldGenChatCommandHandler
{
    /// <summary>
    ///     Sender entity ID used in tests.
    /// </summary>
    private const ulong SenderEntityId = 0x1234;

    [Fact]
    public void Handle_Status_WhenIdle_ReportsIdle()
    {
        var (handler, sender) = CreateHandler();

        handler.Handle("status", SenderEntityId);

        Assert.Contains("Idle", GetSingleSystemMessage(sender), StringComparison.Ordinal);
    }

    [Fact]
    public void Handle_Plan_RespondsNotImplemented()
    {
        var (handler, sender) = CreateHandler();

        handler.Handle("plan 1", SenderEntityId);

        GetSingleSystemMessage(sender).NotImplementedReply();
    }

    [Fact]
    public void Handle_Plan_WithOptions_ParsesAndRespondsNotImplemented()
    {
        var (handler, sender) = CreateHandler();

        handler.Handle("plan 42 --profile custom --at 10,-20", SenderEntityId);

        GetSingleSystemMessage(sender).NotImplementedReply();
    }

    [Fact]
    public void Handle_Plan_MissingSeed_ReportsUsage()
    {
        var (handler, sender) = CreateHandler();

        handler.Handle("plan", SenderEntityId);

        GetSingleSystemMessage(sender).UsageReply();
    }

    [Fact]
    public void Handle_Plan_BadAtOption_ReportsUsage()
    {
        var (handler, sender) = CreateHandler();

        handler.Handle("plan 1 --at bogus", SenderEntityId);

        GetSingleSystemMessage(sender).UsageReply();
    }

    [Fact]
    public void Handle_Commit_RespondsNotImplemented()
    {
        var (handler, sender) = CreateHandler();

        handler.Handle("commit", SenderEntityId);

        GetSingleSystemMessage(sender).NotImplementedReply();
    }

    [Fact]
    public void Handle_Commit_WithSeed_ParsesAndRespondsNotImplemented()
    {
        var (handler, sender) = CreateHandler();

        handler.Handle("commit 7", SenderEntityId);

        GetSingleSystemMessage(sender).NotImplementedReply();
    }

    [Fact]
    public void Handle_Commit_BadSeed_ReportsUsage()
    {
        var (handler, sender) = CreateHandler();

        handler.Handle("commit xyz", SenderEntityId);

        GetSingleSystemMessage(sender).UsageReply();
    }

    [Fact]
    public void Handle_Preview_RespondsNotImplemented()
    {
        var (handler, sender) = CreateHandler();

        handler.Handle("preview", SenderEntityId);

        GetSingleSystemMessage(sender).NotImplementedReply();
    }

    [Fact]
    public void Handle_Replace_ParsesAndRespondsNotImplemented()
    {
        var (handler, sender) = CreateHandler();

        handler.Handle("replace 1 2", SenderEntityId);

        GetSingleSystemMessage(sender).NotImplementedReply();
    }

    [Fact]
    public void Handle_Replace_BadArgs_ReportsUsage()
    {
        var (handler, sender) = CreateHandler();

        handler.Handle("replace 1", SenderEntityId);

        GetSingleSystemMessage(sender).UsageReply();
    }

    [Fact]
    public void Handle_Abort_RespondsNotImplemented()
    {
        var (handler, sender) = CreateHandler();

        handler.Handle("abort", SenderEntityId);

        GetSingleSystemMessage(sender).NotImplementedReply();
    }

    [Fact]
    public void Handle_UnknownSubcommand_ReportsUsage()
    {
        var (handler, sender) = CreateHandler();

        handler.Handle("bogus", SenderEntityId);

        GetSingleSystemMessage(sender).UsageReply();
    }

    [Fact]
    public void Handle_EmptyMessage_ReportsUsage()
    {
        var (handler, sender) = CreateHandler();

        handler.Handle("   ", SenderEntityId);

        GetSingleSystemMessage(sender).UsageReply();
    }

    /// <summary>
    ///     Creates a handler backed by a real system and a recording event sender.
    /// </summary>
    /// <returns>Handler and the event sender that received chat messages.</returns>
    private static (WorldGenChatCommandHandler Handler, FakeEventSender Sender) CreateHandler()
    {
        var system = new WorldGenerationSystem(new EventCommunicator(), new FakeEventLoop(),
            NullLogger<WorldGenerationSystem>.Instance);
        var sender = new FakeEventSender();
        var handler = new WorldGenChatCommandHandler(new WorldGenerationController(),
            new WorldGenerationServices(system), new ServerChatInternalController(sender));

        return (handler, sender);
    }

    /// <summary>
    ///     Gets the single system chat message sent through the given sender.
    /// </summary>
    /// <param name="sender">Recording event sender.</param>
    /// <returns>Message text.</returns>
    private static string GetSingleSystemMessage(FakeEventSender sender)
    {
        var ev = Assert.Single(sender.SentEvents);
        var details = Assert.IsType<SystemChatEventDetails>(ev.EventDetails);
        Assert.Equal(SenderEntityId, details.TargetEntityId);
        return details.Message;
    }
}

/// <summary>
///     Assertions for /worldgen command replies.
/// </summary>
internal static class WorldGenReplyAssertions
{
    /// <summary>
    ///     Asserts that the reply indicates the subcommand is not yet implemented.
    /// </summary>
    /// <param name="message">Reply message.</param>
    public static void NotImplementedReply(this string message)
    {
        Assert.Contains("not yet implemented", message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Asserts that the reply is a usage summary.
    /// </summary>
    /// <param name="message">Reply message.</param>
    public static void UsageReply(this string message)
    {
        Assert.Contains("Usage:", message, StringComparison.Ordinal);
    }
}
