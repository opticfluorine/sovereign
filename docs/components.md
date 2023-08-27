# Components

Sovereign Engine uses an Entity-Component-System (ECS) architecture to provide
a flexible way to compose in-game entities such as players, NPCs, items, and
the voxels (blocks) that comprise the structure of the world. This document
outlines the available component types and their roles.

In addition to components, Sovereign Engine allows *tags* to be associated with entities.
Tags are essentially components with no value; they impart meaning onto an entity by
their presence alone. For example, the `PlayerCharacter` tag designates an entity as
a player character.

## Core Components

Core components are available in both the client and server.

| Component          | Component Collection Class            | Description                                                                                                            |
|--------------------|---------------------------------------|------------------------------------------------------------------------------------------------------------------------|
| `AboveBlock`       | `AboveBlockComponentCollection`       | For material block entities (see `Material` component), denotes the entity ID of the block that sits atop this entity. |
| `Material`         | `MaterialComponentCollection`         | Indicates that the entity is a block of the given material ID.                                                         |
| `MaterialModifier` | `MaterialModifierComponentCollection` | For material block entities (see `Material` component), denotes the material modifier of the block.                    |
| `Name`             | `NameComponentCollection`             | Gives the name of the entity.                                                                                          |
| `PlayerCharacter`  | `PlayerCharacterTagCollection`        | Indicates that an entity is a player character.                                                                        |
| `Position`         | `PositionComponentCollection`         | Three-dimensional position of an entity.                                                                               |
| `Velocity`         | `VelocityComponentCollection`         | Three-dimensional velocity of an entity.                                                                               |

## Client Components

Client components are only available in the client.

| Component        | Component Collection Class          | Description                                                                                                      |
|------------------|-------------------------------------|------------------------------------------------------------------------------------------------------------------|
| `AnimatedSprite` | `AnimatedSpriteComponentCollection` | For non-block drawable entities (see `Drawable` component), denotes the animated sprite ID to use for rendering. |
| `Drawable`       | `DrawableComponentCollection`       | The existence of this component indicates that the entity should be considered for rendering.                    |

