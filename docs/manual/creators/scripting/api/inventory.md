# Inventory Module

The `Inventory` module provides APIs for interacting with inventories.

Entities that have inventories store items in one or more slots. These slots are numbered by the *slot index*. For consistency with Lua, the first slot index is numbered 1 in scripts (this differs from the internal behavior of the engine which numbers slots beginning with 0).

## GetInventory(entityId)

### Definition

```{eval-rst}
.. lua:function:: Inventory.GetInventory(entityId)

   Gets the inventory for a player as a list of item entity IDs in order of inventory slots. Empty slots are listed with an item ID of 0.

   :param entityId: Entity ID.
   :type entityId: integer

   :return: Inventory for the entity.
   :rtype: table
```

### Example

```{code-block} lua
:caption: Using `GetInventory` to get an entity's inventory.
:emphasize-lines: 1
local inv = Inventory.GetInventory(entityId)
```

## GetItem(entityId, slotIndex)

### Definition

```{eval-rst}
.. lua:function:: Inventory.GetItem(entityId, slotIndex)

   Gets the ID of the item in a particular inventory slot.

   :param entityId: Entity ID.
   :type entityId: integer
   :param slotIndex: Slot index. 
   :type slotIndex: integer

   :return: Item ID, or 0 if the slot is empty or does not exist.
   :rtype: integer
```

### Example

```{code-block} lua
:caption: Using `GetItem` to get the contents of a specific inventory slot.
:emphasize-lines: 2
-- Gets the fourth item in the entity's inventory. Note one-based indexing.
local itemId = Inventory.GetItem(entityId, 4)
```

## GetSlotIndexForItem(entityId, itemId)

### Definition

```{eval-rst}
.. lua:function:: Inventory.GetSlotIndexForItem(entityId, itemId)

   Gets the slot index for the given item if it is in the entity's inventory.

   :param entityId: Entity ID.
   :type entityId: integer
   :param itemId: Item entity ID. 
   :type itemId: integer

   :return: Slot index, or 0 if the item is not in a slot belonging to the entity.
   :rtype: integer
```

### Example

```{code-block} lua
:caption: Checking if an item is held by a player using `GetSlotIndexForItem`.
:emphasize-lines: 2
-- Checks whether an item is held by an entity.
local slotIndex = Inventory.GetSlotIndexForItem(entityId, itemId)
if slotIndex > 0 then
    -- Item is held by the entity.
    -- ...
else
    -- Item is not held by the entity.
    -- ...
end
```

## FindFirstMatchingItem(entityId, templateId)

### Definition

```{eval-rst}
.. lua:function:: Inventory.FindFirstMatchingItem(entityId, templateId)

   Finds the first item (in slot order) in the entity's inventory that has the given template ID.

   :param entityId: Entity ID.
   :type entityId: integer
   :param templateId: Item template entity ID. 
   :type templateId: integer

   :return: Item entity ID, or 0 if a matching item is not found in the entity's inventory.
   :rtype: integer
```

### Example

```{code-block} lua
:caption: Using `FindFirstMatchingItem` to check if an entity is holding a specific type of item.
:emphasize-lines: 1
if Inventory.FindFirstMatchingItem(entityId, templateId) > 0 then
    -- The entity is holding an item with the template.
    -- ...
end
```

## GetSlotCount

### Definition

```{eval-rst}
.. lua:function:: Inventory.GetSlotCount(entityId)

   Gets the number of slots in the entity's inventory.

   :param entityId: Entity ID.
   :type entityId: integer

   :return: Number of slots in the entity's inventory.
   :rtype: integer
```

### Example

```{code-block} lua
:caption: Using `GetSlotCount` to check whether an entity has an inventory.
:emphasize-lines: 1
if Inventory.GetSlotCount(entityId) > 0 then
    -- Entity has an inventory.
    -- ...
end
```

## AddSlots

### Definition

```{eval-rst}
.. lua:function:: Inventory.AddSlots(entityId, slotCount)

   Adds one or more slots to an entity's inventory.

   :param entityId: Entity ID.
   :type entityId: integer
   :param slotCount: Number of slots to add. Must be positive.
   :type slotCount: integer
```

### Example

```{code-block} lua
:caption: Using `AddSlots` to expand an entity's inventory.
:emphasize-lines: 2
-- Add eight extra slots to an entity's inventory.
Inventory.AddSlots(entityId, 8)
```

## GetEmptySlot

### Definition

```{eval-rst}
.. lua:function:: Inventory.GetEmptySlot(entityId)

   Gets an empty slot in the entity's inventory. Note that the slot will be flagged as temporarily unavailable for the remainder of the server tick to reduce the risk of multiple scripts competing for the same empty slot.

   :param entityId: Entity ID.
   :type entityId: intege
   :return: Empty slot index, or 0 if no empty slot was found.
   :rtype: integer
```

