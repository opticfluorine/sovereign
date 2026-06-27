-- sovereign.def.lua
-- Definition file for Sovereign Engine scripting APIs.
--
-- Sovereign Engine 
-- Copyright (c) 2025 opticfluorine
--
-- This program is free software: you can redistribute it and/or modify
-- it under the terms of the GNU General Public License as published by
-- the Free Software Foundation, either version 3 of the License, or
-- (at your option) any later version.
--
-- This program is distributed in the hope that it will be useful,
-- but WITHOUT ANY WARRANTY; without even the implied warranty of
-- MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
-- GNU General Public License for more details.
-- You should have received a copy of the GNU General Public License
-- along with this program.  If not, see <https://www.gnu.org/licenses/>.
--

---@meta

--------------------

--
-- Types
--

---Bounding box used for physics calculations.
---@class BoundingBox
---@field Position Vector3 Box position offset relative to entity position in world units.
---@field Size Vector3 Box size in world units.
BoundingBox = {}

---Event details type containing a single entity ID.
---@class EntityEventDetails
---@field EntityId integer The entity ID related to the event.
EntityEventDetails = {}

---Entity specification used by entities.Create. All values are optional, nil values are ignored.
---@class EntitySpecification
---@field EntityId integer? Optional entity ID for new entity. If none provided, one will be automatically chosen.
---@field Template integer? Optional template entity ID.
---@field NonPersistent boolean? Optional. Whether entity is nonpersistent.
---@field AnimatedSprite integer? Optional. Animated sprite ID.
---@field BlockPosition GridPosition? Optional. Position for block entities.
---@field BoundingBox BoundingBox? Optional. Bounding box for physics effects.
---@field CastBlockShadows boolean? Optional. Whether entity casts block shadows when rendered.
---@field CastShadows Shadow? Optional. Shadow cast by a non-block entity.
---@field Drawable Vector2? Optional. Sets entity as drawable; value specifies position offset in projected world space.
---@field EntityType EntityType? Optional. Specifies the type of entity.
---@field Kinematics Kinematics? Optional. Position and velocity for non-block entities.
---@field Name string? Optional. Entity name.
---@field Orientation Orientation? Optional. Orientation.
---@field Parent integer? Optional. Entity ID of parent entity.
---@field Physics boolean? Optional. Whether entity has physics effects.
---@field PointLightSource PointLight? Optional. Point light source.
---@field ServerOnly boolean? Optional. Whether entity is server-only.
EntitySpecification = {}

---Integer-valued 3D vector type.
---@class GridPosition
---@field X integer The X value of the vector.
---@field Y integer The Y value of the vector.
---@field Z integer The Z value of the vector.
GridPosition = {}

---Describes the position and velocity of a non-block entity.
---@class Kinematics
---@field Position Vector3 Entity position in world coordinates.
---@field Velocity Vector3 Entity velocity in world coordinates per second.
---@field StopSystemTime? integer System time at which movement should stop (0 or nil to disable scheduled stop).
Kinematics = {}

---Specifies a point light source attached to an entity.
---@class PointLight
---@field Radius number Radius of the light source in world units.
---@field Intensity number Intensity of the light source. Larger is brighter.
---@field Color integer RGB-packed color of the light source.
---@field PositionOffset Vector3 Offset of the light source relative to the entity position, specified as a percentage of the sprite size. For example, an offset of (0.5, 0.5, 0.0) positions the light source in the center of the entity's front XY plane.
PointLight = {}

---Describes a shadow cast by a non-block entity.
---@class Shadow
---@field Radius number Radius of the shadow in world units when the entity is at the same Z as the cast shadow.
Shadow = {}

---2D vector type.
---@class Vector2
---@field X number X value.
---@field Y number Y value.
Vector2 = {}

---3D vector type.
---@class Vector3
---@field X number X value.
---@field Y number Y value.
---@field Z number Z value.
Vector3 = {}

---@enum EntityType Describes the type of an entity.
EntityType = {
    Npc = 0,
    Item = 1,
    Player = 2,
    Other = 0x7F
}

