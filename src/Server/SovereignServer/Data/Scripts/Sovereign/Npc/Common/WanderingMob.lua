-- WanderingMob.lua
-- Generic behavior script for wandering mobs.
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

------------------------------------------
-- Constants                            --
------------------------------------------

--- Entity data key for the WanderStep parameter.
local ParamWanderStep = "Sovereign.WanderingMob.WanderStep"

--- Entity data key for the WanderDelay parameter.
local ParamWanderDelay = "Sovereign.WanderingMob.WanderDelay"

--- Entity data key for the WanderSpeed parameter.
local ParamWanderSpeed = "Sovereign.WanderingMob.WanderSpeed"

--- Relative amount to vary the delay in either direction.
local DelayRandomScale = 0.1


------------------------------------------
-- Behavior                             --
------------------------------------------

--- Encapsulates the core behaviors so that they don't appear in the NPC editor.
---@class WanderingMob
local WanderingMob = {}


---Entity load hook that begins the wandering behavior.
---@param entityId integer # Loaded entity ID.
function WanderingMob.LoadParams(entityId)
    -- Load per-entity parameters.
    local entityData = data.GetEntityData(entityId)
    local wanderStep = tonumber(entityData[ParamWanderStep])
    local wanderDelay = tonumber(entityData[ParamWanderDelay])
    local wanderSpeed = tonumber(entityData[ParamWanderSpeed])

    -- Validate per-entity parameters.
    local paramsValid = true
    if not wanderStep then
        util.LogError(string.format("Entity %A requires parameter %s of type number.", entityId, ParamWanderStep))
        paramsValid = false
    end
    if not wanderDelay then
        util.LogError(string.format("Entity %A requires parameter %s of type number.", entityId, ParamWanderDelay))
        paramsValid = false
    end
    if not wanderSpeed then
        util.LogError(string.format("Entity %A requires parameter %s of type number.", entityId, ParamWanderSpeed))
        paramsValid = false
    end
    if not paramsValid then
        return false
    end

    if wanderStep <= 0 then
        util.LogError(string.format("Entity %A has invalid wander step.", entityId))
        paramsValid = false
    end
    if wanderDelay < 0 then
        util.LogError(string.format("Entity %A has invalid wander delay.", entityId))
        paramsValid = false
    end
    if wanderSpeed <= 0 then
        util.LogError(string.format("Entity %A has invalid wander speed.", entityId))
        paramsValid = false
    end
    if not paramsValid then
        return false
    end

    -- Create and start a new coroutine for the new entity.
    local wanderTime = wanderStep / wanderSpeed
    return true, wanderTime, wanderDelay, wanderSpeed
end

--- Main coroutine for single entity behavior.
--- @async
--- @param behavior table # Behavior object.
--- @param entityId integer # Entity ID.
function WanderingMob.RunAsync(behavior, entityId)
    local ok, wanderTime, wanderDelay, wanderSpeed = WanderingMob.LoadParams(entityId)
    if not ok then return end

    while true do
        -- Wait until time for the next movement.
        local nextDelay = wanderDelay + (math.random() - 0.5) * DelayRandomScale
        behavior:WaitAsync(entityId, nextDelay)

        -- Get current position and velocity.
        local posVel = components.kinematics.Get(entityId)
        if posVel == nil then
            util.LogError(string.format("No Kinematics data for entity %A.", entityId))
            return
        end

        -- Pick random movement direction, then set up a velocity vector.
        -- Leave Z component alone to respect gravity.
        local dx, dy = WanderingMob.RandomDirection()
        posVel.Velocity = {X = dx * wanderSpeed, Y = dy * wanderSpeed, Z = posVel.Velocity.Z}

        -- Set entity in motion, then wait for movement to complete.
        components.kinematics.Set(entityId, posVel)
        behavior:WaitAsync(entityId, wanderTime)

        -- Movement complete, so stop motion (leave Z axis alone for gravity).
        posVel = components.kinematics.Get(entityId)
        if posVel == nil then
            util.LogError(string.format("No Kinematics data for entity %A.", entityId))
            return
        end

        posVel.Velocity.X = 0.0
        posVel.Velocity.Y = 0.0
        components.kinematics.Set(entityId, posVel)
    end
end

