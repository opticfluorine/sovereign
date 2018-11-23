# Components

Sovereign Engine uses an Entity-Component-System (ECS) architecture to provide
a flexible way to compose in-game entities such as players, NPCs, items, and
the voxels (blocks) that comprise the structure of the world. This document
outlines the available component types and their roles.

## Core Components

Core components are available in all Soverign Engine components.

Component | Component Collection Class | Description
--- | --- | ---
`AboveBlock` | `AboveBlockComponentCollection` | For material block entities (see `Material` component), denotes the entity ID of the block that sits atop this entity.
`Material` | `MaterialComponentCollection` | Indicates that the entity is a block of the given material ID.
`MaterialModifier` | `MaterialModifierComponentCollection` | For material block entities (see `Material` component), denotes the material modifier of the block.
`Position` | `PositionComponentCollection` | Three-dimensional position of an entity.
`Velocity` | `VelocityComponentCollection` | Three-dimensional velocity of an entity.

## Client Components

Client components are only available in the client.

Component | Component Collection Class | Description
--- | --- | ---
`Drawable` | `DrawableComponentCollection` | The existence of this component indicates that the entity should be considered for rendering.

