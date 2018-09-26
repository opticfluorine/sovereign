# Sovereign Engine Changelog

## 2018

### September

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

