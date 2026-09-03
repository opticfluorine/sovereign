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

## Bans

Admins may ban accounts, preventing them from logging in to the server. Bans are
recorded at the account level: once an account is banned, no player character of that
account may log in. Banning a player who is currently online immediately disconnects
that player.

### /ban

**Usage:** `/ban [player_name] [duration_in_days]`

**Parameters:**
* `player_name`: Name of the player whose account is to be banned.
* `duration_in_days`: Optional duration of the ban in days. If omitted, the ban is
  permanent.

Bans the account of the given player. If the player is currently online, the player
will be disconnected immediately. Timed bans expire automatically once their duration
has elapsed; expired bans are removed from the list of active bans when it is next
queried.

### /unban

**Usage:** `/unban [player_name]`

**Parameters:**
* `player_name`: Name of the player whose account is to be unbanned.

Lifts all active bans on the account of the given player. If the account has no active
bans, no change will be made.

### /listbans

**Usage:** `/listbans`

Lists all active bans, including the banned account, the player name used when the ban
was created, the ban duration, and the admin who created the ban.

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
