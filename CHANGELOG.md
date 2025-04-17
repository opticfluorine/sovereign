# Sovereign Engine Changelog

## 2025

### April

#### 16 April 2025

* Renderer: Fix issue where perspective lines were not properly updated when entities are removed.
  Specifically, blocks were being correctly placed on two perspective lines, but only one of those
  lines was updated when the block was removed.
* Physics: More fine-tuning of constants.
* Physics: Fix issue where collision meshes were not properly regenerated when blocks are removed.
* All: Add new `CastShadows` component for specifying how a non-block entity should cast a shadow.

#### 14 April 2025

* Renderer: Fix coordinate error that left camera slightly off-center.

#### 12 April 2025

* Renderer: Fix minor renderer errors where the edges of tiles were sometimes cut off. The cause
  was twofold. First, the world viewport scale factor was not an integer for most screen resolutions,
  which led to sampling artifacts at the edges of tiles. Second, these artifacts had previously been
  misunderstood as errors in texture coordinates, and an incorrect "correction" was applied that
  effectively "zoomed in" by ~0.5px at each border. Both issues have now been corrected and the
  tiles are displayed with pixel-level accuracy at all supported screen resolutions.

#### 7 April 2025

* Physics: Apply physics processing to newly created/loaded entities that have the `Physics` tag.
  Previously physics would not be applied until the first time the entity moved.
* Movement: Handle edge case with long-range teleportation synchronization where clients who are
  not subscribed to the target world segment would not be informed that the entity had moved.
  There are several ways to do this. A trade-off was made here to leak some information about the
  destination of the teleport (specifically, the world segment index) to clients subscribed to the
  source world segment in order to simplify the server-side logic.

#### 6 April 2025

* Physics: Reduce rate of periodic authoritative movement updates to something more reasonable (5 Hz instead of
  the arbitrary initial value of 20 Hz). Preliminary testing seems good - the movement stays cleanly synchronized
  between client and server with minimal artifacts while also avoiding excessive network traffic.
* Renderer: Adjust block shadow map parameters (texture size and frustum scaling) to reduce shadow cutoffs and
  sampling noise at the edges. Some further adjustments to the frustum scaling (particularly the height) may still
  be required when the camera is positioned far above a block, however this has not been tested yet.
* Renderer: Fix an issue where the camera transform for block shadow rendering was slightly inaccurate.
* Chat: Added the `/rescue` command which teleports the player back to the spawn point. This is useful if the
  player gets stuck for any reason. There is still an edge case that needs to be solved for teleport synchronization;
  I have a solution and will get around to implementing it later this week.

### March

#### 25 March 2025

* Physics: Add collisions and gravity.

#### 23 March 2025

* Physics: Generate collision meshes for blocks at runtime. Since a fully-packed world
  segment would contain 32768 block meshes, the engine performs a simplification step
  where each plane of a world segment is reduced to a smaller number of merged meshes,
  taking up to the top four largest rectangles in the plane before falling back on
  individual per-block meshes (which are much faster to generate upfront).
* All: Minor refactorings. Renamed some classes for consistency. Rearranged unit test
  projects into their own directory to keep things clean.

### February

#### 20 February 2025

* World: Change coordinate system so that position specifies the bottom-front-left vertex.
  Previously the position specified the top-back-left vertex. The new approach simplifies
  a lot of things including rendering logic and computing offsets (which are now simple
  vector addition instead of requiring awkward manipulation of the signs of each component).
  Not only does this simplify the existing rendering logic, it should also simplify the
  upcoming physics engine work.

#### 06 February 2025

* All: Add `Physics` and `BoundingBox` components to suuport the physics engine.

#### 05 February 2025

* Movement: Document initial design of the physics engine.

### January

#### 30 January 2025

* **Release v0.5.0**

#### 15 January 2025

* Persistence: Denormalize the database schema so that the `Entity` table contains all
  components, using `NULL` as a sigil to indicate the absence of a component. This eliminates
  most JOINs from loader queries which should give much better scalability at the cost of
  additional disk space (to store the `NULL` values at scale).

#### 13 January 2025

* Point Lights: Update the position offset to be a relative offset calculated from the sprite
  size instead of absolute units. This allows a held item to confer a relatively positioned
  point light source to its holder independent of the holder's sprite size. For example, a torch
  with a centered offset of (0.5, 0.5, 0.0) will appear at the center of its holder regardless of
  whether the holder is a single-tile sprite or a giant monstrosity taking up half of the screen.
* Documentation: Update documentation for changes made in the upcoming v0.5.0 release. Some additional
  updates regarding component persistence will be necessary with the remaining database restructuring.

#### 12 January 2025

* Renderer: Overdraw free sprites in all layers where the sprite overlaps a pixel in projected
  space. This ensures that sprites do not incorrectly clip through walls when partially
  overlapping from the front. Also introduce a depth buffer to occlude the overdrawn sprites
  by foreground objects at higher layers. There's probably still some broken edge cases, but
  we'll identify and fix those as we go.

#### 05 January 2025

* Renderer: Draw free sprites in an XZ plane at the front of the entity's volume instead of an
  XY plane across the volume. This gives a natural ordering of sprites and block faces along
  the Y axis. Later we'll use this to fix the renderer's behavior with sprite occlusion at
  different depths.

#### 01 January 2025

* Happy New Year!
* Scripting: Add autogenerated enum bindings, starting with `Orientation`.

## 2024

### December

#### 31 December 2024

* It's a busy time of year with the holidays, but I've made slow progress on documenting
  the scripting engine in the manual. This is now mostly complete.

#### 19 December 2024

* Scripting: Add new script management command `/listscripts` which lists all currently
  loaded scripts.

#### 18 December 2024

* Scripting: Add new script management chat commands `/reloadallscripts`, `/reloadscript`,
  and `/loadnewscripts`. See the README for more details about what they do and how
  to use them.
* Scripting: Fix issue accidentally introduced yesterday where the Lua marshaller wasn't
  doing anything due to a mismatched name in the generator code.
* Persistence: Update SQLite dependency to latest to eliminate warnings in .NET 8 and later.

#### 17 December 2024

* Server: Add Lua bindings for `IEntityBuilder`. The Lua interface takes a table mapping
  component names to their values. Any components omitted are ignored. The `Without*` APIs
  are not supported in Lua (except for tags, where passing false will call the
  corresponding `Without*` API); removal of components should be done directly through the
  `components` module in Lua.

#### 16 December 2024

* Server: Use packed integers for color where possible instead of vector types. While some
  of the client-side ImGui routines still need vector color values, the packed integer
  format is much more convenient for server-side scripting.

#### 15 December 2024

* Client: Display player name above each player sprite.
* Server: Improve logging of account and player login/logout.
* Scripting: Update `Motd.lua` to announce player logout to all connected players.
* Client: Properly clear chat history when switching players without exiting the client.
* Scripting: Improve generated marshallers to produce tables in place of objects instead
  of one argument per field. Gives a much nicer encapsulation of types that maps directly
  onto the originating managed type.

