# Sovereign/Npc/Spawn Behavior

## Behavior

The Sovereign/Npc/Spawn behavior provides a basic NPC spawner. Each spawner will spawn up to a specific number of NPCs (default: 1) somewhere within a fixed radius (default: 8.0 world units) of the spawner location. NPCs are spawned one at a time every `Sovereign.Spawn.Delay` seconds (default: 30.0).

The spawner checks periodically if it needs to respawn NPCs, and will only spawn new NPCs if the current number of spawned NPCs is less than the configured maximum. Spawned NPCs are placed at random positions within the specified radius around the spawner.

## Entity Hooks

| Hook   | Script              | Function |
| ------ | ------------------- | -------- |
| Load   | Sovereign/Npc/Spawn | OnLoad   |
| Unload | Sovereign/Npc/Spawn | OnUnload |

## Parameters

All parameters are stored per entity as entity key-value data.

| Parameter                  | Type  | Required? | Description                                 | Default |
| -------------------------- | ----- | --------- | ------------------------------------------- | ------- |
| Sovereign.Spawn.TemplateId | Int   | Required  | Absolute template ID for the spawned NPCs.  |         |
| Sovereign.Spawn.Delay      | Float | Optional  | Delay between spawns in seconds.            | 30.0    |
| Sovereign.Spawn.Radius     | Float | Optional  | Spawn radius in world units.                | 8.0     |
| Sovereign.Spawn.Count      | Int   | Optional  | Maximum number of spawned NPCs at one time. | 1       |
