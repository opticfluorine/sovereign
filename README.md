# Sovereign MMORPG Engine - 2D Multiplayer RPG Engine

[Website](https://sovereignengine.com) | [Documentation (v0.3.0)](https://docs.sovereignengine.com) | [Discord](https://discord.gg/Mg2jUmePyx)

Sovereign Engine is a 2.5D MMORPG engine with the 2D graphical style of classic RPGs
together with a 3D voxel-based world. The world is highly dynamic and allows
developers and players alike the ability to build homes, mine for resources, farm the
land, and delve into dungeons. Currently in early development, Sovereign will make it easy to
create and share multiplayer RPGs.

Sovereign is developed in C#, runs on Windows and Linux, and is easy to
extend through its distributed Entity-Component-System (ECS) framework.

Sovereign Engine is made available under the GPLv3 license.

![Screenshot of Sovereign Engine v0.4.0.](https://update.sovereignengine.com/screenshots/Sovereign_v0.4.0.png)

## Current Features (v0.4.0)

* Client and server supporting Windows and Linux platforms
* Login and registration
* Player listing, selection, creation, and deletion via in-game GUI
* Basic rendering of world including automatic borders between neighboring tiles, animated
  sprites, directional sprites
* Sprite metadata including attribution information for easy inclusion of third party
  assets
* Player movement via keyboard
* Distributed entity-component-system (ECS) data model with full client-side synchronization from
  server, full server-side persistence in a relational database (SQLite currently supported,
  Postgres planned)
* Integrated debug GUI in client
* In-game chat
* Admin roles for users
* Integrated editor for world map, server-side data, and graphical resources
* Default graphical resources included with client

For full details of the latest changes and features, see the [changelog](CHANGELOG.md).

## Upcoming Features (v0.5.0)

* Improved renderer with more flexibility to support advanced effects
* Lighting and shadows
* Procedural world generation

## Getting Started

> [!IMPORTANT]
> Sovereign Engine is in a pre-alpha state and is not ready for production use.
> The below instructions are provided for users who are interested in trying out the
> current features or using the engine as a starting point for their own development.

### Install Dependencies

Sovereign Engine requires the .NET 8 (or later) SDK and SQLite 3.x to be
installed on your system.

### Server

1. Download the server binaries for your platform, or compile binaries from source (via
   `dotnet build` and `dotnet publish` from the `src` directory after cloning the Git repository).
2. From the server directory, initialize a new SQLite database. Using bash:
   ```bash
   $ cd Data
   $ sqlite3 sovereign.db < ../Migrations/Full/Full_sqlite.sql
   ```
   Using PowerShell:
   ```powershell
   cd Data
   Get-Content ..\Migrations\Full\Full_sqlite.sql | sqlite3 sovereign.db
   ```
3. Run the server:
   ```bash
   $ ./Sovereign.Server
   ```
4. The server is configured by default to grant the Admin role to all new players. Once you have created
   an initial player with Admin role, it is strongly recommended to disable this option by editing
   the server configuration file and changing `AdminByDefault` to `false`.

### Client

With the server running, simply run the `Sovereign.Client` executable from its directory.

#### Controls

| Key        | Action                                                      |
|------------|-------------------------------------------------------------|
| Esc        | Open in-game menu                                           |
| Arrow keys | Move player                                                 |
| Enter      | Toggle chat                                                 |
| `          | Toggle Resource Editor Window                               |
| Insert     | Toggle Template Entity Editor Window (must be admin player) |
| Del        | Toggle World Editor Window (must be admin player)           |
| F2         | Toggle Player Debug Window                                  |
| F3         | Toggle Entity Debug Window                                  |
| F10        | Toggle Dear ImGui ID Stack Tool                             |
| F11        | Toggle Dear ImGui Metrics/Debug Window                      |
| F12        | Toggle Dear ImGui Debug Log                                 |

#### Chat Commands

| Command      | Arguments             | Action                                                |
|--------------|-----------------------|-------------------------------------------------------|
| /help        |                       | List commands and their descriptions.                 |
| /g           | message               | Broadcasts a message through global chat.             |
| /addadmin    | playerName            | Admin only. Grants admin role to the given player.    |
| /removeadmin | playerName            | Admin only. Revokes admin role from the given player. |
| /addblock    | x, y, z, templateName | Admin only. Adds a block at the given position.       |
| /removeblock | x, y, z               | Admin only. Removes a block at the given position.    |

## Reporting Issues

Please report any issues to our GitHub [issues tracker](https://github.com/opticfluorine/sovereign/issues).

## Third Party Assets

The Sovereign Engine repository contains a number of third party assets
including spritesets. Attribution information may be found alongside these third party
assets.
