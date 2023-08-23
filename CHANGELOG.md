# Sovereign Engine Changelog

## 2023

### August

#### 23 August 2023

* Persist the `PlayerCharacter` tag in the database.

#### 20 August 2023

* Update database schema to include `PlayerCharacter` tag.
* Add a special type of component called a *tag*. This is essentially a void-typed component with no value.
  Under the hood it is implemented as a boolean component, but an abstraction is provided over
  `BaseComponentCollection<T>` to make it easier to work with.
* Add `PlayerCharacterTagCollection` which allows an entity to be tagged as a player character. This tag
  will drive a lot of player-specific logic including character selection, entity synchronization, etc.
* This change was started roughly a month ago, but I'm just now finally circling back to this. Updated the
  documentation to specify how entities will be synchronized between server and client. This will be done via
  a pub-sub model where clients are subscribed to world segments. The server subscribes and unsubscribes the
  clients based on player position.

### July

#### 22 July 2023

* Add `persistence.md` documentation to describe how the persistence system works. This is a work in progress and
  isn't fully synchronized with the code yet.
* Defined the rules for how entities are loaded from the database. These still need to be fully implemented in
  the server, but they're fairly simple (relatively speaking) and unambiguous - shouldn't be that difficult to
  implement. The harder part will be determining when to unload entities. "There are only two hard things in
  computer science: cache invalidation and naming things." Sovereign's architecture was designed with this in mind,
  so it won't be *excessively* painful to implement. (Famous last words, right?)

#### 16 July 2023

* Centrally handle client disconnects in the server. The disconnect is originated in `ServerNetworkManager` from
  a LiteNetLib peer disconnect event. This in turn produces a `Server_Network_ClientDisconnected` event which
  propagates to all systems responsible for cleaning up resources after a disconnect. This ensures there is a single
  consistent flow for disconnect/logout.
* Automatically trigger logout if a client disconnects from the event server.

#### 15 July 2023

* Begin testing event network code. Fix issue where `Event` was not
  deserialized properly.
* First successful roundtrip of a ping/pong event pair! The client and server
  can finally talk.

#### 10 July 2023

* Properly disconnect clients from server.
* Update outbound network pipeline to select delivery method (unreliable,
  sequenced, etc) based on event ID.
* Update `NetworkSystem` to only ingest ping and pong events for right now.
  More events will be added as needed.

#### 09 July 2023

* Map out remaining work to get the event client/server up and running.
* Add event allowlists to client and server to strictly limit which events
  can be received from the network.
* Start work on plumbing outgoing events to their connections.
* Correctly route client-side events to the main connection when the client
  is connected.
* Add a strategy-based connection mapper to the server that selects a mapping
  strategy based on the event ID. For now we just have the global strategy which
  forwards an event to all connections. The ping event in the server is
  distributed to all clients using this strategy.
* Fix issue where the connection manager was not thread-safe.
* Drive events from the outbound queues into the event sending pathway. Looks like
  I implemented half of this and then stopped.
* Finish implementation of event sending. All that's left is to update the outbound
  pipeline to select the delivery method based on event ID, then test everything.

#### 08 July 2023

* Tag each event received over the network with the originating connection ID.
* Report ping roundtrip timings per connection in the server if debug logging
  is enabled. Should be useful for performance monitoring while developing the
  rest of the network code.

#### 07 July 2023

* Add new settings to the server network configuration for ping interval and
  connection timeout period. The default behavior of the server will be
  to disconnect any client which does not send any traffic, including a
  response to a ping, within the timeout period.
* Add `ServerManagementSystem` which currently just enables the auto-ping
  behavior in the server.

#### 01 July 2023

* Finish the initial implementation of `PingSystem`. I'll need to circle back
  to this for the server in order to do per-connection ping. This will require
  associating nonlocal events to their connection, so starting to think about
  what this will look like.
* Next up, need to add client- and server-specific startup sequences that
  send appropriate startup events (e.g. enabling auto ping).

### June

#### 27 June 2023

* Begin adding a `PingSystem` for providing network ping. This will be used
  for both performance measurement and heartbeat. Work in progress.

#### 26 June 2023

* On my lunch break, fix up the `Castle.Core` and `Castle.Windsor` dependency
  versions to get rid of the warnings introduced by the Dependabot upgrade.

#### 25 June 2023

* Update the event server connection protocol to only require the account UUID
  from the login response - no longer require the IP address to match the one
  seen during the login stage. This addresses an edge case where the login occurs
  over IPv4 while the event server connection occurs over IPv6. Defense against
  login hijacking attacks is still maintained as the shared secret is still
  required for all subsequent messages - therefore the worst that can be done
  with knowledge of the account UUID is a denial of service attack against a
  specific user. This is a reasonable tradeoff given that the account UUID should
  not normally be publicly known anyway, and because it would be difficult to
  time the attack to occur during the login stage (and a brute force attack would
  be easily detected and blocked).
