# Callbacks

Sovereign's scripting engine provides a range of callbacks that can be used as integraiton points for scripts. This section describes the available callbacks that can be leveraged to create dynamic game content.

## Event Callbacks

Scripts may register functions to be called when a specific event is received through the event loop. A subset of Sovereign Engine's events are supported. For events that have associated details, the details are marshalled to a Lua table and passed to the callback as an argument. For more information, see [scripting.AddEventCallback](#script-scripting-addeventcallback).

## Timed Callbacks

Many scripts perform periodic actions. Rather than requiring scripts to continuously poll in a loop, the scripting engine provides *timed callbacks* which allow a function to be executed after a specified amount of time. A function can use a timed callback to create a periodic timer by repeatedly setting a timed callback on itself. For more information, see [scripting.AddTimedCallback](#script-scripting-addtimedcallback).

## Entity Lifecycle Callbacks

Entities in Sovereign Engine proceed through several different transitions during their lifecycle:

- **EntityAdded** - The entity is newly created for the first time.
- **EntityLoaded** - The entity is loaded into the active set. This transition occurs for newly added as well as existing entities.
- **EntityUnloaded** - The entity is unloaded from the active set. It may or may not continue to exist in the database.
- **EntityRemoved** - The entity is fully removed and no longer exists in memory or in the database.

EntityAdded always occurs in a pair with EntityLoaded. Similarly, EntityRemoed always occurs in a pair with EntityRemoved. EntityLoaded and EntityRemoved may also occur on their own as an existing entity is moved between memory and the database.

Each of the entity lifecycle transitions may be hooked by a script callback. These callbacks are specified by setting the following keys
in the [entity key-value store](#script-data-keyvaluedata):

|Key                           |Purpose                                                       |
|------------------------------|--------------------------------------------------------------|
|`__OnEntityAdded_Script`      |Name of the script that contains the EntityAdded callback.    |
|`__OnEntityAdded_Function`    |Name of the function to use as the EntityAdded callback.      | 
|`__OnEntityLoaded_Script`     |Name of the script that contains the EntityLoaded callback.   |
|`__OnEntityLoaded_Function`   |Name of the function to use as the EntityLoaded callback.     |
|`__OnEntityUnloaded_Script`   |Name of the script that contains the EntityUnloaded callaback.|
|`__OnEntityUnloaded_Function` |Name of the function to use as the EntityUnloaded callback.   |
|`__OnEntityRemoved_Script`    |Name of the script that contains the EntityRemoved callback.  |
|`__OnEntityRemoved_Function`  |Name of the function to use at the EntityRemoved callback.
