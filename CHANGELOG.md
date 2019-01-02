# Sovereign Engine Changelog

## 2019

### January

#### 01 January 2019

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

