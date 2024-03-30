# Sovereign MMORPG Engine - 2D Multiplayer RPG Engine

[Website](https://sovereignengine.com)

Sovereign Engine is a 2.5D MMORPG engine with the 2D graphical style of classic RPGs
together with a 3D voxel-based world. The world is highly dynamic and allows
developers and players alike the ability to build homes, mine for resources, farm the
land, and delve into dungeons. Currently in early development, Sovereign will make it easy to
create and share multiplayer RPGs.

Sovereign is developed in C#, runs on Windows and Linux, and is easy to
extend through its distributed Entity-Component-System (ECS) framework.

Sovereign Engine is made available under the GPLv3 license.

![Screenshot of Sovereign Client v0.1.0 running with test data.](screenshot.png)

## Current Features (v0.2.0)

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

For full details of the latest changes and features, see the [changelog](CHANGELOG.md).

## Upcoming Features (v0.3.0)

* In-game chat
* Integrated tools for game resource configuration (e.g. sprite definitions, material definitions, etc.)
* Admin roles for users
* Integrated world editing tools in client

## Getting Started

> [!IMPORTANT]
> Sovereign Engine is in a pre-alpha state and is not ready for production use.
> The below instructions are provided for users who are interesting in trying out the
> current features or using the engine as a starting point for their own development.

### Install Dependencies

Sovereign Engine requires the .NET 6 (or later) SDK and SQLite 3.x to be
installed on your system.

### Server

> [!WARNING]
> Ensure that `EnableDebugMode` is set to `false` before exposing the
> server's REST API to an external network. The debug API is not authenticated.

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
3. Enable the debug command interface by editing `Data/Configuration/ServerConfiguration.yaml`
   and changing `EnableDebugMode` to `true`.
4. Run the server:
   ```bash
   $ ./Sovereign.Server
   ```
5. If using a new database, generate a test set of initial world data via the debug interface. Using bash:
   ```bash
   $ curl -X POST -d'{"Type":"GenerateWorldData"}' http://127.0.0.1:8080/debug
   ```
   Using PowerShell:
   ```powershell
   Invoke-RestMethod -Uri http://127.0.0.1:8080/debug -Method POST -Body '{"Type": "GenerateWorldData"}'
   ```
6. The server is configured by default to grant the Admin role to all new players. Once you have created
   an initial player with Admin role, it is strongly recommended to disable this option by editing
   the server configuration file and changing `AdminByDefault` to `false`.

### Client

With the server running, simply run the `Sovereign.Client` executable from its directory.

#### Controls

| Key        | Action                                 |
|------------|----------------------------------------|
| Esc        | Open in-game menu                      |
| Arrow keys | Move player                            |
| Enter      | Toggle chat                            |
| `          | Toggle Resource Editor Window          |
| F2         | Toggle Player Debug Window             |
| F3         | Toggle Entity Debug Window             |
| F10        | Toggle Dear ImGui ID Stack Tool        |
| F11        | Toggle Dear ImGui Metrics/Debug Window |
| F12        | Toggle Dear ImGui Debug Log            |

#### Chat Commands

| Command      | Arguments  | Action                                                |
|--------------|------------|-------------------------------------------------------|
| /help        |            | List commands and their descriptions.                 |
| /g           | message    | Broadcasts a message through global chat.             |
| /addadmin    | playerName | Admin only. Grants admin role to the given player.    |
| /removeadmin | playerName | Admin only. Revokes admin role from the given player. |

## Reporting Issues

Please report any issues to our GitHub [issues tracker](https://github.com/opticfluorine/sovereign/issues).

## Third Party Assets

The Sovereign Engine repository contains a number of third party assets
including spritesets. Attribution information may be found alongside these third party
assets.
