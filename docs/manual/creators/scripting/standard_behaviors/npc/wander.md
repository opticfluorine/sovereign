# Sovereign/Npc/Wander Behavior

## Behavior

The Sovereign/Npc/Wander behavior allows an NPC to randomly wander about the world.
The NPC will take a step in a random cardinal direction with a random delay between
steps. The initial delay to the first step after load is randomized to minimize the
correlation of movement between entities that are loaded at the same time.

This behavior does not stay within a certain area or avoid obstacles (including drops).

## Entity Hooks

| Hook   | Script               | Function |
| ------ | -------------------- | -------- |
| Load   | Sovereign/Npc/Wander | OnLoad   |
| Unload | Sovereign/Npc/Wander | OnUnload |

## Parameters

All parameters are stored per entity as entity key-value data.

| Parameter                    | Type  | Required? | Description                               |
| ---------------------------- | ----- | --------- | ----------------------------------------- |
| Sovereign.Wander.WanderStep  | Float | Required  | Distance per step in world units.         |
| Sovereign.Wander.WanderDelay | Float | Required  | Average delay between steps in seconds.   |
| Sovereign.Wander.WanderSpeed | Float | Required  | Movement speed in world units per second. |