---@enum Orientation Describes the direction an entity is facing.
Orientation = {
    South = 0,
    Southeast = 1,
    East = 2,
    Northeast = 3,
    North = 4,
    Northwest = 5,
    West = 6,
    Southwest = 7
}

--------------------

--
-- Chat Module
--

---@class Chat
Chat = {}

---@param playerEntityId integer
---@param message string
function Chat.SendSystemMessage(playerEntityId, message)
end

---@param playerEntityId integer
---@param color integer
---@param message string
function Chat.SendToPlayer(playerEntityId, color, message)
end

---@param color integer
---@param message string
function Chat.SendToAll(color, message)
end

---@param command string
---@param callback function
function Chat.AddCommand(command, callback)
end

--------------------

--
-- Color Module
--

---@class Color
Color = {}

---White color.
Color.WHITE = 0xFFFFFF
---Black color.
Color.BLACK = 0x000000
---Red color.
Color.RED = 0xFF0000
---Green color.
Color.GREEN = 0x00FF00
---Blue color.
Color.BLUE = 0x0000FF

---Color normally used for MOTD messages.
Color.MOTD = 0xFFFFFF
---Color normally used for alert messages.
Color.ALERT = 0xD20000
---Color normally used for local Chat messages.
Color.CHAT_LOCAL = 0xB3B3B3
---Color normally used for global Chat messages.
Color.CHAT_GLOBAL = 0xFFFFFF
---Color normally used for system Chat messages.
Color.CHAT_SYSTEM = 0x808080

---Creates a color with the given red, green, and blue linear components. The alpha component is
---set to 255.
---@param r integer Red component (0 - 255 inclusive).
---@param g integer Green component (0 - 255 inclusive).
---@param b integer Blue component (0 - 255 inclusive).
---@return integer color Packed color.
function Color.Rgb(r, g, b)
end

---Creates a color with the given red, green, blue, and alpha linear components.
---@param r integer Red component (0 - 255 inclusive).
---@param g integer Green component (0 - 255 inclusive).
---@param b integer Blue component (0 - 255 inclusive).
---@param a integer Alpha component (0 - 255 inclusive).
---@return integer color Packed color.
function Color.Rgba(r,g,b,a)
end

--------------------

--
-- Components Module
--

---Provides access to component data.
---@class Components
Components = {}

---Admin component.
---@class Components.Admin
Components.Admin = {}
---Checks whether the component exists for an entity.
---@param entityId integer Entity ID.
---@return boolean true if exists, false otherwise.
Components.Admin.Exists = function (entityId) end
---Gets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param lookback boolean? If true, enable lookback at components from the last tick.
---@return boolean?
Components.Admin.Get = function(entityId, lookback) end
---Removes the component for the entity if it exists.
---@param entityId integer Entity ID.
Components.Admin.Remove = function (entityId) end
---Sets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param value boolean Value.
Components.Admin.Set = function (entityId, value) end

---AnimatedSprite component.
---@class Components.AnimatedSprite
Components.AnimatedSprite = {}
---Checks whether the component exists for an entity.
---@param entityId integer Entity ID.
---@return boolean true if exists, false otherwise.
Components.AnimatedSprite.Exists = function (entityId) end
---Gets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param lookback boolean? If true, enable lookback at components from the last tick.
---@return integer? 
Components.AnimatedSprite.Get = function(entityId, lookback) end
---Removes the component for the entity if it exists.
---@param entityId integer Entity ID.
Components.AnimatedSprite.Remove = function (entityId) end
---Sets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param value integer Value.
Components.AnimatedSprite.Set = function (entityId, value) end

---BlockPosition component.
---@class Components.BlockPosition
Components.BlockPosition = {}
---Checks whether the component exists for an entity.
---@param entityId integer Entity ID.
---@return boolean true if exists, false otherwise.
Components.BlockPosition.Exists = function (entityId) end
---Gets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param lookback boolean? If true, enable lookback at components from the last tick.
---@return GridPosition?
Components.BlockPosition.Get = function(entityId, lookback) end
---Removes the component for the entity if it exists.
---@param entityId integer Entity ID.
Components.BlockPosition.Remove = function (entityId) end
---Sets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param value GridPosition Value.
Components.BlockPosition.Set = function (entityId, value) end

