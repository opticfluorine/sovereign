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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sovereign.EngineCore.Events;
using Sovereign.ServerCore.Configuration;

namespace Sovereign.ServerCore.Systems.Scripting;

/// <summary>
///     Final status of a single script test.
/// </summary>
public enum TestStatus
{
    /// <summary>Test has been declared but not yet completed.</summary>
    Pending,

    /// <summary>Test completed successfully.</summary>
    Passed,

    /// <summary>Test completed with a failure.</summary>
    Failed,

    /// <summary>Test did not complete before the run watchdog elapsed.</summary>
    TimedOut
}

/// <summary>
///     Thread-safe collector for script test results.
/// </summary>
/// <remarks>
///     A single test run is allowed per process. The run begins when a test is
///     registered or the run is explicitly begun, and completes when every
///     registered test has completed or the watchdog elapses.
/// </remarks>
public sealed class TestHarnessResultsCollector : IDisposable
{
    /// <summary>
    ///     Stable prefix used for the summary log line so that it is easy to find in logs.
    /// </summary>
    public const string SummaryPrefix = "Test run finished:";

    private readonly object lockObject = new();
    private readonly ILogger<TestHarnessResultsCollector> logger;
    private readonly TestHarnessOptions options;
    private readonly TestHarnessController controller;
    private readonly IEventSender eventSender;
    private readonly List<SuiteResults> suites = new();
    private readonly Dictionary<string, SuiteResults> suitesByName = new();

    private RunState state = RunState.NotStarted;
    private Timer? watchdogTimer;
    private int expectedSuites;
    private DateTimeOffset runStartTime;

    /// <summary>
    ///     Creates the test harness results collector.
    /// </summary>
    /// <param name="options">Test harness options.</param>
    /// <param name="controller">Test harness controller.</param>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="logger">Logger.</param>
    public TestHarnessResultsCollector(IOptions<TestHarnessOptions> options, TestHarnessController controller,
        IEventSender eventSender, ILogger<TestHarnessResultsCollector> logger)
    {
        this.options = options.Value;
        this.controller = controller;
        this.eventSender = eventSender;
        this.logger = logger;
    }

    public void Dispose()
    {
        watchdogTimer?.Dispose();
    }

    /// <summary>
    ///     Tries to begin a new test run.
    /// </summary>
    /// <returns>true if the run was begun, false if a run is already active or complete.</returns>
    public bool TryBeginRun()
    {
        lock (lockObject)
        {
            if (state != RunState.NotStarted)
            {
                logger.LogWarning("Test run already {State}; refusing to begin a new run. " +
                                  "Only one test run is supported per server process.", state);
                return false;
            }

            BeginRunLocked();
            return true;
        }
    }

    /// <summary>
    ///     Declares that the given number of test suites are about to be loaded.
    /// </summary>
    /// <param name="count">Number of suites expected to register tests.</param>
    public void ExpectSuites(int count)
    {
        lock (lockObject)
        {
            if (state == RunState.Complete)
            {
                logger.LogWarning("Ignoring expectation of {Count} test suites; run is already complete.", count);
                return;
            }

            expectedSuites += count;
        }
    }

    /// <summary>
    ///     Registers a test in the pending state.
    /// </summary>
    /// <param name="suite">Suite (script) name.</param>
    /// <param name="test">Test name.</param>
    /// <returns>true if the test was registered, false if it was a duplicate.</returns>
    public bool RegisterTest(string suite, string test)
    {
        lock (lockObject)
        {
            if (state == RunState.Complete)
            {
                logger.LogWarning("Test {Suite}.{Test} registered after run completion; ignoring.", suite, test);
                return false;
            }

            if (state == RunState.NotStarted) BeginRunLocked();

            if (TryFindTestLocked(suite, test, out _))
            {
                logger.LogWarning("Duplicate test {Suite}.{Test}; ignoring duplicate registration.", suite, test);
                return false;
            }

            GetOrAddSuiteLocked(suite).Tests.Add(new TestEntry(test));
            return true;
        }
    }

