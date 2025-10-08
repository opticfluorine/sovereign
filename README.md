# Sovereign Engine - 2.5D Multiplayer RPG Engine

[Website](https://sovereignengine.com) | [Documentation (v0.6.0)](https://docs.sovereignengine.com) | [Discord](https://discord.gg/Mg2jUmePyx)

Sovereign Engine is a 2.5D multiplayer RPG engine with the 2D graphical style of classic RPGs
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

## Current Features (v0.6.0)

* Client and server supporting Windows and Linux platforms
* Dynamic 2.5D lighting and shadows with global and point light sources
* Dynamic day/night cycle with full calendar (days, weeks, months, seasons, years)
* 3D physics engine with collision handling
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

## Upcoming Features (v0.7.0)

* Improved rendering for indoor/underground areas
* Player templates and character customization
* Items and inventory
* NPCs

## Installation

> [!IMPORTANT]
> Sovereign Engine is in a pre-alpha state and is not ready for production use.
> The below instructions are provided for users who are interested in trying out the
> current features or using the engine as a starting point for their own development.

### Windows

Download and run the Windows installer for the latest release.

### Linux

Use your distribution's package manager to install the required dependencies:
* libSDL2
* libSDL2_image
* liblua5.4

Download the binary .tar.gz files for the latest release and extract them to the location
where you would like the client and server to be installed.

## Building from Source

Sovereign Engine requires the .NET 9 (or later) SDK to be installed on your system. Clone This
Git repository, then build the `src/Sovereign.sln` solution from your favorite IDE or from
the command line (via `dotnet build` and `dotnet publish`).

## Running the Engine

### Server

On Windows, double-click the `Sovereign.Server.exe` file to launch the server. On Linux, run the
`Sovereign.Server` executable from a shell. This will automatically create a new SQLite database 
at `Data/sovereign.db` if one does not already exist.

The server is configured by default to grant the Admin role to all new players. Once you have created
an initial player with Admin role, it is strongly recommended to disable this option by editing
the server's `appsettings.json` and changing `AdminByDefault` to `false`.

### Client

On Windows, double-click the `Sovereign.Client.exe` file to launch the client. On Linux, run the
`Sovereign.Client` executable from a shell or a desktop launcher. Click "Yes" on the auto-update
prompt to download the latest game data files.

#### Controls

| Key             | Action                                                      |
|-----------------|-------------------------------------------------------------|
| Esc             | Open in-game menu                                           |
| Arrow keys/WASD | Move player                                                 |
| E               | Interact                                                    |
| I               | Toggle Inventory Window                                     |
| 0-9             | Select Inventory Quickslot 0-9                              |
| ,               | Pick Up Item Under Player                                   |
| Space           | Jump                                                        |
| Enter           | Toggle chat                                                 |
| \`              | Toggle Resource Editor Window                               |
| Insert          | Toggle Template Entity Editor Window (must be admin player) |
| Del             | Toggle World Editor Window (must be admin player)           |
| F2              | Toggle Player Debug Window                                  |
| F3              | Toggle Entity Debug Window                                  |
| F7              | Toggle Network Debug Window                                 |
| F8              | Toggle Dear ImGui Debug Log                                 |
| F9              | Toggle Dear ImGui Demo Window                               |
| F10             | Toggle Dear ImGui ID Stack Tool                             |
| F11             | Toggle Dear ImGui Metrics/Debug Window                      |

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
