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
-- chat Module
--

---@class chat
chat = {}

---@param playerEntityId integer
---@param message string
function chat.SendSystemMessage(playerEntityId, message)
end

---@param playerEntityId integer
---@param color integer
---@param message string
function chat.SendToPlayer(playerEntityId, color, message)
end

---@param color integer
---@param message string
function chat.SendToAll(color, message)
end

--------------------

--
-- color Module
--

---@class color
color = {}

---White color.
color.WHITE = 0xFFFFFF
---Black color.
color.BLACK = 0x000000
---Red color.
color.RED = 0xFF0000
---Green color.
color.GREEN = 0x00FF00
---Blue color.
color.BLUE = 0x0000FF

---Color normally used for MOTD messages.
color.MOTD = 0xFFFFFF
---Color normally used for alert messages.
color.ALERT = 0xD20000
---Color normally used for local chat messages.
color.CHAT_LOCAL = 0xB3B3B3
---Color normally used for global chat messages.
color.CHAT_GLOBAL = 0xFFFFFF
---Color normally used for system chat messages.
color.CHAT_SYSTEM = 0x808080

---Creates a color with the given red, green, and blue linear components. The alpha component is
---set to 255.
---@param r integer Red component (0 - 255 inclusive).
---@param g integer Green component (0 - 255 inclusive).
---@param b integer Blue component (0 - 255 inclusive).
---@return integer color Packed color.
function color.Rgb(r, g, b)
end

---Creates a color with the given red, green, blue, and alpha linear components.
---@param r integer Red component (0 - 255 inclusive).
---@param g integer Green component (0 - 255 inclusive).
---@param b integer Blue component (0 - 255 inclusive).
---@param a integer Alpha component (0 - 255 inclusive).
---@return integer color Packed color.
function color.Rgba(r,g,b,a)
end

--------------------

--
-- components Module
--

---Provides access to component data.
---@class components
components = {}

---Admin component.
---@class components.admin
components.admin = {}
---Checks whether the component exists for an entity.
---@param entityId integer Entity ID.
---@return boolean true if exists, false otherwise.
components.admin.Exists = function (entityId) end
---Gets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param lookback boolean? If true, enable lookback at components from the last tick.
---@return boolean?
components.admin.Get = function(entityId, lookback) end
---Removes the component for the entity if it exists.
---@param entityId integer Entity ID.
components.admin.Remove = function (entityId) end
---Sets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param value boolean Value.
components.admin.Set = function (entityId, value) end

---AnimatedSprite component.
---@class components.animated_sprite
components.animated_sprite = {}
---Checks whether the component exists for an entity.
---@param entityId integer Entity ID.
---@return boolean true if exists, false otherwise.
components.animated_sprite.Exists = function (entityId) end
---Gets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param lookback boolean? If true, enable lookback at components from the last tick.
---@return integer? 
components.animated_sprite.Get = function(entityId, lookback) end
---Removes the component for the entity if it exists.
---@param entityId integer Entity ID.
components.animated_sprite.Remove = function (entityId) end
---Sets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param value integer Value.
components.animated_sprite.Set = function (entityId, value) end

---BlockPosition component.
---@class components.block_position
components.block_position = {}
---Checks whether the component exists for an entity.
---@param entityId integer Entity ID.
---@return boolean true if exists, false otherwise.
components.block_position.Exists = function (entityId) end
---Gets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param lookback boolean? If true, enable lookback at components from the last tick.
---@return GridPosition?
components.block_position.Get = function(entityId, lookback) end
---Removes the component for the entity if it exists.
---@param entityId integer Entity ID.
components.block_position.Remove = function (entityId) end
---Sets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param value GridPosition Value.
components.block_position.Set = function (entityId, value) end

