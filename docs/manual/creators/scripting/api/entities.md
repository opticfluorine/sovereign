(script-entities)=
# Entities Module

:::{contents}
:local:
:depth: 2
:::

The `Entities` module provides functions for creating and removing entities.

Entity IDs are passed to and from the `Entities` module as Lua
`lightuserdata` values. Entity IDs are opaque 64-bit values; they support only
equality comparison in scripts. To display an entity ID or to convert between
integers and entity IDs, use the [entity ID conversion functions](#script-entities-conversion).

## Entity Management Functions

### Create(spec)

#### Definition

```{eval-rst}
.. lua:function:: Entities.Create(spec)

   Creates a new entity according to the provided specification.
   
   :param spec: Entity specification describing the entity to create.
   :type spec: table (see below for allowed entries)

   :return: Entity ID of the newly created entity, or `nil` on error.
   :rtype: lightuserdata
```

The entity specification accepted by `Create(spec)` is a table containing
zero or more of the following entries describing the initial state of the entity.
The absence of any entry indicates that no value will be initially set, with the
exception of the `EntityId` entry for which a value will be automatically selected
if one is not provided.

| Key              | Value Type                                   | Meaning                                          |
|------------------|----------------------------------------------|--------------------------------------------------|
| EntityId         | lightuserdata                                | Entity ID                                        |
| Template         | lightuserdata                                | Template entity ID                               |
| NonPersistent    | boolean                                      | Whether entity is nonpersistent                  |
| AnimatedSprite   | integer                                      | Animated sprite ID                               |
| BlockPosition    | [GridPosition](#script-types-gridposition)   | Position (block entities)                        |
| BoundingBox      | [BoundingBox](#script-types-boundingbox)     | Bounding box for physics effects                 |
| CastBlockShadows | boolean                                      | Whether entity casts block shadows when rendered |
| CastShadows      | [Shadow](#script-types-shadow)               | Shadow cast by a non-block entity                |
| Drawable         | [Vector2](#script-types-vector2)             | Draw entity; value specifies offset              |
| EntityType       | [EntityType](#script-constants-entitytype)   | Special type of entity (NPC/item/etc.), if any   |
| Kinematics       | [Kinematics](#script-types-kinematics)       | Position and velocity (non-block entities)       |
| Name             | string                                       | Name                                             |
| Orientation      | [Orientation](#script-constants-orientation) | Orientation                                      |
| Parent           | lightuserdata                                | Entity ID of parent entity                       |
| Physics          | boolean                                      | Whether entity has physics effects               |
| PointLightSource | [PointLight](#script-types-pointlight)       | Point light source                               |
| ServerOnly       | boolean                                      | Whether entity is server-only                    |
| UseRange         | float                                        | Maximum tool use distance (item entities)        |

#### Example

```{code-block} lua
:caption: Creating an entity using Entities.Create(spec)
:emphasize-lines: 1,2,3,4,5,6,7,8,9
local entityId = Entities.Create({
    Name = "Test Entity",
    Kinematics = {
        Position = {X = 0.0, Y = 0.0, Z = 0.0},
        Velocity = {X = 0.0, Y = 0.0, Z = 0.0}
    },
    Drawable = true,
    AnimatedSprite = 4
})

if (entityId) then
    Util.LogDebug("Created new entity with ID " .. Entities.FormatEntityId(entityId) .. ".")
else
    Util.LogError("Error while creating entity.")
end
```

### Remove(entityId)

#### Definition

```{eval-rst}
.. lua:function:: Entities.Remove(entityId)

   Removes the given entity from the game world.
   
   :param entityId: Entity to remove.
   :type entityId: lightuserdata
```   

#### Example

```{code-block} lua
:caption: Removing an entity using Entities.Remove(entityId)
:emphasize-lines: 1
Entities.Remove(targetEntityId)
```

### GetTemplate(entityId)

#### Definition

```{eval-rst}
.. lua:function:: Entities.GetTemplate(entityId)

   Gets the template entity ID for the given entity, if any.
   
   :param entityId: Entity to query.
   :type entityId: lightuserdata
   :return: Template entity ID, or nil if not set.
   :rtype: lightuserdata or nil
```

#### Example

```{code-block} lua
:caption: Getting the template entity ID for an entity
:emphasize-lines: 1
local templateId = Entities.GetTemplate(entityId)
```

### SetTemplate(entityId, templateId)

#### Definition

```{eval-rst}
.. lua:function:: Entities.SetTemplate(entityId, templateId)

   Sets the template entity ID for the given entity.
   
   :param entityId: Entity to modify.
   :type entityId: lightuserdata
   :param templateId: Template entity ID to assign.
   :type templateId: lightuserdata
```

#### Example

```{code-block} lua
:caption: Setting the template entity ID for an entity
:emphasize-lines: 1
Entities.SetTemplate(entityId, templateId)
```

### IsTemplate(entityId)

#### Definition

```{eval-rst}
.. lua:function:: Entities.IsTemplate(entityId)

   Returns true if the given entity ID is a template entity.
   
   :param entityId: Entity ID to check.
   :type entityId: lightuserdata
   :return: True if entity is a template, false otherwise.
   :rtype: boolean
```

#### Example

```{code-block} lua
:caption: Checking if an entity is a template entity
:emphasize-lines: 1
if Entities.IsTemplate(entityId) then
    print("Entity is a template.")
end
```

### Sync(entityId)

#### Definition

```{eval-rst}
.. lua:function:: Entities.Sync(entityId)

   Synchronizes the given entity or entities to any subscribed clients.

   :param entityId: Entity ID or table of multiple entity IDs to synchronize.
   :type entityId: lightuserdata|table
```

#### Example

```{code-block} lua
:caption: Synchronizing entities
:emphasize-lines: 3,7
-- Single entity.
local singleId = Entities.ToEntityId(0x7FFF000000000000)
Entities.Sync(singleId)

-- Multiple Entities.
local multipleIds = { Entities.ToEntityId(0x7FFF000000000000), Entities.ToEntityId(0x7FFF000000000001) }
Entities.Sync(multipleIds)
```

### SyncTree(entityId)

#### Definition

```{eval-rst}
.. lua:function:: Entities.SyncTree(entityId)

   Synchronizes the given entity or entities and all descendants to any subscribed clients.

   :param entityId: Entity ID or table of multiple entity IDs to synchronize.
   :type entityId: lightuserdata|table
```

#### Example

```{code-block} lua
:caption: Synchronizing entities
:emphasize-lines: 3,7
-- Single entity and its descendants.
local singleId = Entities.ToEntityId(0x7FFF000000000000)
Entities.SyncTree(singleId)

-- Multiple entities and their descendants.
local multipleIds = { Entities.ToEntityId(0x7FFF000000000000), Entities.ToEntityId(0x7FFF000000000001) }
Entities.SyncTree(multipleIds)
```

(script-entities-conversion)=
## Entity ID Conversion Functions

The following functions convert entity IDs between their `lightuserdata`
representation and other representations. Entity IDs support only equality
comparison in scripts; use these functions for arithmetic, display, or storage.

### AbsoluteTemplateId(relativeId)

#### Definition

```{eval-rst}
.. lua:function:: Entities.AbsoluteTemplateId(relativeId)

   Converts a relative template entity ID to an absolute template entity ID.

   :param relativeId: Relative template entity ID.
   :type relativeId: integer
   :return: Absolute template entity ID.
   :rtype: lightuserdata
```

#### Example

```{code-block} lua
:caption: Using `AbsoluteTemplateId` to convert from relative to absolute template ID.
:emphasize-lines: 1
local absId = Entities.AbsoluteTemplateId(4) -- returns 0x7ffe000000000004
```

### FormatEntityId(entityId)

#### Definition

```{eval-rst}
.. lua:function:: Entities.FormatEntityId(entityId)

   Formats an entity ID as an uppercase hexadecimal string (e.g. ``"7FFE000000000004"``).

   :param entityId: Entity ID to format.
   :type entityId: lightuserdata
   :return: Uppercase hexadecimal representation of the entity ID.
   :rtype: string
```

#### Example

```{code-block} lua
:caption: Using `FormatEntityId` to display an entity ID.
:emphasize-lines: 1
Util.LogDebug("Created entity " .. Entities.FormatEntityId(entityId))
```

### ToEntityId(integer)

#### Definition

```{eval-rst}
.. lua:function:: Entities.ToEntityId(integer)

   Converts an integer to the corresponding entity ID.

   :param integer: Integer value of the entity ID.
   :type integer: integer
   :return: Entity ID as a lightuserdata value.
   :rtype: lightuserdata
```

#### Example

```{code-block} lua
:caption: Using `ToEntityId` to convert from an integer.
:emphasize-lines: 1
local entityId = Entities.ToEntityId(0x7FFF000000000000)
```

### ToTemplateEntityId(relativeId)

#### Definition

```{eval-rst}
.. lua:function:: Entities.ToTemplateEntityId(relativeId)

   Converts a relative template entity ID to the corresponding absolute
   template entity ID. Equivalent to `AbsoluteTemplateId`.

   :param relativeId: Relative template entity ID.
   :type relativeId: integer
   :return: Absolute template entity ID as a lightuserdata value.
   :rtype: lightuserdata
```

#### Example

```{code-block} lua
:caption: Using `ToTemplateEntityId` to convert from a relative template ID.
:emphasize-lines: 1
local templateId = Entities.ToTemplateEntityId(4) -- same as Entities.AbsoluteTemplateId(4)
```

## Constants

The following constants are available in the `Entities` module. The constants
are entity IDs and are provided as `lightuserdata` values.

| Constant                      | Value Type      | Description                                 |
|-------------------------------|-----------------|---------------------------------------------|
| `FirstTemplateEntityId`       | `lightuserdata` | First template entity ID                    |
| `LastTemplateEntityId`        | `lightuserdata` | Last template entity ID                     |
| `FirstBlockEntityId`          | `lightuserdata` | First block entity ID                       |
| `LastBlockEntityId`           | `lightuserdata` | Last block entity ID                        |
| `FirstPersistedEntityId`      | `lightuserdata` | First persisted entity ID                   |
