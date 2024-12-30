# chat Module

The `chat` module provides APIs for sending chat messages to players.

## Chat Message Functions

### SendSystemMessage(playerEntityId, message)

#### Definition

```{eval-rst}
.. lua:function:: chat.SendSystemMessage(playerEntityId, message)

   Sends a system message to a specific player. System messages are
   informational text intended to communicate the result of a chat
   command or similar (e.g. the response to the `/help` command).
   
   :param playerEntityId: Entity ID of the target player.
   :type playerEntityId: integer
   :param message: Message.
   :type message: string
```

#### Example

```{code-block} lua
:caption: Sending a system chat message to a player using chat.SendSystemMessage.
:emphasize-lines: 1
chat.SendSystemMessage(playerEntityId, "This is a system message.")
```

(script-chat-sendtoplayer)=
### SendToPlayer(playerEntityId, color, message)

#### Definition

```{eval-rst}
.. lua:function:: chat.SendToPlayer(playerEntityId, color, message)

   Sends a generic chat message to a specific player.
   
   :param playerEntityId: Entity ID of the target player.
   :type playerEntityId: integer
   :param color: RGB text color.
   :type color: integer
   :param message: Message.
   :type message: string
```

#### Example

```{code-block} lua
:caption: Sending a chat message to a player using chat.SendToPlayer.
:emphasize-lines: 1, 2
chat.SendToPlayer(playerEntityId, color.Rgb(210, 210, 0),
    "This is a message sent with SendToPlayer.")
```

### SendToAll(color, message)

#### Definition

```{eval-rst}
.. lua:function:: chat.SendToAll(color, message)

   Sends a generic chat message to all players.
   
   :param color: RGB text color.
   :type color: integer
   :param message: Message.
   :type message: string
```

#### Example

```{code-block} lua
:caption: Sending a chat message to all players using chat.SendToAll.
:emphasize-lines: 1
chat.SendToAll(color.Rgb(210, 210, 0), "This is a message sent with SendToAll.")
```
