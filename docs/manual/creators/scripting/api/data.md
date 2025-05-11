# data Module

:::{contents}
:local:
:depth: 2
:::

The `data` module provides support for reading and writing non-component data such as global key-value pairs.

## Global Key-Value Data

:::{caution}
Global key-value data is intended for long-term persisted storage of data,
not for rapid updates and frequent computations. There are performance
considerations that must be taken into account when using global key-value
data. Try to minimize the amount of global key-value data access (both read
and write) in your scripts.
:::

Sovereign provides a global key-value data store that can be used for
long-term storage of arbitrary data. This data store is persisted to the
database and is also kept in memory by the server; as such, the global
store supports read and write operations without any (immediate) round trip
to the database and back.

All global keys and values are stored internally as strings. When writing
to the global store, the conversion of values to string format is handled
automatically. When reading from the global store, the calling script is
responsible for converting the string to the appropriate type. Many
conversions (including conversion from string to number) can be done
implicitly; others (such as booleans) require explicit conversion.

The global key-value data store is exposed to scripts via the
`data.global` table. This table behaves mostly like any other Lua table
with the exception that writing to a key is an asynchronous operation; as
such, the table may not immediately reflect the updated value after it
is written to the appropriate field. This is because behind the scenes the
write operation sends an asynchronous event to the `DataSystem` to make
the update.

The `data.RemoveGlobal` method is provided for removing a global key-value
pair. As with writes to `data.global`, this operation is asynchronous and
the result may not be immediately reflected in the `data.global` table.

### Reading Global Key-Value Pairs

A key-value pair may be read from the global store by indexing into the
`data.global` table with the key. This returns the string representation of
the value if the key-value pair is present, or `nil` if the key-value pair
is not present.

#### Example

```{code-block} lua
:caption: Reading a global key-value pair.
:emphasize-lines: 1
local MyValue = data.global["MyValue"]  -- MyValue is string if key exists, nil otherwise.
```

### Writing Global Key-Value Pairs

A key-value pair may be created or updated in the global store by assigning
a value to `data.global[key]`. Fields in `data.global` may be assigned
strings, integers, numbers, or booleans; all input values will be converted
to strings. Note that this is an asynchronous operation, and there may be
a delay before the new value is reflected in subsequent reads.

:::{note}
Keys that start with two underscores (`__`) are considered special reserved 
values managed by the engine. Any attempt to modify or remove one of these
values from a script will fail with an error.
:::

#### Example

```{code-block} lua
:caption: Writing a global key-value pair.
:emphasize-lines: 1
data.global["MyValue"] = 12345
print(data.global["MyValue"])   -- warning: may not show the latest value
```

### RemoveGlobal(key)

```{eval-rst}
.. lua:function:: data.RemoveGlobal(key)

    Removes the given key from the global key-value store, or does nothing
    if the key is not currently in the global key-value store.

    :param key: Global key.
    :type key: string
```

#### Example

```{code-block} lua
:caption: Using `RemoveGlobal` to remove a global key-value pair.
:emphasise-lines: 1
data.RemoveGlobal("MyKey")  -- removes "MyKey" from the global key-value store
```