WanderingMob.xstep = {-1.0, 1.0,  0.0, 0.0}
WanderingMob.ystep = { 0.0, 0.0, -1.0, 1.0}

--- Selects a random direction and returns a (x,y) vector.
--- @return number dx # X step
--- @return number dy # Y step
function WanderingMob.RandomDirection()
    local direction = math.random(4)
    return WanderingMob.xstep[direction], WanderingMob.ystep[direction]
end


------------------------------------------
-- Behavior Framework Class             --
------------------------------------------

--- Wraps an asynchronous function into a per-entity behavior with life cycle hooks.
---@class EntityBehavior
---@field private _main fun(behavior: table, entityId: integer, ...: any) Behavior function.
---@field private _coroutines table Coroutines indexed by entity ID.
---@field private _startArgs any[] Additional arguments to pass to behavior function at start.
EntityBehavior = {}
EntityBehavior.__index = EntityBehavior
setmetatable(EntityBehavior, EntityBehavior)

--- Wraps an asynchronous function into a per-entity behavior.
--- @param f fun(behavior: table, entityId: integer, ...: any) # Behavior function.
--- @return table # Behavior object.
function EntityBehavior.Create(f, ...)
    local obj = {}
    setmetatable(obj, EntityBehavior)

    obj._main = f
    obj._coroutines = {}
    obj._startArgs = {...}

    return obj
end

--- Entity load life cycle hook. Starts behavior for newly loaded entity.
--- @param entityId integer # Entity ID.
function EntityBehavior:OnLoad(entityId)
    if self._coroutines[entityId] ~= nil then
        util.LogWarn(string.format("Behavior for entity %A already exists; replacing.", entityId))
        coroutine.close(self._coroutines[entityId])
        self._coroutines[entityId] = nil
    end

    local thread = coroutine.create(self._main)
    self._coroutines[entityId] = thread
    local ok, err = coroutine.resume(thread, self, entityId, table.unpack(self._startArgs))
    if not ok then
        util.LogError(string.format("Failed to add behavior for %A: %s", entityId, err))
    end
end

--- Entity unload life cycle hook. Stops behavior for unloaded entity and frees resources.
--- @param entityId integer # Entity ID.
function EntityBehavior:OnUnload(entityId)
    local thread = self._coroutines[entityId]
    if thread == nil then
        util.LogError(string.format("Cannot unload behavior for entity %A without loading behavior first.", entityId))
        return
    end

    coroutine.close(thread)
    self._coroutines[entityId] = nil
end

--- Resumes the behavior for the given entity.
--- @param entityId integer # Entity ID.
--- @param ... any # Additional parameters to pass back to the coroutine.
function EntityBehavior:Resume(entityId, ...)
    local thread = self._coroutines[entityId]
    if thread == nil then
        util.LogError(string.format("Error resuming behavior for entity %A: behavior not loaded.", entityId))
        return
    end

    local ok, err = coroutine.resume(thread, ...)
    if not ok then
        util.LogError(string.format("Error resuming behavior for entity %A: %s", entityId, err))
    end
end

--- Installs entity life cycle hooks to the global scope.
--- @param prefix string? # Optional prefix to prepend hook names with.
function EntityBehavior:InstallGlobalHooks(prefix)
    local loadName = (prefix == nil and "" or prefix .. "_") .. "OnLoad"
    local unloadName = (prefix == nil and "" or prefix .. "_") .. "OnUnload"

    _G[loadName] = function(entityId) self:OnLoad(entityId) end
    _G[unloadName] = function(entityId) self:OnUnload(entityId) end
end

--- Waits for a period of time before continuing.
--- @async
--- @param entityId integer # Entity ID.
--- @param delaySeconds number # Time in seconds to wait.
function EntityBehavior:WaitAsync(entityId, delaySeconds)
    scripting.AddTimedCallback(delaySeconds, function (entityId) self:Resume(entityId) end, entityId)
    coroutine.yield()
end


------------------------------------------
-- Export Public API to NPC Editor      --
------------------------------------------

EntityBehavior.Create(WanderingMob.RunAsync):InstallGlobalHooks()

scripting.AddEntityParameterHint("OnLoad", ParamWanderStep)
scripting.AddEntityParameterHint("OnLoad", ParamWanderDelay)
scripting.AddEntityParameterHint("OnLoad", ParamWanderSpeed)
