# Data Module

:::{contents}
:local:
:depth: 2
:::

The `Data` module provides support for reading and writing non-component data such as global key-value pairs.

(script-data-keyvaluedata)=
## Key-Value Data

Sovereign provides key-value data stores that can be used for
long-term storage of arbitrary data. This data store is persisted to the
database and is also kept in memory by the server; as such, the key-value
store supports read and write operations without any (immediate) round trip
to the database and back. Sovereign provides a global key-value store as well
as entity-scoped key-value stores. Entity-scoped key-value stores are only
available for non-block entities.

All keys and values are stored internally as strings. When writing
to the key-value store, the conversion of values to string format is handled
automatically. When reading from the global store, the calling script is
responsible for converting the string to the appropriate type. Many
conversions (including conversion from string to number) can be done
implicitly; others (such as booleans) require explicit conversion.

### Accessing Key-Value Stores

The global key-value store is provided to every script as the `Data.global` table.

Entity-scoped key-value stores may be retrieved using the `Data.GetEntityData(entityId)` function.

```{eval-rst}
.. lua:function:: Data.GetEntityData(entityId)

   Gets the key-value store associated with the given entity.
   
   :param entityId: Entity ID.
   :type entityId: lightuserdata

   :return: Key-value store for the requested entity.
   :rtype: table
```

### Reading Global Key-Value Pairs

A key-value pair may be read from the key-value store by indexing into the
appropriate table with the key. This returns the string representation of
the value if the key-value pair is present, or `nil` if the key-value pair
is not present. 

For entity key-value stores, if the key is not found, the key
for the template entity (if any) is also checked; this allows entities to inherit
key-value pairs held by their template entities.

#### Example

```{code-block} lua
:caption: Reading key-value pairs.
:emphasize-lines: 1,4
local MyValue = Data.Global["MyKey"]              -- MyValue is string if key exists, nil otherwise.
-- ...
local MyEntityData = Data.GetEntityData(entityId)
local MyEntityValue = MyEntityData["MyEntityKey"] -- MyEntityValue is string if key exists for entity or its template, nil otherwise.
```

### Writing Key-Value Pairs

A key-value pair may be created or updated in the key-value store by assigning
a value to the appropriate table. Fields in the key-value store may be assigned
strings, integers, numbers, or booleans; all input values will be converted
to strings.

For entity key-value stores, this will only ever modify the key-value pair for the
entity itself. Unlike with reading values, writing values will not affect the
template entity.

:::{note}
Keys that start with two underscores (`__`) are considered special reserved 
values managed by the engine. Any attempt to modify or remove one of these
values from a script will fail with an error.
:::

#### Example

```{code-block} lua
:caption: Writing key-value pairs.
:emphasize-lines: 1,4
Data.Global["MyValue"] = 12345
-- ...
local MyEntityData = Data.GetEntityData(entityId)
MyEntityData["MyEntityKey"] = 3.14159
```

### Deleting Key-Value Pairs

To delete a key-value pair, assign the value `nil` to the key.

#### Example

```{code-block} lua
:caption: Deleting key-value pairs.
:emphasize-lines: 1
Data.Global["MyKey"] = nil         -- removes "MyKey" from the global key-value store
-- ...
local MyEntityData = Data.GetEntityData(entityId)
MyEntityData["MyEntityKey"] = nil  -- removes "MyEntityKey" from the entity key-value store
```

(script-data-segmentflags)=
## Segment Flags

World segment flags are bitwise flags that apply to an entire world segment, such as whether
player-versus-player combat is allowed. The flag constants are provided in the
`WorldSegmentFlag` table (see {ref}`script-constants-worldsegmentflag`); for example,
`WorldSegmentFlag.Pvp` is the player-versus-player flag.

The `Data` module provides functions to read and write the flags for the world segment
containing a given world position, or for a specific segment index or inclusive box of
segment indices.

