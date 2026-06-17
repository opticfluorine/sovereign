-- Sovereign Engine Scripting Library
-- Copyright (c) 2025 opticfluorine
--
-- This program is free software: you can redistribute it and/or modify
-- it under the terms of the GNU Lesser General Public License as published by
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

local Units = require("Sovereign.Units")
local Vectors = require("Sovereign.Vectors")

-------------------------------------

--- Proxy class that provides read-write access to an entity's components
--- as if they were fields on an object.
--- @class ComponentsProxy
--- @field private _entityId integer Entity ID.
--- @field [string] any
local ComponentsProxy = {}
setmetatable(ComponentsProxy, ComponentsProxy)

-------------------------------------

--- Creates a components proxy for the given entity.
--- @param entityId integer Entity ID.
--- @return ComponentsProxy # ComponentsProxy object.
function ComponentsProxy.Create(entityId)
    local obj = {}

    obj._entityId = entityId

    setmetatable(obj, ComponentsProxy)
    return obj
end

-------------------------------------

--- Proxy getter that gets a component by name.
--- @param key string Component key.
--- @return any # Component value, or nil if entity lacks this component.
function ComponentsProxy:__index(key)
    local component = Components[key]
    if not component then return nil end
    return component.Get(self._entityId)
end

-------------------------------------

--- Proxy setter that sets a component by name.
--- @param key string Component key.
--- @param value any New component value.
function ComponentsProxy:__newindex(key, value)
    local component = Components[key]
    if not component then return end
    component.Set(self._entityId, value)
end

-------------------------------------

--- Proxy class that provides read-write access to various entity properties
--- as if they were fields on an object.
--- @class PropertyProxy
--- @field TemplateId integer Template ID.
--- @field private _entityId integer Entity ID.
--- @field private _getters table Getters.
--- @field private _setters table Setters.
local PropertyProxy = {}
setmetatable(PropertyProxy, PropertyProxy)

-------------------------------------

function PropertyProxy.Create(entityId)
    local obj = {}

    obj._entityId = entityId

    obj._getters = {}
    function obj._getters.TemplateId()
        return Entities.GetTemplate(obj._entityId)
    end

    obj._setters = {}
    function obj._setters.TemplateId(value)
        Entities.SetTemplate(obj._entityId, value)
    end

    setmetatable(obj, PropertyProxy)
    return obj
end

-------------------------------------

function PropertyProxy:__index(key)
    local getter = self._getters[key]
    if not getter then return nil end
    return getter()
end

-------------------------------------

function PropertyProxy:__newindex(key, value)
    local setter = self._setters[key]
    if not setter then rawset(self, key, value) end
    setter(value)
end

-------------------------------------

--- Lightweight object-oriented wrapper for an entity and its components.
--- @class Entity
--- @field EntityId integer Entity ID.
--- @field Properties PropertyProxy Provides access to the entity's properties.
--- @field Components ComponentsProxy Provides access to the entity's components.
--- @field Data table Entity key-value data.
local Entity = {}
Entity.__index = Entity
setmetatable(Entity, Entity)

-------------------------------------

--- Gets an object wrapper for the given entity.
--- @param entityId integer Entity ID.
--- @return Entity # Entity object.
function Entity.Get(entityId)
    local obj = {}
    setmetatable(obj, Entity)

    obj.EntityId = entityId
    obj.Properties = PropertyProxy.Create(entityId)
    obj.Components = ComponentsProxy.Create(entityId)
    obj.Data = Data.GetEntityData(entityId)

    return obj
end

-------------------------------------
-------------------------------------
--   Movement Methods              --
-------------------------------------
-------------------------------------

--- Moves the entity at the given constant velocity for up to a given length of time.
--- @param velocity Vector3 Velocity.
--- @param maxTime number Maximum movement time in seconds.
function Entity:Move(velocity, maxTime)
    local posVel = self.Components.Kinematics
    local endTime = Time.GetSystemTime() + maxTime * Units.Time.SToUs
    posVel.Velocity = velocity
    posVel.StopSystemTime = endTime
    self.Components.Kinematics = posVel
end

-------------------------------------

--- Tries to move the entity in a straight line by a given displacement.
--- @param displacement Vector3 Desired displacement in position.
--- @param speed number Magnitude of velocity in blocks per second.
function Entity:MoveBy(displacement, speed)
    local moveTime = Vectors.Vector3.Length(displacement) / speed
    local velocity = Vectors.Vector3.Scale(displacement, 1.0 / moveTime)
    local endTime = Time.FutureSystemTime(moveTime)

    local posVel = self.Components.Kinematics
    posVel.Velocity = velocity
    posVel.StopSystemTime = endTime
    self.Components.Kinematics = posVel
end

-------------------------------------

--- Tries to move the entity in a straight line to the given position.
--- @param position Vector3 Target position.
--- @param speed number Magnitude of velocity in blocks per second.
function Entity:MoveTo(position, speed)
    local posVel = self.Components.Kinematics
    local displacement = Vectors.Vector3.Difference(position, posVel.Position)
    self:MoveBy(displacement, speed)
end

-------------------------------------

return {
    Entity = Entity,
    Get = Entity.Get
}
