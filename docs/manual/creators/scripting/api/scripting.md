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

### AddEntityParameterHint(callbackFunctionName, entityParameter)

```{eval-rst}
.. lua:function:: scripting.AddEntityParameterHint(callbackFunctionName, entityParameter)

   Adds an entity parameter hint for the named callback function. A parameter hint is the name of a key in
   the entity key-value store that is referenced by the given callback function. Registering a parameter hint
   allows the editor GUIs to prompt the creator to specify a value for the parameter when binding the
   callback to an entity or template entity.

   :param callbackFunctionName: Callback function name.
   :type callbackFunctionName: string
   :param entityParameter: Name of a key in the entity key-value store.
   :type entityParameter: string
```

#### Example

```{code-block} lua
:caption: Registering an entity parameter hint.
:emphasize-lines: 8
function my_callback(entityId)
    -- ...
    local entityData = data.GetEntityData(entityId)
    local myEntityParameter = entityData["MyEntityKey"]
    -- ...
end

scripting.AddEntityParameterHint("my_callback", "MyEntityKey")
```