    /// <summary>
    ///     Records the completion of a pending test.
    /// </summary>
    /// <param name="suite">Suite (script) name.</param>
    /// <param name="test">Test name.</param>
    /// <param name="passed">Whether the test passed.</param>
    /// <param name="message">Failure message, or empty if the test passed.</param>
    /// <param name="durationMs">Test duration in milliseconds, if known.</param>
    /// <returns>true if the completion was recorded, false if the test was unknown or already completed.</returns>
    public bool CompleteTest(string suite, string test, bool passed, string message, double durationMs = 0.0)
    {
        lock (lockObject)
        {
            if (!TryFindTestLocked(suite, test, out var entry))
            {
                logger.LogWarning("Completion for unknown test {Suite}.{Test}; ignoring.", suite, test);
                return false;
            }

            if (entry.Status != TestStatus.Pending)
            {
                logger.LogWarning("Duplicate completion for test {Suite}.{Test}; first result wins.", suite, test);
                return false;
            }

            entry.Status = passed ? TestStatus.Passed : TestStatus.Failed;
            entry.Message = passed ? "" : message;
            entry.DurationMs = durationMs;

            CheckForCompletionLocked();
            return true;
        }
    }

    /// <summary>
    ///     Begins a run, recording the start time and arming the watchdog.
    /// </summary>
    private void BeginRunLocked()
    {
        state = RunState.Active;
        runStartTime = DateTimeOffset.UtcNow;

        var timeoutMs = Math.Max(1, options.TestTimeoutSeconds) * 1000;
        watchdogTimer = new Timer(OnWatchdog, null, timeoutMs, Timeout.Infinite);
        logger.LogInformation("Test run started (watchdog {TimeoutSeconds} s).", options.TestTimeoutSeconds);
    }

    /// <summary>
    ///     Watchdog callback that marks all pending tests as timed out and finalizes the run.
    /// </summary>
    /// <param name="unused">Unused timer state.</param>
    private void OnWatchdog(object? unused)
    {
        lock (lockObject)
        {
            if (state != RunState.Active) return;

            var timedOut = 0;
            foreach (var suite in suites)
            foreach (var test in suite.Tests)
            {
                if (test.Status != TestStatus.Pending) continue;
                test.Status = TestStatus.TimedOut;
                test.Message = $"Test did not complete within {options.TestTimeoutSeconds} s.";
                timedOut++;
            }

            if (timedOut > 0)
                logger.LogWarning("Test run watchdog elapsed; marked {Count} tests as timed out.", timedOut);

            FinalizeRunLocked();
        }
    }

    /// <summary>
    ///     Checks whether all expected suites have registered and all registered tests
    ///     have completed, finalizing the run if so.
    /// </summary>
    private void CheckForCompletionLocked()
    {
        if (state != RunState.Active) return;
        if (suites.Count < expectedSuites) return;

        foreach (var suite in suites)
        foreach (var test in suite.Tests)
            if (test.Status == TestStatus.Pending)
                return;

        FinalizeRunLocked();
    }

    /// <summary>
    ///     Finalizes the run, reporting the results.
    /// </summary>
    private void FinalizeRunLocked()
    {
        state = RunState.Complete;
        watchdogTimer?.Dispose();
        watchdogTimer = null;

        var total = 0;
        var passed = 0;
        var failed = 0;
        var timedOut = 0;
        foreach (var suite in suites)
        foreach (var test in suite.Tests)
        {
            total++;
            switch (test.Status)
            {
                case TestStatus.Passed:
                    passed++;
                    break;
                case TestStatus.Failed:
                    failed++;
                    break;
                case TestStatus.TimedOut:
                    timedOut++;
                    break;
            }
        }

        var durationMs = (DateTimeOffset.UtcNow - runStartTime).TotalMilliseconds;
        logger.LogInformation(
            SummaryPrefix + " {Suites} suites, {Total} tests: {Passed} passed, {Failed} failed, " +
            "{TimedOut} timed out ({DurationSeconds:F1} s)",
            suites.Count, total, passed, failed, timedOut, durationMs / 1000.0);

        foreach (var suite in suites)
        foreach (var test in suite.Tests)
            if (test.Status is TestStatus.Failed or TestStatus.TimedOut)
                logger.LogError("Test {Test} in suite {Suite} {Status}: {Message}",
                    test.Name, suite.Name, test.Status, test.Message);

        if (!string.IsNullOrWhiteSpace(options.ResultsFile)) WriteResultsFile(durationMs, total, passed, failed, timedOut);

        if (options.QuitOnCompletion)
        {
            logger.LogInformation("Requesting server shutdown with exit code {ExitCode}.", failed + timedOut > 0 ? 1 : 0);
            controller.RequestShutdown(eventSender, failed + timedOut > 0);
        }
    }

