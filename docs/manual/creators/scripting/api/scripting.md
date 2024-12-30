# scripting Module

The `scripting` module provides APIs for interacting with the server-side
scripting engine. Primarily this takes the form of registering callbacks
with which the scripting engine will invoke a script under certain
conditions (e.g. a specific event is received).

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
:caption: Registering an event callback using `scripting.AddEventCallback(eventId, callback).
:emphasize-lines: 6
function on_player_entered(event)
    local playerEntityId = event.EntityId
    util.LogInfo(string.format("Player ID %s has logged in.", playerEntityId))
end

scripting.AddEventCallback(events.Server_Persistence_PlayerEnteredWorld, on_player_entered)
```