---CastBlockShadows tag.
---@class Components.CastBlockShadows
Components.CastBlockShadows = {}
---Checks whether the component exists for an entity.
---@param entityId integer Entity ID.
---@return boolean true if exists, false otherwise.
Components.CastBlockShadows.Exists = function (entityId) end
---Gets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param lookback boolean? If true, enable lookback at components from the last tick.
---@return boolean?
Components.CastBlockShadows.Get = function(entityId, lookback) end
---Removes the component for the entity if it exists.
---@param entityId integer Entity ID.
Components.CastBlockShadows.Remove = function (entityId) end
---Sets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param value boolean Value.
Components.CastBlockShadows.Set = function (entityId, value) end

---CastShadows component.
---@class Components.CastShadows 
Components.CastShadows = {}
---Checks whether the component exists for an entity.
---@param entityId integer Entity ID.
---@return boolean true if exists, false otherwise.
Components.CastShadows.Exists = function (entityId) end
---Gets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param lookback boolean? If true, enable lookback at components from the last tick.
---@return Shadow?
Components.CastShadows.Get = function(entityId, lookback) end
---Removes the component for the entity if it exists.
---@param entityId integer Entity ID.
Components.CastShadows.Remove = function (entityId) end
---Sets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param value Shadow Value.
Components.CastShadows.Set = function (entityId, value) end

---Drawable component.
---@class Components.Drawable 
Components.Drawable = {}
---Checks whether the component exists for an entity.
---@param entityId integer Entity ID.
---@return boolean true if exists, false otherwise.
Components.Drawable.Exists = function (entityId) end
---Gets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param lookback boolean? If true, enable lookback at components from the last tick.
---@return Vector2?
Components.Drawable.Get = function(entityId, lookback) end
---Removes the component for the entity if it exists.
---@param entityId integer Entity ID.
Components.Drawable.Remove = function (entityId) end
---Sets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param value Vector2 Value.
Components.Drawable.Set = function (entityId, value) end

---EntityType component.
---@class Components.EntityType 
Components.EntityType = {}
---Checks whether the component exists for an entity.
---@param entityId integer Entity ID.
---@return boolean true if exists, false otherwise.
Components.EntityType.Exists = function (entityId) end
---Gets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param lookback boolean? If true, enable lookback at components from the last tick.
---@return EntityType?
Components.EntityType.Get = function(entityId, lookback) end
---Removes the component for the entity if it exists.
---@param entityId integer Entity ID.
Components.EntityType.Remove = function (entityId) end
---Sets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param value EntityType Value.
Components.EntityType.Set = function (entityId, value) end

---Kinematics component.
---@class Components.Kinematics 
Components.Kinematics = {}
---Checks whether the component exists for an entity.
---@param entityId integer Entity ID.
---@return boolean true if exists, false otherwise.
Components.Kinematics.Exists = function (entityId) end
---Gets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param lookback boolean? If true, enable lookback at components from the last tick.
---@return Kinematics?
Components.Kinematics.Get = function(entityId, lookback) end
---Removes the component for the entity if it exists.
---@param entityId integer Entity ID.
Components.Kinematics.Remove = function (entityId) end
---Sets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param value Kinematics Value.
Components.Kinematics.Set = function (entityId, value) end
---Sets the velocity without modifying the position.
---@param entityId integer Entity ID.
---@param value Kinematics Kinematics object with velocity to use.
Components.Kinematics.SetVelocity = function (entityId, value) end
---Adds to the position without modifying the velocity.
---@param entityId integer Entity ID.
---@param value Kinematics Kinematics object with position increment to use.
Components.Kinematics.AddPosition = function (entityId, value) end

---Material component.
---@class Components.BlockTile 
Components.BlockTile = {}
---Checks whether the component exists for an entity.
---@param entityId integer Entity ID.
---@return boolean true if exists, false otherwise.
Components.BlockTile.Exists = function (entityId) end
---Gets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param lookback boolean? If true, enable lookback at components from the last tick.
---@return integer?
Components.BlockTile.Get = function(entityId, lookback) end
---Removes the component for the entity if it exists.
---@param entityId integer Entity ID.
Components.BlockTile.Remove = function (entityId) end
---Sets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param value integer Value.
Components.BlockTile.Set = function (entityId, value) end