* Replace Protobuf with MessagePack throughout the existing code. This is currently
  untested, I think there will likely be an issue with `Vector3` that will need
  to be handled.
* Greatly simplify the event serialization/deserialization logic.
* Add utility methods for serializing and deserialzing objects using MessagePack
  to `MessageConfig`. These methods ensure that the correct settings are applied
  to handle potentially untrusted data.

#### 24 June 2023

* Remove the old `OutboundNetworkPipeline` scaffolding - I have a new concept
  for how this will work and it requires a slightly different architecture.
  Previously, the pipeline took in an event and its connection, applied some
  _minimal and reductive_ filters and transformations to the event, and then sent
  it out. In effect, this passed two responsibilities to the rest of the server
  at large: first, it left it to some other undefined system to map the event
  to the correct connections, and second, it spread responsibility to the entire
  server to produce "primary defining events" (see `docs/networking.md`) that
  would enter the pipeline. The new design uses a two-stage model: first, a
  _definitive upconverter_ that monitors the secondary defining events and
  periodically produces primary defining events, and second, an
  _additive pipeline_ that takes in both primary and secondary events, transforms
  them as needed, and maps them to the correct connections (plural). This design
  consolidates the responsibilities onto two systems only, leaving much more
  freedom for the design of the remaining game systems.
* Add skeletons for basic outbound pipeline stages to client and server.
* Process login response when connecting to the event server.
* Clean up client-side disconnect handling.

#### 20 June 2023

* Minor bugfixes to the client that were introduced by recent untested changes.
* Begin testing the register/login/connect sequence at startup. Currently running
  into issues with account registration.
* Fix issue where `RestClient` did not play nicely with Watson Webserver. The
  `HttpContent` subclasses do not automatically set the `Content-Length` header,
  and so the server would not parse the request.
* Get registration and login to work between server and client!

#### 19 June 2023

* Adjust some performance thresholds to avoid wasting cycles while the engine
  is idling. This drives tradeoffs between power consumption and latency, but
  the thresholds are low enough where both the client and server should be able
  to quickly shift between low- and high-power states in response to changes in
  system load.

#### 18 June 2023

* Fix additional memory allocation issues found through dynamic analysis.

#### 17 June 2023

* Investigated the performance issue with event latency. This turns out to be
  intentional and I forgot about it - `SystemExecutor` is configured to put
  the executor thread to sleep if only a small number of threads are processed,
  yielding some performance back to the OS when the server is under very light
  load. Disabling this behavior restores the latency to the expected value of
  roughly 40 - 50 us. We're seeing this currently because the server just isn't
  doing anything substantial yet.
* Fixed performance issue when dispatching events from the main thread where
  a dictionary lookup was being performed redundantly.
* Fixed performance issue where the `SystemExecutor` was performing a huge
  number of very small and short-lived allocations on the Small Object
  Heap.

#### 16 June 2023

* Fix issues with debug REST service.

#### 15 June 2023

* Begin testing lots of previously untested changes.
* Fix minor IoC-related issue in `DebugRestService`.
* Fix issue in `Persistence` where the migration level check was not reading
  in a row of data. This resulted in an exception being thrown at startup. It's
  not clear to me why this was working before - earlier testing was done with
  an older version on .NET and using Windows instead of Linux, so maybe that
  played a role. Regardless, it's fixed now.
* Upgraded `Microsoft.Data.Sqlite` dependency to latest stable version.
* Noticed that event latency has degraded on Linux, is now hovering around 0.5ms
  in the server. This should really be an order of magnitude smaller. Need to
  investigate why this is happening now.
* Fix issue where REST server was not started.
* Cleanly handle REST server shutdown without crashing.

#### 13 June 2023

* Add skeleton for a `DebugRestService` to provide a debug REST interface to the
  server for automating certain debug-related tasks. Currently this only contains
  a stub for commanding the server to generate some test world data.
* Add a debug command for generating test world data. It generates the same
  single layer of checkerboard tiles that the client used to generate internally,
  so once the client is integrated with the server, we should see the same
  familiar pattern appear.

#### 10 June 2023

* Finish wiring up the registration client to `ClientNetworkSystem`.
* Update `TestContentSystem` to use the registration client to register a new user
  before authenticating with the server. If the registration fails, it just assumes
  the user already exists and tries to authenticate. This is a bit of a hack, but
  it will work for testing.

#### 09 June 2023

* Publish event in the client when a connection is successful.
* Update `TestContentSystem` to wait for successful connection before trying to load
  the test world segments.
* Fix issue where world segment data was not correctly setting air as the default
  block type for sparse layers.
