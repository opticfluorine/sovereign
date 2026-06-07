(script-components)=
# Components Module

:::{contents}
:local:
:depth: 2
:::

All components and tags share a common API provided through the `Components` module.

## Component Submodules

Each component collection that is accessible through the scripting engine may be accessed through a submodule of the global `Components` module. The following table lists the component submodules, their associated component collection, and the corresponding value type.

| Submodule                     | Component Collection                  | Value Type     |
| ----------------------------- | ------------------------------------- | -------------- |
| `Components.Admin`            | `AdminTagCollection`                  | `boolean`      |
| `Components.AnimatedSprite`   | `AnimatedSpriteComponentCollection`   | `integer`      |
| `Components.BlockPosition`    | `BlockPositionComponentCollection`    | `GridPosition` |
| `Components.BlockTile`        | `BlockTileComponentCollection`        | `BlockTile`    |
| `Components.CastBlockShadows` | `CastBlockShadowsTagCollection`       | `boolean`      |
| `Components.CastShadows`      | `CastShadowsComponentCollection`      | `Shadow`       |
| `Components.Drawable`         | `DrawableComponentCollection`         | `Vector2`      |
| `Components.EntityType`       | `EntityTypeComponentCollection`       | `EntityType`   |
| `Components.Kinematics`       | `KinematicsComponentCollection`       | `Kinematics`   |
| `Components.Name`             | `NameComponentCollection`             | `string`       |
| `Components.Orientation`      | `OrientationComponentCollection`      | `integer`      |
| `Components.Parent`           | `ParentComponentCollection`           | `integer`      |
| `Components.Physics`          | `PhysicsTagCollection`                | `boolean`      |
| `Components.PlayerCharacter`  | `PlayerCharacterTagCollection`        | `boolean`      |
| `Components.PointLight`       | `PointLightSourceComponentCollection` | `PointLight`   |
| `Components.ServerOnly`       | `ServerOnlyTagCollection`             | `boolean`      |

## General Component Functions

The following functions are common to multiple component types.

### Exists(entityId)

#### Definition

```{eval-rst}
.. lua:function:: Components.<component>.Exists(entityId, lookback=false)

   Gets whether the given entity has this component.
   
   :param entityId: Entity ID.
   :type entityId: integer
   :param lookback: Whether to look back at components deleted in the last tick (default: false).
   :type lookback: boolean
   :return: true if the entity has this component, or false otherwise.
   :rtype: boolean
```

#### Example

```{code-block} lua
:caption: Using `Exists(entityId)` to check for the presence of a tag.
:emphasize-lines: 1, 8
if (Components.PlayerCharacter.Exists(entityId)) then
  -- This entity is a player character.
  Util.LogInfo(string.format("%x is a player.", entityId))
end

-- If the player just logged out, the tag may have been removed before the script
-- was called. Use lookback to handle this situation.
if (Components.PlayerCharacter.Exists(entityId, true)) then
  -- ...
end
```

### Get(entityId)

#### Definition

```{eval-rst}
.. lua:function:: Components.<component>.Get(entityId, lookback=false)

   Gets the component value for the given entity.

   :param entityId: Entity ID.
   :type entityId: integer
   :param lookback: Whether to look back at components deleted in the last tick (default: false).
   :type lookback: boolean
   :return: Component value, or nil if no the entity does not have this component.
            For tags, returns false instead of nil.
   :rtype: Component value type
```

#### Example

```{code-block} lua
:caption: Using `Get(entityId)` to get the name of an entity.
:emphasize-lines: 1, 8
local name = Components.Name.Get(entityId)
if (name) then
  Util.LogInfo(string.format("%x is named %s.", entityId, name))
end

-- If the player just logged out, the name component may have been unloaded before
-- the script was called. Use lookback to handle this situation.
local nameWithLookback = Components.Name.Get(entityId, true)
if (name) then
   -- ...
end
```

### Remove(entityId)

#### Definition

```{eval-rst}
.. lua:function:: Components.<component>.Remove(entityId)

   Removes the component value for the given entity if it exists.
   
   :param entityId: EntityID
   :type entityId: integer
```

#### Example

```{code-block} lua
:caption: Using `Remove(entityId)` to remove a component from an entity.
:emphasize-lines: 2
-- Remove the point light source from an entity.
Components.PointLight.Remove(entityId)
```

### Set(entityId, value)

#### Definition

```{eval-rst}
.. lua:function:: Components.<component>.Set(entityId, value)

   Enqueues an update to the component value for the given entity
   with the next tick. Note that the effects of this operation
   will not be visible until the next tick.
   
   For tag components, a value of true applies the flag to the entity,
   while a value of false removes the flag from the entity.

   :param entityId: Entity ID.
   :type entityId: integer
   :param value: New component value.
   :type value: Component value type, or boolean for tags
```

#### Example

```{code-block} lua
:caption: Using `Set(entityId, value) to update a component.
:emphasize-lines: 2
-- Apply the drawable tag to the entity.
Components.Drawable.Set(entityId, true)

-- Component values are updated at the boundary between ticks, so the
-- earlier call to Set only enqueued an update. The result will not
-- be visible until the next tick, as demonstrated here:
if (Components.Drawable.Get(entityId)) then
    -- This path never executes (assuming the entity did not already
    -- have the tag applied) as component updates do not take effect
    -- until the following tick.
    -- ...