---Name component.
---@class Components.Name 
Components.Name = {}
---Checks whether the component exists for an entity.
---@param entityId integer Entity ID.
---@return boolean true if exists, false otherwise.
Components.Name.Exists = function (entityId) end
---Gets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param lookback boolean? If true, enable lookback at components from the last tick.
---@return string?
Components.Name.Get = function(entityId, lookback) end
---Removes the component for the entity if it exists.
---@param entityId integer Entity ID.
Components.Name.Remove = function (entityId) end
---Sets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param value string Value.
Components.Name.Set = function (entityId, value) end

---Orientation component.
---@class Components.Orientation 
Components.Orientation = {}
---Checks whether the component exists for an entity.
---@param entityId integer Entity ID.
---@return boolean true if exists, false otherwise.
Components.Orientation.Exists = function (entityId) end
---Gets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param lookback boolean? If true, enable lookback at components from the last tick.
---@return Orientation?
Components.Orientation.Get = function(entityId, lookback) end
---Removes the component for the entity if it exists.
---@param entityId integer Entity ID.
Components.Orientation.Remove = function (entityId) end
---Sets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param value Orientation Value.
Components.Orientation.Set = function (entityId, value) end

---Parent component.
---@class Components.Parent 
Components.Parent = {}
---Checks whether the component exists for an entity.
---@param entityId integer Entity ID.
---@return boolean true if exists, false otherwise.
Components.Parent.Exists = function (entityId) end
---Gets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param lookback boolean? If true, enable lookback at components from the last tick.
---@return integer?
Components.Parent.Get = function(entityId, lookback) end
---Removes the component for the entity if it exists.
---@param entityId integer Entity ID.
Components.Parent.Remove = function (entityId) end
---Sets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param value integer Value.
Components.Parent.Set = function (entityId, value) end

---Physics tag.
---@class Components.Physics 
Components.Physics = {}
---Checks whether the component exists for an entity.
---@param entityId integer Entity ID.
---@return boolean true if exists, false otherwise.
Components.Physics.Exists = function (entityId) end
---Gets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param lookback boolean? If true, enable lookback at components from the last tick.
---@return boolean?
Components.Physics.Get = function(entityId, lookback) end
---Removes the component for the entity if it exists.
---@param entityId integer Entity ID.
Components.Physics.Remove = function (entityId) end
---Sets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param value boolean Value.
Components.Physics.Set = function (entityId, value) end

---PlayerCharacter tag.
---@class Components.PlayerCharacter 
Components.PlayerCharacter = {}
---Checks whether the component exists for an entity.
---@param entityId integer Entity ID.
---@return boolean true if exists, false otherwise.
Components.PlayerCharacter.Exists = function (entityId) end
---Gets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param lookback boolean? If true, enable lookback at components from the last tick.
---@return boolean?
Components.PlayerCharacter.Get = function(entityId, lookback) end
---Removes the component for the entity if it exists.
---@param entityId integer Entity ID.
Components.PlayerCharacter.Remove = function (entityId) end
---Sets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param value boolean Value.
Components.PlayerCharacter.Set = function (entityId, value) end

---PointLightSource component.
---@class Components.PointLightSource 
Components.PointLightSource = {}
---Checks whether the component exists for an entity.
---@param entityId integer Entity ID.
---@return boolean true if exists, false otherwise.
Components.PointLightSource.Exists = function (entityId) end
---Gets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param lookback boolean? If true, enable lookback at components from the last tick.
---@return PointLight?
Components.PointLightSource.Get = function(entityId, lookback) end
---Removes the component for the entity if it exists.
---@param entityId integer Entity ID.
Components.PointLightSource.Remove = function (entityId) end
---Sets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param value PointLight Value.
Components.PointLightSource.Set = function (entityId, value) end

