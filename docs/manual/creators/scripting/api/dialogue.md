# dialogue Module

The `dialogue` module provides APIs for displaying dialogue to players.

## Dialogue Functions

### Show(targetEntityId, subject, message)

#### Definition

```{eval-rst}
.. lua:function:: dialogue.Show(targetEntityId, subject, message)

   Shows a dialogue message to a specific player.

   :param targetEntityId: Entity ID of the target player.
   :type targetEntityId: integer
   :param subject: Dialogue subject (e.g. who is speaking).
   :type subject: string
   :param message: Dialogue message.
   :type message: string
```

#### Example

```{code-block} lua
:caption: Showing a dialogue message to a player using dialogue.Show.
:emphasize-lines: 1
dialogue.Show(playerEntityId, "NPC Name", "Welcome to the world!")
```

### ShowProfile(targetEntityId, profileSpriteId, subject, message)

#### Definition

```{eval-rst}
.. lua:function:: dialogue.ShowProfile(targetEntityId, profileSpriteId, subject, message)

   Shows a dialogue message with a profile sprite to a specific player.

   :param targetEntityId: Entity ID of the target player.
   :type targetEntityId: integer
   :param profileSpriteId: Profile sprite ID.
   :type profileSpriteId: integer
   :param subject: Dialogue subject (e.g. who is speaking).
   :type subject: string
   :param message: Dialogue message.
   :type message: string
```

#### Example

```{code-block} lua
:caption: Showing a dialogue message with a profile sprite using dialogue.ShowProfile.
:emphasize-lines: 1
dialogue.ShowProfile(playerEntityId, 101, "NPC Name", "This message has a profile sprite.")
```
