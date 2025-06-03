# Client and Server Configuration

This document describes the configuration options available in the `appsettings.json` file for both the client and server. These options are located under the `"Sovereign"` section.

:::{tip}
Any configuration option can be overridden from the command line by passing a `--key=value` argument, where `key` is the full path to the setting in `appsettings.json` with each level separated by `:`. For example, to override the address of the server to connect to, specify `--Sovereign:ConnectionOptions:Host=<hostname>` as a command line argument to the client.
:::

---

## Client Configuration Options

### ConnectionOptions
- **Host**: The IP address or hostname of the server to connect to.
- **Port**: The port number for the main server connection.
- **RestHost**: The IP address or hostname for REST API communication.
- **RestPort**: The port number for REST API communication.
- **RestTls**: A boolean indicating whether to use TLS for REST API communication.

### AutoUpdaterOptions
- **UpdateServerUrl**: The URL of the update server.
- **UpdateOnStartup**: A boolean indicating whether to check for updates on startup.
- **PromptForUpdate**: A boolean indicating whether to prompt the user before applying updates. Only relevant if `UpdateOnStartup` is true.

### DisplayOptions
- **ResolutionWidth**: The width of the display resolution, in pixels.
- **ResolutionHeight**: The height of the display resolution, in pixels.
- **Fullscreen**: A boolean indicating whether to run in fullscreen mode.
- **MaxFramerate**: The maximum framerate for rendering.
- **Font**: The font file to use for rendering text.
- **BaseFontSize**: The base font size for text rendering.
- **BaseScalingHeight**: The base height for scaling the UI.

### RendererOptions
- **TileWidth**: The width of tiles in pixels.
- **RenderSearchSpacerX**: X padding to include when rendering (for e.g. offscreen shadows).
- **RenderSearchSpacerY**: Y/Z padding to include when rendering (for e.g. offscreen shadows).

### DayNightOptions
- **GlobalLightSteps**: A list of steps in the day/night global light cycle. Each step
  is an object having the following fields:
  - **SecondOfDay**: In-game second of day at which this step begins.
  - **Red**: Red component of global light for this step (0.0 to 1.0).
  - **Green**: Green component of global light for this step (0.0 to 1.0).
  - **Blue**: Blue component of global light for this step (0.0 to 1.0).
:::{tip}
The global light color for a specific second of the day can be found by linearly
interpolating the two steps around the point in time.
:::

---

## Server Configuration Options

### NetworkOptions
- **NetworkInterfaceIPv4**: The IPv4 network interface to bind to.
- **NetworkInterfaceIPv6**: The IPv6 network interface to bind to.
- **Host**: The hostname of the event server.
- **Port**: The port number for the event server.
- **RestHostname**: The hostname for the REST API server.
- **RestPort**: The port number for the REST API server.
- **PingIntervalMs**: The interval in milliseconds for sending ping messages.
- **ConnectionTimeoutMs**: The timeout in milliseconds for client connections.
- **EntitySyncBatchSize**: The batch size for entity synchronization. Larger batches have less overhead per batch but may be more likely to be dropped (especially on low-MTU networks).

### DatabaseOptions
- **DatabaseType**: The type of database to use. Currently only `Sqlite` is supported.
- **Host**: The database file path (for SQLite).
- **Port**: The port number for the database connection. Unused for SQLite.
- **Database**: The name of the database. Unused for SQLite.
- **Username**: The username for database authentication. Unused for SQLite.
- **Password**: The password for database authentication. Unused for SQLite.
- **Pooling**: A boolean indicating whether to use connection pooling.
- **PoolSizeMin**: The minimum size of the connection pool.
- **PoolSizeMax**: The maximum size of the connection pool.
- **SyncIntervalSeconds**: The interval in seconds for database synchronization.
- **CreateIfMissing**: Create the database if it does not already exist. SQLite only.

### NewPlayersOptions
- **AdminByDefault**: A boolean indicating whether new players are granted admin privileges by default. It is strongly recommended to disable this option after the first admin player has been created.

### AccountsOptions
- **MaxFailedLoginAttempts**: The maximum number of failed login attempts before locking the account.
- **LoginDenialPeriodSeconds**: The duration in seconds for which login is denied when the account is locked.
- **MinimumPasswordLength**: The minimum length for user passwords.
- **HandoffPeriodSeconds**: The duration in seconds for handoff operations.

### ScriptingOptions
- **ScriptDirectory**: The directory where scripts are stored.
- **MaxDirectoryDepth**: The maximum directory depth for script discovery.

---

## Common Configuration Options (Client and Server)

### TimeOptions
:::{note}
All time-related options must match between client and server configurations in order for in-game clock
synchronization to work correctly.
:::
 - **SecondsPerHour**: Number of real-world seconds per in-game hour.
 - **HoursPerDay**: Number of in-game hours per in-game day.
 - **DaysPerWeek**: Number of in-game days per in-game week.
 - **WeeksPerMonth**: Number of in-game weeks per in-game month.
 - **MonthsPerSeason**: Number of in-game months per in-game season (one quarter of an in-game year).
 - **SyncIntervalSeconds**: Number of seconds between clock synchronization events. (Only relevant for server.)

### MovementOptions
:::{note}
Movement-related options should match between client and server.
:::
 - **RequestIntervalTicks**: Number of ticks between subsequent movement requests.
 - **MoveExpirationTicks**: Number of ticks after which movement is stopepd if no further request is received.
 - **UpdateIntervalTicks**: Number of ticks between sending subsequent movement updates.

### PerformanceOptions
 - **SystemExecutorCount**: Number of threads to use for executing systems.
 - **YieldEventLoop**: Whether to periodically yield control of the event loop thread. Lowers CPU usage at the cost of increased event latency.
 - **YieldSystemLoop**: Whether to periodically yield control of each system exeuctor thread. Lowers CPU usage at the cost of increased system latency.
 - **NetworkUpdateTimeMs**: Polling interval for the network thread. Higher values reduce CPU usage at the cost of increased (effective) network latency.

### DebugOptions
- **EnableEventLogging**: A boolean indicating whether to enable event logging.
- **EventLogDirectory**: The directory where event logs are stored.

---
