# Inventory Module

The `Inventory` module provides APIs for interacting with inventories.

Entities that have inventories store items in one or more slots. These slots are numbered by the *slot index*. For consistency with Lua, the first slot index is numbered 1 in scripts (this differs from the internal behavior of the engine which numbers slots beginning with 0).

## GetInventory(entityId)

### Definition

```{eval-rst}
.. lua:function:: Inventory.GetInventory(entityId)

   Gets the inventory for a player as a list of item entity IDs in order of inventory slots. Empty slots are listed with entity ID 0 (i.e. ``Entities.ToEntityId(0)``).

   :param entityId: Entity ID.
   :type entityId: lightuserdata

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
   :type entityId: lightuserdata
   :param slotIndex: Slot index. 
   :type slotIndex: integer

   :return: Item ID, or entity ID 0 (``Entities.ToEntityId(0)``) if the slot is empty or does not exist.
   :rtype: lightuserdata
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
   :type entityId: lightuserdata
   :param itemId: Item entity ID. 
   :type itemId: lightuserdata

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
   :type entityId: lightuserdata
   :param templateId: Item template entity ID. 
   :type templateId: lightuserdata

   :return: Item entity ID, or entity ID 0 (``Entities.ToEntityId(0)``) if a matching item is not found in the entity's inventory.
   :rtype: lightuserdata
```

### Example

```{code-block} lua
:caption: Using `FindFirstMatchingItem` to check if an entity is holding a specific type of item.
:emphasize-lines: 1
if Inventory.FindFirstMatchingItem(entityId, templateId) ~= Entities.ToEntityId(0) then
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
   :type entityId: lightuserdata

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
   :type entityId: lightuserdata
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
   :type entityId: lightuserdata
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
   :type entityId: lightuserdata
   :param itemId: Item entity ID.
   :type itemId: lightuserdata
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
   :type entityId: lightuserdata
   :param itemId: Item entity ID.
   :type itemId: lightuserdata
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
   :type entityId: lightuserdata
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
   :type entityId: lightuserdata
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
   :type entityId: lightuserdata
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

## SwapAsActor

### Definition

```{eval-rst}
.. lua:function:: Inventory.SwapAsActor(actorId, inventoryId1, slotIndex1, inventoryId2, slotIndex2)

   Swaps two slots between potentially different inventories, acting on behalf of an actor. The usual restrictions on the actor (e.g. range to inventory, permissions checks) are applied; if the restrictions are not met, this function will fail silently.

   :param actorId: Actor entity ID performing the swap.
   :type actorId: lightuserdata
   :param inventoryId1: Entity ID of the first inventory.
   :type inventoryId1: lightuserdata
   :param slotIndex1: First slot index to swap.
   :type slotIndex1: integer
   :param inventoryId2: Entity ID of the second inventory.
   :type inventoryId2: lightuserdata
   :param slotIndex2: Second slot index to swap.
   :type slotIndex2: integer
```

### Example

```{code-block} lua
:caption: Using ``SwapAsActor`` to swap items between two different inventories.
:emphasize-lines: 2
-- Swap the first slot of the player's inventory with the third slot of a chest.
Inventory.SwapAsActor(playerId, playerId, 1, chestId, 3)
```

## SwapQuantityAsActor

### Definition

```{eval-rst}
.. lua:function:: Inventory.SwapQuantityAsActor(actorId, inventoryId1, slotIndex1, inventoryId2, slotIndex2, quantity)

   Swaps a partial quantity between two slots across potentially different inventories, acting on behalf of an actor. The usual restrictions on the actor (e.g. range to inventory, permissions checks) are applied; if the restrictions are not met, this function will fail silently.

   :param actorId: Actor entity ID performing the swap.
   :type actorId: lightuserdata
   :param inventoryId1: Entity ID of the first inventory.
   :type inventoryId1: lightuserdata
   :param slotIndex1: First slot index to swap.
   :type slotIndex1: integer
   :param inventoryId2: Entity ID of the second inventory.
   :type inventoryId2: lightuserdata
   :param slotIndex2: Second slot index to swap.
   :type slotIndex2: integer
   :param quantity: Quantity to transfer from the first slot.
   :type quantity: integer
```

### Example

```{code-block} lua
:caption: Using ``SwapQuantityAsActor`` to transfer a partial stack between inventories.
:emphasize-lines: 2
-- Transfer 5 items from the player's first slot to the chest's second slot.
Inventory.SwapQuantityAsActor(playerId, playerId, 1, chestId, 2, 5)
```

## RemoveItem

### Definition

```{eval-rst}
.. lua:function:: Inventory.RemoveItem(entityId, slotIndex)

   Removes the item in an inventory slot, destroying the item entity.

   :param entityId: Entity ID.
   :type entityId: lightuserdata
   :param slotIndex: Slot index of the item to remove.
   :type slotIndex: integer
```

### Example

```{code-block} lua
:caption: Using `RemoveItem` to consume an item from a player's inventory.
:emphasize-lines: 5
-- Check whether the player is holding a key. If so, consume the key.
local keyId = Inventory.FindFirstMatchingItem(playerId, keyTemplateId)
if keyId ~= Entities.ToEntityId(0) then
    local slotIndex = Inventory.GetSlotIndexForItem(playerId, keyId)
    Inventory.RemoveItem(playerId, slotIndex)

    -- Key consumed, now do some action that was gated by the key.
    -- ...