* Cache the compressed world segment data in the server instead of compressing on the
  fly in the REST service. This avoids the overhead of compressing the data on every
  request.
* Begin implementing a registration client and everything around it. This isn't quite done
  yet, but it's a start. Shouldn't take much longer to finish up, mostly just needs to be
  wired into `ClientSystem` with the new events.

#### 08 June 2023

* Continue client connection sequencing. Work in progress.

#### 07 June 2023

* Refactor `ClientNetwork` library, merging it directly into `ClientCore`. Turns out that was
  a bad boundary to divide the libraries along, it introduces all sorts of awkward couplings
  and other issues.
* Start sequencing the connection process through the client network system. Add a variety of events
  to control this process and report back with status. Untested.

#### 06 June 2023

* Add `AuthenticationClient` for authenticating with the server.

#### 05 June 2023

* Set up a basic CI/CD pipeline (build only for right now) to perform automatic build
  check on the main branch as well as any Dependabot PRs.

#### 04 June 2023

* Gracefully handle connection loss in the client by transitioning to the
  disconnected state when a connection loss is detected.

#### 03 June 2023

* Parse world segment data and load blocks through the client.

#### 02 June 2023

* Begin implementation of the client-side world segment loader.
* Fix issue where default blocks weren't updated in segment data generation.
* Move REST endpoint definitions to constants in a common class so that they can be
  reused easily from the client code.

### May

#### 30 May 2023

* Move the REST client from `ClientNetwork` to `ClientCore` - it will be needed in core, and
  doesn't really fit in `ClientNetwork` since it doesn't implement common network-related interfaces.

#### 29 May 2023

* Implement a simple REST client for interacting with the REST server.
* Update client-side connection parameters to include REST server host and port in addition to the
  host and port for the UDP server.

#### 28 May 2023

* Add a REST service to the server for serving world block segment data (untested). In the future
  this will need to be updated to validate that the requesting user is within a valid range of
  the requested block to prevent information leakage. Currently the transfer uses LZ4 compression
  via the MessagePack library, will need to later evaluate the tradeoff between performance and
  size reduction.
* Generate block segment data as soon as the segment is loaded from the database. Still need to
  wire up the update handler.

#### 27 May 2023

* Finish initial implementation of world block segment conversion (from blocks to structured form).
  This has not yet been tested. Still to be done is conversion back from structured form to blocks
  in the client, and implementation of a transfer interface between server and client.
* For updating world block segment data in the server, just regenerate the segment instead of
  trying to do an incremental update. There will be a performance penalty since constructing a
  segment is an expensive operation, however these can proceed in the background so shouldn't be
  a huge issue except under very large workloads. We can circle back and optimize this with an
  incremental update if it turns out to be a problem, otherwise chalk this one up to avoiding
  a premature optimization.

#### 26 May 2023

* For Windows, copy SDL binaries to build directory automatically. In the future, probably want
  to only do this for Windows and multi-target builds.

#### 25 May 2023

* Trying again to get back into working on this project!
* Get Veldrid renderer up and running on Windows.
* Fix various issues with the material ID 0 change from last July. This introduced an off-by-one
  error in the materials list, and there was some weird stuff going on with material definition
  validation at startup.

## 2022

### July

#### 23 July 2022

* Reserve material ID 0 as a special material corresponding to the absence of
  a block (or "air"). This enables an optimization trick in world block data
  transfer from server to client by leveraging block sparsity (especially above the surface
  layer) to reduce encoded block data size.

#### 05 July 2022

* Fix `PersistenceController` which wasn't filling out any event details.
* Add support to `PersistenceSystem` for loading entire world segments based on
  segment index. A load completion event is sent when this succeeds.
* Allow events to be "synced to tick" based on a flag in `Event`. When this
  flag is sent, the event loop will wait for dispatch until the beginning of
  the first full tick where the event is eligible to be sent (i.e. the start
  of the first tick after the scheduled event time). This allows a completion
  event to be deferred until the main loop has a chance to process any changes
  to the entities and components.

#### 03 July 2022

* Fix various performance issues with component processing and tile
  sprite cacheing.
* Process updates to the tile sprite cache in a background thread so that
  the main thread is not blocked by large cache updates following world
  segment load.

### June

#### 30 June 2022

* Veldrid renderer is functional using the Vulkan backend.
  This still needs to be tested under Win32, but going to
  defer this for now.

#### 13 June 2022

* Completed the initial implementation of the Veldrid renderer port.
  This needs to be tested, which means the client needs to run under
  .NET 6.0 - so, time to do lots of testing of old commits that were
  never tested.

#### 08 June 2022

* Port `D3D11UpdateBuffer` to `VeldridUpdateBuffer`.

#### 04 June 2022

