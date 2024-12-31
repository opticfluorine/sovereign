# Getting Started with Scripting

Sovereign Engine's scripting engine enables creators to extend and customize
game behavior on the server by writing Lua scripts. Some of the key features
of the scripting engnine include:

* **Full control over in-game entities.** The scripting engine provides a
  deep level of access to Sovereign's Entity-Component-System (ECS) framework,
  including the ability to get and set component values, create entities,
  and delete entities dynamically.
* **Event-driven.** The scripting engine is based around the use of event-driven
  callbacks. Each script can register any number of callback functions for
  specific events in order to react to only those events. This allows for
  simpler and cleaner scripts while also promoting overall scalability.
* **Reloadable at runtime.** Scripts can be loaded, reloaded, and unloaded at
  runtime without restarting the server, allowing content to be added and
  updated on a live server without restarting.

## How Scripts Work

### Script Files

Server-side Lua scripts are kept in the server's `Data/Scripts` directory. Any
file in this directory (or its subdirectories) that has a `.lua` extension will
be executed by the server at startup. The *name* of the script is the relative
path of the script file (relative to the `Data/Scripts` directory) without the
`.lua` extension.

:::{warning}
Ensure that the `Data/Scripts` directory has appropriate permissions on
production servers. The server executes any Lua script found in this directory,
so loose permissions (i.e. allowing untrusted users to write to this directory)
could lead to untrusted code execution.
:::

It is strongly recommend to organize your scripts into one or more subdirectories
under the `Data/Scripts` directory. Most projects will use a single subdirectory
(e.g. `Data/Scripts/ProjectName`, where `ProjectName` is the name of the project),
and possibly a number of subdirectories below this level to keep scripts organized.
The special subdirectory `Data/Scripts/Sovereign` is reserved for official
scripts that come with Sovereign Engine.

### Loading and Running Scripts

All scripts are automatically loaded when the server starts. There are also
several chat commands available to admins for managing scripts; refer to
[Scripting Admin Commands](#chat-admin-scripting) for details.

### Event Callbacks

All scripts are based on callback functions which are triggered by various
server-side events. This allows scripts to remain idle except for when events
of interest occur. Callback functions must be registered with the scripting
engine before they can be used; the [scripting module](#script-scripting) 
contains functions for this purpose.

### Working With Entities and Components

Scripts will commonly interact with the entities that make up the game world
along with their components. The [entities module](#script-entities) provides
functions for creating and removing entities, while the
[components module](#script-components) provides functions for getting and
setting components on entities.

## Writing Your First Script

Take a look at the [tutorials](#script-tutorials) for ideas and step-by-step
guides for writing your first script!

## Need Help?

Support for scripting is available on the Sovereign Engine Discord server
located [here](https://discord.gg/7nPmhJ8XtX).

## Additional Resources

* [Lua 5.4 Reference Manual](https://lua.org/manual/5.4/)
* [Programmming in Lua, First Edition](https://lua.org/pil/contents.html)