---ServerOnly tag.
---@class Components.ServerOnly 
Components.ServerOnly = {}
---Checks whether the component exists for an entity.
---@param entityId integer Entity ID.
---@return boolean true if exists, false otherwise.
Components.ServerOnly.Exists = function (entityId) end
---Gets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param lookback boolean? If true, enable lookback at components from the last tick.
---@return boolean?
Components.ServerOnly.Get = function(entityId, lookback) end
---Removes the component for the entity if it exists.
---@param entityId integer Entity ID.
Components.ServerOnly.Remove = function (entityId) end
---Sets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param value boolean Value.
Components.ServerOnly.Set = function (entityId, value) end

--------------------

--
-- Data Module
--

---Provides access to global and per-entity key-value data.
---@class Data
Data = {}

---Global key-value data table.
Data.Global = {}

---Gets the per-entity key-value data table for the given entity.
---@param entityId integer Entity ID.
---@return table? entityData Per-entity data table, or nil if the entity does not exist or is unloaded.
function Data.GetEntityData(entityId)
end

--------------------

--
-- Dialogue Module
--

---Provides functions for displaying dialogue to players.
---@class Dialogue
Dialogue = {}

---Shows a dialogue message to a specific player.
---@param targetEntityId integer Entity ID of the target player.
---@param subject string Dialogue subject (e.g. who is speaking).
---@param message string Dialogue message.
function Dialogue.Show(targetEntityId, subject, message)
end

---Shows a dialogue message with a profile sprite to a specific player.
---@param targetEntityId integer Entity ID of the target player.
---@param profileSpriteId integer Profile sprite ID.
---@param subject string Dialogue subject (e.g. who is speaking).
---@param message string Dialogue message.
function Dialogue.ShowProfile(targetEntityId, profileSpriteId, subject, message)
end

--------------------

--
-- Entities Module
--

---Provides functions for working with entities.
---@class Entities
Entities = {}

---Creates a new entity according to the provided specification.
---@param spec EntitySpecification Specification for new entity.
---@return integer? Entity ID of the newly created entity, or nil on error.
function Entities.Create(spec)
end

---Removes the given entity if it exists and is loaded in the game world.
---@param entityId integer ID of entity to remove.
function Entities.Remove(entityId)
end

---Gets the template associated with the given entity, if any.
---@param entityId integer Entity ID.
---@return integer? templateId Template entity ID, or nil if no template entity.
function Entities.GetTemplate(entityId)
end

---Sets the template associated with the given entity.
---@param entityId integer Entity ID.
---@param templateId integer Template entity ID.
function Entities.SetTemplate(entityId, templateId)
end

---Checks whether the given entity is a template entity.
---@param entityId integer Entity ID to check.
---@return boolean isTemplate true if the entity is a template entity, false otherwise.
function Entities.IsTemplate(entityId)
end

---Synchronizes the given entity or entities with subscribed clients.
---@param entityId integer|table Entity ID(s) to sync.
function Entities.Sync(entityId)
end

---Synchronizes the given entity or entities and all descendants with subscribed clients.
---@param entityId integer|table Parent entity ID(s) to sync.
function Entities.SyncTree(entityId)
end

---First template entity ID.
Entities.FirstTemplateEntityId  = 0x7FFE000000000000
---Last template entity ID.
Entities.LastTemplateEntityId   = 0x7FFEFFFFFFFFFFFF
---First block entity ID.
Entities.FirstBlockEntityId     = 0x6FFF000000000000
---Last block entity ID.
Entities.LastBlockEntityId      = 0x6FFFFFFFFFFFFFFF
---First persisted entity ID.
Entities.FirstPersistedEntityId = 0x7FFF000000000000

--------------------

--
-- Events Module
--

---Event types supported in scripts.
---@enum Events
Events = {
    ---Event emitted once per tick.
    Core_Tick = 1,
    ---Event emitted when a logout occurs.
    Core_Network_Logout = 700,
    ---Event emitted when a player enters the world.
    Server_Persistence_PlayerEnteredWorld = 200002
}

--------------------

--
-- Inventory Module
--

---@class Inventory
Inventory = {}

