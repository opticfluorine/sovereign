# World Structure

## Game World

The world exists in a three-dimensional coordinate system where the
x coordinate runs west to east, the y coordinate runs south to north, and
the z coordinate runs bottom to top (in depth). The domain is comprised of
a three-dimensional grid of blocks - cubes of unit length - each of which
is assigned (at a minimum) a material identifier and a material modifier.
As each block is tracked within the engine as an independent entity,
additional information can be associated with a block by linking the
corresponding component.

## Materials

A material defines the base type of a block. The material of a block
is specified by a pair of unsigned 32-bit integers, the material ID and the
material modifier. The material ID defines the specific type of material
(e.g. grass, sand, water). The material modifier indicates which particular
appearance of the material the block will take.

Each material specifies three tile sprites: the top face, the side face,
and an alternate top face to be shown if a face is obscured. The top face
is the horizontal surface in the xy plane, and the side face is the vertical
surface in the xz plane.

## Blocks

The world is constructed of a collection of blocks as described above.
A block is a unit cube (in position units) with side lengths equal to the
tile dimension (in pixels) when rendered. Material blocks must be of unit
length, but other entities may have arbitrary dimensions. All entities
(material blocks and otherwise) are positioned by the lower-bottom-left corner
of the unit length block whose base is centered on the bottom plane of
the entity. This choice of coordinates allows us to specify the three-dimensional
range of a block as starting with its position coordinate {math}`(x, y, z)` and
ending at {math}`(x+1, y+1, z+1)`.

The game world is composed of a non-overlapping set of blocks positioned at integer
coordinates on a three-dimensional grid. By non-overlapping, we mean that each coordinate
(x,y,z) (where x, y, and z are signed integers) is the position of zero or one blocks.
The absence of a block at a coordinate implies that the special "air" block exists at
that position. The "air" block is not stored in memory as a real block; it is only used
to imply the absence of a block which enables certain optimizations when transferring
world data from server to client.

## World Segments

The game world is normally handled in small three-dimensional regions known as
*world segments*. Each world segment is a cube having a length of 32 blocks, therefore
containing up to 32768 blocks per world segment. The game state is loaded, unloaded, and
synchronized one world segment at a time.

