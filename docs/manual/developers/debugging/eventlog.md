# Event Logs

Sovereign Engine operates by passing events between its ECS systems, both locally and
over the network. As the event rate is too high for every event to be written to the main
log file, a separate event logger is provided to allow for deep inspection of the main
event loop's behavior.

## Enabling Event Logging

To enable event logging, set the `Sovereign:DebugOptions:EnableEventLogging` flag to `true`. This can be done in two ways:
- **Using `appsettings.json`:** Add or modify the following entry in your `appsettings.json` file:
  ```json
  {
    "Sovereign": {
      "DebugOptions": {
        "EnableEventLogging": true
      }
    }
  }
  ```
- **Using a command-line option:** Pass the flag as a command-line argument when starting the application:
  ```
  --Sovereign:DebugOptions:EnableEventLogging=true
  ```
  :::{tip}
  When developing in an IDE (e.g. Visual Studio or Rider), you can create a run configuration for the client
  or server that passes this command line option. This allows you to select whether or not to enable the
  event log while debugging in your IDE by switching the active run configuration.
  :::

## Accessing the Event Log

Once enabled, the event log will be written to a `Logs/eventLog_<timestamp>.json` file in the application's working directory. This file contains a JSON array of serialized Event objects in the order in which they were dispatched by the main event loop.

## Querying the Event Log with jq

[`jq`](https://jqlang.org) is a lightweight and flexible command-line JSON processor. You can use it to query and analyze the event log using functional programming techniques.

For more information on `jq`, refer to its [official documentation](https://jqlang.org/manual/).

### Examples of Using jq on the Event Log

1. **Count the total number of events:**
   ```bash
   jq '. | length' Logs/eventLog_<timestamp>.json
   ```
   This filter outputs a single number equal to the number of recorded events.

2. **Filter events by type (e.g., "Core_Movement_Jump"):**
   ```bash
   jq '[.[] | select(.EventId == "Core_Movement_Jump")]' Logs/eventLog_<timestamp>.json
   ```
   This filter outputs a JSON-formatted list of all `Core_Movement_Jump` events.

3. **Group events by type and count occurrences:**
   ```bash
   jq 'group_by(.EventId) | map({EventId: .[0].EventId, count: length})' Logs/eventLog_<timestamp>.json
   ```
   This filter outputs a JSON-formatted list of tuples of EventId and event count.

4. **Determine average movement events per second since the first login:**
   ```bash
   jq 'map(select(.EventId == "Core_Movement_Move" or .EventId == "Server_Persistence_PlayerEnteredWorld")) | map(.EventTime) | length / (max - min) * 1E6' Logs/eventLog_<timestamp>.json
   ```
   This filter does some arithmetic over the `EventTime` field to find the average rate of `Core_Movement_Move`
   events since the time of the first `Server_Persistence_PlayerEnteredWorld` event (assuming that no movement
   events occurred before the first player login, which is a reasonable assumption).

### Streaming the Log with jq

To monitor the event log in real-time, you can use `tail` to stream the log file and pipe it through `jq`. This is useful for debugging live clients and servers.

1. **Stream all events with jq:**
   ```bash
   tail -n+1 -f Logs/eventLog_<timestamp>.json | jq .
   ```

2. **Stream specific events (e.g., "Core_Movement_Jump"):**
   ```bash
   tail -n+1 -f Logs/eventLog_<timestamp>.json | jq 'select(.EventId == "Core_Movement_Jump")'
   ```

This approach allows you to observe events as they are logged, formatted, and filtered in real-time.