    /// <summary>
    ///     Writes the machine-readable JSON results file.
    /// </summary>
    /// <param name="durationMs">Total run duration in milliseconds.</param>
    /// <param name="total">Total test count.</param>
    /// <param name="passed">Passed test count.</param>
    /// <param name="failed">Failed test count.</param>
    /// <param name="timedOut">Timed out test count.</param>
    private void WriteResultsFile(double durationMs, int total, int passed, int failed, int timedOut)
    {
        try
        {
            var results = new TestRunResults
            {
                RunDurationMs = durationMs,
                Total = total,
                Passed = passed,
                Failed = failed,
                TimedOut = timedOut
            };

            foreach (var suite in suites)
            {
                var suiteResults = new SuiteRunResults { Name = suite.Name };
                foreach (var test in suite.Tests)
                    suiteResults.Tests.Add(new TestRunEntry
                    {
                        Name = test.Name,
                        Status = test.Status,
                        Message = test.Status is TestStatus.Failed or TestStatus.TimedOut ? test.Message : "",
                        DurationMs = test.DurationMs
                    });
                results.Suites.Add(suiteResults);
            }

            var serializeOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() }
            };
            File.WriteAllText(options.ResultsFile, JsonSerializer.Serialize(results, serializeOptions));
            logger.LogInformation("Wrote test results to {ResultsFile}.", options.ResultsFile);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to write test results file {ResultsFile}.", options.ResultsFile);
        }
    }

    /// <summary>
    ///     Gets the suite results for the given suite, adding it if not yet present.
    /// </summary>
    /// <param name="suite">Suite (script) name.</param>
    /// <returns>Suite results.</returns>
    private SuiteResults GetOrAddSuiteLocked(string suite)
    {
        if (suitesByName.TryGetValue(suite, out var existing)) return existing;

        var created = new SuiteResults(suite);
        suites.Add(created);
        suitesByName[suite] = created;
        return created;
    }

    /// <summary>
    ///     Finds the test entry for the given suite and test names.
    /// </summary>
    /// <param name="suite">Suite (script) name.</param>
    /// <param name="test">Test name.</param>
    /// <param name="entry">Found test entry, or null if not found.</param>
    /// <returns>true if the test was found, false otherwise.</returns>
    private bool TryFindTestLocked(string suite, string test, [NotNullWhen(true)] out TestEntry? entry)
    {
        entry = null;
        if (!suitesByName.TryGetValue(suite, out var suiteResults)) return false;

        foreach (var candidate in suiteResults.Tests)
            if (candidate.Name == test)
            {
                entry = candidate;
                return true;
            }

        return false;
    }

    /// <summary>
    ///     Run state.
    /// </summary>
    private enum RunState
    {
        NotStarted,
        Active,
        Complete
    }

    /// <summary>
    ///     Mutable record for a single test within a suite.
    /// </summary>
    private sealed class TestEntry
    {
        public TestEntry(string name)
        {
            Name = name;
        }

        public string Name { get; }
        public TestStatus Status { get; set; } = TestStatus.Pending;
        public string Message { get; set; } = "";
        public double DurationMs { get; set; }
    }

    /// <summary>
    ///     Mutable record for the tests belonging to one suite.
    /// </summary>
    private sealed class SuiteResults
    {
        public SuiteResults(string name)
        {
            Name = name;
        }

        public string Name { get; }
        public List<TestEntry> Tests { get; } = new();
    }

    /// <summary>
    ///     Root object for the JSON results file.
    /// </summary>
    private sealed class TestRunResults
    {
        public double RunDurationMs { get; set; }
        public int Total { get; set; }
        public int Passed { get; set; }
        public int Failed { get; set; }
        public int TimedOut { get; set; }
        public List<SuiteRunResults> Suites { get; } = new();
    }

    /// <summary>
    ///     Per-suite object for the JSON results file.
    /// </summary>
    private sealed class SuiteRunResults
    {
        public string Name { get; set; } = "";
        public List<TestRunEntry> Tests { get; } = new();
    }

    /// <summary>
    ///     Per-test object for the JSON results file.
    /// </summary>
    private sealed class TestRunEntry
    {
        public string Name { get; set; } = "";
        public TestStatus Status { get; set; }
        public string Message { get; set; } = "";
        public double DurationMs { get; set; }
    }
}
