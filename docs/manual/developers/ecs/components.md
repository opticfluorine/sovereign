# Components

Sovereign Engine uses an Entity-Component-System (ECS) architecture to provide a flexible way to compose in-game entities such as players, NPCs, items, and the voxels (blocks) that comprise the structure of the world. This document outlines the available component types and their roles.

In addition to components, Sovereign Engine allows *tags* to be associated with entities. Tags are essentially components with no value; they impart meaning onto an entity by their presence alone. For example, the `PlayerCharacter` tag designates an entity as a player character.

## Core Components

Core components are available in both the client and server.

| Component          | Component Collection Class            | Description                                                                                                            |
| ------------------ | ------------------------------------- | ---------------------------------------------------------------------------------------------------------------------- |
| `AboveBlock`       | `AboveBlockComponentCollection`       | For material block entities (see `Material` component), denotes the entity ID of the block that sits atop this entity. |
| `Admin`            | `AdminTagCollection`                  | Denotes a player character as an admin.                                                                                |
| `AnimatedSprite`   | `AnimatedSpriteComponentCollection`   | For non-block drawable entities (see `Drawable` component), denotes the animated sprite ID to use for rendering.       |
| `BlockPosition`    | `BlockPositionComponentCollection`    | Grid-aligned position of a block entity.                                                                               |
| `BoundingBox`      | `BoundingBoxComponentCollection`      | For non-block entities, specifies the bounding box used for physics calculations.                                      |
| `CastBlockShadows` | `CastBlockShadowsTagCollection`       | For block entities, indicates that the block should be included for calculating shadows.                               |
| `CastShadows`      | `CastShadowsComponentCollection`      | For non-block entities, specifies the shape of the shadow cast by the entity.                                          |
| `Drawable`         | `DrawableComponentCollection`         | For non-block entities, specifies that the entity should be drawn.                                                     |
| `EntityType`       | `EntityTypeComponentCollection`       | Denotes that the entity is a special type (e.g. item, NPC, player).                                                    |
| `Kinematics`       | `KinematicsComponentCollection`       | For non-block positioned entities, contains the position and velocity of the entity.                                   |
| `Material`         | `MaterialComponentCollection`         | Indicates that the entity is a block of the given material ID.                                                         |
| `MaterialModifier` | `MaterialModifierComponentCollection` | For material block entities (see `Material` component), denotes the material modifier of the block.                    |
| `Name`             | `NameComponentCollection`             | Gives the name of the entity.                                                                                          |
| `Orientation`      | `OrientationComponentCollection`      | Specifies the directional orientation of the entity.                                                                   |
| `Parent`           | `ParentComponentCollection`           | Maps an entity to its parent entity.                                                                                   |
| `Physics`          | `PhysicsTagCollection`                | Indicates that a non-block entity has physics effects.                                                                 |
| `PlayerCharacter`  | `PlayerCharacterTagCollection`        | Indicates that an entity is a player character.                                                                        |
| `PointLightSource` | `PointLightSourceComponentCollection` | Specifies the properties of a point light source attached to the entity.                                               |
| `ServerOnly`       | `ServerOnlyComponentCollection`       | Specifies that the entity is server-only and should not be shared with players (except admins).                        |

## Client Components

Client components are only available in the client.

| Component        | Component Collection Class          | Description                                                 |
| ---------------- | ----------------------------------- | ----------------------------------------------------------- |
| `AnimationPhase` | `AnimationPhaseComponentCollection` | Animation phase for the entity (e.g. static, moving, etc.). |

## Server Components

Server components are only available in the server.

| Component | Component Collection Class   | Description                                                                |
| --------- | ---------------------------- | -------------------------------------------------------------------------- |
| `Account` | `AccountComponentCollection` | Associates an entity (typically a player character) to a specific account. |
