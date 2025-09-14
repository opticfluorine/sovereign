# Entities

```{contents}
:local:
```

## Introduction

**Entities** are the building blocks of all content in Sovereign Engine. An entity is any in-game object which has data and/or behavior associated with it. The following are all examples of entities:

* **NPCs** - non-player characters, dynamic entities which exist in the game world and have some behavior
* **Items** - entities which may be held by a player or NPC in their inventory
* **Players** - every player in the game world is an entity similar to an NPC
* **Blocks** - every block exists as a lightweight entity that references another *block template* entity

One way to think about entities is as a collection of data and behaviors with a unique ID number. An entity with no data has no meaning or behavior; an entity's presence in the game world comes from its data and associated behaviors. As such, entities are a sort of "blank slate" on which all types of game content can be created.

## Entity Lifecycle

Every non-template entity has a *lifecycle* through which it progresses:

```{mermaid}
stateDiagram-v2
  direction LR
  TC: Template Changing
  [*] --> Created: Create
  Created --> Active: Load
  Active --> Unloaded: Unload
  Unloaded --> Active: Load
  Active --> Removing: Unload
  Active --> TC: Unload
  TC --> Active: Load
  Removing --> [*]: Remove
```

When an entity is first created, it enters the **Created** state through the *Created* transition. It then immediately undergoes a *Load* transition to enter the **Active** state, at which point the entity is loaded in memory and is continuously processed by the engine.

After some time, the entity may undergo an *Unload* transition to enter the **Unloaded** state. An entity in the Unloaded state is released from the engine's memory, but it continues to exist in the server's database. The *Unload* transition can occur for several reasosns including:
* The entity is located in a world segment which has been unloaded due to a lack of players in its vicinity.
* The entity is connected to a player who has logged out.

An Unloaded entity may later undergo another *Load* transition to re-enter the **Active** state, typically for one of the following reasons:
* The entity is located in a world segment that is being loaded due to a player entering its vicinity.
* The entity is connected to a player who is logging in.

Finally, many entities will eventually reach a point where they are fully destroyed (e.g. a monster is defeated, a block is destroyed, etc.). A destroyed entity first undergoes the *Unload* transition to enter the short-lived **Removing** state, then immediately undergoes the *Remove* transition, after which the entity is deleted and no longer exists either in memory or in the database.

Template changes are a special case. In a template change, the template ID of an entity is changed, and this change can be accompanied by a change in behavior. To allow the entity's behavior to reconfigure with the change, the entity will undergo the *Unload* transition under its original template followed by the *Load* transition under its new template.

:::{note}
Some transitions appear between multiple pairs of states in the above diagram. This is intentional and allows for certain behaviors to be performed consistently in all relevant cases (e.g. performing an action through the *Unload* transition regardless of whether an entity is unloaded or removed).
:::

Custom behaviors may be attached to any entity through the four lifecycle transitions using [behavior scripts](scripting/behaviors). This is typically done by setting the lifecycle hooks for [template entities](templates/index). The recommended pattern for attaching behaviors to entities through the lifecycle transitions is summarized in the table below.

| Transition  | Behavior Actions                                                                                                                                                    |
| ----------  | ------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Create      | Perform any one-time entity configuration (e.g. randomize stats, randomize inventory, etc.)                                                                         |
| Load/Unload | Start/stop behavior processing, typically via a [standard behavior](scripting/standard_behaviors/index) or a custom [EntityBehavior](scripting/sdk/entity_behavior) |
| Remove      | Perform any one-time entity cleanup (e.g. updating global counters, etc.)                                                                                           |

## Entity Data

Sovereign Engine supports three types of entity data:

* **Metadata** - Metadata is information held by every entity such as the entity ID number, the associated [template](templates/index) ID, and the "nonpersistent" flag which tells the server whether to store the entity in the database.
* **Components** - Components are specific data built into the engine with a specific purpose. Some examples of components include Kinematics (position and velocity), Drawable (tells the engine how to draw an entity to the screen), and Physics (a flag that tells the engine to apply physics processing to the entity).
* **Key-Value Data** - Entity key-value data (also called *entity properties*) is free-form data associated with entities by a unique *key*. These key-value pairs may be freely read and modified by server-side scripts, and so they can be used by creators to support custom behaviors.

Each type of entity data is discussed in detail below.

### Metadata

Every entity has three associated pieces of metadata:

