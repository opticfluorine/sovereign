Tiling Engine
=============

The tiling engine is responsible for selecting the correct sprite to be drawn
for each tile sprite being rendered.

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

