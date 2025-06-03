# Sovereign MMORPG Engine - 2.5D Multiplayer RPG Engine

[Website](https://sovereignengine.com) | [Documentation (v0.5.0)](https://docs.sovereignengine.com) | [Discord](https://discord.gg/Mg2jUmePyx)

Sovereign Engine is a 2.5D MMORPG engine with the 2D graphical style of classic RPGs
together with a 3D voxel-based world. The world is highly dynamic and allows
developers and players alike the ability to build homes, mine for resources, farm the
land, and delve into dungeons. Currently in early development, Sovereign will make it easy to
create and share multiplayer RPGs.

Sovereign is developed in C#, runs on Windows and Linux, and is easy to
extend through its distributed Entity-Component-System (ECS) framework.
It also provides a Lua 5.4 scripting engine integrated with the server for
easy extensibility through server-side scripting.

Sovereign Engine is made available under the GPLv3 license.

![Screenshot of Sovereign Engine v0.5.0.](https://update.sovereignengine.com/screenshots/Sovereign_v0.5.0.png)

## Current Features (v0.5.0)

* Client and server supporting Windows and Linux platforms
* Dynamic 2.5D lighting and shadows with global and point light sources
* Server-side Lua scripting engine for easy extensibility
* Login, registration, player creation and selection
* Player movement via keyboard
* In-game chat (local and global)
* Integrated editor for world map, server-side data, and graphical resources
* Integrated debug GUI in client
* Admin roles for users
* Default graphical resources included with client
* Distributed entity-component-system (ECS) data model with full client-side synchronization from
  server, full server-side persistence in a relational database (SQLite currently supported,
  Postgres planned)

For full details of the latest changes and features, see the [changelog](CHANGELOG.md).

## Upcoming Features (v0.6.0)

* Scalable physics engine with gravity and collisions
* More shadows
* Day/night cycle
* Automatic database creation on first startup (SQLite only)

## Getting Started

> [!IMPORTANT]
> Sovereign Engine is in a pre-alpha state and is not ready for production use.
> The below instructions are provided for users who are interested in trying out the
> current features or using the engine as a starting point for their own development.

### Install Dependencies

Sovereign Engine requires the .NET 9 (or later) SDK to be
installed on your system. The SDL2, SDL2_image, and Lua 5.4 libraries are also
required for Linux installations (these are included by default for Windows builds).

### Server

1. Download the server binaries for your platform, or compile binaries from source (via
   `dotnet build` and `dotnet publish` from the `src` directory after cloning the Git repository).
2. Run the server:
   ```bash
   $ ./Sovereign.Server
   ```
   This will automatically create a new SQLite database at `Data/sovereign.db` if one does not
   already exist.
3. The server is configured by default to grant the Admin role to all new players. Once you have created
   an initial player with Admin role, it is strongly recommended to disable this option by editing
   the server's `appsettings.json` and changing `AdminByDefault` to `false`.

### Client

With the server running, simply run the `Sovereign.Client` executable from its directory.

#### Controls

| Key        | Action                                                      |
|------------|-------------------------------------------------------------|
| Esc        | Open in-game menu                                           |
| Arrow keys | Move player                                                 |
| Space      | Jump                                                        |
| Enter      | Toggle chat                                                 |
| `          | Toggle Resource Editor Window                               |
| Insert     | Toggle Template Entity Editor Window (must be admin player) |
| Del        | Toggle World Editor Window (must be admin player)           |
| F2         | Toggle Player Debug Window                                  |
| F3         | Toggle Entity Debug Window                                  |
| F7         | Toggle Network Debug Window                                 |
| F8         | Toggle Dear ImGui Debug Log                                 |
| F9         | Toggle Dear ImGui Demo Window                               |
| F10        | Toggle Dear ImGui ID Stack Tool                             |
| F11        | Toggle Dear ImGui Metrics/Debug Window                      |

#### Chat Commands

| Command           | Arguments             | Action                                                |
|-------------------|-----------------------|-------------------------------------------------------|
| /help             |                       | List commands and their descriptions.                 |
| /g                | message               | Broadcasts a message through global chat.             |
| /rescue           |                       | Emergency teleport back to spawn point.               |
| /addadmin         | playerName            | Admin only. Grants admin role to the given player.    |
| /removeadmin      | playerName            | Admin only. Revokes admin role from the given player. |
| /addblock         | x, y, z, templateName | Admin only. Adds a block at the given position.       |
| /removeblock      | x, y, z               | Admin only. Removes a block at the given position.    |
| /reloadallscripts |                       | Admin only. Reloads all server-side scripts.          |
| /reloadscript     | scriptName            | Admin only. Reloads the specific named script.        |
| /loadnewscripts   |                       | Admin only. Loads any new scripts.                    |
| /listscripts      |                       | Admin only. Lists all currently loaded scripts.       |

## Reporting Issues

Please report any issues to our GitHub [issues tracker](https://github.com/opticfluorine/sovereign/issues).

## Third Party Assets

The Sovereign Engine repository contains a number of third party assets
including spritesets. Attribution information may be found alongside these third party
assets.