* **Entity ID** - This is a 64-bit number (written as sixteen hexadecimal digits) that provides a unique identifier for the entity. Entity IDs are automatically assigned by the engine whenever a new entity is created. Entity IDs are grouped into several ranges based on the intended use of the entity:
  | First ID         | Last ID          | Purpose                 |
  | ---------------- | ---------------- | ----------------------- |
  | 0000000000000000 | 6FFEFFFFFFFFFFFF | Client-only entities    |
  | 6FFF000000000000 | 6FFFFFFFFFFFFFFF | Block entities          |
  | 7000000000000000 | 7FFDFFFFFFFFFFFF | Reserved for future use |
  | 7FFE000000000000 | 7FFEFFFFFFFFFFFF | Template entities       |
  | 7FFF000000000000 | FFFFFFFFFFFFFFFF | Non-block entities      |

* **Template ID** - This is an optional 64-bit number that specifies the [template](templates/index) which an entity inherits. Components and key-value pairs are inherited from an entity's template unless they are overridden in the entity. For example, an entity which inherits a template with an AnimatedSprite component will have the same value as the template for that component unless the entity has its own AnimatedSprite component.

* **Nonpersistent Flag** - This is a flag that, if set, tells the server that it should not save the entity to the database. This is useful for cases such as spawned monsters where the entities are short-lived and quickly respawned, eliminating any need for persistent storage in the database.

### Components

Components are data values associated with an entity that serve a specific purpose and are stored in memory in such a way that the engine's systems can process updates to them very quickly. The full list of component types and their purposes are as follows:

| Component          | Description                                                                                                            |
| ------------------ | ---------------------------------------------------------------------------------------------------------------------- |
| AboveBlock       | For material block entities (see Material component), denotes the entity ID of the block that sits atop this entity. |
| Account          | Associates an entity (typically a player character) to a specific account.                                             |
| Admin            | Denotes a player character as an admin.                                                                                |
| AnimationPhase   | Animation phase for the entity (e.g. static, moving, etc.).                                                            |
| AnimatedSprite   | For non-block drawable entities (see Drawable component), denotes the animated sprite ID to use for rendering.       |
| BlockPosition    | Grid-aligned position of a block entity.                                                                               |
| BoundingBox      | For non-block entities, specifies the bounding box used for physics calculations.                                      |
| CastBlockShadows | For block entities, indicates that the block should be included for calculating shadows.                               |
| CastShadows      | For non-block entities, specifies the shape of the shadow cast by the entity.                                          |
| Drawable         | For non-block entities, specifies that the entity should be drawn.                                                     |
| EntityType       | Denotes that the entity is a special type (e.g. item, NPC, player).                                                    |
| Kinematics       | For non-block positioned entities, contains the position and velocity of the entity.                                   |
| Material         | Indicates that the entity is a block of the given material ID.                                                         |
| MaterialModifier | For material block entities (see Material component), denotes the material modifier of the block.                    |
| Name             | Gives the name of the entity.                                                                                          |
| Orientation      | Specifies the directional orientation of the entity.                                                                   |
| Parent           | Maps an entity to its parent entity.                                                                                   |
| Physics          | Indicates that a non-block entity has physics effects.                                                                 |
| PlayerCharacter  | Indicates that an entity is a player character.                                                                        |
| PointLightSource | Specifies the properties of a point light source attached to the entity.                                               |
| ServerOnly       | Specifies that the entity is server-only and should not be shared with players (except admins).                        |

:::{note}
The component types are built into the engine and cannot be extended without modifying the source code. Advanced creators who want to add new components should follow [this guide](/developers/ecs/adding_components).
:::

### Key-Value Data

Dynamic content (such as behavior scripts) often requires custom data beyond metadata and components. For this purpose, Sovereign Engine supports per-entity key-value data pairs. Every entity, including [template entities](templates/index), may be associated with any number of key-value pairs. Like components, key-value data is inherited by entities from their template.

One common application of entity key-value data is to specify per-entity (or per-template entity) parameters to [behavior scripts](scripting/behaviors). Scripts also often use key-value data to store information about an entity's behavior that persists even when the entity is unloaded and reloaded. For example, the [TimedChange](scripting/standard_behaviors/generic/timedchange) standard behavior stores the in-game time at which the entity will change in a key-value pair so that the transformation can be applied if needed when the entity is reloaded from the database.

The key of a key-value pair may be any non-empty string. Keys that begin with two underscores (`__`) are treated as reserved for the engine's internal use and may not be modified by scripts (though they may be read by scripts freely). The standard convention for naming keys is to use a series of identifiers separated by periods with the first identifier being unique for your project. For example, all standard behaviors included in Sovereign Engine use keys of the form `Sovereign.X`, `Sovereign.X.Y`, etc. By prefixing all of your keys with a unique identifier for your project, you minimize the risk of name conflicts between your scripts, the standard behavior scripts, and any third-party scripts that you are using.

Unlike components, entity key-value data is not normally sent to the client (except when using a template editor as an admin player). Therefore, entity key-value data may be used to store secret information that should not be revealed to players.