---CastBlockShadows tag.
---@class components.cast_block_shadows
components.cast_block_shadows = {}
---Checks whether the component exists for an entity.
---@param entityId integer Entity ID.
---@return boolean true if exists, false otherwise.
components.cast_block_shadows.Exists = function (entityId) end
---Gets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param lookback boolean? If true, enable lookback at components from the last tick.
---@return boolean?
components.cast_block_shadows.Get = function(entityId, lookback) end
---Removes the component for the entity if it exists.
---@param entityId integer Entity ID.
components.cast_block_shadows.Remove = function (entityId) end
---Sets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param value boolean Value.
components.cast_block_shadows.Set = function (entityId, value) end

---CastShadows component.
---@class components.cast_shadows 
components.cast_shadows = {}
---Checks whether the component exists for an entity.
---@param entityId integer Entity ID.
---@return boolean true if exists, false otherwise.
components.cast_shadows.Exists = function (entityId) end
---Gets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param lookback boolean? If true, enable lookback at components from the last tick.
---@return Shadow?
components.cast_shadows.Get = function(entityId, lookback) end
---Removes the component for the entity if it exists.
---@param entityId integer Entity ID.
components.cast_shadows.Remove = function (entityId) end
---Sets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param value Shadow Value.
components.cast_shadows.Set = function (entityId, value) end

---Drawable component.
---@class components.drawable 
components.drawable = {}
---Checks whether the component exists for an entity.
---@param entityId integer Entity ID.
---@return boolean true if exists, false otherwise.
components.drawable.Exists = function (entityId) end
---Gets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param lookback boolean? If true, enable lookback at components from the last tick.
---@return Vector2?
components.drawable.Get = function(entityId, lookback) end
---Removes the component for the entity if it exists.
---@param entityId integer Entity ID.
components.drawable.Remove = function (entityId) end
---Sets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param value Vector2 Value.
components.drawable.Set = function (entityId, value) end

---EntityType component.
---@class components.entity_type 
components.entity_type = {}
---Checks whether the component exists for an entity.
---@param entityId integer Entity ID.
---@return boolean true if exists, false otherwise.
components.entity_type.Exists = function (entityId) end
---Gets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param lookback boolean? If true, enable lookback at components from the last tick.
---@return EntityType?
components.entity_type.Get = function(entityId, lookback) end
---Removes the component for the entity if it exists.
---@param entityId integer Entity ID.
components.entity_type.Remove = function (entityId) end
---Sets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param value EntityType Value.
components.entity_type.Set = function (entityId, value) end

---Kinematics component.
---@class components.kinematics 
components.kinematics = {}
---Checks whether the component exists for an entity.
---@param entityId integer Entity ID.
---@return boolean true if exists, false otherwise.
components.kinematics.Exists = function (entityId) end
---Gets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param lookback boolean? If true, enable lookback at components from the last tick.
---@return Kinematics?
components.kinematics.Get = function(entityId, lookback) end
---Removes the component for the entity if it exists.
---@param entityId integer Entity ID.
components.kinematics.Remove = function (entityId) end
---Sets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param value Kinematics Value.
components.kinematics.Set = function (entityId, value) end
---Sets the velocity without modifying the position.
---@param entityId integer Entity ID.
---@param value Kinematics Kinematics object with velocity to use.
components.kinematics.SetVelocity = function (entityId, value) end
---Adds to the position without modifying the velocity.
---@param entityId integer Entity ID.
---@param value Kinematics Kinematics object with position increment to use.
components.kinematics.AddPosition = function (entityId, value) end

---Material component.
---@class components.material 
components.material = {}
---Checks whether the component exists for an entity.
---@param entityId integer Entity ID.
---@return boolean true if exists, false otherwise.
components.material.Exists = function (entityId) end
---Gets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param lookback boolean? If true, enable lookback at components from the last tick.
---@return integer?
components.material.Get = function(entityId, lookback) end
---Removes the component for the entity if it exists.
---@param entityId integer Entity ID.
components.material.Remove = function (entityId) end
---Sets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param value integer Value.
components.material.Set = function (entityId, value) end

