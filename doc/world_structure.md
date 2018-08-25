World Structure
===============

Game World
----------

The game world is a collection of one or more "domains", where a domain is a
disconnected three-dimensional segment of the world. By "disconnected", we
mean that there is no trajectory in three-dimensional space by which an
object may transition from one domain to another. Note that this does not
preclude the existence of "portals" or other scripted gateways which move
objects from one domain to another.

Domains
-------

Each domain exists in its own three-dimensional coordinate system where the
x coordinate runs west to east, the y coordinate runs south to north, and
the z coordinate runs bottom to top (in depth). The domain is comprised of
a three-dimensional grid of blocks - cubes of unit length - each of which
is assigned (at a minimum) a material identifier and a material modifier.
As each domain block is tracked within the engine as an independent entity,
additional information can be associated with a block by linking the
corresponding component.

Materials
---------

A material defines the base type of a domain block. The material of a block
is specified by a pair of unsigned 32-bit integers, the material ID and the 
material modifier. The material ID defines the specific type of material
(e.g. grass, sand, water). The material modifier indicates which particular
appearance of the material the block will take.

Each material specifies two tile sprites, the top face and the side face.
The top face is the horizontal surface in the xy plane, and the side face is 
the vertical surface in the xz plane.

Tile Sprites
------------

A tile sprite is a set of sprites together with a selection rule that
determines the list of sprites to be drawn depending on the neighboring 
tile sprites. For example, a grass tile sprite might consist of an all-grass
sprite together with semi-transparent border/corner grass sprites to be
drawn over the main sprite of a neighboring tile sprite.

Tile sprites are modeled with a tree structure that encodes one or more
patterns (referred to as tile contexts) based on the neighboring tile sprites
in the four cardinal directions in the plane of the tile. Tile contexts take 
precedence in order from most specific to least specific. Each tile context
is associated with a list of zero or more sprites in the order in which they
are to be drawn.