* Continue porting `D3D11Renderer` to `VeldridRenderer`.
* Add `VeldridTexture` class for wrapping a 2D Veldrid texture created from a `Surface`.
* Add `VeldridResourceManager` for managing GPU resources.
* Update `Surface` to expose additional details through its `Properties` field.

### May

#### 30 May 2022

* Start implementing the Veldrid renderer. For the MVP this will only
  support OpenGL rendering (even on Windows).

#### 29 May 2022

* Routine upgrades of dependencies to go with the migration to .NET 6.
  Various minor code changes to fix deprecations, etc.
* Note that a lot of the dependency upgrades (essentially all) have not been
  tested while the renderer rewrite is in progress. We'll accept some technical
  debt here for a while.

#### 28 May 2022

* Refactor the project structures into Common, Client, and Server source trees.
  Previously the projects were organized in the VS solution file, but this change
  provides similar organization independent of the IDE. This makes it easier to
  develop on Linux using VS Code, for example.

#### 22 May 2022

* Well, life has been busy, work has been busy, and it's been two years since
  I've done any serious work on this project. Let's get started.
* Previously had started a migration to .NET 5. Since then, .NET 5 went EOL,
  so migrate again to .NET 6.
* Set up a development container for use with VS Code.

## 2020

### May

#### 03 May 2020

* Update the roadmap with more features. Reorder some of the milestone features
  to reflect current development.

### March

#### 22 March 2020

* Add throttling to all threads to reduce CPU utilization under light
  workloads. The throttling is automatically disabled when the number of
  events processed per loop exceeds a threshold.
* When selecting the initial screen resolution, only consider display modes
  with the same aspect ratio as the default screen resolution.
* Update video adapter selection logic to only select devices with one or
  more display outputs. This adds support for certain devices with multiple
  GPUs where one GPU serves as the "primary" GPU and the driver makes its
  own selection of the actual device to use (for example, certain laptops
  with Radeon integrated graphics). It also handles the edge case where
  the most powerful GPU is a compute-only card.

## 2019

### November

#### 29 November 2019

* Merge the ImGui font atlas into the main `TextureAtlas`. This will allow for
  rendering both text and sprites without re-binding a texture on the GPU.

#### 24 November 2019

* Refactor code around `SDLEventAdapter` to tie in ImGui. At this point ImGui
  should be integrated with SDL2. The next step to to integrate ImGui with the
  D3D11 renderer.

#### 17 November 2019

* Begin integrating ImGui into the client.

#### 09 November 2019

* Add `PerformanceSystem` for monitoring engine performance at runtime.
* Add local event latency monitor to `PerformanceSystem`. This performance
  monitor tracks the average latency of sending an event across the local event
  bus to another thread. The value is logged once per minute to the debug log.
* Remove `Thread.Sleep(1)` call in `SystemExecutor`. The local event latency
  monitor revealed that the resulting context switches had a large impact on
  event latency. On my laptop, event latency averages around 35 us without the
  call, but around 9 ms with - over 250 times slower.

### October

#### 06 October 2019

* Add `NewConnectionProcessor` class for processing handoffs from successful
  login to new event bus connections. This is currently untested; it will
  undergo integration testing once default login is added to the client.
* Add support for login handoffs.

#### 05 October 2019

* Add `TODO.org` for tracking sub-issue-level development tasks.

### September

#### 15 September 2019

* Add REST service for account login.

#### 02 September 2019

* Add REST service for account registration.

#### 01 September 2019

* Add registration support to the account services.

### August

#### 20 August 2019

* Add account services to be consumed by the REST API and network code that
  cares about authentication.
* Add `AccountsSystem` which currently drives a periodic purge of the list
  of accounts which are currently locked out due to too many failed
  login attempts.

#### 18 August 2019

* Start adding authentication code.
* Add account retrieval services to `Persistence`.
* Add account queries to `Persistence`.

### July

#### 07 July 2019

* Add an embedded HTTP server for exposing REST APIs. This will be used for
  out-of-band communications including authentication and block data transfer.
* Wire up network pipelines.

#### 05 July 2019

* Implement `ClientNetworkManager`. Still need to test everything.
* Implement `ServerNetworkManager`. It should be possible to connect to the
  server now, or at least it will be once the client-side networking is
  implemented.

#### 04 July 2019

* Update network documentation to cover the HMAC key.
* Update roadmap.

#### 02 July 2019

* Add support for serializing packets.

#### 01 July 2019

* Add support for deserializing packets.
* When debug logging is enabled, output the full mapping between `EventId` and
  `IEventDetails` at startup.

### June

#### 30 June 2019

* Add `EventDescriptions` for mapping entity IDs to their corresponding
  `IEventDetails` types. Events are dynamically registered in the constructors
  of the responsible systems.