### Example

```{code-block} lua
:caption: Using `GetEmptySlot` to find an empty inventory slot.
:emphasize-lines: 2
-- Check if the player has an empty inventory slot.
local emptySlotIndex = Inventory.GetEmptySlot(playerId)
if emptySlotIndex > 0 then
   -- emptySlotIndex is an empty slot.
   -- ...
end
```

## AddItem

### Definition

```{eval-rst}
.. lua:function:: Inventory.AddItem(entityId, itemId)

   Tries to add an existing item to a free slot in the entity's inventory.

   :param entityId: Entity ID.
   :type entityId: integer
   :param itemId: Item entity ID.
   :type itemId: integer
   :return: true if the item was added to a free slot, false otherwise.
   :rtype: boolean
```

### Example

```{code-block} lua
-- Try to place an item in the player's inventory, e.g. as a quest reward.
local itemId = Entities.Create(...)
if Inventory.AddItem(playerId, itemId) then
   -- Item is now in the player's inventory.
   -- ...
else
   -- Item could not be added to a free slot; clean up, try again later.
   Entities.Remove(itemId)
   -- ...
end
```

## PickUp

### Definition

```{eval-rst}
.. lua:function:: Inventory.PickUp(entityId, itemId)

   Picks up an item into the entity's inventory. The item must be within range of the entity.

   :param entityId: Entity ID.
   :type entityId: integer
   :param itemId: Item entity ID.
   :type itemId: integer
```

### Example

```{code-block} lua
:caption: Using `PickUp` to pick up an item into a player's inventory.
:emphasize-lines: 2
-- Pick up an item near the player.
Inventory.PickUp(playerId, swordItemId)
```

## Drop

### Definition

```{eval-rst}
.. lua:function:: Inventory.Drop(entityId, slotIndex)

   Drops the item in a slot at the position of the entity.

   :param entityId: Entity ID.
   :type entityId: integer
   :param slotIndex: Slot index of the item to drop.
   :type slotIndex: integer
```

### Example

```{code-block} lua
:caption: Using `Drop` to drop an item from a player's inventory.
:emphasize-lines: 2
-- Drop the item in the player's second slot at the player's current position.
Inventory.Drop(playerId, 2)
```

## DropAt

### Definition

```{eval-rst}
.. lua:function:: Inventory.DropAt(entityId, slotIndex, dropPosition)

   Drops the item in a slot at a specific position. The position must be within the allowed drop range.

   :param entityId: Entity ID.
   :type entityId: integer
   :param slotIndex: Slot index of the item to drop.
   :type slotIndex: integer
   :param dropPosition: Position to drop the item.
   :type dropPosition: Vector3
```

### Example

```{code-block} lua
:caption: Using `DropAt` to drop an item at a specific position.
:emphasize-lines: 4
-- Drop the item in the player's second slot to the right of the player.
local playerPos = Components.Kinematics.Get(playerId).Position
local dropPos = { X = playerPos.X + 1.0, Y = playerPos.Y, Z = playerPos.Z }
Inventory.DropAt(playerId, 2, dropPos)
```

## Swap

### Definition

```{eval-rst}
.. lua:function:: Inventory.Swap(entityId, slotIndex1, slotIndex2)

   Swaps two slots in an entity's inventory.

   :param entityId: Entity ID.
   :type entityId: integer
   :param slotIndex1: First slot index to swap.
   :type slotIndex1: integer
   :param slotIndex2: Second slot index to swap.
   :type slotIndex2: integer
```

### Example

```{code-block} lua
:caption: Using `Swap` to exchange two items in an inventory.
:emphasize-lines: 2
-- Swap the first two slots in the player's inventory.
Inventory.Swap(playerId, 1, 2)
```

## RemoveItem

### Definition

```{eval-rst}
.. lua:function:: Inventory.RemoveItem(entityId, slotIndex)

   Removes the item in an inventory slot, destroying the item entity.

   :param entityId: Entity ID.
   :type entityId: integer
   :param slotIndex: Slot index of the item to remove.
   :type slotIndex: integer
```

### Example

```{code-block} lua
:caption: Using `RemoveItem` to consume an item from a player's inventory.
:emphasize-lines: 5
-- Check whether the player is holding a key. If so, consume the key.
local keyId = Inventory.FindFirstMatchingItem(playerId, keyTemplateId)
if keyId > 0 then
    local slotIndex = Inventory.GetSlotIndexForItem(playerId, keyId)
    Inventory.RemoveItem(playerId, slotIndex)

    -- Key consumed, now do some action that was gated by the key.
    -- ...
end
```
