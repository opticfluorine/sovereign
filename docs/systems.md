# Systems

Sovereign Engine uses an Entity-Component-System (ECS) architecture to provide
a flexible way to compose in-game entities such as players, NPCs, items, and
the voxels (blocks) that comprise the structure of the world. This document
outlines the systems that govern the game logic.

## Core Systems

Core systems are present in both the client and server.

| System        | System Class        | Description                                                        |
|---------------|---------------------|--------------------------------------------------------------------|
| `Block`       | `BlockSystem`       | Responsible for managing the lifecycle of block entities.          |
| `Movement`    | `MovementSystem`    | Responsible for controlling the movement of positionable entities. |
| `Network`     | `NetworkSystem`     | Responsible for replicating local events onto the network.         |
| `Performance` | `PerformanceSystem` | Responsible for monitoring engine performance.                     |
| `Ping`        | `PingSystem`        | Responsible for periodic and aperiodic network ping.               |

## Client Systems

Client systems are only present in the client.

| System                  | System Class                  | Description                                                               |
|-------------------------|-------------------------------|---------------------------------------------------------------------------|
| `Camera`                | `CameraSystem`                | Responsible for managing the camera used for rendering.                   |
| `ClientChat`            | `ClientChatSystem`            | Responsible for managing chat functions in the client.                    |
| `ClientNetwork`         | `ClientNetworkSystem`         | Responsible for managing the client-side network connection.              |
| `ClientState`           | `ClientStateSystem`           | Responsible for managing the top-level client state machine.              |
| `EntitySynchronization` | `EntitySynchronizationSystem` | Responsible for non-block entity synchronization in the client.           |
| `Input`                 | `InputSystem`                 | Responsible for responding to user input (keyboard, mouse, gamepad, etc). |

## Server Systems

Server systems are only present in the server.

| System             | System Class             | Description                                                       |
|--------------------|--------------------------|-------------------------------------------------------------------|
| `Accounts`         | `AccountsSystem`         | Responsible for managing the locked account list.                 |
| `Debug`            | `DebugSystem`            | Responsible for providing a debug interface for test.             |
| `Persistence`      | `PersistenceSystem`      | Responsible for managing the database.                            |
| `ServerManagement` | `ServerManagementSystem` | Responsible for managing the engine in its server configuration.  |
| `ServerNetwork`    | `ServerNetworkSystem`    | Responsible for managing server network resources.                |
| `WorldManagement`  | `WorldManagementSystem`  | Responsible for managing the in-memory lifecycle of all entities. |