---MaterialModifier component.
---@class components.material_modifier 
components.material_modifier = {}
---Checks whether the component exists for an entity.
---@param entityId integer Entity ID.
---@return boolean true if exists, false otherwise.
components.material_modifier.Exists = function (entityId) end
---Gets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param lookback boolean? If true, enable lookback at components from the last tick.
---@return integer?
components.material_modifier.Get = function(entityId, lookback) end
---Removes the component for the entity if it exists.
---@param entityId integer Entity ID.
components.material_modifier.Remove = function (entityId) end
---Sets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param value integer Value.
components.material_modifier.Set = function (entityId, value) end

---Name component.
---@class components.name 
components.name = {}
---Checks whether the component exists for an entity.
---@param entityId integer Entity ID.
---@return boolean true if exists, false otherwise.
components.name.Exists = function (entityId) end
---Gets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param lookback boolean? If true, enable lookback at components from the last tick.
---@return string?
components.name.Get = function(entityId, lookback) end
---Removes the component for the entity if it exists.
---@param entityId integer Entity ID.
components.name.Remove = function (entityId) end
---Sets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param value string Value.
components.name.Set = function (entityId, value) end

---Orientation component.
---@class components.orientation 
components.orientation = {}
---Checks whether the component exists for an entity.
---@param entityId integer Entity ID.
---@return boolean true if exists, false otherwise.
components.orientation.Exists = function (entityId) end
---Gets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param lookback boolean? If true, enable lookback at components from the last tick.
---@return Orientation?
components.orientation.Get = function(entityId, lookback) end
---Removes the component for the entity if it exists.
---@param entityId integer Entity ID.
components.orientation.Remove = function (entityId) end
---Sets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param value Orientation Value.
components.orientation.Set = function (entityId, value) end

---Parent component.
---@class components.parent 
components.parent = {}
---Checks whether the component exists for an entity.
---@param entityId integer Entity ID.
---@return boolean true if exists, false otherwise.
components.parent.Exists = function (entityId) end
---Gets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param lookback boolean? If true, enable lookback at components from the last tick.
---@return integer?
components.parent.Get = function(entityId, lookback) end
---Removes the component for the entity if it exists.
---@param entityId integer Entity ID.
components.parent.Remove = function (entityId) end
---Sets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param value integer Value.
components.parent.Set = function (entityId, value) end

---Physics tag.
---@class components.physics 
components.physics = {}
---Checks whether the component exists for an entity.
---@param entityId integer Entity ID.
---@return boolean true if exists, false otherwise.
components.physics.Exists = function (entityId) end
---Gets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param lookback boolean? If true, enable lookback at components from the last tick.
---@return boolean?
components.physics.Get = function(entityId, lookback) end
---Removes the component for the entity if it exists.
---@param entityId integer Entity ID.
components.physics.Remove = function (entityId) end
---Sets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param value boolean Value.
components.physics.Set = function (entityId, value) end

---PlayerCharacter tag.
---@class components.player_character 
components.player_character = {}
---Checks whether the component exists for an entity.
---@param entityId integer Entity ID.
---@return boolean true if exists, false otherwise.
components.player_character.Exists = function (entityId) end
---Gets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param lookback boolean? If true, enable lookback at components from the last tick.
---@return boolean?
components.player_character.Get = function(entityId, lookback) end
---Removes the component for the entity if it exists.
---@param entityId integer Entity ID.
components.player_character.Remove = function (entityId) end
---Sets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param value boolean Value.
components.player_character.Set = function (entityId, value) end

