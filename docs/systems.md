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
`TestContent` | `TestContentSystem` | Generates content for development and testing.

## Client Systems

Client systems are only present in the client.

System | System Class | Description
--- | --- | ---
`Camera` | `CameraSystem` | Responsible for managing the camera used for rendering.
`Input` | `InputSystem` | Responsible for responding to user input (keyboard, mouse, gamepad, etc).

## Server Systems

Server systems are only present in the server.

System | System Class | Description
--- | --- | ---
`Persistence` | `PersistenceSystem` | Responsible for managing the database.

