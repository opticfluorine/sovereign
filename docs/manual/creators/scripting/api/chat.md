# Chat Module

The `Chat` module provides APIs for sending chat messages to players.

## Chat Message Functions

### SendSystemMessage(playerEntityId, message)

#### Definition

```{eval-rst}
.. lua:function:: Chat.SendSystemMessage(playerEntityId, message)

   Sends a system message to a specific player. System messages are
   informational text intended to communicate the result of a chat
   command or similar (e.g. the response to the `/help` command).
   
   :param playerEntityId: Entity ID of the target player.
   :type playerEntityId: lightuserdata
   :param message: Message.
   :type message: string
```

#### Example

```{code-block} lua
:caption: Sending a system chat message to a player using Chat.SendSystemMessage.
:emphasize-lines: 1
Chat.SendSystemMessage(playerEntityId, "This is a system message.")
```

(script-chat-sendtoplayer)=
### SendToPlayer(playerEntityId, color, message)

#### Definition

```{eval-rst}
.. lua:function:: Chat.SendToPlayer(playerEntityId, color, message)

   Sends a generic chat message to a specific player.
   
   :param playerEntityId: Entity ID of the target player.
   :type playerEntityId: lightuserdata
   :param color: RGB text color.
   :type color: integer
   :param message: Message.
   :type message: string
```

#### Example

```{code-block} lua
:caption: Sending a chat message to a player using Chat.SendToPlayer.
:emphasize-lines: 1, 2
Chat.SendToPlayer(playerEntityId, Color.Rgb(210, 210, 0),
    "This is a message sent with SendToPlayer.")
```

### SendToAll(color, message)

#### Definition

```{eval-rst}
.. lua:function:: Chat.SendToAll(color, message)

   Sends a generic chat message to all players.
   
   :param color: RGB text color.
   :type color: integer
   :param message: Message.
   :type message: string
```

#### Example

```{code-block} lua
:caption: Sending a chat message to all players using Chat.SendToAll.
:emphasize-lines: 1
Chat.SendToAll(Color.Rgb(210, 210, 0), "This is a message sent with SendToAll.")
```

### AddCommand)command, callback, flags)

#### Definition

```{eval-rst}
.. lua:function:: Chat.AddCommand(command, callback, flags)

   Adds a new chat command for all players.

   :param command: Command to add (case-insensitive).
   :type command: string
   :param callback: Callback function. When ``flags`` is ``ChatCommandFlags.None`` (the default), the first parameter is any text entered after the command and the second parameter is the entity ID of the player who sent the command. When ``flags`` is ``ChatCommandFlags.CommaSeparatedArgs``, the first parameter is a 1-indexed table of trimmed strings produced by splitting the command remainder on commas, and the second parameter is the entity ID of the player who sent the command.
   :type callback: function
   :param flags: Behaviour flags (see ChatCommandFlags).
   :type flags: ChatCommandFlags
```

#### ChatCommandFlags

The following flags are defined in the ``ChatCommandFlags`` library:

```{eval-rst}
.. lua:attribute:: ChatCommandFlags.None

   No special behaviour. The callback receives the raw command remainder.

.. lua:attribute:: ChatCommandFlags.CommaSeparatedArgs

   Splits the command remainder on commas, trims whitespace from each token, and passes the resulting 1-indexed table of strings to the callback as its first argument. An empty remainder yields a one-element table ``{""}``.
```

#### Example

```{code-block} lua
:caption: Adding a "/hello" chat command using Chat.AddCommand.
:emphasize-lines: 6
function chat_hello(msg, senderEntityId)
   Chat.SendSystemMessage(senderEntityId, "Hello World!")
end

-- Typing "/hello" in chat will send a Hello World message.
Chat.AddCommand("hello", chat_hello, ChatCommandFlags.None)
```