---PointLight component.
---@class components.point_light 
components.point_light = {}
---Checks whether the component exists for an entity.
---@param entityId integer Entity ID.
---@return boolean true if exists, false otherwise.
components.point_light.Exists = function (entityId) end
---Gets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param lookback boolean? If true, enable lookback at components from the last tick.
---@return PointLight?
components.point_light.Get = function(entityId, lookback) end
---Removes the component for the entity if it exists.
---@param entityId integer Entity ID.
components.point_light.Remove = function (entityId) end
---Sets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param value PointLight Value.
components.point_light.Set = function (entityId, value) end

---ServerOnly tag.
---@class components.server_only 
components.server_only = {}
---Checks whether the component exists for an entity.
---@param entityId integer Entity ID.
---@return boolean true if exists, false otherwise.
components.server_only.Exists = function (entityId) end
---Gets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param lookback boolean? If true, enable lookback at components from the last tick.
---@return boolean?
components.server_only.Get = function(entityId, lookback) end
---Removes the component for the entity if it exists.
---@param entityId integer Entity ID.
components.server_only.Remove = function (entityId) end
---Sets the value of the component for the entity.
---@param entityId integer Entity ID.
---@param value boolean Value.
components.server_only.Set = function (entityId, value) end

--------------------

--
-- data Module
--

---Provides access to global and per-entity key-value data.
---@class data
data = {}

---Global key-value data table.
data.global = {}

---Gets the per-entity key-value data table for the given entity.
---@param entityId integer Entity ID.
---@return table? entityData Per-entity data table, or nil if the entity does not exist or is unloaded.
function data.GetEntityData(entityId)
end

--------------------

--
-- dialogue Module
--

---Provides functions for displaying dialogue to players.
---@class dialogue
dialogue = {}

---Shows a dialogue message to a specific player.
---@param targetEntityId integer Entity ID of the target player.
---@param subject string Dialogue subject (e.g. who is speaking).
---@param message string Dialogue message.
function dialogue.Show(targetEntityId, subject, message)
end

---Shows a dialogue message with a profile sprite to a specific player.
---@param targetEntityId integer Entity ID of the target player.
---@param profileSpriteId integer Profile sprite ID.
---@param subject string Dialogue subject (e.g. who is speaking).
---@param message string Dialogue message.
function dialogue.ShowProfile(targetEntityId, profileSpriteId, subject, message)
end

--------------------

--
-- entities Module
--

---Provides functions for working with entities.
---@class entities
entities = {}

---Creates a new entity according to the provided specification.
---@param spec EntitySpecification Specification for new entity.
---@return integer? Entity ID of the newly created entity, or nil on error.
function entities.Create(spec)
end

---Removes the given entity if it exists and is loaded in the game world.
---@param entityId integer ID of entity to remove.
function entities.Remove(entityId)
end

---Gets the template associated with the given entity, if any.
---@param entityId integer Entity ID.
---@return integer? templateId Template entity ID, or nil if no template entity.
function entities.GetTemplate(entityId)
end

---Sets the template associated with the given entity.
---@param entityId integer Entity ID.
---@param templateId integer Template entity ID.
function entities.SetTemplate(entityId, templateId)
end

---Checks whether the given entity is a template entity.
---@param entityId integer Entity ID to check.
---@return boolean isTemplate true if the entity is a template entity, false otherwise.
function entities.IsTemplate(entityId)
end

---Synchronizes the given entity or entities with subscribed clients.
---@param entityId integer|table Entity ID(s) to sync.
function entities.Sync(entityId)
end

---Synchronizes the given entity or entities and all descendants with subscribed clients.
---@param entityId integer|table Parent entity ID(s) to sync.
function entities.SyncTree(entityId)
end

---First template entity ID.
entities.FirstTemplateEntityId  = 0x7FFE000000000000
---Last template entity ID.
entities.LastTemplateEntityId   = 0x7FFEFFFFFFFFFFFF
---First block entity ID.
entities.FirstBlockEntityId     = 0x6FFF000000000000
---Last block entity ID.
entities.LastBlockEntityId      = 0x6FFFFFFFFFFFFFFF
---First persisted entity ID.
entities.FirstPersistedEntityId = 0x7FFF000000000000