end
```

## ConsumeItem

### Definition

```{eval-rst}
.. lua:function:: Inventory.ConsumeItem(entityId, templateId)

   Synchronously consumes (removes and destroys) a single item with the given template ID from the entity's inventory. The operation is atomic within the current server tick: once an item is claimed by this call it is locked for the remainder of the tick, preventing duplication. A second call targeting the same item in the same tick will return ``false`` by design.

   :param entityId: Entity ID.
   :type entityId: lightuserdata
   :param templateId: Item template entity ID.
   :type templateId: lightuserdata

   :return: ``true`` if an item was found and removed, ``false`` otherwise.
   :rtype: boolean
```

### Example

```{code-block} lua
:caption: Using ``ConsumeItem`` to remove a key item from a player's inventory.
:emphasize-lines: 1
if Inventory.ConsumeItem(playerId, keyTemplateId) then
    -- Key was consumed; the player had the key.
    -- ...
end
```

## UseItem

### Definition

```{eval-rst}
.. lua:function:: Inventory.UseItem(actorId, itemId, targetId)

   Uses an item as a tool on a target entity, acting on behalf of an actor. The same validation as the player use-item path is applied: the item must be held in one of the actor's hotbar slots, must have a ``UseRange`` component, and the target must be within that range. If validation succeeds on the server and the target has an interaction callback, the callback is invoked with the actor entity ID as its first argument, followed by the item entity ID and the target entity ID.

   :param actorId: Actor entity ID performing the use.
   :type actorId: lightuserdata
   :param itemId: Item entity ID to use as a tool.
   :type itemId: lightuserdata
   :param targetId: Entity ID of the target.
   :type targetId: lightuserdata
```

### Example

```{code-block} lua
:caption: Using ``UseItem`` to use a tool on a target entity.
:emphasize-lines: 1
Inventory.UseItem(playerId, toolItemId, targetId)
```

## ConsumeItemQuantity

### Definition

```{eval-rst}
.. lua:function:: Inventory.ConsumeItemQuantity(entityId, templateId, quantity)

   Synchronously consumes the requested quantity of items with the given template ID from the entity's inventory. The operation is atomic within the current server tick: if insufficient items are present, nothing is changed and ``false`` is returned. If enough items are present, the exact quantity is removed across one or more stacks and ``true`` is returned.

   Works with both stackable and non-stackable items. For non-stackable items, each matching item contributes a quantity of 1, so the function can be used to remove multiple individual items across slots.

   :param entityId: Entity ID.
   :type entityId: lightuserdata
   :param templateId: Item template entity ID.
   :type templateId: lightuserdata
   :param quantity: Required quantity. Must be at least 1.
   :type quantity: integer

   :return: ``true`` if the requested quantity was removed, ``false`` if insufficient items were available.
   :rtype: boolean
```

### Example

```{code-block} lua
:caption: Using ``ConsumeItemQuantity`` to remove multiple crafting ingredients.
:emphasize-lines: 3
-- Check if the player has 5 wood logs and consume them if so.
if Inventory.ConsumeItemQuantity(playerId, woodTemplateId, 5) then
    -- Player had 5 wood logs; they were consumed. Craft the item.
    -- ...
end
```

## GetEquipment(entityId, equipmentType)

### Definition

```{eval-rst}
.. lua:function:: Inventory.GetEquipment(entityId, equipmentType)

   Gets the item currently equipped by the player in the given equipment type slot.

   :param entityId: Entity ID.
   :type entityId: lightuserdata
   :param equipmentType: Equipment type slot to query.
   :type equipmentType: EquipmentType

   :return: Equipped item entity ID, or ``Entities.None`` if nothing is equipped in that slot.
   :rtype: lightuserdata
```

### Example

```{code-block} lua
:caption: Checking what a player has equipped as a weapon.
:emphasize-lines: 1
local weaponId = Inventory.GetEquipment(playerId, EquipmentType.Weapon)
if weaponId ~= Entities.None then
    -- The player has a weapon equipped.
    -- ...
end
```

## Equip(entityId, slotIndex)

### Definition

```{eval-rst}
.. lua:function:: Inventory.Equip(entityId, slotIndex)

   Equips the item in one of the entity's inventory slots into the matching equipment slot. If the equipment slot is occupied, the previously equipped item is moved into the source inventory slot.

   The request is applied by the authoritative server and the resulting state reaches clients via entity synchronization of the affected slots. Component and indexer updates commit at the next tick, so scripts must wait a tick between equipping and reading the result.

   :param entityId: Entity ID.
   :type entityId: lightuserdata
   :param slotIndex: Slot index of the item to equip. Must be at least 1.
   :type slotIndex: integer
```

### Example

```{code-block} lua
:caption: Equipping the item in the player's first inventory slot.
:emphasize-lines: 2
-- Equip the sword in the player's first slot as their weapon.
Inventory.Equip(playerId, 1)
```

## Unequip(entityId, equipmentType, slotIndex)

### Definition

```{eval-rst}
.. lua:function:: Inventory.Unequip(entityId, equipmentType, slotIndex)

   Unequips the player's equipped item of the given equipment type, moving it to an empty inventory slot. If the target slot is occupied or invalid, nothing is changed.

   The request is applied by the authoritative server and the resulting state reaches clients via entity synchronization of the affected slots. Component and indexer updates commit at the next tick, so scripts must wait a tick between unequipping and reading the result.

   :param entityId: Entity ID.
   :type entityId: lightuserdata
   :param equipmentType: Equipment type to unequip.
   :type equipmentType: EquipmentType
   :param slotIndex: Slot index to which the item should be moved. Must be at least 1.
   :type slotIndex: integer
```

### Example

```{code-block} lua
:caption: Unequipping a player's weapon into their first inventory slot.
:emphasize-lines: 2
-- Unequip the weapon into the player's first slot.
Inventory.Unequip(playerId, EquipmentType.Weapon, 1)
```
