(script-test-harness)=
# Script Test Harness

The server includes a test harness for its [scripting APIs](../../creators/scripting/api/index.md).
The harness runs a suite of Lua test scripts inside a live server, collects per-test results,
and reports a summary to the log and (optionally) a machine-readable JSON results file. This
is useful both for verifying the scripting APIs during development and for exercising a full
server in CI/CD pipelines.

:::{note}
The harness is disabled by default. A production server that leaves the default configuration
in place never loads test scripts and never installs the `Test` Lua library, so the harness
is effectively absent.
:::

## Enabling the Harness

The harness is controlled by the `Sovereign:TestHarnessOptions` configuration section:

| Option              | Default | Description                                                                    |
|---------------------|---------|--------------------------------------------------------------------------------|
| `Enabled`           | `false` | Whether the harness is available at all. Installs the `Test` Lua library and permits test scripts to be loaded. |
| `AutoRun`           | `false` | Whether to load and run the test suite automatically at server startup.        |
| `QuitOnCompletion`  | `false` | Whether to shut the server down when the run completes. The process exit code is 0 if all tests passed and 1 otherwise. |
| `TestTimeoutSeconds`| `60`    | Global watchdog for the run. Tests still pending when the timeout elapses are marked as timed out. |
| `ResultsFile`       | `""`    | Path of an optional JSON results file, relative to the server working directory. |

Any option may be overridden on the command line, e.g.:

```
./Sovereign.Server --Sovereign:TestHarnessOptions:Enabled=true
```

## Running the Suite

### Automatically (CI mode)

Set both `Enabled` and `AutoRun` to run the suite at startup. With `QuitOnCompletion` set,
the server shuts itself down once the run finishes, making the server usable as a CI test
step:

```bash
cd src/Server/SovereignServer/bin/Debug/net10.0
./Sovereign.Server \
    --Sovereign:TestHarnessOptions:Enabled=true \
    --Sovereign:TestHarnessOptions:AutoRun=true \
    --Sovereign:TestHarnessOptions:QuitOnCompletion=true \
    --Sovereign:TestHarnessOptions:ResultsFile=Logs/test-results.json
echo $?
```

The process exit code is 0 if all tests passed and 1 if any test failed or timed out.

### On demand (admin chat command)

With only `Enabled` set, an admin can trigger a run from an in-game chat command:

```
/runtests
```

The server loads the test suite, runs it, and logs the summary. Only one test run is
supported per server process; requesting a second run while one is active (or after one
has completed) is refused and logged.

## Test Results

When a run completes, the harness logs a summary line such as:

```
[TestHarnessResultsCollector] Test run finished: 15 suites, 62 tests: 62 passed, 0 failed, 0 timed out (3.4 s)
```

Each failed or timed-out test is additionally logged at error level with its suite, test
name, and failure message. If `ResultsFile` is set, a JSON file with the following shape is
written:

```json
{
  "runDurationMs": 3400.1,
  "total": 62,
  "passed": 62,
  "failed": 0,
  "timedOut": 0,
  "suites": [
    {
      "name": "Test/Sovereign/Tests/TestInventory",
      "tests": [
        {
          "name": "AddItemAndGetItem",
          "status": "Passed",
          "message": "",
          "durationMs": 0.4
        }
      ]
    }
  ]
}
```

## Writing Test Suites

Each `.lua` file under `Data/Scripts/Test` is a test *suite*. Suites live in
`Data/Scripts/Test/Sovereign/Tests` (committed to the repository); scripts placed directly
in `Data/Scripts/Test` are treated as ad-hoc scratch scripts and are ignored by git.

Every suite script gets its own isolated Lua host and runs in parallel with the other
suites. A suite registers tests with the `Test` library:

- `Test.Case(name, fn)` — declares a synchronous test and runs `fn` immediately.
- `Test.Async(name)` — declares an asynchronous test that will be completed later, e.g.
  from timed callbacks registered with
  [Scripting.AddTimedCallback](../../creators/scripting/api/scripting.md).
- `Test.Step(name, fn)` — runs one step of an asynchronous test under `pcall`; a Lua error
  in `fn` marks the test failed.
- `Test.Pass(name)` / `Test.Fail(name, [message])` — complete an asynchronous test.

Assertions raise ordinary Lua errors and may be used inside `Test.Case` and `Test.Step`:

- `Test.AssertTrue(condition, [message])`
- `Test.AssertEqual(expected, actual, [message])`
- `Test.AssertNear(expected, actual, epsilon, [message])`
- `Test.AssertNil(value, [message])`
- `Test.AssertFailed(message)`

A minimal suite looks like this:

```{code-block} lua
:caption: A minimal test suite.

Test.Case("Arithmetic", function()
    Test.AssertEqual(4, 2 + 2)
end)

Test.Async("DelayedCheck")

local onCheck = function()
    Test.Pass("DelayedCheck")
end

Scripting.AddTimedCallback(0.5, onCheck)
```

### Guidelines

- The tick interval is 10 ms, and entity/component changes are committed at tick
  boundaries. Read back written state in a later timed callback (delays of 0.2–1.0 s are
  ample) rather than immediately after writing it.
- Use `NonPersistent = true` and distinctive names for fixture entities so suites never
  write to the database or collide with each other.
- Log with a distinctive `[SuiteName]` prefix using `Util.LogInfo` so suite output is easy
  to attribute in the server log; each suite's log lines also carry its own script name.
- Slots are 1-based, and `Inventory.AddItem` grants at most one empty slot per inventory
  entity per tick.
- Suites that exercise item APIs (e.g. `TestInventory`, `TestItems`) require item templates
  to exist in the server's database, such as the `Sword` item template used by the committed
  suite. Create such templates with the in-game template editor.
- Player-facing APIs (`Chat`, `Dialogue`) can only be smoke-tested headlessly by invoking
  them without a connected player; real delivery is verified manually with a game client.