--------------------

--
-- events Module
--

---Event types supported in scripts.
---@enum events
events = {
    ---Event emitted once per tick.
    Core_Tick = 1,
    ---Event emitted when a logout occurs.
    Core_Network_Logout = 700,
    ---Event emitted when a player enters the world.
    Server_Persistence_PlayerEnteredWorld = 200002
}

--------------------

--
-- scripting Module
--

---@class scripting
scripting = {}

---Registers a callback to handle the given type of event.
---@param eventId events Event ID that this callback should respond to.
---@param callback function Callback function. Must accept a single argument having the event details type, or zero arguments for event types without details.
function scripting.AddEventCallback(eventId, callback)
end

---Registers a timed callback that will be triggered after a delay period.
---@param delaySeconds number Delay in seconds before the callback is invoked.
---@param callback fun(argument: any?): nil Callback function.
---@param argument any? An optional argument to pass to the callback.
function scripting.AddTimedCallback(delaySeconds, callback, argument)
end

---Adds an entity parameter hint for the named callback function. A parameter hint is the name of a key in the entity key-value store that is referenced by the given callback function. Registering a parameter hint allows the editor GUIs to prompt the creator to specify a value for the parameter when binding the callback to an entity or template entity.
---@param functionName string Callback function name.
---@param parameterName string Parameter name.
---@param type string Parameter type (one of "String", "Bool", "Int", "Float", "NpcTemplate", "SameTypeTemplate").
---@param tooltip string Tooltip text for the parameter.
function scripting.AddEntityParameterHint(functionName, parameterName, type, tooltip)
end

--------------------

--
-- time Module
--

---@class time
time = {}

---Gets the absolute time in the game world measured in seconds since the game started.
---@return integer absoluteTime Absolute time.
function time.GetAbsoluteTime()
end

---Gets the current year in the game world.
---@return integer year Current year.
function time.GetYear()
end

---Gets the current season in the game world.
---@return string season Current season.
function time.GetSeason()
end

---Gets the current month in the game world.
---@return integer month Current month.
function time.GetMonth()
end

---Gets the current month of year in the game world.
---@return integer monthOfYear Current month of year.
function time.GetMonthOfYear()
end

---Gets the current week of the month in the game world.
---@return integer weekOfMonth Current week of month.
function time.GetWeekOfMonth()
end

---Gets the current day of the month in the game world.
---@return integer dayOfMonth Current day of month.
function time.GetDayOfMonth()
end

---Gets the current week in the game world.
---@return integer week Current week.
function time.GetWeek()
end

---Gets the current day of week in the game world.
---@return integer dayOfWeek Current day of week.
function time.GetDayOfWeek()
end

---Gets the current day in the game world.
---@return integer day Current day.
function time.GetDay()
end

---Gets the current hour of the day in the game world.
---@return integer hourOfDay Current hour of day.
function time.GetHourOfDay()
end

---Gets the current second of the day in theg ame world.
---@return integer secondOfDay Current second of day.
function time.GetSecondOfDay()
end

--------------------

---The util global module provides common utility functions for scripts.
---@class util
util = {}

---Writes an informational message to the log.
---@param message string Message.
function util.LogInfo(message)
end

---Writes a warning message to the log.
---@param message string Message.
function util.LogWarn(message)
end

---Writes an error message to the log.
---@param message string Message.
function util.LogError(message)
end

---Writes a critical message to the log.
---@param message string Message.
function util.LogCrit(message)
end

---Writes a debug message to the log.
---@param message string Message.
function util.LogDebug(message)
end

---Writes a trace message to the log.
---@param message string Message.
function util.LogTrace(message)
end

---Converts a string to a boolean value.
---@param value string String to convert.
---@return boolean? converted Converted value, or nil if no conversion was possible.
function util.ToBool(value)
end

--------------------