```{eval-rst}
.. lua:function:: Data.GetSegmentFlags(x, y, z)

   Gets the flags for the world segment at the given segment index.

   :param x: Segment X index.
   :type x: integer
   :param y: Segment Y index.
   :type y: integer
   :param z: Segment Z index.
   :type z: integer

   :return: Bitwise OR of the segment's flags, or 0 if the segment has no flags.
   :rtype: integer

.. lua:function:: Data.GetSegmentFlagsForPosition(px, py, pz)

   Gets the flags for the world segment containing the given world position.

   :param px: World X position.
   :type px: number
   :param py: World Y position.
   :type py: number
   :param pz: World Z position.
   :type pz: number

   :return: Bitwise OR of the segment's flags, or 0 if the segment has no flags.
   :rtype: integer

.. lua:function:: Data.SetSegmentFlag(x, y, z, flag)

   Sets the given flag on the world segment at the given segment index.

   :param x: Segment X index.
   :type x: integer
   :param y: Segment Y index.
   :type y: integer
   :param z: Segment Z index.
   :type z: integer
   :param flag: Flag to set. Must be a non-zero combination of ``WorldSegmentFlag`` values.
   :type flag: integer

.. lua:function:: Data.ClearSegmentFlag(x, y, z, flag)

   Clears the given flag on the world segment at the given segment index.

   :param x: Segment X index.
   :type x: integer
   :param y: Segment Y index.
   :type y: integer
   :param z: Segment Z index.
   :type z: integer
   :param flag: Flag to clear. Must be a non-zero combination of ``WorldSegmentFlag`` values.
   :type flag: integer

.. lua:function:: Data.SetSegmentFlagForPosition(px, py, pz, flag)

   Sets the given flag on the world segment containing the given world position.

   :param px: World X position.
   :type px: number
   :param py: World Y position.
   :type py: number
   :param pz: World Z position.
   :type pz: number
   :param flag: Flag to set. Must be a non-zero combination of ``WorldSegmentFlag`` values.
   :type flag: integer

.. lua:function:: Data.ClearSegmentFlagForPosition(px, py, pz, flag)

   Clears the given flag on the world segment containing the given world position.

   :param px: World X position.
   :type px: number
   :param py: World Y position.
   :type py: number
   :param pz: World Z position.
   :type pz: number
   :param flag: Flag to clear. Must be a non-zero combination of ``WorldSegmentFlag`` values.
   :type flag: integer

.. lua:function:: Data.SetSegmentFlagInBox(x1, y1, z1, x2, y2, z2, flag)

   Sets the given flag on every world segment in the inclusive box bounded by the two corners.

   :param x1: Segment X index of the first corner.
   :type x1: integer
   :param y1: Segment Y index of the first corner.
   :type y1: integer
   :param z1: Segment Z index of the first corner.
   :type z1: integer
   :param x2: Segment X index of the second corner.
   :type x2: integer
   :param y2: Segment Y index of the second corner.
   :type y2: integer
   :param z2: Segment Z index of the second corner.
   :type z2: integer
   :param flag: Flag to set. Must be a non-zero combination of ``WorldSegmentFlag`` values.
   :type flag: integer

.. lua:function:: Data.ClearSegmentFlagInBox(x1, y1, z1, x2, y2, z2, flag)

   Clears the given flag on every world segment in the inclusive box bounded by the two corners.

   :param x1: Segment X index of the first corner.
   :type x1: integer
   :param y1: Segment Y index of the first corner.
   :type y1: integer
   :param z1: Segment Z index of the first corner.
   :type z1: integer
   :param x2: Segment X index of the second corner.
   :type x2: integer
   :param y2: Segment Y index of the second corner.
   :type y2: integer
   :param z2: Segment Z index of the second corner.
   :type z2: integer
   :param flag: Flag to clear. Must be a non-zero combination of ``WorldSegmentFlag`` values.
   :type flag: integer
```

The box corners are normalized per axis, so either corner may be given first. A box may
affect at most 65,536 segments; larger boxes are rejected.

:::{note}
Segment flag writes are asynchronous. Calling a `Set` or `Clear` function enqueues the
change and returns immediately; the write is applied when the persistence system processes
it. Reads reflect the live in-memory cache, so a script's own write is not visible to a
read until after the persistence system has processed it. Set and clear are applied as
read-modify-write OR / AND-NOT operations, so repeated calls are idempotent.
:::

#### Example

```{code-block} lua
:caption: Reading and writing segment flags.
-- Read the flags for the segment containing a world position.
local flags = Data.GetSegmentFlagsForPosition(position.X, position.Y, position.Z)
if flags & WorldSegmentFlag.Pvp == 0 then                     -- PVP is not enabled here
    Data.SetSegmentFlagForPosition(position.X, position.Y, position.Z, WorldSegmentFlag.Pvp)
end

-- Clear PVP over an inclusive box of segment indices.
Data.ClearSegmentFlagInBox(0, 0, 0, 3, 3, 3, WorldSegmentFlag.Pvp)
```
