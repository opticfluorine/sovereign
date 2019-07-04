# Sovereign Engine Roadmap

This file is an incomplete list of future features planned.
Planned features are grouped by release, with earliest releases listed first.

All features shall be implemented following the accessibility guidelines
outlined in docs/accessibility.md.


## Milestone 1

### Basic World Rendering

> :white_check_mark: Complete.

The client should be able to perform basic rendering of the game world as
described in docs/world_structure.md.

### Client-Server Model

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


### Player Character Movement

The player should be able to control a character using the keyboard.


## Milestone 2

### Client Authentication

The client should be able to authenticate with the server with a persistent
account for each player.

### Non-Player Characters

Non-player characters should be able to exist within the game world. NPCs
should be able to be either persistent or non-persistent.

### Items and Inventory System

Items should be able to exist within the game world or in character inventories.

### PostgreSQL Backend Support

The server should be able to optionally use a PostgreSQL database.


## Future Features Not Assigned to a Release

### Client-Server Binding

The client should be able to manage a local bound copy of the server for 
single-player or multiplayer gameplay.

### Combat

An action combat system should be implemented.

### Skills

Characters should be able to develop skills.

### Dynamic Magic System

A dynamic magic system with procedurally generated spells should be implemented.

### Server Scripting

A scripting engine should be integrated with the server.

### Resource Editor

A resource editor should allow for the management of the various resource
definition files (e.g. sprite definitions, animated sprite definitions, etc.)
through a UI.

Ideally this would be integrated directly into the client, supporting the
goal of allowing all aspects of the game world to be edited from within
the Client. Some issues that will need to be addressed include:
* How are resources known to the server (e.g. material definitions) updated?
* How is consistency between the game resources and the world database 
  maintained?