#### 13 December 2024

* GUI: Scale GUI dimensions with resolution in order to support various DPI settings.
* GUI: Various minor style and usability improvements to all GUIs.

#### 12 December 2024

* Scripting: Add `color` module providing RGB packing functions as well as a number of
  predefined color constants. Update `chat` module to use packed integers for color
  instead of separate arguments per color component.
* GUI: Load Noto font instead of the default font that ships with ImGui. This will be
  necessary in order to properly scale the GUI with changing resolutions.

#### 11 December 2024

* Scripting: Add generator for component collection bindings.
* Chat: Add new `Core_Chat_Generic` event for sending generic messages from server to
  client with variable color.
* Scripting: Updated the demo `Motd.lua` script to announce players as they enter the
  world to all players. Also issue a warning to admins that they have admin privileges
  when they enter the world.
* Scripting: Fix issue where binding delegates would be garbage collected between script
  calls, resulting in server crashes.
* Scripting: Improve Lua stack size checking to avoid possible crashes.

#### 09 December 2024

* Server: Send new event type when a player enters the world.
* Scripting: Add support for scripts reacting to players entering the world.
* Scripting: Get a basic MOTD script working that greets the player on login.

#### 08 December 2024

* Scripting: Add `events` table to all Lua scripts, which provides an enumeration of all
  event IDs that a Lua script can react to. Also added a callback system that allows scripts
  to register callback functions to receive events. The sample `Motd.lua` script is updated
  to react to the `Core_Tick` event.
* Scripting: Improve error handling, logging full error details including a Lua traceback
  when an error occurs.
* Scripting: Execute all scripts in separate tasks to ensure that a misbehaving script does
  not trap an executor task in an infinite loop, deadlock, or other bad things.

#### 06 December 2024

* Scripting: Add all levels of logging functions to the `util` module. The log category is
  set uniquely for each script, allowing traceability of log messages back to specific scripts.

#### 04 December 2024

* Scripting: Add new source generator for registering all Lua library bindings with the DI
  service container at startup.
* Chat: Automatically open the chat window when first logging in. Fix the default position of
  the chat window so that it doesn't extend beyond the bottom of the screen. Lose window focus
  when pressing Enter without any text entered into the chat window. Lose window focus when
  first logging in. Altogether, these changes greatly improve the usability of the chat feature.

#### 03 December 2024

* All: Spent the last week or so eliminating technical debt. Sovereign no longer depends on
  any of the Castle libraries (which are effectively no longer maintained), instead using
  the built-in .NET dependency injection alongside Serilog for logging. This required a very
  significant amount of rework to get things ported over. However, it dramatically improves
  startup time for both client and server, and it positions the engine for better backend
  scripting support on the server.

### November

#### 23 November 2024

* Scripting: Made additional progress on Lua scripting backend for the server. Added some source
  generators to automatically create bindings between interface provider classes in the systems
  and the Lua scripting backend.

#### 14 November 2024

* Optimization: Move checks for the `Drawable` tag out of the renderer's main loop and into
  the (much less frequent) perspective line updates. This gives a ~ 10% speedup in generation
  of the per-frame render plan.
* Fix issue where the global shadow map was zoomed in too much, leading to errors in shadows
  in the bottom-left of the viewport.

#### 13 November 2024

* Fixed a series of bugs in the renderer that were delaying certain effects by a delay.
  Specifically, the camera position was lagging its target by one tick, and the center position
  of each point light was lagging by one frame.

### October

#### 31 October 2024

* Didn't keep up with the changelog during October, was busy with various things but
  did some work on point lighting and shadows.

### September

#### 29 September 2024

* Add new `CastBlockShadows` tag to block template entities to indicate that
  a block should be included as part of the solid geometry when calculating
  shadows. This defaults to `true` for all new block template entities.

#### 28 September 2024

* Renderer: Fix issue where texture coordinates were off-center, resulting in
  occasional graphical errors where lines would appear at the boundaries
  between blocks.

#### 27 September 2024

* Renderer: Add global lighting and block shadows. One of the advantages of
  modeling the world as a 3D grid of blocks is that we can calculate full
  3D lighting and shadow effects. This update adds world lighting from a
  distant source (e.g. sun, moon) at variable angle and with variable color.
  Future updates will also add point sources of light.

#### 22 September 2024

* Renderer: Fix bug when rendering a single depth layer only where the
  layer grouper would begin to build the rendering plan in a layer object
  that is no longer active, leading to a blank render plan and large
  memory leaks.

#### 21 September 2024

* Persistence: Properly treat world segment block data as *loaded* rather
  than *added*. This was causing an issue where the server was issuing a
  large number of redundant block update events whenever a segment was loaded.
* World Management: Send the world segment loaded event after block data is
  processed rather than after the binary blob is pulled from the database.
  Also send this event in the client as well.
* Player selection: Delay transition from the main menu to in-game state
  until the initial set of block data is fully loaded. Also include a short
  delay once ready to allow the tile sprite cache to catch up. This gives a
  smoother transition where the visible part of the world is fully loaded
  and processed when it first appears.
* Renderer: Rewrite the world renderer to pull entities from the perspective
  lines. Avoid drawing tile sprites that would be totally obscured by other
  tile sprites higher on the same perspective line. This drastically reduces
  the rendering workload.
* Perspective: Replace the use of `SortedSet<T>` with sorted `List<T>` divided
  into buckets. This trades a small amount of added complexity in updating the
  perspective lines in exchange for much higher performance when the perspective
  lines are repeatedly iterated in the renderer. The compute performance of
  `SortedSet<T>` was fine; however, rapid iteration over the set causes very high
  GC pressure that causes stuttering.

#### 18 September 2024

* Sprites: Automatically detect opaque sprites on load and flag them as
  such. This will be used for renderer optimizations (no need to draw any
  sprites that are completely covered by an opaque sprite).

#### 17 September 2024

* Renderer: Refactored to use an extensible `RenderPlan` class instead of the
  previous handful of arrays that were passed around to be populated. This will
  make it significantly easier to extend the renderer's behavior, such as with
  lighting and shadows as planned for the upcoming v0.5.0 release.

#### 16 September 2024

* **Release v0.4.0**
* Renderer: Reduce memory allocations in renderer critical path to ease GC pressure
  at high framerates.
* Documentation: Finish updates to documentation for the v0.4.0 release.

#### 14 September 2024

* Block Template Entity Editor: Default new block template entities to have the Drawable
  tag since almost all block types will be drawable.
* Documentation: Add documentation for Block Template Entity Editor.

#### 13 September 2024

