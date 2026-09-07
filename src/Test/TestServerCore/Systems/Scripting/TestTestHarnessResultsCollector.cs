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
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Sovereign.EngineCore.Events;
using Sovereign.ServerCore.Configuration;
using Sovereign.ServerCore.Systems.Scripting;
using Xunit;
using EventId = Sovereign.EngineCore.Events.EventId;

namespace TestServerCore.Systems.Scripting;

/// <summary>
///     Unit tests for <see cref="TestHarnessResultsCollector" />.
/// </summary>
public class TestTestHarnessResultsCollector
{
    public TestTestHarnessResultsCollector()
    {
        Environment.ExitCode = 0;
    }

    [Fact]
    public void TryBeginRun_SecondCall_ReturnsFalse()
    {
        var collector = CreateCollector();

        Assert.True(collector.TryBeginRun());
        Assert.False(collector.TryBeginRun());
    }

    [Fact]
    public void RegisterTest_LazilyBeginsRun_ThenSecondBeginIsRefused()
    {
        var collector = CreateCollector();

        Assert.True(collector.RegisterTest("SuiteA", "test1"));
        Assert.False(collector.TryBeginRun());
    }

    [Fact]
    public void RegisterTest_DuplicateName_ReturnsFalse()
    {
        var collector = CreateCollector();

        Assert.True(collector.RegisterTest("SuiteA", "test1"));
        Assert.False(collector.RegisterTest("SuiteA", "test1"));
    }

    [Fact]
    public void CompleteTest_UnknownTest_ReturnsFalse()
    {
        var collector = CreateCollector();

        Assert.False(collector.CompleteTest("SuiteA", "missing", true, ""));
    }

    [Fact]
    public void CompleteTest_AlreadyCompleted_ReturnsFalse_FirstResultWins()
    {
        var collector = CreateCollector();
        collector.RegisterTest("SuiteA", "test1");

        Assert.True(collector.CompleteTest("SuiteA", "test1", false, "first failure"));
        Assert.False(collector.CompleteTest("SuiteA", "test1", true, ""));
        Assert.False(collector.CompleteTest("SuiteA", "test1", false, "second failure"));
    }

    [Fact]
    public void CompleteTest_AfterRunCompletion_ReturnsFalse()
    {
        var options = new TestHarnessOptions { ResultsFile = Path.Join(Path.GetTempPath(), Path.GetRandomFileName()) };
        var collector = CreateCollector(options);
        try
        {
            collector.RegisterTest("SuiteA", "test1");
            Assert.True(collector.CompleteTest("SuiteA", "test1", true, ""));

            Assert.False(collector.RegisterTest("SuiteA", "test2"));
            Assert.False(collector.CompleteTest("SuiteA", "test2", true, ""));
        }
        finally
        {
            File.Delete(options.ResultsFile);
        }
    }

    [Fact]
    public void Run_DoesNotFinalize_UntilAllExpectedSuitesRegister()
    {
        var options = new TestHarnessOptions { ResultsFile = Path.Join(Path.GetTempPath(), Path.GetRandomFileName()) };
        var collector = CreateCollector(options);
        try
        {
            collector.ExpectSuites(2);
            collector.RegisterTest("SuiteA", "test1");
            Assert.True(collector.CompleteTest("SuiteA", "test1", true, ""));

            Thread.Sleep(200);
            Assert.False(File.Exists(options.ResultsFile));

            collector.RegisterTest("SuiteB", "test2");
            Assert.True(collector.CompleteTest("SuiteB", "test2", true, ""));
            Assert.True(File.Exists(options.ResultsFile));
        }
        finally
        {
            File.Delete(options.ResultsFile);
        }
    }

    [Fact]
    public void Run_AllTestsComplete_WritesResultsFileWithSummaryMath()
    {
        var options = new TestHarnessOptions { ResultsFile = Path.Join(Path.GetTempPath(), Path.GetRandomFileName()) };
        var collector = CreateCollector(options);
        try
        {
            collector.ExpectSuites(2);

            collector.RegisterTest("SuiteA", "pass1");
            collector.CompleteTest("SuiteA", "pass1", true, "", 1.5);
            collector.RegisterTest("SuiteA", "fail1");
            collector.CompleteTest("SuiteA", "fail1", false, "boom", 2.5);

            collector.RegisterTest("SuiteB", "pass2");
            collector.CompleteTest("SuiteB", "pass2", true, "");

            using var document = JsonDocument.Parse(File.ReadAllText(options.ResultsFile));
            var root = document.RootElement;

            Assert.Equal(3, root.GetProperty("total").GetInt32());
            Assert.Equal(2, root.GetProperty("passed").GetInt32());
            Assert.Equal(1, root.GetProperty("failed").GetInt32());
            Assert.Equal(0, root.GetProperty("timedOut").GetInt32());
            Assert.True(root.GetProperty("runDurationMs").GetDouble() >= 0.0);

            var suites = root.GetProperty("suites");
            Assert.Equal(2, suites.GetArrayLength());

            Assert.Equal("SuiteA", suites[0].GetProperty("name").GetString());
            var suiteATests = suites[0].GetProperty("tests");
            Assert.Equal(2, suiteATests.GetArrayLength());
            Assert.Equal("pass1", suiteATests[0].GetProperty("name").GetString());
            Assert.Equal("Passed", suiteATests[0].GetProperty("status").GetString());
            Assert.Equal(1.5, suiteATests[0].GetProperty("durationMs").GetDouble(), 5);
            Assert.Equal("fail1", suiteATests[1].GetProperty("name").GetString());
            Assert.Equal("Failed", suiteATests[1].GetProperty("status").GetString());
            Assert.Equal("boom", suiteATests[1].GetProperty("message").GetString());

            Assert.Equal("SuiteB", suites[1].GetProperty("name").GetString());
        }
        finally
        {
            File.Delete(options.ResultsFile);
        }
    }

