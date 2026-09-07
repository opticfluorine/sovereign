#!/usr/bin/env bash
#
# Runs the Sovereign Server Lua script test suites from the server build
# directory and reports the results.
#
# Usage: test-scripts.sh [build-dir]
#
# build-dir is the server build directory containing the Sovereign.Server
# executable and its Data directory (default:
# <repo>/src/Server/SovereignServer/bin/Debug/net10.0). It must be up to date;
# run dotnet build first.
#
# The server is started with the script test harness enabled in auto-run mode
# by overriding the Sovereign:TestHarnessOptions configuration section through
# environment variables. The harness loads every script under Data/Scripts/Test,
# runs all registered tests, and requests a graceful shutdown when the run
# completes; the process exit code is 0 if all tests passed and 1 otherwise.
#
# Tests always run against a fresh database: any existing database is moved
# aside before the run so that the server creates a new one seeded from the
# baseline migration (which includes the "Sword" and "Shield" item templates
# that several committed suites require), and the original database is
# restored when the run finishes.
#
# Results are reported from the JSON results file (jq) or, failing that, from
# the captured server log. The captured log and JSON results are left in the
# build directory as test-run.log and test-results.json for artifact upload.
#
# Requires: bash, sqlite3, timeout, pgrep; jq is optional and improves reporting.

set -u
set -o pipefail

readonly SERVER_EXE="Sovereign.Server"
readonly RESULTS_FILE="test-results.json"
readonly RUN_LOG="test-run.log"
readonly DB_BACKUP="sovereign.db.bak-script-test"
readonly TEST_TIMEOUT_SECS=300
readonly TEST_HARNESS_WATCHDOG_SECS=45

readonly SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
readonly REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

BUILD_DIR="${1:-$REPO_ROOT/src/Server/SovereignServer/bin/Debug/net10.0}"
DB_PATH=""
SERVER_PID=""
SERVER_RC=0
BACKED_UP=false
HAVE_JQ=false

log() {
    printf '%s\n' "$*"
}

fail() {
    printf 'ERROR: %s\n' "$*" >&2
    exit 1
}

need() {
    command -v "$1" > /dev/null 2>&1
}

# Terminates a tracked server process: SIGTERM, 15 s grace, then SIGKILL.
stop_server() {
    [ -n "$SERVER_PID" ] || return 0
    if kill -0 "$SERVER_PID" 2> /dev/null; then
        kill -TERM "$SERVER_PID" 2> /dev/null
        for _ in {1..15}; do
            kill -0 "$SERVER_PID" 2> /dev/null || break
            sleep 1
        done
        if kill -0 "$SERVER_PID" 2> /dev/null; then
            kill -KILL "$SERVER_PID" 2> /dev/null
        fi
    fi
    wait "$SERVER_PID" 2> /dev/null
    SERVER_PID=""
}

# Restores the pre-run database backup, if one was taken. Stale WAL/SHM files
# must be removed or they would be replayed over the restored database.
restore_database() {
    $BACKED_UP || return 0
    rm -f "$DB_PATH-wal" "$DB_PATH-shm"
    mv -f "$BUILD_DIR/Data/$DB_BACKUP" "$DB_PATH"
    log "Database restored from Data/$DB_BACKUP."
}

cleanup() {
    stop_server
    restore_database
}

# Sets an existing database aside for the duration of the run: the WAL is
# checkpointed first so the moved file is complete, and stale WAL/SHM files
# are dropped so the server creates a fresh, migration-seeded database.
# restore_database puts the original back when the run finishes.
backup_database() {
    [ -f "$DB_PATH" ] || return 0
    sqlite3 "$DB_PATH" "PRAGMA wal_checkpoint(TRUNCATE);" > /dev/null 2>&1
    mv -f "$DB_PATH" "$BUILD_DIR/Data/$DB_BACKUP"
    rm -f "$DB_PATH-wal" "$DB_PATH-shm"
    BACKED_UP=true
    log "Existing database moved aside to Data/$DB_BACKUP; a fresh one will be created."
}

# Warns about scripts under Data/Scripts/Test that do not use the Test harness.
# They count toward the expected suite count but never register tests, so run
# completion falls through to the watchdog timer.
warn_ad_hoc_scripts() {
    local scratch
    scratch="$(grep -rLE 'Test\.(Case|Async)' "$BUILD_DIR/Data/Scripts/Test" --include='*.lua' 2> /dev/null || true)"
    if [ -n "$scratch" ]; then
        log "WARNING: the following scripts do not use the Test harness and are not"
        log "covered by the reported results; they also delay run completion until"
        log "the watchdog elapses:"
        printf '%s\n' "$scratch" | sed 's/^/  /'
    fi
}

