(script-scripting)=
# scripting Module

:::{contents}
:local:
:depth: 2
:::

The `scripting` module provides APIs for interacting with the server-side scripting engine. Primarily this takes the form of registering callbacks with which the scripting engine will invoke a script under certain conditions (e.g. a specific event is received, a timer has elapsed, etc.).

## Callback Registration Functions

(script-scripting-addeventcallback)=
### AddEventCallback(eventId, callback)

#### Definition

```{eval-rst}
.. lua:function:: scripting.AddEventCallback(eventId, callback)

   Registers a callback to handle the given type of event.
   
   :param eventId: Event ID that this callback should respond to.
   :type eventId: integer
   :param callback: Callback function. The function must accept a single
                    argument having the event details type, or zero
                    arguments for event types without details.
   :type callback: function
```

#### Example

```{code-block} lua
:caption: Registering an event callback using `scripting.AddEventCallback(eventId, callback)`.
:emphasize-lines: 6
function on_player_entered(event)
    local playerEntityId = event.EntityId
    util.LogInfo(string.format("Player ID %s has logged in.", playerEntityId))
end

scripting.AddEventCallback(events.Server_Persistence_PlayerEnteredWorld, on_player_entered)
```

(script-scripting-addtimedcallback)=
### AddTimedCallback(delaySeconds, callback, [argument])

```{eval-rst}
.. lua:function:: scripting.AddTimedCallback(delaySeconds, callback, [argument])

   Registers a timed callback that will be triggered after a delay period.

   :param delaySeconds: Delay in seconds before the callback is invoked. Must be positive and finite.
   :type delaySeconds: number
   :param callback: Callback function.
   :type callback: function
   :param argument: An argument to pass to the callback. (Optional, defaults to `nil`)
   :type argument: any
```

#### Example

```{code-block} lua
:caption: Registering a timed callback using `scripting.AddTimedCallback(delaySeconds, callback, argument)`.
:emphasize-lines: 3,6
function on_timer(count)
    util.LogInfo("Callback " .. count)
    scripting.AddTimedCallback(1.0, on_timer, count + 1)
end

scripting.AddTimedCallback(1.0, on_timer, 0)
```

### AddCollisionCallback(entityId, callback)

```{eval-rst}
.. lua:function:: scripting.AddCollisionCallback(entityId, callback)

   Registers a callback that is called whenever the given entity collides with another object (either a block or non-block entity). The callback will be called whenever the entity stops moving as a result of collision; it is not called if the entity is not moving and another object collides with it.

   :param entityId: Entity ID to listen for collisions on.
   :type entityId: number
   :param callback: Callback function, which may accept the entity ID as its first argument.
   :type callback: function
   
   :return: Callback handle (needed for removing the callback later).
   :rtype: number
```

#### Example

```{code-block} lua
:caption: Adding an entity collision callback using `scripting.AddCollisionCallback(entityId, callback)`.
:emphasize_lines: 5
function on_collision(entityId)
    -- ...
end

local collisionHandle = scripting.AddCollisionCallback(entityId, on_collision)
```

### RemoveCollisionCallback(entityId, callbackHandle)

```{eval-rst}
.. lua:function:: scripting.RemoveCollisionCallback(entityId, callbackHandle)
   
   Removes a collision callback that was previously registered by the same script. If no such callback is found, this function logs an error and otherwise does nothing.

   :param entityId: Entity ID on which the callback was registered.
   :type entityId: number
   :param callbackHandle: Callback handle returned by the corresponding call to `scripting.AddCollisionCallback(entityId, callback)`.
   :type callbacHanle: number
```

#### Example

```{code-block} lua
:caption: Removing an entity collision callback using `scripting.RemoveCollisionCallback(entityId, callbackHandle)`.
:emphasize_lines: 7
function on_collision(entityId)
    -- ...
end

local collisionHandle = scripting.AddCollisionCallback(entityId, on_collision)
-- ...
scripting.RemoveCollisionCallback(entityId, collisionHandle)
```

### AddEntityParameterHint(callbackFunctionName, parameterName, type, tooltip)

```{eval-rst}
.. lua:function:: scripting.AddEntityParameterHint(callbackFunctionName, parameterName, type, tooltip)

   Adds an entity parameter hint for the named callback function. A parameter hint is the name of a key in
   the entity key-value store that is referenced by the given callback function. Registering a parameter hint
   allows the editor GUIs to prompt the creator to specify a value for the parameter when binding the
   callback to an entity or template entity.

   :param callbackFunctionName: Callback function name.
   :type callbackFunctionName: string
   :param parameterName: Name of a key in the entity key-value store.
   :type parameterName: string
   :param type: Parameter type (see table below).
   :type type: string
   :param tooltip: Tooltip text for the parameter.
   :type tooltip: string
```

#### Supported Types for AddEntityParameterHint

The `type` argument for `scripting.AddEntityParameterHint` must be one of the following values:

| Type       | Description              |
|------------|--------------------------|
| `"String"` | String-valued parameter  |
| `"Int"`    | Integer-valued parameter |
| `"Float"`  | Float-valued parameter   |
| `"Bool"`   | Boolean-valued parameter |

#### Example

```{code-block} lua
:caption: Registering an entity parameter hint with type and tooltip.
:emphasize-lines: 8
function my_callback(entityId)
    -- ...
    local entityData = data.GetEntityData(entityId)
    local myEntityParameter = entityData["MyEntityKey"]
    -- ...
end

scripting.AddEntityParameterHint("my_callback", "MyEntityKey", "String", "This is my entity parameter.")
```