    [Fact]
    public void Run_AllTestsComplete_WithQuitOnCompletion_SendsQuitWithSuccessExitCode()
    {
        var options = new TestHarnessOptions { QuitOnCompletion = true };
        var eventSender = new FakeEventSender();
        var collector = CreateCollector(options, eventSender);

        try
        {
            collector.RegisterTest("SuiteA", "test1");
            collector.CompleteTest("SuiteA", "test1", true, "");

            Assert.Single(eventSender.SentEvents);
            Assert.Equal(EventId.Core_Quit, eventSender.SentEvents[0]);
            Assert.Equal(0, Environment.ExitCode);
        }
        finally
        {
            Environment.ExitCode = 0;
        }
    }

    [Fact]
    public void Run_FailedTest_WithQuitOnCompletion_SendsQuitWithFailureExitCode()
    {
        var options = new TestHarnessOptions { QuitOnCompletion = true };
        var eventSender = new FakeEventSender();
        var collector = CreateCollector(options, eventSender);

        try
        {
            collector.RegisterTest("SuiteA", "test1");
            collector.CompleteTest("SuiteA", "test1", false, "boom");

            Assert.Single(eventSender.SentEvents);
            Assert.Equal(EventId.Core_Quit, eventSender.SentEvents[0]);
            Assert.Equal(1, Environment.ExitCode);
        }
        finally
        {
            Environment.ExitCode = 0;
        }
    }

    [Fact]
    public void Run_WatchdogElapsed_MarksPendingTestsTimedOutAndFinalizes()
    {
        var options = new TestHarnessOptions
        {
            TestTimeoutSeconds = 1,
            ResultsFile = Path.Join(Path.GetTempPath(), Path.GetRandomFileName())
        };
        var collector = CreateCollector(options);
        try
        {
            collector.RegisterTest("SuiteA", "slow");

            // Wait for the watchdog to fire and the run to finalize.
            var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(5);
            while (!File.Exists(options.ResultsFile) && DateTime.UtcNow < deadline) Thread.Sleep(50);

            Assert.True(File.Exists(options.ResultsFile));

            using var document = JsonDocument.Parse(File.ReadAllText(options.ResultsFile));
            var root = document.RootElement;

            Assert.Equal(1, root.GetProperty("total").GetInt32());
            Assert.Equal(0, root.GetProperty("passed").GetInt32());
            Assert.Equal(0, root.GetProperty("failed").GetInt32());
            Assert.Equal(1, root.GetProperty("timedOut").GetInt32());
            Assert.Equal("TimedOut", root.GetProperty("suites")[0].GetProperty("tests")[0]
                .GetProperty("status").GetString());
        }
        finally
        {
            collector.Dispose();
            File.Delete(options.ResultsFile);
        }
    }

    /// <summary>
    ///     Creates a collector with the given options and event sender.
    /// </summary>
    /// <param name="options">Test harness options, or null for defaults.</param>
    /// <param name="eventSender">Event sender, or null for a fresh fake sender.</param>
    /// <returns>Collector.</returns>
    private static TestHarnessResultsCollector CreateCollector(TestHarnessOptions? options = null,
        FakeEventSender? eventSender = null)
    {
        options ??= new TestHarnessOptions();
        eventSender ??= new FakeEventSender();

        return new TestHarnessResultsCollector(Options.Create(options),
            new Sovereign.EngineCore.Main.CoreController(), eventSender,
            NullLogger<TestHarnessResultsCollector>.Instance);
    }

    /// <summary>
    ///     Fake event sender that records sent events.
    /// </summary>
    private sealed class FakeEventSender : IEventSender
    {
        public List<EventId> SentEvents { get; } = new();

        public void SendEvent(Event ev)
        {
            SentEvents.Add(ev.EventId);
        }

        public bool TryGetOutgoingEvent(out Event? ev)
        {
            ev = null;
            return false;
        }
    }
}