* Add network configuration to server.
* Open server port at startup.
* Refactor code around `IEventAdapter` to avoid unnecessary cyclic
  dependencies. This eliminates dependencies on collections of adapters
  and instead registers adapters with the `EventAdapterManager` from their
  constructors. This has the side effect of requiring each event adapter to
  be depended on by another class; the three existing adapters have been
  linked to relevant classes. It's still not a great solution, but it's a step
  forward from collection dependence.

#### 26 June 2019

* After taking a month away, continue working on networking design.

### May

#### 19 May 2019

* Wire up the inbound and outbound network pipelines. Output a summary to the
  log at startup (debug level only).
* Add interfaces for the inbound and outbound network pipelines. These perform
  event filtering, security, and data transforms at the network boundary as
  needed.
* Add high-level documentation of networking.
* Add `BaseComponentReducer<T>` for producing events from component updates.

#### 13 May 2019

* Add `NetworkEventAdapter` to transfer received events into the event loop.

#### 12 May 2019

* Add `NetworkCore` project with a `NetworkSystem` and `NetworkingService` that
  will be responsible for managing client/server networking.
* Implement `ClientWorldSegmentLoader` to create test sets of blocks. This was
  previously done by `TestContentSystem`'s `BlockSource` class which has now
  been removed. `TestContentSystem` now uses `WorldManagementController` to
  load the nine center world segments, producing equivalent results.

#### 11 May 2019

* Add `WorldManagementController` to `EngineCore`.
* Implement `ServerWorldSegmentLoader` to load world segments from the database.
* Implement `WorldSegmentUnloader` to unload world segments.

#### 07 May 2019

* Consolidate `WorldLoaderSystem` and `WorldManagementSystem` into the latter.
  Handle client/server specific functionality by implementing interfaces in
  `ClientCore` and `ServerCore` respectively.

### April

#### 29 April 2019

* Update `Persistence` to properly handle removed and unloaded entity IDs.
* Add a new `Unload` operation to components. This removes the entity from
  memory but not from persistence.

#### 28 April 2019

* Start adding `WorldLoaderSystem` for managing the loading and unloading of
  world data as needed.
* Move `TestContentSystem` from `EngineCore` to `ClientCore`.
  With `PersistenceSystem` implemented, it is no longer required by the server.

#### 27 April 2019

* Wire up synchronization and successfully write to the database.
* Add prototype `IRemoveComponentQuery` implementations for SQLite.
* Add prototype `IModifyComponentQuery` implementations for SQLite.
* Add prototype `IAddComponentQuery` implementations for SQLite.
* Add `IAddEntityQuery` and its SQLite implementation for adding entity
  IDs to the database.
* Fix cases where `IDisposable` objects in the persistence system were
  not being properly disposed.
* Denormalize `Material` and `MaterialModifier` tables in the database
  to simplify component updates. While the data schema does not logically
  permit Material and MaterialModifier to be specified independently, the
  component code treats them independently and so the database should reflect
  this. This highlights an interesting design decision - the component code
  collectively forms a sort of in-memory database, and this database defines
  the true schema of the data. The relational database schema merely reflect
  the underlying schema rather than defining it.
* Create state trackers and wire them up to the persistence system.

#### 26 April 2019

* Add `BaseStateTracker` for translating component updates into queued
  database actions.

#### 21 April 2019

* Add `ComponentType` enum and tag each `ComponentCollection` with its
  `ComponentType`. This will be used in database synchronization.

#### 20 April 2019

* Add unit tests for `SqliteNextPersistedIdQuery`.
* Clean up `SqliteNextPersistedIdQuery`.

#### 19 April 2019

* Add unit tests for `SqliteMigrationQuery`.
* Fix `SqliteMigrationQuery`.

#### 17 April 2019

* Update `docs/systems.md`.
* Update `docs/projects.md`.
* Add unit tests for `SqliteRetrieveRangeQuery`.
* Migrate `Shaders` project to VS 2019.

#### 16 April 2019

* Fix minor issue with `SqliteRetrieveEntityQuery`.
* Add unit tests for `SqliteRetrieveEntityQuery`.
* Add test fixture for SQLite implementations of persistence interfaces.
* Create full database setup scripts.

#### 15 April 2019

* Implement `Server_Persistence_RetrieveEntitiesInRange` event.
* Add query for retrieving entities in a given position range.

#### 14 April 2019

* Add support for retrieving a single entity from the database.
* Renormalize the database to better reflect component schema constraints.

#### 06 April 2019

* Trigger a persistence synchronization with a regular interval (60 seconds
  by default, can be configured).
* Set up event handlers for persistence system.

### February

#### 10 February 2019

* Check the migration level of the database at startup.
* Open and close SQLite database correctly.
* Properly handle Ctrl+C on the server.

#### 09 February 2019

* Wire up persistence initialization.
* Set up database schema.

