# Tutorial: Creating a Login Message Script

Server-side scripting is one of the most powerful features of Sovereign Engine, allowing creators
to develop custom content and behavior for nearly every aspect of the game.
In this guide, we will create a simple script that sends a custom welcome message to players 
when they log into the game.

## Step 1: Create a new script

Create a new file called `WelcomeMessage.lua` inside your server's `Data/Scripts` directory. The server automatically loads
any Lua scripts it finds within this directory and its subdirectories at startup (or when a manual relaod is triggered with
the `/reloadscripts` or `/loadnewscripts` chat commands are used).

It is recommended to create a subdirectory to keep your scripts organized and separated 
from other official and third party scripts; for example, the official scripts that ship
with the engine are kept in a `Sovereign` subdirectory.

## Step 2: Define the callback function

Let's start our script by defining a new callback function that sends a welcome message when a player
enters the world:

```{code-block} lua
:lineno-start: 1
function send_welcome_message(event)
```

Sovereign scripts are based around callback functions that are triggered by events. When you write a new script, you
will add one or more callback functions that will be called by the server. Some events such as `Core_Tick` have no
details and expect a callback without any arguments. Other events have a details object that will be passed to the
callback as its only argument. More information regarding the supported events and their details objects can be
found in [Supported Event Types](#script-api-event-types).

```{code-block} lua
:lineno-start: 2
    if event and event.EntityId then
        chat.SendToPlayer(event.EntityId, color.MOTD, "Welcome back!")
```

Generally, it is considered a best practice to check for existence of the details fields before accessing them.
In Lua, an absent object or field will return `nil` when read instead of raising an error, and this `nil` value
can propagate and introduce bugs that are difficult to track down.

We then send a message to the player who logged in using the [`chat.SendToPlayer`](#script-chat-sendtoplayer) function.
We address the player by their *entity ID*, a 64-bit integer that uniquely identifies the entity that corresponds
to the player character. Most scripting functions that interact with players do so through the entity ID. We also
use a standard color from the [`color`](#script-color) module. Finally, we provide the message to be sent to the
player.

:::{tip}
While we used a constant string for this example, you can use Lua's `string.format` function to include dynamic
values in the string.
:::

```{code-block} lua
:lineno-start: 4
    else
        util.LogError("Bad event data in callback.")
    end
end
```

Finally, we do some basic error handling by logging an error if any of the earlier `nil` checks failed.
For this we use the [`util.LogError`](#script-util-logerror) function which takes a message and logs it to the server error log.
The log message will appear in the server log labeled by the script that produced it.

Putting it all together, we get this callback function:

```{code-block} lua
:lineno-start: 1
function send_welcome_message(event)
    if event and event.EntityId then
        chat.SendToPlayer(event.EntityId, color.MOTD, "Welcome back!")
    else
        util.LogError("Bad event data in callback.")
    end
end
```

## Step 3: Register the callback function

Next, let's register our new callback function with the scripting engine:

```{code-block} lua
:lineno-start: 9
scripting.AddEventCallback(events.Server_Persistence_PlayerEnteredWorld, send_welcome_message)
```

Callbacks are only called if they are registered with the scripting engine through a call to
[`scripting.AddEventCallback`](#script-scripting-addeventcallback). These registration calls
are usually placed in the *global scope* outside of any function. When the scripting engine
loads a script, it executes any code in the global scope immediately, making it the perfect
place to register callbacks.

## Complete code

Here's the complete code for our welcome script:

```{code-block} lua
:lineno-start: 1
function send_welcome_message(event)
    if event and event.EntityId then
        chat.SendToPlayer(event.EntityId, color.MOTD, "Welcome back!")
    else
        util.LogError("Bad event data in callback.")
    end
end

scripting.AddEventCallback(events.Server_Persistence_PlayerEnteredWorld, send_welcome_message)
```

To load this script, you can do one of several options:

* Restart the server. The server automatically loads all scripts in the `Data/Scripts` directory
  at startup.
* If you don't want to restart the server, you can use the `/reloadscripts` chat command from
  an admin player to reload all scripts without taking the server offline.
* If you don't want to interrupt any currently loaded scripts, you can use the `/loadnewscripts`
  command to load any new scripts that have not already been loaded. If the script was already
  loaded, you can use the `/reloadscript <name>` command instead (e.g. `/reloadscript MySubdir/WelcomeMessage`
  where `MySubdir` is replaced by the name of your subdirectory under the `Data/Scripts` directory).
