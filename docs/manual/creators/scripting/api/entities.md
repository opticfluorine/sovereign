(script-entities)=
# entities Module

:::{contents}
:local:
:depth: 2
:::

The `entities` module provides functions for creating and removing entities.

## Entity Management Functions

### Create(spec)

#### Definition

```{eval-rst}
.. lua:function:: entities.Create(spec)

   Creates a new entity according to the provided specification.
   
   :param spec: Entity specification describing the entity to create.
   :type spec: table (see below for allowed entries)

   :return: Entity ID of the newly created entity, or `nil` on error.
   :rtype: integer
```

The entity specification accepted by `Create(spec)` is a table containing
zero or more of the following entries describing the initial state of the entity.
The absence of any entry indicates that no value will be initially set, with the
exception of the `EntityId` entry for which a value will be automatically selected
if one is not provided.

| Key              | Value Type                                 | Meaning                                          |
|------------------|--------------------------------------------|--------------------------------------------------|
| EntityId         | integer                                    | Entity ID                                        |
| Template         | integer                                    | Template entity ID                               |
| Kinematics       | [Kinematics](#script-types-kinematics)     | Position and velocity (non-block entities)       |
| BlockPosition    | [GridPosition](#script-types-gridposition) | Position (block entities)                        |
| Drawable         | boolean                                    | Whether entity is visible when rendered          |
| AnimatedSprite   | integer                                    | Animated sprite ID                               |
| Name             | string                                     | Name                                             |
| Parent           | integer                                    | Entity ID of parent entity                       |
| Orientation      | integer                                    | Orientation                                      |
| CastBlockShadows | boolean                                    | Whether entity casts block shadows when rendered |
| PointLightSource | [PointLight](#script-types-pointlight)     | Point light source                               |

#### Example

```{code-block} lua
:caption: Creating an entity using entities.Create(spec)
:emphasize-lines: 1,2,3,4,5,6,7,8,9
local entityId = entities.Create({
    Name = "Test Entity",
    Kinematics = {
        Position = {X = 0.0, Y = 0.0, Z = 0.0},
        Velocity = {X = 0.0, Y = 0.0, Z = 0.0}
    },
    Drawable = true,
    AnimatedSprite = 4
})

if (entityId) then
    util.LogDebug(string.format("Created new entity with ID %x.", entityId))
else
    util.LogError("Error while creating entity.")
end
```

### Remove(entityId)

#### Definition

```{eval-rst}
.. lua:function:: entities.Remove(entityId)

   Removes the given entity from the game world.
   
   :param entityId: Entity to remove.
   :type entityId: integer
```   

#### Example

```{code-block} lua
:caption: Removing an entity using entities.Remove(entityId)
:emphasize-lines: 1
entities.Remove(targetEntityId)
```
