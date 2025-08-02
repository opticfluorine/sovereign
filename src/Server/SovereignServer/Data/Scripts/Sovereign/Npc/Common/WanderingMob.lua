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
-- Script Globals                       --
------------------------------------------

--- Table of coroutines per entity.
local entityThreads = {}

--- Encapsulates the core behaviors so that they don't appear in the NPC editor.
---@class WanderingMob
local WanderingMob = {}


------------------------------------------
-- Callbacks                            --
------------------------------------------

---Entity load hook that begins the wandering behavior.
---@param entityId integer # Loaded entity ID.
function OnLoad(entityId)
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
        return
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
        return
    end

    -- Create and start a new coroutine for the new entity.
    local wanderTime = wanderStep / wanderSpeed
    local thread = coroutine.create(WanderingMob.RunAsync)
    entityThreads[entityId] = thread
    local ok, err = coroutine.resume(thread, entityId, wanderTime, wanderDelay, wanderSpeed)
    if not ok then
        util.LogError(string.format("Error starting behavior for entity %A: %s", entityId, err))
        entityThreads[entityId] = nil
        coroutine.close(thread)
    end
end

--- Entity unload hook.
--- @param entityId integer # Unloaded entity ID.
function OnUnload(entityId)
    local thread = entityThreads[entityId]
    if thread == nil then
        util.LogWarn(string.format("Unexpected unload hook for entity %A.", entityId))
        return
    end

    entityThreads[entityId] = nil
    coroutine.close(thread)
end


------------------------------------------
-- Behavior Functions                   --
------------------------------------------

--- Timed callback that resumes the coroutine for an entity.
--- @param entityId integer # Entity whose behavior is to be resumed.
function WanderingMob.Continue(entityId)
    local thread = entityThreads[entityId]
    if thread == nil then
        return
    end

    -- If entity was unloaded, terminate the coroutine.
    if not components.kinematics.Exists(entityId) then
        entityThreads[entityId] = nil
        coroutine.close(thread)
        return
    end

    -- Resume coroutine.
    local ok, err = coroutine.resume(thread)
    if not ok then
        util.LogError(string.format("Error continuing behavior for entity %A: %s", entityId, err))
        entityThreads[entityId] = nil
        coroutine.close(thread)
    end
end


--- Main coroutine for single entity behavior.
--- @async
--- @param entityId integer # Entity ID.
--- @param wanderTime number # Time in seconds that entity moves.
--- @param wanderDelay number # Mean time in seconds between steps.
--- @param wanderSpeed number # Speed of moving entity in world units per second.
function WanderingMob.RunAsync(entityId, wanderTime, wanderDelay, wanderSpeed)
    while true do
        -- Wait until time for the next movement.
        local nextDelay = wanderDelay + (math.random() - 0.5) * DelayRandomScale
        scripting.AddTimedCallback(nextDelay, WanderingMob.Continue, entityId)
        coroutine.yield()

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
        scripting.AddTimedCallback(wanderTime, WanderingMob.Continue, entityId)
        coroutine.yield()

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


------------------------------------------
-- Utility Functions                    --
------------------------------------------

local xstep = {-1.0, 1.0,  0.0, 0.0}
local ystep = { 0.0, 0.0, -1.0, 1.0}

--- Selects a random direction and returns a (x,y) vector.
--- @return number dx # X step
--- @return number dy # Y step
function WanderingMob.RandomDirection()
    local direction = math.random(4)
    return xstep[direction], ystep[direction]
end


------------------------------------------
-- Export Public API to NPC Editor      --
------------------------------------------

scripting.AddEntityParameterHint("OnLoad", ParamWanderStep)
scripting.AddEntityParameterHint("OnLoad", ParamWanderDelay)
scripting.AddEntityParameterHint("OnLoad", ParamWanderSpeed)

util.LogDebug("WanderingMob script loaded.")
