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

Keys are always strings. Each value is stored with a type tag, and the supported
types and their Lua representations are:

| Type       | Lua type on read | Accepted on write                            |
| ---------- | ---------------- | -------------------------------------------- |
| `String`   | string           | string                                       |
| `Int`      | integer          | integral number                              |
| `Float`    | number           | non-integral number                          |
| `Bool`     | boolean          | boolean                                      |
| `Vector3`  | table            | table with numeric `X`, `Y`, and `Z` fields  |
| `EntityId` | lightuserdata    | lightuserdata entity handle                  |

Values are never converted between types: a value read back has the same type tag
it was written with. A number assigned to the store is stored as `Int` if it has no
fractional part, and as `Float` otherwise.

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

### Reading Key-Value Pairs

A key-value pair may be read from the key-value store by indexing into the
appropriate table with the key. This returns the value as the native Lua type
matching its tag (see the table above) if the key-value pair is present, or `nil`
if the key-value pair is not present. A typed read of a key that holds a different
type logs an error and returns `nil` rather than converting the value.

For entity key-value stores, if the key is not found, the key
for the template entity (if any) is also checked; this allows entities to inherit
key-value pairs held by their template entities.

#### Example

```{code-block} lua
:caption: Reading key-value pairs.
:emphasize-lines: 1,4
local MyValue = Data.Global["MyKey"]              -- native Lua value if key exists, nil otherwise.
-- ...
local MyEntityData = Data.GetEntityData(entityId)
local MyEntityValue = MyEntityData["MyEntityKey"] -- value if key exists for entity or its template, nil otherwise.
```

### Writing Key-Value Pairs

A key-value pair may be created or updated in the key-value store by assigning
a value to the appropriate table. The type tag is inferred from the Lua value
being assigned (see the table above); an integer number is stored as `Int`, any
other number as `Float`, and a table with numeric `X`, `Y`, and `Z` fields as
`Vector3`. Assigning any other value logs a script error and does not write.

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
local MyPosition = { X = 0.5, Y = 0.5, Z = 1.0 }
Data.Global["MyValue"] = 12345        -- stored as an Int
Data.Global["MyPosition"] = MyPosition -- stored as a Vector3
-- ...
local MyEntityData = Data.GetEntityData(entityId)
MyEntityData["MyEntityKey"] = 3.14159  -- stored as a Float
MyEntityData["MyTarget"] = targetEntity -- stored as an EntityId
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

:::{note}
Key-value writes update the live in-memory store when the assignment runs, so a
later read in the same script sees the new value. Persistence to the database
happens asynchronously, so a write is not durable until the persistence system
has processed it.
:::


(script-data-segmentflags)=
## Segment Flags

World segment flags are bitwise flags that apply to an entire world segment, such as whether
player-versus-player combat is allowed. The flag constants are provided in the
`WorldSegmentFlag` table (see {ref}`script-constants-worldsegmentflag`); for example,
`WorldSegmentFlag.Pvp` is the player-versus-player flag.

The `Data` module provides functions to read and write the flags for the world segment
containing a given world position, or for a specific segment index or inclusive box of
segment indices. Segment indices and world positions are both passed as `Vector3` values
(see {ref}`script-types-vector3`); segment indices must have integer components, while
positions may be fractional.

```{eval-rst}
.. lua:function:: Data.GetSegmentFlags(segment)

   Gets the flags for the world segment at the given segment index.

   :param segment: Segment index.
   :type segment: Vector3

   :return: Bitwise OR of the segment's flags, or 0 if the segment has no flags.
   :rtype: integer

.. lua:function:: Data.GetSegmentFlagsForPosition(position)

   Gets the flags for the world segment containing the given world position.

   :param position: World position.
   :type position: Vector3

   :return: Bitwise OR of the segment's flags, or 0 if the segment has no flags.
   :rtype: integer

.. lua:function:: Data.SetSegmentFlag(segment, flag)

   Sets the given flag on the world segment at the given segment index.

   :param segment: Segment index.
   :type segment: Vector3
   :param flag: Flag to set. Must be a non-zero combination of ``WorldSegmentFlag`` values.
   :type flag: integer

.. lua:function:: Data.ClearSegmentFlag(segment, flag)

   Clears the given flag on the world segment at the given segment index.

   :param segment: Segment index.
   :type segment: Vector3
   :param flag: Flag to clear. Must be a non-zero combination of ``WorldSegmentFlag`` values.
   :type flag: integer

.. lua:function:: Data.SetSegmentFlagForPosition(position, flag)

   Sets the given flag on the world segment containing the given world position.

   :param position: World position.
   :type position: Vector3
   :param flag: Flag to set. Must be a non-zero combination of ``WorldSegmentFlag`` values.
   :type flag: integer

.. lua:function:: Data.ClearSegmentFlagForPosition(position, flag)

   Clears the given flag on the world segment containing the given world position.

   :param position: World position.
   :type position: Vector3
   :param flag: Flag to clear. Must be a non-zero combination of ``WorldSegmentFlag`` values.
   :type flag: integer

.. lua:function:: Data.SetSegmentFlagInBox(corner1, corner2, flag)

   Sets the given flag on every world segment in the inclusive box bounded by the two corners.

   :param corner1: Segment index of the first corner.
   :type corner1: Vector3
   :param corner2: Segment index of the second corner.
   :type corner2: Vector3
   :param flag: Flag to set. Must be a non-zero combination of ``WorldSegmentFlag`` values.
   :type flag: integer

.. lua:function:: Data.ClearSegmentFlagInBox(corner1, corner2, flag)

   Clears the given flag on every world segment in the inclusive box bounded by the two corners.

   :param corner1: Segment index of the first corner.
   :type corner1: Vector3
   :param corner2: Segment index of the second corner.
   :type corner2: Vector3
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
local flags = Data.GetSegmentFlagsForPosition(position)
if bit.band(flags, WorldSegmentFlag.Pvp) == 0 then               -- PVP is not enabled here
    Data.SetSegmentFlagForPosition(position, WorldSegmentFlag.Pvp)
end

-- Clear PVP over an inclusive box of segment indices.
local corner1 = { X = 0, Y = 0, Z = 0 }
local corner2 = { X = 3, Y = 3, Z = 3 }
Data.ClearSegmentFlagInBox(corner1, corner2, WorldSegmentFlag.Pvp)
```

:::{note}
The scripting engine runs on LuaJIT, so the Lua 5.4 bitwise operators (``&``, ``|``, ``~``)
are not available. Use the built-in `bit` module (``bit.band``, ``bit.bor``, ``bit.bnot``)
instead, as shown above.
:::
