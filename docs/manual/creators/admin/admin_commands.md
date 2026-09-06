# Admin Chat Commands

Players with the Admin role have access to a number of administrative chat commands
for server and world management. This section describes the available admin
commands.

## Role Management

### /addadmin

**Usage:** `/addadmin [player_name]`

**Parameters:**
* `player_name`: Name of the player to be granted admin privileges.

Grants the Admin role to the given player. If the player already has the Admin role,
no change will be made.

### /removeadmin

**Usage:** `/removeadmin [player_name]`

**Parameters:**
* `player_name`: Name of the player to have admin privileges revoked.

Revokes the Admin role from the given player. If the player does not already have the
Admin role, no change will be made. This command cannot be used to revoke the Admin
role from yourself.

(chat-admin-scripting)=
## Scripting

### /listscripts

**Usage:** `/listscripts`

Lists all currently loaded scripts.

### /loadnewscripts

**Usage:** `/loadnewscripts`

Loads any scripts that are not currently loaded, but does not reload any currently
loaded scripts. Useful for when new scripts need to be loaded but you don't want to
reset the state of currently running scripts.

### /reloadscript

**Usage:** `/reloadscript [script_name]`

**Parameters:**
* `script_name`: Name of the script to reload.

Reloads the specified script.

### /reloadscripts

**Usage:** `/reloadallscripts`

Reloads all scripts, including any scripts which are not currently loaded.

### /reloadentity

**Usage:** `/reloadentity [entity_id]`

**Parameters:**
* `entity_id`: The hex-encoded entity ID of the entity to reload. If the ID is given with
  no more than 12 hex digits, it is interpreted as an offset from the first persisted entity
  ID `7fff000000000000` (e.g. `0` for entity ID `7fff000000000000`). Longer values are
  interpreted as absolute entity IDs.

Soft-reloads the given entity without unloading it. The entity's `OnEntityUnloaded` callback
is invoked, followed by its `OnEntityLoaded` callback. The entity remains in memory, no
entity unload or load events are fired, and the `OnEntityAdded`/`OnEntityRemoved` callbacks
are not called. Only entities that are currently loaded can be reloaded; template entities
and block entities are not eligible.

### /reloadtemplate

**Usage:** `/reloadtemplate [template_rel_id]`

**Parameters:**
* `template_rel_id`: The relative entity ID of the template entity (e.g. `0` for entity ID
  `7ffe000000000000`), given as a decimal integer.

Soft-reloads all currently loaded entities that have the given template. As with
`/reloadentity`, each affected entity's `OnEntityUnloaded` callback is invoked followed by
its `OnEntityLoaded` callback, and the entities remain in memory. The template entity itself
is not reloaded, since template entities do not receive entity lifecycle callbacks. Callback
hooks defined on the template entity are used for entities that do not define their own.

(chat-admin-key-value-data)=
## Key-Value Data

The key-value data stores described in the [Data Module](../scripting/api/data.md)
can also be inspected and modified with the following admin commands. These
commands follow the same rules as the scripting API: keys beginning with two
underscores (`__`) are reserved for internal use by the engine and are
read-only, and entity key-value stores are only available for loaded
non-block entities that are not template entities.

### /getvalue

**Usage:** `/getvalue key`

**Parameters:**
* `key`: Key to look up in the global key-value store.

Prints the value of the given key-value pair from the global key-value store, or an
error message if the key does not exist.

### /setvalue

**Usage:** `/setvalue key [value]`

**Parameters:**
* `key`: Key of the global key-value pair to be modified or removed.
* `value`: New value for the key. If omitted, the key is deleted.

Sets the value of the given key-value pair in the global key-value store, creating
the key if it does not already exist. If the value is omitted, the key is deleted
from the global key-value store.

### /getentityvalue

**Usage:** `/getentityvalue entity_id key`

**Parameters:**
* `entity_id`: The hex-encoded entity ID of the entity, using the same convention as
  `/reloadentity`.
* `key`: Key to look up in the entity's key-value store.

Prints the value of the given key-value pair from the given entity's key-value store.
If the entity does not define the key itself but its template entity does, the
inherited value is printed along with a note identifying the template entity from
which it is inherited.

### /setentityvalue

**Usage:** `/setentityvalue entity_id key [value]`

**Parameters:**
* `entity_id`: The hex-encoded entity ID of the entity, using the same convention as
  `/reloadentity`.
* `key`: Key of the entity key-value pair to be modified or removed.
* `value`: New value for the key. If omitted, the key is deleted.

Sets the value of the given key-value pair on the given entity, creating the key if
it does not already exist. The value is always written to the entity itself and never
to its template entity. If the value is omitted, the key is deleted from the entity;
if the entity's template entity defines the same key, the template's value will then
be inherited again.

## World Editing

### /addblock

**Usage:** `/addblock x y z (template_rel_id | template_name)`

**Parameters:**
* `x`: X coordinate of the new block.
* `y`: Y coordinate of the new block.
* `z`: Z coordinate of the new block.
* `template_rel_id`: The relative entity ID of the block template entity to use for
  the new block (e.g. `0` for entity ID `7ffe000000000000`).
* `template_name`: The name of the block template entity to use for the new block
  (case insensitive).

Adds a new block at the given position. If there is already a block at the given
position, no change will be made. The block template entity for the new block may
be specified by its entity ID or its name.

:::{note}
Since block template entity names are not unique, there may be conflicts when
adding a block by name. If the block has an unexpected type, remove it and add it
again using its entity ID instead of its name.
:::

### /removeblock

**Usage:** `/removeblock x y z`


**Parameters:**
* `x`: X coordinate of the block to be removed.
* `y`: Y coordinate of the block to be removed.
* `z`: Z coordinate of the block to be removed.

Removes the block at the given position. If there is not a block at the given
position, no change will be made.

:::{warning}
This action cannot be undone. If there are any child entities attached to the
removed block, they will be permanently lost.
:::
