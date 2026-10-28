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

The EntityUnloaded and EntityLoaded callbacks can also be re-triggered for a loaded entity
by an admin using the `/reloadentity` and `/reloadtemplate` commands without a full
unload/load cycle.

(script-callbacks-entity-interaction)=
## Entity Interaction Callbacks

When a player or script uses an item as a tool on a target, the engine can invoke two callbacks:

| Hook        | Attached to | Keys                                                       | Purpose                                      |
|-------------|-------------|------------------------------------------------------------|----------------------------------------------|
| `OnInteract`| Target      | `__OnInteract_Script`, `__OnInteract_Function`             | Runs behavior on the entity being used on.   |
| `OnUseItem` | Tool        | `__OnUseItem_Script`, `__OnUseItem_Function`               | Runs behavior specific to the tool being used.|

The target's `OnInteract` callback and the tool's `OnUseItem` callback are both optional. When both are
present, the engine enqueues `OnUseItem` first and `OnInteract` second, and invokes both. The two callbacks
run on independent per-entity FIFO chains, so the enqueue order is defined but the completion order is not.

Both callbacks receive four arguments, in this order:

| Argument | Type          | Meaning                                                                 |
|----------|---------------|-------------------------------------------------------------------------|
| `actor`  | `lightuserdata` | Entity ID of the player or entity using the tool.                     |
| `tool`   | `lightuserdata` | Entity ID of the item used as the tool.                               |
| `target` | `lightuserdata` | Entity ID of the target entity.                                       |
| `face`   | integer       | Targeted block face, matching `ItemUseFace` (`0` = None, `1` = front, `2` = top). |

For example:

```{code-block} lua
:caption: Handling an item use on both the tool and the target.
function OnUseItem(actor, tool, target, face)
    if face == ItemUseFace.BlockTopFace then
        -- The tool was used on the top face of a block.
    end
end

function OnInteract(actor, tool, target, face)
    -- ...
end
```

### Item Use on Block Faces

A left click resolves the clicked block face to a `ItemUseFace` value. Tools that declare an `ItemUse`
component are then gated by the matching face flag: `ItemUse.BlockFrontFace` for the front face and
`ItemUse.BlockTopFace` for the top face. A tool with no `ItemUse` component remains unrestricted, including
on block faces. A block target with no resolved face (`ItemUseFace.None`) cannot match an `ItemUse` tool and
is rejected.

When a tool's only matching flag is `ItemUse.EmptyBlock`, the use targets the empty cell adjacent to the
clicked face instead of the block itself: the cell above the block for the top face, and the cell in front
of the block (one cell in `-Y`) for the front face. The use is permitted only if that adjacent cell is empty.
The callback still receives the clicked block's entity ID as `target` and the clicked face as `face`, so the
adjacent cell can be derived from those values.