end
```

### Add(entityId, value)

#### Definition

```{eval-rst}
.. lua:function:: Components.<component>.Add(entityId, value)

   Enqueues an update to the component value which adds the given
   value to the current component value. This operation takes effect
   on the next tick and is cumulative with any other component changes.
   
   This function is only supported for component types which define
   an addition operation. Attempting to use this function with other
   component types will result in no effect with an error being logged.
   
   :param entityId: Entity ID.
   :type entityId: integer
   :param value: Amount to be added to the current component value.
   :type value: Component value type
```

#### Example

```{code-block} lua
:caption: Using `Add(entityId, value)` to update a component.
:emphasize-lines: 3
-- Decrease the entity's health by a fixed amount.
local currentHealth = Components.Health.Get(entityId)
Components.Health.Add(entityId, -10)

-- Component values are updated at the boundary between ticks, so the
-- earlier call to Add only enqueued an update. The result will not
-- be visible until the next tick, as demonstrated here:
if (Components.Health.Get(entityId) == currentHealth) then
    -- Entity health has not been changed yet - this branch executes.
    Util.LogInfo("Entity health has not changed yet.")
end
```

### Multiply(entityId, value)

#### Definition

```{eval-rst}
.. lua:function:: Components.<component>.Multiply(entityId, value)

   Enqueues an update to the component value which multiplies the current
   component value by the given value. This operation takes effect
   on the next tick and is cumulative with any other component changes.
   
   This function is only supported for component types which define
   a multiplication operation. Attempting to use this function with other
   component types will result in no effect with an error being logged.
   
   :param entityId: Entity ID.
   :type entityId: integer
   :param value: Amount by which to multiply the current component value.
   :type value: Component value type
```

#### Example

```{code-block} lua
:caption: Using `Multiply(entityId, value)` to update a component.
:emphasize-lines: 3
-- Deal poison damage for 5% of the entity's current health.
local currentHealth = Components.Health.Get(entityId)
Components.Health.Multiply(entityId, 0.95)

-- Component values are updated at the boundary between ticks, so the
-- earlier call to Multiply only enqueued an update. The result will not
-- be visible until the next tick, as demonstrated here:
if (Components.Health.Get(entityId) == currentHealth) then
    -- Entity health has not been changed yet - this branch executes.
    Util.LogInfo("Entity health has not changed yet.")
end
```

### Divide(entityId, value)

#### Definition

```{eval-rst}
.. lua:function:: Components.<component>.Divide(entityId, value)

   Enqueues an update to the component value which divides the current
   component value by the given value. This operation takes effect
   on the next tick and is cumulative with any other component changes.
   
   This function is only supported for component types which define
   a division operation. Attempting to use this function with other
   component types will result in no effect with an error being logged.
   
   :param entityId: Entity ID.
   :type entityId: integer
   :param value: Amount by which to divide the current component value.
   :type value: Component value type
```

#### Example

```{code-block} lua
:caption: Using `Divide(entityId, value)` to update a component.
:emphasize-lines: 5
-- Sacrifice half of entity's health to fully restore own mana.
local currentHealth = Components.Health.Get(entityId)
local maxMana = Components.Max_Mana.Get(entityId)
Components.Mana.Set(entityId, maxMana)
Components.Health.Divide(entityId, 2.0)

-- Component values are updated at the boundary between ticks, so the
-- earlier call to Divide only enqueued an update. The result will not
-- be visible until the next tick, as demonstrated here:
if (Components.Health.Get(entityId) == currentHealth) then
    -- Entity health has not been changed yet - this branch executes.
    Util.LogInfo("Entity health has not changed yet.")
end
```

## Kinematics Functions

### SetVelocity(entityId, value)

#### Definition

```{eval-rst}
.. lua:function:: Components.kinematics.SetVelocity(entityId, value)

   Enqueues an update to a kinematics component value which changes
   the velocity value only, leaving the position value unchanged.
   The new value is provided as a full `Kinematics` object where the
   position is ignored. This option takes effect on the next tick.
    
   :param entityId: Entity ID.
   :type entityId: integer
   :param value: Kinematics object with velocity to use.
   :type value: Kinematics
```

#### Example

```{code-block} lua
:caption: Using `SetVelocity(entityId, value)` to update a component.
:emphasize-lines: 2,3,4,5
-- Stop any entity motion.
Components.Kinematics.SetVelocity(entityId, {
  Position = { X = 0.0, Y = 0.0, Z = 0.0 }, -- ignored
  Velocity = { X = 0.0, Y = 0.0, Z = 0.0 }  -- new velocity
})
```

### AddPosition(entityId, value)

#### Definition

```{eval-rst}
.. lua:function:: Components.kinematics.AddPosition(entityId, value)

   Enqueues an update to a kinematics component value which adds to the
   position only, leaving the velocity value unchanged.
   The new value is provided as a full `Kinematics` object where the
   velocity is ignored. This option takes effect on the next tick.
    
   :param entityId: Entity ID.
   :type entityId: integer
   :param value: Kinematics object with position increment to use.
   :type value: Kinematics
```

#### Example

```{code-block} lua
:caption: Using `AddPosition(entityId, value)` to update a component.
:emphasize-lines: 2,3,4,5
-- Shift the entity by one block upward (+z direction).
Components.Kinematics.AddPosition(entityId, {
  Position = { X = 0.0, Y = 0.0, Z = 1.0 }, -- position step
  Velocity = { X = 0.0, Y = 0.0, Z = 0.0 }  -- ignored
})
```