---Gets the inventory for a player as a list of item entity IDs in order of inventory slots. Empty slots are listed with an item ID of 0.
---@param entityId integer Entity ID.
---@return table # Inventory as a list of item IDs in slot order; 0 indicates an empty slot.
function Inventory.GetInventory(entityId)
end

---Gets the ID of the item in a particular inventory slot.
---@param entityId integer Entity ID.
---@param slotIndex integer Slot index.
---@return integer # Item ID, or 0 if the slot is empty or does not exist.
function Inventory.GetItem(entityId, slotIndex)
end

---Gets the slot index for the given item if it is in the entity's inventory.
---@param entityId integer Entity ID.
---@param itemId integer Item ID.
---@return integer # Slot index containing the item, or 0 if not found.
function Inventory.GetSlotIndexForItem(entityId, itemId)
end

---Finds the first item (in slot order) in the entity's inventory that has the given template ID.
---@param entityId integer Entity ID.
---@param templateId integer Item template ID to search for.
---@return integer # Item ID of the first matching item, or 0 if no matches found.
function Inventory.FindFirstMatchingItem(entityId, templateId)
end

---Gets the number of slots in the entity's inventory.
---@param entityId integer Entity ID.
---@return integer # Slot count.
function Inventory.GetSlotCount(entityId)
end

---Adds one or more slots to an entity's inventory.
---@param entityId integer Entity ID.
---@param slotCount integer Number of slots to add. Must be positive.
function Inventory.AddSlots(entityId, slotCount)
end

---Gets an empty slot in the entity's inventory.
---@param entityId integer Entity ID.
---@return integer # Empty slot index, or 0 if no empty slot was found.
function Inventory.GetEmptySlot(entityId)
end

---Tries to add an existing item to a free slot in the entity's inventory.
---@param entityId integer Entity ID.
---@param itemId integer Item ID.
---@return boolean # true if the item was added to a free slot, false otherwise.
function Inventory.AddItem(entityId, itemId)
end

---Picks up an item into the entity's inventory. The item must be within range of the entity.
---@param entityId integer Entity ID.
---@param itemId integer Item ID.
function Inventory.PickUp(entityId, itemId)
end

---Drops the item in a slot at the position of the entity.
---@param entityId integer Entity ID.
---@param slotIndex integer Slot index of the item to drop.
function Inventory.Drop(entityId, slotIndex)
end

---Drops the item in a slot at a specific position. The position must be within the allowed drop range.
---@param entityId integer Entity ID.
---@param slotIndex integer Slot index of the item to drop.
---@param dropPosition Vector3 Position to drop the item.
function Inventory.DropAt(entityId, slotIndex, dropPosition)
end

---Swaps two slots in an entity's inventory.
---@param entityId integer Entity ID.
---@param slotIndex1 integer First slot index to swap.
---@param slotIndex2 integer Second slot index to swap.
function Inventory.Swap(entityId, slotIndex1, slotIndex2)
end

---Removes the item in an inventory slot, destroying the item entity.
---@param entityId integer Entity ID.
---@param slotIndex integer Slot index of the item to remove.
function Inventory.RemoveItem(entityId, slotIndex)
end

--------------------

--
-- Scripting Module
--

---@class Scripting
Scripting = {}

---Registers a callback to handle the given type of event.
---@param eventId Events Event ID that this callback should respond to.
---@param callback function Callback function. Must accept a single argument having the event details type, or zero arguments for event types without details.
function Scripting.AddEventCallback(eventId, callback)
end

---Registers a timed callback that will be triggered after a delay period.
---@param delaySeconds number Delay in seconds before the callback is invoked.
---@param callback fun(argument: any?): nil Callback function.
---@param argument any? An optional argument to pass to the callback.
function Scripting.AddTimedCallback(delaySeconds, callback, argument)
end

---Adds a callback to be called when the given entity collides with another object.
---@param entityId integer Entity ID to subscribe to.
---@param callback function Callback function. May accept the entity ID as a single argument.
---@return integer callbackHandle Callback handle.
function Scripting.AddCollisionCallback(entityId, callback)
end

