# Sovereign Engine Changelog

## 2018

### November

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