#### 03 February 2019

* Add `EntityMapper` for mapping entity IDs to and from the database.

### January

#### 27 January 2019

* Implement required interfaces in `ServerCore`.
* Convert `EngineCore` to a .NET Standard 2.0 class library.

#### 26 January 2019

* Implement issue 4 - move tile sprite resolution outside of the main
  rendering loop into an `IBlockAnimatedSpriteCache` implementation. This
  cache only updates modified blocks and their neighbors once per tick.
* Combine `IEntityBuilder.Material(int)` and
  `IEntityBuilder.MaterialModifier(int)` into a single method
  `IEntityBuilder.Material(int, int)`. Since a block isn't valid unless it has
  both a material and a material modifier, it doesn't make sense to allow the
  two components to be set separately.

#### 06 January 2019

* Add configuration option for running in fullscreen mode.

#### 02 January 2019

* Change checkerboard to something less awful to look at, and use sprites that have
  some variation in their color across their surface - this showed that the texture
  atlas is being sampled correctly.

#### 01 January 2019

* Fix byte ordering issue in `Surface` that was storing texture atlas pixels in
  ABGR order instead of RGBA, leading to weird overly red graphics. Colors are now
  correct.
* Add a `GetHashCode()` method to `GridPosition`. This improved the performance of
  the `WorldTileSpriteSequencer` by approximately 6.5x by improving the performance
  of lookups from the `BlockGridPositionIndexer`.
* The renderer draws a checkerboard for the first time. The colors in the texture
  are completely wrong however, so this needs to be investigated.
* Change base resolution in `DisplayViewport`, the original values were too large
  and the resulting tiles were far too small.
* Update aspect ratio in `DisplayModeSelector` to 16:9. The engine is still locked
  to this aspect ratio; writing an issue to make this adjustable in the future.
* Update `BlockSource` to produce a checkerboard pattern for testing the renderer.

## 2018

### December

#### 31 December 2018

* Decrease vertex and index buffer sizes.
* Normalize texture atlas coordinates.
* Add a `SamplerState` to `WorldPixelShader`.

#### 30 December 2018

* First time to display a screen that's not entirely black! Of course,
  it's just half-black and half blood-red, which is not the desired
  result, so back to debugging...
* Fix buffer overrun issues in `WorldSpriteSequencer`.
* Increase GPU buffer sizes significantly to accomodate larger scenes.
* Fix `NullReferenceException` in `WorldTileSpriteSequencer` when
  sequencing the edge of the world.
* Forcibly terminate worker threads if they fail to respond to engine
  shutdown within a short timeout.
* Fix issues in `OctreeNode` that caused infinite loops when
  attempting to rebalance a tree.
* Update `BlockManager` to designate block entities as `Drawable`
  by default.
* Add `IncrementalGuard` synchronization primitive to resolve the
  possible race condition in `ComponentManager`. Adding an entity with
  an `IEntityBuilder` is now atomic with respect to the per-tick
  component updates.
* Fix issue in `BaseComponentCollection` and the various
  `*EventFilter` classes where newly created entities are not properly
  indexed due to the filters not checking for pending component
  additions. Note that this also reveals a possible race condition
  where an entity is improperly indexed if
  `ComponentManager.UpdateAllComponents()` is called before an
  `EntityBuilder` on another thread finishes building its entity.
* Fix issue in `StructBuffer` where the iterator always skipped the
  first element.
* Fix issue in `EntityAssigner` that was causing entity ID collisions.
* Throttle `SystemExecutor` with a `Thread.Sleep(1)` call (instead of 0)
  to reduce CPU usage. This can be changed back later if the penalty to
  system latency is too high. Attempting the same change on the main
  thread dropped FPS to 45 in a debug build, so the `Thread.Sleep(0)`
  call in `EngineBase` was left alone.

#### 28 December 2018

* Fix issue with thread synchronization in `StructBuffer`.
* Fix issue where the event loop was dropping all events because the
  communicator map was not being populated.
* Refactor some of the event code to partially avoid the use of
  dependency-injected `ICollection<T>` objects, specifically in the case of
  `ISystem` and `IEventSender` dependencies. These dependencies were reversed,
  with the implementations now depending on `IEventLoop` and registering with
  the event loop in the constructor. This fixes an issue when using multiple
  `IEventSender` objects and avoids cyclic dependencies. Note that injected
  `ICollection<T>` objects are still used for `IEventAdapter` objects, as the
  event loop is the owner of the event adapters.
* Add `TestContentSystem` to supply some sample data for testing the engine.
  This system will be removed in the future once data is persistent.

#### 27 December 2018

* Implement `WorldTileSpriteSequencer` for mapping tile sprites to
  animated sprites during world rendering.
