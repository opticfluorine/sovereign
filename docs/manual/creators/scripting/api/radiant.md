# Radiant Module

The `Radiant` module provides APIs for evaluating radiant scalar fields.

A radiant field is a per-category scalar field summed from the `RadiantData`
components of nearby entities. Each `RadiantData` component contributes a
value determined by its function and parameters; the `Linear` function
evaluates `Param0 * d + Param1`, where `d` is the distance between the
queried position and the contributing entity's position. The field value at
a position is the plain sum of the contributions of all matching entries in
the world segments near the queried position (see `RadiantOptions` in the
server configuration to control the search range).

Radiant entities are assumed to be static: a contributing entity's position
is resolved when its `RadiantData` component is added or modified, and later
movement does not change its contribution until the component is re-applied.
An entity's position may be provided directly by its own `Kinematics`
component or, for unpositioned child entities, inherited from its nearest
positioned ancestor.

## GetValue(category, position)

### Definition

```{eval-rst}
.. lua:function:: Radiant.GetValue(category, position)

   Evaluates the radiant scalar field of the given category at the queried
   position.

   :param category: Radiant field category, e.g. ``RadiantCategory.MineralDrop``.
   :type category: integer
   :param position: Queried position in world coordinates.
   :type position: table
   :return: Sum of the field contributions of all radiant entities in the
            scanned segments (0.0 if there are none).
   :rtype: number
```

### Example

```{code-block} lua
:caption: Evaluating the mineral drop radiant field at a position.
:emphasize-lines: 1,2
local value = Radiant.GetValue(RadiantCategory.MineralDrop, {X = 3.0, Y = 4.0, Z = 0.0})
Util.LogInfo("Mineral drop field value: " .. tostring(value))
```
