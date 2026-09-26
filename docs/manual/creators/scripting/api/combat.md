# combat Module

The `Combat` module provides APIs for performing attacks.

## Combat Functions

### Attack(actorEntityId)

#### Definition

```{eval-rst}
.. lua:function:: Combat.Attack(actorEntityId)

   Requests that the given actor perform an attack. The attack is validated
   and applied by the server's combat system; any valid targets within the
   actor's attack range and facing direction are damaged.

   :param actorEntityId: Entity ID of the attacking actor.
   :type actorEntityId: lightuserdata
```

#### Example

```{code-block} lua
:caption: Performing an attack using Combat.Attack.
:emphasize-lines: 1
Combat.Attack(npcEntityId)
```