* Use unsafe pointer arithmetic for updating vertex and index buffers.
  The array bounds are checked once per layer at the outset.

#### 26 December 2018

* Route entity velocities to the vertices in `WorldSpriteSequencer`.
* Implement `WorldSpriteSequencer` to actually populate the vertex and index
  buffers given a list of animated sprites. For now the transformation from
  animated sprite to texture coordinates is done on the CPU; it would be
  interesting in the future to offload this task to the vertex shader using
  a tbuffer.
* Interpolate positions based on velocities in the vertex shader.
* Add velocity to the world rendering vertex data.

#### 25 December 2018

* Merry Christmas!
* Start implementing `WorldLayerVertexSequencer`.
* Fix the vertex shader to output normalized device coordinates. This is done
  by using a world-view transformation matrix that centers the camera and
  applies the proper 3/4 perspective.
* Ensure that index buffer is copied to the GPU once per frame.

#### 24 December 2018

* Use indexed rendering instead of directly rendering the vertices.
* Wire an index buffer into the high-level renderer.
* Add `Thread.Sleep()` call to `SystemExecutor` to help with high CPU usage.
* Add support for limiting the framerate.

#### 21 December 2018

* Fix startup issue where `IClientConfiguration` could not be resolved.

#### 01 December 2018

* Automatically set the `AboveBlock` components as needed when adding or
  removing blocks.
* Limit block system "add block" events to grid positions since voxel
  blocks should be aligned to grid positions.
* Add a `BlockGridPositionIndexer` to allow fast lookup of block entities
  by their grid position.
* Refactor component event filters into a `BaseComponentEventFilter` class
  and derived classes.

### November

#### 30 November 2018

* Implement a `BaseIntegerPositionComponentIndexer` class to index
  positions on a grid (e.g. for block positions).
* Refactor component indexers into a `BaseComponentIndexer` class and
  derived classes thereof.

#### 24 November 2018

* Update `EntityManager` to support removing entities.
* Move `MaterialComponentCollection`, `MaterialModifierComponentCollection`,
  and `AboveBlockComponentCollection` into a new `BlockSystem` for managing
  block entities.

#### 23 November 2018

* Use an octree-based indexer to identify the drawable entities within
  range of the camera. Since the range of the renderer is somewhat
  arbitrary (each unit of y gives access to another unit of z, and vice
  versa), added a pair of tunable rendering cutoffs to `IClientConfiguration`.
* Update `IEntityBuilder` with the new component types.
* Fix Octree unit tests that were broken by a recent refactoring.
* Continue implementing the high-level world renderer.

#### 22 November 2018

* Start implementing the high-level world renderer.
* Define the tile width in `ClientEngineConfiguration`. An `IClientConfiguration`
  interface is added to provide access to client-specific configuration constants.
* Give a better definition of the block structure in docs/world_structure.md.
* Clear the back buffer prior to rendering.

#### 21 November 2018

* Update world vertex shader to ensure that the camera focus is in
  the center of the screen.
* Populate game scene vertex shader constants buffer using camera data
  and appropriate scaling factors based on the screen resolution.
* Document the unit conventions used by the engine in docs/units.md.

#### 20 November 2018

* Manage camera position and velocity directly in `CameraManager` rather
  than in a dedicated camera entity. If the position and velocity were
  tracked by an entity, the camera would lag the target entity by one tick.

#### 19 November 2018

* Fix issue in `BaseComponentCollection` where attempting to modify the
  value of a component that is not associated to an entity throws an
  exception. The new behavior is to silently do nothing. This is needed
  since a new component is not associated to an entity until the beginning
  of the next tick.
* Ensure that the camera velocity is reset to zero when not tracking an
  entity. This ensures that the camera position does not appear to drift
  between ticks due to interpolation by the renderer.
* Fix issue where objects could not be added to `StructBuffer`s.
* Fix issue where component collections could not be instantiated.
* Update the `Camera` system to only update its position once per tick.
  The motion of the camera should be smoothed through interpolation by
  the renderer.

#### 18 November 2018

* Add `Camera` system to the client to provide a basic tracking
  camera for rendering.
* Add `IEntityFactory` and `IEntityBuilder` to provide a simplified
  interface for creating new entities with various components.

#### 17 November 2018

* Set up rasterizer and output-merger stages for world rendering.
* Update D3D11Device to automatically roll the output target when the
  swapchain buffers are flipped.

#### 11 November 2018

* Refactor some of the game scene rendering code into a world renderer.
  This gives clean layer-by-layer rendering of the game world and will
  allow for lighting and other effects to be easily added in the future.

#### 10 November 2018

* More work on the vertex shader and the input assembly stage.

#### 09 November 2018

* Start configuring the vertex shader for game rendering.

#### 08 November 2018

