# data Module

:::{contents}
:local:
:depth: 2
:::

The `data` module provides support for reading and writing non-component data such as global key-value pairs.

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

The global key-value store is provided to every script as the `data.global` table.

Entity-scoped key-value stores may be retrieved using the `data.GetEntityData(entityId)` function.

```{eval-rst}
.. lua:function:: data.GetEntityData(entityId)

   Gets the key-value store associated with the given entity.
   
   :param entityId: Entity ID.
   :type entityId: integer

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
local MyValue = data.global["MyKey"]              -- MyValue is string if key exists, nil otherwise.
-- ...
local MyEntityData = data.GetEntityData(entityId)
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
data.global["MyValue"] = 12345
-- ...
local MyEntityData = data.GetEntityData(entityId)
MyEntityData["MyEntityKey"] = 3.14159
```

### Deleting Key-Value Pairs

To delete a key-value pair, assign the value `nil` to the key.

#### Example

```{code-block} lua
:caption: Deleting key-value pairs.
:emphasize-lines: 1
data.global["MyKey"] = nil         -- removes "MyKey" from the global key-value store
-- ...
local MyEntityData = data.GetEntityData(entityId)
MyEntityData["MyEntityKey"] = nil  -- removes "MyEntityKey" from the entity key-value store
```
