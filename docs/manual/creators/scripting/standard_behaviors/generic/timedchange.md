# Sovereign/Generic/TimedChange Behavior

## Behavior

The Sovereign/Generic/TimedChange behavior defines a simple mechanism for entities to change their template after a fixed amount of in-game time has passed. This behavior correctly accounts for time that has passed while the entity was unloaded; if the entity is loaded again after its change time has passed, the change will be immediately applied.

Chains of template changes are supported by using the TimedChange behavior for each template in the chain. The behavior sets an entity property `Sovereign.TimedChange.PrevTime` at the end of each change, and any following change will use this value as its starting time. If you interrupt the chain with a different behavior, ensure your custom behavior removes the `PrevTime` property before changing to a template which uses TimedChange again, otherwise the change may occur earlier than expected.

## Entity Hooks

| Hook   | Script                        | Function |
| ------ | ----------------------------- | -------- |
| Load   | Sovereign/Generic/TimedChange | OnLoad   |
| Unload | Sovereign/Generic/TimedChange | OnUnload |

## Parameters

All parameters are stored per entity as entity key-value data.

| Parameter                        | Type  | Required? | Description                                             |
| -------------------------------- | ----- | --------- | ------------------------------------------------------- |
| Sovereign.TimedChange.NextId     | Int   | Required  | Template ID of the next template in the chain. |
| Sovereign.TimedChange.ChangeTime | Float | Required  | In-game seconds that must elapse before a change.       |

## Entity Properties

The behavior uses the following entity properties for timing. These properties are considered part of the behavior's public interface and may be safely referenced by other scripts.

| Property                            | Type  | Description                                                                                                    |
| ----------------------------------- | ----- | -------------------------------------------------------------------------------------------------------------- |
| Sovereign.TimedChange.PrevTime      | Float | In-game time at which the previous change occurred (used for chaining).                                        |
| Sovereign.TimedChange.[ID].NextTime | Float | Absolute in-game time at which the next change should occur. [ID] is the template ID or "Self" if no template. |
