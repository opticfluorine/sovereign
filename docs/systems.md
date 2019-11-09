# Systems

Sovereign Engine uses an Entity-Component-System (ECS) architecture to provide
a flexible way to compose in-game entities such as players, NPCs, items, and
the voxels (blocks) that comprise the structure of the world. This document
outlines the systems that govern the game logic.

## Core Systems

Core systems are present in both the client and server.

System | System Class | Description
--- | --- | ---
`Block` | `BlockSystem` | Responsible for managing the lifecycle of block entities.
`Movement` | `MovementSystem` | Responsible for controlling the movement of positionable entities.
`Network` | `NetworkSystem` | Responsible for replicating local events onto the network.
`Performance` | `PerformanceSystem` | Responsible for monitoring engine performance.
`WorldManagement` | `WorldManagementSystem` | Responsible for managing the in-memory lifecycle of all entities.

## Client Systems

Client systems are only present in the client.

System | System Class | Description
--- | --- | ---
`Camera` | `CameraSystem` | Responsible for managing the camera used for rendering.
`Input` | `InputSystem` | Responsible for responding to user input (keyboard, mouse, gamepad, etc).
`TestContent` | `TestContentSystem` | Generates content for development and testing.

## Server Systems

Server systems are only present in the server.

System | System Class | Description
--- | --- | ---
`Accounts` | `AccountsSystem` | Responsible for managing the locked account list.
`Persistence` | `PersistenceSystem` | Responsible for managing the database.