---Removes a collision callback previously added with scripting.AddCollisionCallback.
---@param entityId integer Entity ID.
---@param callbackHandle integer Callback handle previously returned by scripting.AddCollisionCallback.
function Scripting.RemoveCollisionCallback(entityId, callbackHandle)
end

---Adds a callback to be called when the given entity stops moving due to a scheduled motion timeout.
---@param entityId integer Entity ID to subscribe to.
---@param callback function Callback function. May accept the entity ID as a single argument.
---@return integer callbackHandle Callback handle.
function Scripting.AddScheduledStopCallback(entityId, callback)
end

---Removes a collision callback previously added with scripting.AddScheduledStopCallback.
---@param entityId integer Entity ID.
---@param callbackHandle integer Callback handle previously returned by scripting.AddScheduledStopCallback.
function Scripting.RemoveScheduledStopCallback(entityId, callbackHandle)
end

---Adds an entity parameter hint for the named callback function. A parameter hint is the name of a key in the entity key-value store that is referenced by the given callback function. Registering a parameter hint allows the editor GUIs to prompt the creator to specify a value for the parameter when binding the callback to an entity or template entity.
---@param functionName string Callback function name.
---@param parameterName string Parameter name.
---@param type string Parameter type (one of "String", "Bool", "Int", "Float", "NpcTemplate", "SameTypeTemplate").
---@param tooltip string Tooltip text for the parameter.
function Scripting.AddEntityParameterHint(functionName, parameterName, type, tooltip)
end

--------------------

--
-- Time Module
--

---@class Time
Time = {}

---Gets the system time in microseconds since an arbitrary reference point.
---@return integer systemTime System time in microseconds.
function Time.GetSystemTime()
end

---Gets a system time (in microseconds) delaySeconds seconds in the future.
---@param delaySeconds number Seconds to add to the current system time.
---@return integer futureTime Future time in microseconds.
function Time.FutureSystemTime(delaySeconds)
end

---Gets the absolute time in the game world measured in seconds since the game started.
---@return integer absoluteTime Absolute time.
function Time.GetAbsoluteTime()
end

---Gets the current year in the game world.
---@return integer year Current year.
function Time.GetYear()
end

---Gets the current season in the game world.
---@return string season Current season.
function Time.GetSeason()
end

---Gets the current month in the game world.
---@return integer month Current month.
function Time.GetMonth()
end

---Gets the current month of year in the game world.
---@return integer monthOfYear Current month of year.
function Time.GetMonthOfYear()
end

---Gets the current week of the month in the game world.
---@return integer weekOfMonth Current week of month.
function Time.GetWeekOfMonth()
end

---Gets the current day of the month in the game world.
---@return integer dayOfMonth Current day of month.
function Time.GetDayOfMonth()
end

---Gets the current week in the game world.
---@return integer week Current week.
function Time.GetWeek()
end

---Gets the current day of week in the game world.
---@return integer dayOfWeek Current day of week.
function Time.GetDayOfWeek()
end

---Gets the current day in the game world.
---@return integer day Current day.
function Time.GetDay()
end

---Gets the current hour of the day in the game world.
---@return integer hourOfDay Current hour of day.
function Time.GetHourOfDay()
end

---Gets the current second of the day in theg ame world.
---@return integer secondOfDay Current second of day.
function Time.GetSecondOfDay()
end

--------------------

---The Util global module provides common Utility functions for scripts.
---@class Util
Util = {}

---Writes an informational message to the log.
---@param message string Message.
function Util.LogInfo(message)
end

---Writes a warning message to the log.
---@param message string Message.
function Util.LogWarn(message)
end

---Writes an error message to the log.
---@param message string Message.
function Util.LogError(message)
end

---Writes a critical message to the log.
---@param message string Message.
function Util.LogCrit(message)
end

---Writes a debug message to the log.
---@param message string Message.
function Util.LogDebug(message)
end

---Writes a trace message to the log.
---@param message string Message.
function Util.LogTrace(message)
end

---Converts a string to a boolean value.
---@param value string String to convert.
---@return boolean? converted Converted value, or nil if no conversion was possible.
function Util.ToBool(value)
end

--------------------