* Default Resources: Created an initial set of sprite/animated sprite/tile sprite/material
  definitions to accompany the "programmer art" (see yesterday's update). Tile sprite
  definitions in particular are finicky for bordered sprites and there are almost certainly
  gaps in coverage, so these will be addressed in future updates as more issues appear. What
  I'm learning from working with these tile sprites is that you want to define the patterns
  as specifically as possible (few wildcards), then for bordered sprites also have a set of
  fallback definitions with lots of wildcards to sweep up the edge cases. Sometimes I regret
  not simply using a bitmask-based tiling system, but this is more flexible and will (hopefully)
  be worth it in the long run.

#### 12 September 2024

* Default Resources: Spent the last few weeks on some "programmer art" for a small default set
  of tile sprites/block materials. These can be found in the accompanying `sovereign-resources`
  repository and via the auto-updater through the main update server.
* Block System: Fix bug where block creation would occasionally fail due to unexpected behavior
  from the covered block check.
* GUI: Add support for scaling up the display of graphical resources in the GUI to improve
  visibility. Scaled up the GUI resources to approximately match the size in the world renderer.
* Client World Editor: Fix a quiet bug where scrolling past the ends of the block templates was
  handled by catching an exception and responding appropriately. This doesn't play nice with
  enabling a breakpoint on all exceptions thrown, which I like to do when testing the client
  prior to a release.

### August

#### 21 August 2024

* Tile Sprites: Fix issue where the client would crash when trying to show a
  preview of a tile sprite in the editor.

#### 20 August 2024

* Persistence: Consolidate all queries to a single thread (the Executor thread responsible for
  the `PersistenceSystem`). This resolves an issue (primarily with Windows builds) where the SQLite
  persistence backend was not thread-safe, leading to data load issues.

#### 18 August 2024

* Client: Properly refresh tile sprite resolutions when block entities are added
  or deleted.

#### 17 August 2024

* Renderer: Fix issue where the orthographic projection to 3:4 perspective was
  applying depth in the wrong direction. Looks like this may have been introduced
  while changing screen space coordinate systems during the Vulkan/Veldrid port,
  and never detected because all testing was done with z=0 until now.

#### 14 August 2024

* Persistence: Store block data in a sparse tree format per world segment instead
  of as individual block entities. This should give a very significant reduction
  in storage space for the database, particularly for dense uniform world segments
  (e.g. underground world segments). The format used is the same as is already used
  for transferring block data between server and client, but without the additional
  LZ4 compression applied.
* Block Entities: Assign a special range of volatile entity IDs to block entities
  since they are no longer persisted with their IDs. This prevents any block entity
  data from being written to the database except for in the space-efficient tree
  structure described above.

#### 10 August 2024

* Tile Sprites: Resolve tile contexts based on neighbors in the same plane as the
  central tile sprite (e.g. along the ground if dealing with the top face of a block,
  or along the wall if dealing with the side face). Previously tile sprites were
  always resolved in the xy plane, which is not the desired behavior for walls (which
  should be resolved in the xz plane instead).
* Tile Sprites: Allow matching tile contexts against empty neighbors (e.g. matching
  a block face that does not have a neighboring block).

#### 09 August 2024

* Client: Fix build on Windows so that the console window doesn't appear when
  running the client.
* Client: Fix build so that files under `Data/` directory are only copied to the build
  output directory if they do not already exist. This avoids the issue where `dotnet build`
  overwrites resource JSON files that were modified by the Resource Editor during a
  previous run.
* Resource Editor: Fix bug where removing existing resources would lead to incorrect
  graphics being rendered in the GUI (the lookup table used for GUI rendering was
  corrupted by incorrect reuse of a reclaimed entry in the table).
* Tile Sprites: Expand tile contexts to include diagonal directions.

#### 07 August 2024

* Resource Editor: Add new tool for generating static animated sprites directly
  from a spritesheet. This eliminates a lot of tedious manual creation of static
  animated sprites for materials, items, etc. that only have a single phase,
  orientation, and frame.
* Client: Fix DPI scaling on Windows.

#### 05 August 2024

* Auto-updater: Clean up the user interfaces and make them look nice enough
  for an initial prototype.

#### 03 August 2024

* Renderer: Reload device-side textures whenever the host-side resources are
  reloaded (i.e. after transitioning out of the Update state).
* Auto-updater: Completely skip the `Update` state when the configuration file
  is set to skip updates on startup; this avoids a redundant reload of resources
  in this case.
* Client: Remove most default resources from the Git repository, instead retrieving them
  from the main update server when the client is first run. This will greatly reduce the
  amount of Git LFS transfers from cloning the repository.

### July

#### 31 July 2024

* Client: Allow for reloading of client-side resources after the initial load.
  Still need to recreate the GPU-side resources in the renderer as well.
* Client: Trigger resource reload when exiting the `Update` state.

#### 30 July 2024

* Auto-updater: Implement backend for updater client. Begin adding a
  new Update scene for handling the updater at startup. This currently
  breaks the client startup, so use an older commit/build if you are looking
  to try out the engine. Also added the frontend GUI for monitoring updater
  progress. Still need to late-load the updated resources when the transition
  to the `MainMenu` state occurs instead of at startup.

#### 27 July 2024

* Auto-updater: Start adding an auto-updater for client side resources.
  Added a command line utility for generating auto-updater support files.
  Also defined the format of the index file that will drive the
  auto-updater.

#### 26 July 2024

* Client: Added some new spritesheets for grass and dirt blocks. I've been
  spending some time reading up on pixel art and trying to make my own.

#### 17 July 2024

* World Editor: Allow drawing blocks with left mouse button, erasing with right mouse button.
* Client: Unload entities whenever the player logs out.

#### 14 July 2024

* World Editor: Add editable controls to GUI for users who do not have a mouse scroll wheel (or prefer
  not to use one).

#### 13 July 2024

* Input: Listen for mouse scroll events.

#### 12 July 2024

* Client: Add simple GUI for world editor.

### June

#### 29 June 2024

* Input: Allow use of WASD keys for movement in addition to the arrow keys.
* Input: Publish events whenever the mouse wheel is scrolled by a fixed interval.
* ClientWorldEdit: New system for client-side world editing. Currently tracks state and makes changes
  based on keyboard and mouse inputs.

#### 28 June 2024

* Input: Export keyboard and mouse state through `InputServices` to be consumed by scene processing code.

#### 26 June 2024

* Input: Add adapters for more mouse-related SDL events (button up, button down, scrolling).
* Network: Update Litenetlib to 1.2.0. For some reason Dependabot has not been catching these
  updates.

#### 22 June 2024

* Update README documentation to match latest.
* Server Network: Add `PlayerFilterInboundPipelineStage` for applying pipeline-level filters to incoming
  events based on the sending player. For example, this can allow for events to be restricted to admin players
  only before the events reach the systems.
* Documentation: Updated developer guide for new events to cover `PlayerFilterInboundPipelineStage` usage.
* Server: Add new `WorldEdit` system for handling server-side processing of events generated by the client's
  world editor. For now there are two world editing events: `SetBlock` and `RemoveBlock`. World editing events
  are only accepted from connections which are linked to players with the admin role.

#### 21 June 2024

* I haven't been keeping up with commits or the changelog since things have been busy, but here's a summary of
  changes made in the last eight days...
* World Management: Add support for synchronizing individual block entities from server to client whenever they
  change. Since the engine only conveys template IDs and positions during bulk transfer, the same limitation was
  taken for individual block synchronization.
* Block System: Add client-side support for receiving block updates as described above.
* Block System: Fix issue where blocks were not being properly uncovered when a block was removed. This happened
  due to an attempt to find the block position from the `Kinematics` component instead of the `BlockPosition`
  component - this was missed when the two position components were split apart recently.
* Chat: Added new admin commands `/addblock` and `/removeblock` for adding and removing single blocks. Useful
  for testing, as well as for emergency situations where the entire world is deleted and you need one block to
  start from. (Hopefully nobody ever runs into that, but you never know...)
* Documentation: Added documentation for admin chat commands.
* Housekeeping: I'm going back to committing directly to the `main` branch for the time being instead of using
  pull requests. For the time being, since I'm the only contributor and the features being worked are relatively
  large, it's less overhead to just work directly with `main`. I'll switch back to the feature branch/pull request
  workflow once the project is a bit larger.

#### 13 June 2024

* Renderer: Fix issue where blocks were being rendered in the wrong position all along. This was revealed by the
  mouseover entity detection code being "wrong". I adjusted the mouse world position by a constant factor to
  correct the error but couldn't figure out where in my math I had dropped a one - well, I didn't, and that should
  have been a clue.
* Perspective System: Properly handle entity extents and partial overlap of perspective lines.
* Camera System: Properly center camera on the center of the targeted entity, not on its upper-left corner
  (the entity's "position").

#### 8 June 2024

* Perspective System: Fix issue where the incorrect entity was removed from a perspective line
  during certain movements.

#### 6 June 2024

* Input System: Track mouse motion events.
* Perspective System: Add public API for retrieving the world coordinates (at camera depth) of the current
  mouse pointer position.
* Entity Debugger: Add new mouse hover mode that shows components for the entity under the mouse pointer, if any.

#### 4 June 2024

* Client: Add `PerspectiveSystem` for tracking where positioned entities fall relative to lines of perspective
  into the screen. This will enable multiple algorithms in the client including entity mouseover detection,
  ceiling hiding, and more efficient rendering of dense world segments.

### May

#### 28 May 2024

* Client: Allow full editing of block template entities through the client-side editor.

#### 27 May 2024

* World Block Data: Send template IDs instead of material/modifier pairs in world segment block data transfers.
  One consequence of this is that all block entities now require a template entity. This also likely means that the
  "Air" material can be deprecated and material indices can run from zero, but deferring this change until
  further analysis is done.
* Client: Fix issue where template entities were not picked up by `BlockAnimatedSpriteCache` which rendered all
  block entities unrenderable with the above update.
* Documentation: Move networking overview to the full manual, update for the above world block data changes.

#### 26 May 2024

* Client: Add basic GUI for creating and updating block template entities. Right now only creation and
  selection are supported, editing components has not yet been implemented.
* Server: Fixed a variety of issues related to template entity synchronization. Most notably, the `EntityMapper`
  was incorrectly reassigning new template entities a persisted ID, thereby promoting new templates to full
  entities.
* Server: Added some default data to the database migration scripts. This obsoletes the debug rest service
  and `DebugSystem` which have been removed from the server. Following the earlier removal of
  `TestContentSystem`, this means that Sovereign is now fully bootstrapped and stands on its own without
  placeholder debug scaffolds.

#### 22 May 2024

* Server: Add `TemplateEntitySystem` which exposes an event-based API for modifying the
  template entities from the client (requires admin role).

#### 18 May 2024

* All: Automatically load the initial set of template entities when the client first logs into
  the server.

#### 16 May 2024

* All: Add entity templates which allow an entity to inherit a default set of components from a
  template. Still need to configure the templates to load on startup.

#### 13 May 2024

* All: Update `BaseComponentCollection<T>` to be array-backed instead of list-backed. This produced a
  noticeable performance improvement.

#### 12 May 2024

* All: Consolidate the `Position` and `Velocity` components into a single `Kinematics` component.
  This allows the position and velocity data to be interleaved within the same contiguous block of
  memory, which sets the stage for CPU cache optimizations for the `Movement` system through the ECS.
* All: Update the component update process to include a *direct access* phase after pending changes
  are processed but before component events are fired. During this phase, systems are allowed direct
  access to the underlying component list to read and modify components. This allows for bulk operations
  to be performed while leveraging the CPU cache for maximum performance.
* Movement System: Process entity movement during the Kinematics component direct access phase instead of
  within the normal system logic during each tick.

#### 11 May 2024

* All: Split the `Position` component into a `Position` component for non-block entities and a `BlockPosition`
  component for block entities. The `Position` component is affected by velocity, whereas the `BlockPosition`
  is static. This enables a future optimization of the ECS where the movement system could function by
  direct iteration over the dynamical `Position` components, thereby taking maximum advantage of the CPU
  cache. This change will be coming shortly.
* Server: Update world management classes to use world segment indices instead of octrees for looking up
  entities in a world segment.
* Renderer: Pull block entities for rendering from a world segment indexer instead of an octree. This causes a
  substantial increase in the number of blocks being rendered, so also expanded the vertex and index buffer sizes
  and allowed for partial copy of the buffers to hardware. This may or may not scale to dense world segments, so
  additional fine tuning may be required in the future.

#### 7 May 2024

* Creators' Manual: Add documentation for resources, resource editor.

### April

#### 30 April 2024

* Resource editor: Add support for adding/modifying/removing materials. This is the last client-side
  rendering resource for now. Next step is to create some documentation for the resource editor.

#### 24 April 2024

* Tile sprite editor: New tile contexts are added to the top of the list instead of the bottom.
  This makes sure that the live preview of the tile resolves to the newly added context prior
  to sorting. Tile sprites would never resolve when added to the end since they would always
  be preempted by the default tile context.
* Sprite editor: Fix top bar layout behavior when resizing the resource editor window.
* All: Migrate to .NET 8. Fixed a few deprecated exception constructors, otherwise it was a very
  straightforward migration.

#### 23 April 2024

* Finish up the tile sprite editor.
* Fix a variety of bugs that were found while testing the tile sprite editor.
* Update the animated sprite editor to improve UI/UX.

#### 10 April 2024

* Fix various memory allocation issues in the core and client. These fixes greatly reduce the number
  of garbage collections, which in turn greatly improves client performance.

#### 7 April 2024

* Add `EntityAnimationSystem` for managing the animation phase (e.g. standing, moving) of each entity.
* Update the animated sprite format to include the animation phase as part of the animated sprite.
* Update the animated sprite editor to support animation phases.

#### 6 April 2024

* Finish up the animated sprite editor.
* Prevent deletion of animated sprites which are used in a tile sprite. Indicate these dependencies to the
  user through a tooltip that appears when hovering over the disabled remove sprite button.

#### 3 April 2024

* Add internal support for modifying the animated sprite table at runtime. This raises the possibility
  that an entity with an `AnimatedSprite` component may end up pointing to a nonexistent animated sprite
  (in fact that possibility already exists). No warnings will be provided by the client in such a case.
* Fix issue where sprite editor tooltips appeared outside of the editor window for large spritesheets.

### March

#### 31 March 2024

* Disable Vulkan validation temporarily. Since upgrading my Arch Linux install last night, I'm seeing what
  I think may be a false positive `VUID-VkPresentInfoKHR-pImageIndices-01430` error on the *third* swapchain
  swap. The first swap presents swapchain image 0 which passes validation. The third swap presents swapchain
  image 0 yet again, which suddenly triggers an error saying that image 0 does not belong to the swapchain.
  Debugger shows that the swapchain does in fact change, so this might be a true validation error, but it's
  new after an Arch update and works fine if validation is disabled. Disabling validation for now.
* Updated the Vulkan fork to not cause a fatal erorr when a Vulkan validation error occurs, just log it and
  move on instead.

#### 30 March 2024

* Switch to a new fork of the Veldrid library since the main project isn't producing releases any longer.
  The new fork includes a bugfix for the Vulkan backend which was causing a dramatic flickering effect in the GUI
  with certain (Radeon RX 580) hardware (aka my desktop).
* Make the sprite editor tab look much nicer - the grid showing sprites with definitions now has a better color
  and a nice alpha blending effect that makes the editor much easier on the eyes.
* Add support for generating missing sprite definitions for a spritesheet from the sprite editor tab.
* Generate sprite definitions for all of the default spritesheets.

#### 28 March 2024

* Highlight sprites in the Sprite Editor which already have defined sprites. Also show the corresponding sprite ID
  when the mouse is hovered over the sprite in the editor.

#### 27 March 2024

* Update the GUI renderer to support rendering images other than just animated sprites. Effectively this
  lets us render anything we want from the texture atlas into the GUI.
* Render entire spritesheets into the Sprite Editor window.
* Remove thread-level performance throttles. This drives up CPU usage in exchange for large performance benefits;
  on my (not very good) laptop (a 2017-era i7 with a GTX 1050), consistently able to hit 300+ FPS in the renderer
  and one-way event latencies on the same timescale as a single context switch in the server.

#### 26 March 2024

* Build out the basic framework for the resource editor window.

#### 25 March 2024

* Make screen resolution configurable through the client configuration window. Only resolutions with a
  16:9 aspect ratio are supported.
* Change the default screen resolution to 1920x1080.
* Increase the number of tiles displayed on screen. Now an entire world segment fits across the width of the
  display.
* Made the sprite table mutable in preparation for adding an integrated sprite definition editor. Modification of
  the sprite table triggers rebuild of the atlas map and the animated sprites. Note that there is not any additional
  validation or error handling for the case where an animated sprite points to a newly removed sprite. Currently I'm
  thinking that deletion of sprites won't be supported; the sprite editor will probably be a "Generate" button that
  automatically generates a full grid of sprites for a selected spritesheet, and a browser to identify a sprite by
  mouseover.

#### 24 March 2024

* Fix issue where player selection GUI did not look nice when long player names were in use.
* Fix issue where player creation GUI had a name input box that was way too small.
* Fix issue where a connection lost error message would be incorrectly displayed when the player logs out
  from the player selection GUI.

#### 23 March 2024

* Add new commands `/addadmin` and `/removeadmin` for granting and revoking the admin role to players.
* Fix issue where the client did not log out automatically when the connection is lost.
* Fix issue where client state was not fully reset after logout or connection loss.
* Fix issue where chat history was not cleared from the client after a logout.
* Add server-side API to resynchronize entity trees on demand in cases where a change was made directly instead
  of through a repeated event sequence (e.g. adding or removing admin roles for an online player).

#### 19 March 2024

* Add `Admin` tag to denote a player as an admin. Admin roles are granted at the player level rather than
  the account level for simplicity.
* Add option to server config file to create new players as admins by default. This is enabled in the default
  configuration file so that the first player created on a new server will have admin privileges; it should then
  be disabled for obvious reasons.

#### 18 March 2024

* Add `/help` command to list other chat commands.
* Lock chat window scroll to bottom if the chat window is already scrolled to the bottom, this way new
  chat messages automatically scroll into view. On the other hand, the lock is released if the chat window
  is scrolled up, so it's possible to look back at earlier chat messages without a constant annoying
  automatic scroll.
* Improve startup GUIs to set default focus, accept Enter key to process forms.

#### 17 March 2024

* Add chat routing to server with command processing support.
* Add a new "system chat" event for delivery of system messages from server to single client.
* Add support for local and global chat. Chat is mostly implemented now, still need to test. I also want to
  add a `/help` command that provides a list of all known commands.

#### 16 March 2024

* Add chat-related events along with proper network routing for these events.
* Add a client-side `ClientChatSystem` for managing chat functions in the client.
* Send chat messages to server from the chat window.

#### 11 March 2024

* Add chat window that can be opened in-game by pressing Enter. It doesn't currently do anything.

#### 10 March 2024

* Fix issues with missing runtime dependencies in the Windows build of the client.

#### 8 March 2024

* **Feature freeze for release v0.2.0.**
* Update GitHub Actions workflow to fix various issues that came up in v0.1.0.

#### 7 March 2024

* Add a basic entity debug window (toggle via F3 key) for examining entities and components at runtime.
* Fix issue where position caches were not unloading correctly in response to remove/unload events due to
  filtered components being removed before the position component is removed/unloaded.
* Add developer documentation for keeping track of common types of difficult-to-debug issues that arise
  during development (such as the component filtering issues with remove/unload).
* Remove `TestContentSystem` as it is no longer needed now that the startup GUIs are developed.
* Add support for deleting player characters.

#### 6 March 2024

* Add player debug window to the in-game scene, toggled via the F2 key, for viewing local player data
  at runtime.
* Fix issue in `WorldSegmentResolver` where negative segments had an off-by-one error in their resolution
  from position coordinates due to an arithmetic error in the direction of rounding.

#### 5 March 2024

* Add keyboard binding processing to the various client states. Initially these are hardcoded and only mapped
  to various debug windows from Dear ImGui, along with the in-game menu via the Escape key.
* Allow players to log out but keep their account logged in, returning to the player selection screen.
* Unload all entities from the client upon logout or disconnect. Also clear all world segment subscriptions.

#### 3 March 2024

* Fix issue where client did not properly disconnect due to a logic error.
* Start testing the various startup GUIs through player creation and selection.

### February

#### 26 February 2024

* Implement the registration GUI.
* Remove the event-based client API for registration, instead having the GUI use the async method from
  `RegistrationClient` directly.
* Implement the login GUI.
* Wire up "Exit" button on the startup screen to exit the client.

#### 23 February 2024

* Add `ClientStateSystem` for managing the top-level client state (main menu, in-game, etc.).
* Update `SceneManager` to select the renderer scene based on the current client state.
* Create a main menu scene for displaying the startup GUIs covering login, registration, etc. Start implementing
  these GUIs.
* Disable the automatic registration/login/etc actions in `TestContentSystem`. These behaviors are transitioning
  to being driven by the `MainMenuScene` and its GUI classes. Once implementation is done, it should be possible
  to entirely remove `TestContentSystem`.

#### 22 February 2024

* Load client configuration from file instead of hardcoding the values.
* Move client connection settings to the configuration file.

#### 21 February 2024

* Successfully integrated Dear ImGui with the renderer - now the client can have GUIs. Still need
  to do some additional testing, reskinning, etc. So far so good.
* Increase GUI vertex and index buffer sizes - they were filling up with rounded windows.
* Fix handling of SDL2 text input events.

#### 18 February 2024

* More work on Dear ImGui integration. Refactor the current GUI rendering code into a separate
  `GuiRenderer` class that coordinates everything

#### 14 February 2024

* Continue work on Dear ImGui integration by adding a GUI rendering pipeline to the Veldrid renderer.
  There's still some work to be done with resource binding and texture loading.

#### 13 February 2024

* Start integrating Dear ImGui into the renderer.

#### 12 February 2024

* Update Dear ImGui to v1.90.1.1.
* Update `CommonGuiManager` to mirror latest Dear ImGui SDL2 backend source.

### January

#### 31 January 2024

* **Release 0.1.0**
* Fix issue where the server would accept a `NaN` movement request which led to an infinite loop in both client
  and server where an octree tried to expand endlessly until it contained the `NaN` (which it never could).
* Fix issue where the client could generate a `NaN` relative velocity in response to player input.
* Update `README.md`.

#### 30 January 2024

* Update deployment targets.
* Add release step to GitHub Actions build pipeline.

#### 29 January 2024

* Desynchronize entity trees from affected clients when a positioned entity is removed or unloaded.
  Note that this does not desynchronize individual child entities as they are removed. This aligns with
  entity synchronization which synchronizes an entire tree at once beginning from the positioned root entity.
  This has impacts on future design - child entities should be created/removed outside of the tree synchronization
  by separate events which deterministically produce the correct entity tree (up to a change in entity IDs) when
  played back on both client and server. Since these are only guaranteed to be synchronized up to a change in entity
  ID, events referencing these child entities will also need to use an indirect reference if the events are to
  be replicated across the network. Alternatively, the separate events could specify the entity ID themselves - this
  could be useful for e.g. inventory where reference to a specific entity is desired.

#### 28 January 2024

* Fix issue where the player entity was not synchronized to the client under certain conditions when
  logging in. Specifically, if the world segment where the player is located is already loaded into memory,
  the initial entity synchronization will typically occur faster than the load of the player entity tree
  from the database. As a result, the player entity is not synchronized in the first call (or, in some rare
  cases, only partially synchronized when the sync event occurs in the middle of a persistence load spread
  across two or more ticks). Sending (or potentially resending) the synchronization events for the player
  entity tree only ensures that the entity is fully synchronized.
* Add `Orientation` component for tracking which direction an enttiy is facing. The orientation is set in
  the `Movement` system based on the direction of the entity's velocity.
* Revert some changes related to database transactions from last night that were breaking things. I should
  really go to bed when I'm tired instead of messing with database transactions.
* Update renderer to apply orientations to the animated sprites as they are rendered. Also update the animated
  sprite definitions to include orientation information.
* Synchronize entity trees to subscribed clients when they are committed to memory. This supersedes the earlier
  work from the first bullet. Again, I should get more sleep.
* Update entity retrieval queries to sort null parents to the end of the result set. This ensures that when
  an entity tree is retrieved, the root of the tree is the last entity processed. Therefore, the entity loaded
  event for the root node is guaranteed to come after the full tree is committed to memory, and so it is safe
  to synchronize the tree once the event is invoked.

#### 27 January 2024

* Fix issue where a read-only query executing at the same time as persistence synchronization would fail
  due to a lack of database transaction.
* Fix issue where a player logout and re-login before a persistence synchronization completes would result
  in a reload of out-of-date entity data from the last synchronization point, effectively allowing a player
  to "roll back" the player by a single persistence synchronization interval.

#### 26 January 2024

* Fix issue where a poorly timed REST call would trigger world segment block data generation while the
  block data was partially loaded from the database (due to the retrieval task overlapping multiple ticks
  and the block entities being committed in several batches as a result). This, in turn, led to a situation
  where the client would load part of a world segment and never reload it until unsubscribing and resubscribing
  again.
* At this point the world segment block data generators and loaders appear stable and performant.
* Unload entities in the client that move to a world segment that the client is not currently subscribed to,
  since the client will receive no further synchronization updates for that entity until it resubscribes to a
  (possibly different) world segment where that entity exists - at which point it will be synchronized in full
  again.
* Proper player and account logout on disconnect (untested).

#### 25 January 2024

* Fix race condition in `StructBuffer<T>` (and `BaseComponentCollection<T>`) caused by a mismatched lock
  target between modification and enumeration.
* Update `EntityManager` to also publish a C# `event` that is invoked when entity/component processing begins.
* Change the set of component events tracked in `BlockAnimatedSpriteCache` to avoid large amounts of redundant
  processing when block entities are added, modified, and removed.
* Expand the unsubscribe distance for world segments to one segment greater than the subscribe distance. This
  ensures there is a full segment between the player and the subscribe point after an unsubscribe (and vice
  versa) so the player can't rapidly subscribe and unsubscribe by straddling the line between two segments.
  This comes at the cost of having to maintain synchronization events for a larger set of segments; however, the
  initial synchronization on subscribe is unaffected as the subscribe radius is unchanged. (In fact it may under
  some cases be better overall, since the client is effectively caching the bulk of the data for longer - so if
  the player backtracks, it may save a few extra bulk synchronizations on subscribe.)

#### 24 January 2024

* Fix various issues with `Octree` again. This time the logic for identifying a leaf node was incorrect,
  ultimately leading to incomplete range query results - a child node could be left unpopulated because
  the octree incorrectly treated its parent as a leaf node, and if that empty child node was completely
  interior to the search range but the parent was not, any entities belonging to that node would not be
  returned. This was responsible for at least some of the issues seen in testing where
  `WorldSegmentBlockDataGenerator` was missing large numbers of blocks depending on the order in which
  world segments were loaded from the database.
* Add some additional unit tests for `Octree`.
* Start work on unloading world segments on unsubscribe in the client. This broke a lot of things and/or
  revealed many strange bugs, so working through these now.

#### 23 January 2024

* Fix issue in `WorldSegmentBlockDataGenerator` where non-air default blocks were never being selected
  as the algorithm was counting down from the maximum number of air blocks in an entire world segment,
  not just the depth plane in question.
* Traced the issue with occasional gaps in loaded block entities to the server-side `Octree` instance,
  or possibly the indexer that populates it. The missing blocks when the bug occurs appear exactly at the
  edge of the octree's root node. Need to investigate further.

#### 22 January 2024

* Fix issue in `Octree` where moving entities were not properly removed from nodes they no longer overlap,
  leading to the incorrect rendering of duplicate entities whenever the player moves across an (arbitrary)
  boundary between two octree nodes.

#### 21 January 2024

* Fix rare race condition with tile sprite caching.
* Fix issue where component modify events were not being sent.
* Movement is sort of working now. The velocity is way too high and there appears to be some sort of issue where
  the client is rendering the player in multiple locations simultaneously with full animation - is this a caching
  issue in the renderer or are we incorrectly generating extra player entities through a sync bug? Need to investigate
  further.
* Fix velocity, I used the wrong units when I set the default movement velocity. Still need to figure out the caching
  issue and forward the authoritative updates from the server.
* Add connection mapper for movement events.
* Send authoritative position and velocity updates from server to client.

#### 20 January 2024

* Finish initial implementation of movement, although there's an issue where it doesn't really work and
  needs testing.

#### 19 January 2024

* Add `MovingComponentIndexer` to track entities which are currently in motion so that they can be
  iterated over directly by the `Movement` system.
* Update `InputSystem` to periodically generate movement requests while direction keys are pressed.
* Fix issue where disconnecting a client from the server would cause the server to crash.

#### 18 January 2024

* Fix issue where multiple renders from the same index buffer did not use the correct buffer offset, leading
  to incorrect output after the first draw call.
* Remove the old `Movement` system to be replaced by a new implementation. The original implementation is from
  an old test of the ECS layer and does not map well onto the networking model that was designed later.
* Begin implementing a new `Movement` system with better reconciliation between client and server.

#### 17 January 2024

* Load Drawable component from database.
* Fix minor issue in `BaseTagCollection` that was affecting Drawables.
* Move `AnimatedSprite` component to `EngineCore` similar to `Drawable`, along with the necessary persistence
  changes.
* Begin work on getting non-block animated sprites to render correctly.

#### 16 January 2024

* Fix issue in `SqliteListPlayersQuery` that incorrectly returned no results even when players were
  associated to an account.
* Fix issue in `PlayerManagementClient` where the select player call didn't work.
* Convert `Drawable` from a `BaseComponentCollection` to a `BaseTagCollection`. Refactor to move this component
  into `EngineCore` so that drawable entities can be tagged at the server with this information communicated
  through non-block entity synchronization. Note that block entities continue to be tagged as drawable in the
  *client* rather than in the server.
* Update entity synchronization to populate `Drawable` component in entity definitions generated at the server.
* Add `Drawable` component to persistence.

#### 15 January 2024

* Fix race condition in `BaseComponentCollection<T>` when multiple workers try to enqueue
  creates/updates/removes/unloads simultaneously (e.g. when multiple world segments are loaded in parallel
  by `Persistence`).
* Refactor the component event APIs to distinguish between add/load similar to remove/unload. This is needed
  to allow the persistence state trackers to ignore loaded components and avoid attempting to duplicate
  components in the database.

#### 14 January 2024

* Fix issue in `PlayerComponentEventFilter` that caused event triggers for new player characters to be missed.
* Fix various issues with world segment subscriptions and entity synchronization.
* Add custom resolver/formatter for MsgPack to handle `Vector3`.
* Saw the player character entity synchronized from server to client. More testing is needed, but this is
  fantastic progress.
* Fix issue where players were immediately unsubscribed from world segments as soon as they subscribed.
* Fix issue where world segment subscribe events were not sent for new players because they were routed into the
  outbound pipeline before the player select event could be processed (since the entity is created for a new player
  before the select event is sent, leading to a race condition).
* Fix issue where the client interpreted subscribe as unsubscribe, and vice versa.
* Add a one tick grace period before reclaiming component memory after a remove or unload in
  `BaseComponentCollection<T>`. This way the value before removal can be referenced in an event handler immediately
  following the component removal.
* Wire up `WorldSegmentBlockDataManager` to support lazy regeneration of world segment block data. It actively tracks
  which segments have expired based on component updates, then regenerates the segment on the fly if a request is
  received and the segment is flagged for update. This way most updates do not trigger a full regeneration of the
  segment data, only the update closest to the next request.
* Fix some minor issues in `WorldSegmentBlockDataGenerator`. Successfully (sort of) sent some block data to the
  client where it was rendered to the screen.
* Fix race condition in `EntityTable`.

#### 13 January 2024

* Fix several errors with client startup that were introduced by the null checks.
* Fix issue with reversed logic for entity load following player selection.

#### 12 January 2024

* Fix race condition in entity table where entity creation could cross a tick boundary.
* Noticed something odd where when the `Sleep()` calls in the various threads are relaxed, some sort
  of deadlock occurs when a debugger is attached. It does not appear when the threads yield periodically,
  or when the `Sleep()` calls are disabled but no debugger is attached. Not sure what this is yet, but
  documenting it here in case it reappears in other cases.

#### 11 January 2024

* Fix minor issues preventing the server from starting after a long list of untested changes.

#### 10 January 2024

* Add `NonBlockWorldSegmentIndexer` for tracking which entities are positioned in each active world segment.
* Add `EntityHierarchyIndexer` to track the hierarchy of parent-child relationships among the loaded entities.
* These two indexers are the building blocks for the non-block entity synchronization process.
* Add restriction to the synchronization documentation in `networking.md` that any child entities of block entities
  will not be synchronized to the client. These entities should be considered as hidden and can be used for
  storing private data on the server. For example, you could have a child entity on a rock-type block indicating
  what type of resource will be dropped if the rock is mined; this would be hidden from the client so that the
  player can't "cheat" at mining by inspecting the client memory to determine the location of rare resources.
* Implement entity definition generation on the server side.

#### 09 January 2024

* Fix bug where world segment load events were dropped by the Persistence system.
* Begin implementation of world segment synchronization from server to client.

#### 08 January 2024

* Add event details validation to all inbound network pipelines. If a validator type is not found for a
  received event ID, the event will be rejected and an error logged.

#### 03 January 2024

* Add `EntitySynchronizationSystem` for processing non-block entity synchronization updates from the server.

#### 02 January 2024

* Finish up null checking updates for all projects.
* Add `EntityTable` for tracking the existence of entities above the component level.
* Update `IEntityBuilder` API to support modification of existing entities in order to
  support entity synchronization.

#### 01 January 2024

* Merge `Performance` into `EngineCore` to clean things up; fix nullability warnings.
* Clean up null checking warnings in `NetworkCore`, `ServerCore`, `Persistence`, `Accounts`,
  `ServerNetwork`, `SovereignServer`.
* Flag all reference typed members in JSON-serialized types as nullable since `System.Text.Json`
  currently doesn't enforce nullability rules on deserialization. This way we can get compile-time
  errors if we forget to check for missing fields in untrusted inputs.

## 2023

### December

#### 31 December 2023

* Treat all warnings as errors. Thought I had already enabled this but apparently not.
  Together with the nullability warnings, this should help catch a lot of potential bugs
  at compile time instead of at runtime.
* Enable nullability warnings and fix those warnings for `EngineUtil`. These are extremely
  valuable, so I'm going to do the same for the other class libraries moving forward.

#### 29 December 2023

* Begin implementation of entity synchronization starting with an event layer over the
  existing `IEntityBuilder` API.
* Lots of minor fixes around nullability checks in `EngineCore`.

#### 17 December 2023

* Send event in client when the player ID is established.
* Update `SelectPlayerRestService` to use a URL parameter instead of a JSON request. This is a
  more RESTful design since the player entity "object" is associated with a unique URI; other APIs
  such as player deletion then follow naturally.
* Attach camera to player entity once the player entity is selected.

#### 16 December 2023

* Bring back the code for loading world segment block data in the client that was accidentally
  deleted during the `WorldManagement` refactoring; refactor this into a REST client class
  with the others in the client network infrastructure namespace.
* Update `AccountLoginTracker` and friends to maintain an additional mapping from player
  entity ID back to the associated connection ID. This will allow the outbound network pipeline
  in the server to correctly route outbound network events when the context is player-dependent.
* Allow world segment subscribe/unsubscribe events to pass from server to client. These are
  sent reliable-unordered as the client needs to know when it has been subscribed to a world
  segment, but does not care about the relative ordering of subscriptions.
* Load world segment block data in the client when a subscribe event is received for that world
  segment.

#### 13 December 2023

* Heavily refactor `WorldManagement` even further to accomodate the pub-sub approach to
  world synchronization between client and server. Most things are wired up now, aside from
  a mechanism to deactivate segments with no subscribers (which is straightforward to do with
  the new architecture).

#### 10 December 2023

* Rework connection sequence to simplify player entity load process.
* Load player character entity tree from database after selection.
* Add component event filter for filtering down to player position component events.

#### 9 December 2023

* Implement `SelectPlayerRestService` for selecting a player character that already exists.
* Adjust connection sequence diagram to show correct position of event server connection.
* Refactor `WorldManagement` into a server-only system within ServerCore, not as its own
  class library. With the new pub-sub design for players and world segments, it no longer
  makes sense to replicate this logic in both applications. Instead, the server will govern
  all world management functions, and the client will react to a (yet to be created) set
  of events which drive the synchronization of state.

#### 6 December 2023

* Update sequence diagram for login/player select process to add detail about when entity
  trees are loaded along with allocation of responsibilities to systems.

#### 5 December 2023

* Update entity retrieval queries to pull entire entity trees at once via a recursive query.
  For the range retrieval query, also added logic to the query to exclude player character
  entities as these are never activated by a range retrieval, only by an ID retrieval.
* Document the current state of the login/player select process in a sequence diagram
  in `networking.md`. This is a starting point for designing the rest of the behavior.

#### 3 December 2023

* Add `Parent` component for mapping entities to parent entities, enabling entities to
  be organized as a tree.
* Update database schema to include table for the `Parent` component.
* Set up persistence code for synchronizing `Parent` component with database.

### November

#### 19 November 2023

* Begin testing the last batch of changes.
* Fix issue with SQL in `SqliteListPlayersQuery`.

#### 5 November 2023

* Fix warnings in unit tests that were introduced with latest dependency updates.
* Implement `ListPlayersRestService` for getting the list of player characters associated
  with the currently logged in account.
* Add support for listing players in an account to `PlayerManagementClient`.
* Get the unit tests running again.
* Update `TestContentSystem` to automatically create a debug player when first connected.
  This is just temporary for debugging; it will eventually be removed and replaced by the
  full player character selection/creation system.

#### 1 November 2023

* Fix random issues that were introduced to the persistence system.
* Upgrade to Castle Windsor 6.0.0 for dependency injection.

### October

#### 31 October 2023

* Add database query for retrieving player character entities associated
  with a given account.

#### 26 October 2023

* Add safety checks to REST client calls to make sure that the response size
  is appropriately bounded.

#### 24 October 2023

* Fix various issues with REST services.
* Create `PlayerManagementClient` as an interface to the player management
  REST services. For now only player creation is supported, other APIs
  will be added soon.

#### 15 October 2023

* Switch to GPLv3 license for all source code. Over time my feelings about using the MIT
  license for a game engine like this have changed based on what I've observed in the
  broader open source community. I want to make sure that the development of this engine
  remains open source in the future - open source is ultimately a good thing for the
  community, and I don't want to see the proliferation of closed source forks of this
  codebase. I stopped short of adopting AGPL for this project, despite its client-server
  nature, as I think there is still value in having secret closed-source modifications to
  specific instances of the server where the change is meant to implement a game feature
  not possible with scripting and where releasing the changes to the players would spoil
  a gameplay secret.

### September

#### 29 September 2023

* Simplify query for getting account associated with player.

### August

#### 31 August 2023

* Include player entity ID in response to successful character creation requests.

#### 30 August 2023

* Add public API to Accounts for checking if a player character belongs to a given account.

#### 29 August 2023

* Add persistence support for the `Account` component.
* Default new player characters to position (0, 0, 0). In the future we should make this configurable
  at the server level.
* Check for duplicate player names in the database before creating a new player character.
* Send response on normal success or failure during player creation.
* Replace logged in flag in `AccountLoginTracker` with a state enum. The login state is being expanded to include
  an intermediate step after authentication but before a player character is selected. Still need to update logic
  elsewhere - for example, the event server connection needs to be deferred until after player selection is complete.
* When a new player character is created, automatically select that character and proceed with login.
* Add `IGetAccountForPlayerQuery` for retrieving the account associated with a given player character.

#### 28 August 2023

* Add an `Account` component to associate player character entities to a specific account. Still need to do
  persistence for this component; this part is a work in progress.

#### 27 August 2023

* More work on player creation.

#### 26 August 2023

* Automatically enable authenticated REST requests from the client once a successful login has occurred.
* Add logging for failed authentication attempts in REST API calls. It should be possible to set up a regex
  in fail2ban based on this.
* Switch `WorldSegmentRestService` to an authenticated REST service - there's no need to be pulling down world block
  segment data if you're not logged in. In the future, I plan to make this more restrictive by refusing requests for
  blocks that are too far from the player's current position.
* Start implementing the `CreatePlayerRestService` stub. Basically just request handling, need to make the necessary
  persistence updates before going further.
* Add `Name` component for naming entities, along with the associated persistence code.

#### 25 August 2023

* Added `AuthenticatedRestService` to make it easy to create REST endpoints that require login credentials.
  Along with this, added a REST API key that is uniquely generated for each successful login. Authenticated
  REST API endpoints use HTTP Basic authentication with the account ID as username and the API key as password,
  so these should still go over a TLS encrypted connection to be safe. Still need to add the client-side implementation
  for interacting with the authenticated endpoints.

#### 23 August 2023

* Persist the `PlayerCharacter` tag in the database.
* Define REST interfaces for player character operations.

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