run_tests() {
    log "Running script tests (watchdog ${TEST_HARNESS_WATCHDOG_SECS} s, outer timeout ${TEST_TIMEOUT_SECS} s) ..."
    (
        cd "$BUILD_DIR"
        exec env \
            "Sovereign__TestHarnessOptions__Enabled=true" \
            "Sovereign__TestHarnessOptions__AutoRun=true" \
            "Sovereign__TestHarnessOptions__QuitOnCompletion=true" \
            "Sovereign__TestHarnessOptions__TestTimeoutSeconds=$TEST_HARNESS_WATCHDOG_SECS" \
            "Sovereign__TestHarnessOptions__ResultsFile=$RESULTS_FILE" \
            timeout "$TEST_TIMEOUT_SECS" "./$SERVER_EXE"
    ) > "$BUILD_DIR/$RUN_LOG" 2>&1 &
    SERVER_PID=$!
    wait "$SERVER_PID"
    SERVER_RC=$?
    SERVER_PID=""
}

# Prints the result summary and any failed or timed out tests, preferring the
# JSON results file (via jq) and falling back to the captured server log.
report_results() {
    local summary=""
    local failures=""

    if $HAVE_JQ && [ -f "$BUILD_DIR/$RESULTS_FILE" ]; then
        summary="$(jq -r '"Test run finished: \(.suites | length) suites, \(.total) tests: \(.passed) passed, \(.failed) failed, \(.timedOut) timed out (\(.runDurationMs / 1000 | floor) s)"' "$BUILD_DIR/$RESULTS_FILE")"
        failures="$(jq -r '.suites[] as $s | $s.tests[] | select(.status != "Passed") | "  \($s.name) :: \(.name): \(.status) - \(.message)"' "$BUILD_DIR/$RESULTS_FILE")"
    else
        summary="$(grep -F 'Test run finished:' "$BUILD_DIR/$RUN_LOG" | tail -n 1 | sed -E 's/^[0-9:]+ [A-Z]+ \[[^]]*\] //')"
        failures="$(grep -E ' in suite .*(Failed|TimedOut)' "$BUILD_DIR/$RUN_LOG" | sed -E 's/^[0-9:]+ [A-Z]+ \[[^]]*\] //; s/^/  /')"
    fi

    log ""
    log "=== Script test results ==="
    if [ -n "$summary" ]; then
        log "$summary"
    else
        log "No test run summary was found; the test run did not complete."
        log "Server errors and last log lines:"
        grep -E ' (FTL|ERR) ' "$BUILD_DIR/$RUN_LOG" | tail -n 5 | sed 's/^/  /'
        tail -n 10 "$BUILD_DIR/$RUN_LOG" | sed 's/^/  /'
        return 1
    fi
    if [ -n "$failures" ]; then
        log "Failed tests:"
        printf '%s\n' "$failures"
    fi
    log "Artifacts: $BUILD_DIR/$RUN_LOG, $BUILD_DIR/$RESULTS_FILE"
    return 0
}

main() {
    need sqlite3 || fail "sqlite3 is required (database backup)."
    need jq && HAVE_JQ=true

    [ -d "$BUILD_DIR" ] || fail "Build directory not found: $BUILD_DIR"
    BUILD_DIR="$(cd "$BUILD_DIR" && pwd)"
    [ -x "$BUILD_DIR/$SERVER_EXE" ] || fail "Server executable not found at $BUILD_DIR/$SERVER_EXE (is the build up to date?)"
    DB_PATH="$BUILD_DIR/Data/sovereign.db"

    local test_script_count
    test_script_count="$(find "$BUILD_DIR/Data/Scripts/Test" -name '*.lua' 2> /dev/null | wc -l)"
    [ "$test_script_count" -gt 0 ] || fail "No test scripts found in $BUILD_DIR/Data/Scripts/Test."

    if pgrep -f "$SERVER_EXE" > /dev/null 2>&1; then
        fail "A $SERVER_EXE process appears to be running; stop it before running script tests."
    fi

    log "Script test runner"
    log "  Build directory: $BUILD_DIR"
    log "  Test scripts:    $test_script_count"
    log ""

    trap cleanup EXIT

    backup_database
    warn_ad_hoc_scripts

    run_tests

    if [ "$SERVER_RC" -eq 124 ]; then
        tail -n 40 "$BUILD_DIR/$RUN_LOG"
        fail "Server did not exit within ${TEST_TIMEOUT_SECS} s (outer timeout)."
    fi
    if [ "$SERVER_RC" -ne 0 ] && [ "$SERVER_RC" -ne 1 ]; then
        log ""
        log "Server exited with code $SERVER_RC; last log lines:"
        tail -n 40 "$BUILD_DIR/$RUN_LOG"
        exit "$SERVER_RC"
    fi

    if ! report_results; then
        exit 1
    fi

    local totals
    totals="$(grep -F 'Test run finished:' "$BUILD_DIR/$RUN_LOG" | tail -n 1 | grep -oE '[0-9]+ (passed|failed|timed out)' | grep -oE '[0-9]+' | tr '\n' ' ')"
    local passed failed timed_out
    read -r passed failed timed_out _ << EOF
$totals
EOF
    if [ "${passed:-0}" -eq 0 ]; then
        log ""
        fail "No tests were registered; test scripts must register tests via Test.Case or Test.Async."
    fi
    if [ "${failed:-0}" -gt 0 ] || [ "${timed_out:-0}" -gt 0 ]; then
        exit 1
    fi

    exit 0
}

main "$@"
