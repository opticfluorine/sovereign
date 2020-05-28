# Sovereign Engine Roadmap

This file is an incomplete list of future features planned.
Planned features are grouped by release, with earliest releases listed first.

All features shall be implemented following the accessibility guidelines
outlined in docs/accessibility.md.


## Milestone 1

### Infrastructure

#### Basic World Rendering

> :white_check_mark: Complete.

The client should be able to perform basic rendering of the game world as
described in docs/world_structure.md.

#### Client-Server Model

> :arrow_forward: In progress.

The client should defer to the server as the authority on the world state.

#### Client-Server Communication

> :arrow_forward: In progress.

The client and server should be able to communicate over the network. This
will be done by extending the existing event system to repeat certain events
over a network connection.

The initial release will not support client authentication.

#### Server Backend

> :white_check_mark: Complete.

The server should be able to use SQLite as the default backend database.

#### Client Authentication

The client should be able to authenticate with the server with a persistent
account for each player.

### Gameplay

#### Player Character Movement

The player should be able to control a character using the keyboard.


## Milestone 2

### Infrastructure

#### PostgreSQL Backend Support

The server should be able to optionally use a PostgreSQL database.

#### In-Game Chat

The server should support in-game chat. An optional web interface to the in-game
chat should be provided for accessibility purposes.

#### Server-Side Docker Support

The server should be (optionally) delivered as a Docker container so that it can
be run in a reproducible and isolated environment on Linux targets.

### Gameplay

#### Non-Player Characters

Non-player characters should be able to exist within the game world. NPCs
should be able to be either persistent or non-persistent.

#### Items and Inventory System

Items should be able to exist within the game world or in character inventories.


## Future Features Not Assigned to a Release

### Infrastructure

#### Client-Server Binding

The client should be able to manage a local bound copy of the server for 
single-player or multiplayer gameplay.

#### Server Scripting

A scripting engine should be integrated with the server. One option is to use NLua
to provide an integrated Lua scripting engine. Scripts will be loaded into a sandboxed
Lua environment prepopulated with a restricted engine API; this way, third-party scripts
may be safely included in a server.

#### Account-Level Permissions

Certain server-side permissions should be defined at the account level, for example:

* Admin/moderator/developer privileges
* Login permission (i.e. account banning)

#### Character-Level Permissions

Certain server-side permissions should be defined at the character level, for example:

* Build privileges (per world region)

Some of these permissions should be inherited from associated entities (i.e. basic
guild permissions inherited from a guild entity associated with a character).


### Gameplay

#### Combat

An action combat system should be implemented.

#### Skills

Characters should be able to develop skills.

#### Dynamic Magic System

A dynamic magic system with procedurally generated spells should be implemented.

#### Building

The game should let players build and destroy structures on land where they have
build permission.

#### Land Ownership

Players should be able to purchase plots of land in the world to obtain exclusive
building privileges. Items placed by the owner of a plot of land on that plot of
land should be protected (i.e. only can be picked up by the owner of that plot of
land).

#### Mining

Players should be able to dig underground to mine for resources. A few ideas of
how this could work:

* Each block that is unearthed can yield zero or more resources (stone, ore, etc.).
  The contents of each block can either be predetermined or randomly determined.

* Each block that is unearthed has a small chance of revealing a vein of ore that
  replenishes with time. Again, these ore veins can either be predetermined or
  randomly determined at runtime.

For now I'm leaning toward the second of these options, randomly determined at runtime.
This allows for a space optimization in the game database (don't need to store a large
number of underground resource blocks that may or may not ever be mined), and it creates
an intrinsic value of certain plots of land that are found to contain rare ore veins.
The economics of the ore veins can be controlled via an exponentially decaying
replenishment time (with the decay determined by a moving average of the time delays between
successive mining) - i.e. if an ore vein is harvested as quickly as possible, the
replenishment time quickly rises to a maximum limit. If the ore vein is allowed to rest
for a time greater than its replenishment time, however, it recovers toward its original
replenishment time.

### Tooling

#### Procedural World Generator

A procedural world generator should allow a game developer to generate a full game
world and store it into an empty game database.

#### Resource Editor

A resource editor should allow for the management of the various resource
definition files (e.g. sprite definitions, animated sprite definitions, etc.)
through a UI.

Ideally this would be integrated directly into the client, supporting the
goal of allowing all aspects of the game world to be edited from within
the Client. Some issues that will need to be addressed include:
* How are resources known to the server (e.g. material definitions) updated?
* How is consistency between the game resources and the world database 
  maintained?

#### Integrated World Editor

A world editor should be integrated into the client to allow game developers to easily
edit the game world while logged into the server.