* `SceneManager` starts with `GameScene` as the active scene by default.
  This will change in the future when other scenes are added.

#### 07 November 2018

* Create D3D11 `InputLayout` for game rendering.

#### 06 November 2018

* Fix vertex shaders.

#### 05 November 2018

* Split WorldShader.hlsl into separate vertex and pixel shader files.
* Load compiled shaders at startup.
* Compile HLSL shaders at build time.

#### 04 November 2018

* Populate vertex buffers during game rendering.

#### 03 Novemeber 2018

* Fix texture coordinates in the `IScene.PopulateBuffers` signature.

#### 02 November 2018

* Update the `IScene.PopulateBuffers` signature based on the resource buffer
  code.
* Rework the resource buffer code.

### October

#### 31 October 2018

* Start working on some code for managing Direct3D resource buffers.
* Start reworking the Direct3D 11 renderer to use the new scene-based
  architecture.
* Start defining the interface between the two components of the rendering
  architecture. Scenes implement, at minimum, a method (still need to define
  the signature) called from the low-level renderer to populate the buffers.
  The low-level renderer is then responsible for interpreting how to use these
  buffers. The key here is that the buffers are populated with a single method
  call across the interface.

#### 30 October 2018

* Remove existing rendering graph code. There wasn't much there and I don't
  think what I had there fits well with the planned rendering architecture
  now that I've taken a step back and thought about it.
* Start some rough high-level documentation of the renderer.

#### 26 October 2018

* Add Material component for indicating the material of a block entity.
* Add new position component indexer that filters out non-Drawable entities.

#### 24 October 2018

* Add Drawable component for indicating whether an entity can be drawn to
  the screen.

#### 23 October 2018

* Automatically populate mapping from sprites to texture atlas at startup.
* Remove SheetId field from spritesheet definitions.

#### 21 October 2018

* Improve logging while loading resources.
* Automatically populate mapping from materials to tile sprites at startup.

#### 20 October 2018

* Automatically populate mappings from tile sprites down through sprites
  at startup. Still need to populate the mapping from materials to tile sprites
  and from sprites to texture atlas coordinates.
* When tile sprite definitions are validated, check that the animated sprites
  referenced in each tile context are defined in the animated sprite definitions.
* Clean up error messages for all sprite definition managers.
* Load animated sprite definitions.

#### 06 October 2018

* Create default sprite definitions.
* Fix issues with sprite definition validation.
* Fix issues with tile sprite definition validation.

### September

#### 30 September 2018

* Migrate to YamlDotNet 5.1.0.

#### 26 September 2018

* Continue work on the rendering graph. Describe the rendering graph as
  a tree to be executed in depth-first order, with nodes at the smae level
  sorted in order of priority. Refer to the documentation comments for the
  RenderingGraph class for further information.

#### 25 September 2018

* Begin work on the rendering graph. The rendering graph provides a layer
  of abstraction between the game model and the renderer. Frames are then
  rendered in a three-step process:
    1. The game state is encoded into a rendering graph. For example, tile
       sprites might be transformed through the sprite pipeline down to the
       level of the texture atlas. Instructions for additional processing
       such as lighting effects are also sequenced in the graph.
    2. The updated rendering graph is passed to the renderer.
    3. The renderer iterates through the rendering graph and executes the
       appropriate draw commands to render the next frame.

#### 24 September 2018

* Require that tile sprites do not contain duplicate tile contexts.
* Continue implementing tile context resolution. Sort tile contexts by
  priority when unpacking from the deserialized record. This sort is
  deterministic.
* Clean up project directory structures, moving all code into a top-level
  src directory.

#### 23 September 2018

* Start implementing tile context resolution for tile sprites. This allows
  tile sprites to be mapped to the corresponding animated sprites based on
  the neighboring tile sprites.
    - The initial implementation lazily resolves the tile context for a given
      tile sprite and set of neighboring tile sprites using an O(n) search of
      the tile contexts for the center tile, then caches the resolved context
      for O(1) lookup when the same combination of tiles appears again.
    - For the time being the sprite resolution code is not thread-safe, as it
      should only need to be called from the rendering thread. Resolving tile
      sprites from another thread could cause a race condition on the context
      cache. That being said, the worst behavior that will happen is that a
      cache check will be missed and both threads will perform an O(n) search.
* Tile sprites are required to have a default tile context (a tile context
  where all neighboring tile IDs are set to wildcards). The tile sprite
  definitions validator throws an exception if any tile sprites lack a
  default tile context.
* Refactor tile sprite definitions to use a separate serializable record
  class instead of loading the internal representation directly, similar
  to what is implemented for animated sprites.

#### 22 September 2018

* Add sprite ID validation to animated sprites.

#### 21 September 2018

* Rename 'doc' directory to 'docs'.
* Add a changelog to keep track of progress.

