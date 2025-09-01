-- Wander.lua
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

local EntityBehavior = require("EntityBehavior")

------------------------------------------
-- Constants                            --
------------------------------------------

--- Entity data key for the WanderStep parameter.
local ParamWanderStep = "Sovereign.Wander.WanderStep"

--- Entity data key for the WanderDelay parameter.
local ParamWanderDelay = "Sovereign.Wander.WanderDelay"

--- Entity data key for the WanderSpeed parameter.
local ParamWanderSpeed = "Sovereign.Wander.WanderSpeed"

--- Relative amount to vary the delay in either direction.
local DelayRandomScale = 0.1


------------------------------------------
-- Behavior                             --
------------------------------------------

--- Encapsulates the core behaviors so that they don't appear in the NPC editor.
---@class Wander
local Wander = {}

---Entity load hook that begins the wandering behavior.
---@param entityId integer # Loaded entity ID.
function Wander.LoadParams(entityId)
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
function Wander.RunAsync(behavior, entityId)
    local ok, wanderTime, wanderDelay, wanderSpeed = Wander.LoadParams(entityId)
    if not ok then return end

    -- Start at a random time within the movement period.
    -- This decorrelates the motion between entities that are loaded at the same time.
    behavior:WaitAsync(entityId, math.random() * wanderDelay)

    while true do
        -- Wait until time for the next movement.
        local nextDelay = wanderDelay + (math.random() - 0.5) * DelayRandomScale * wanderDelay
        behavior:WaitAsync(entityId, nextDelay)

        -- Get current position and velocity.
        local posVel = components.kinematics.Get(entityId)
        if posVel == nil then
            util.LogError(string.format("No Kinematics data for entity %A.", entityId))
            return
        end

        -- Pick random movement direction, then set up a velocity vector.
        -- Leave Z component alone to respect gravity.
        local dx, dy = Wander.RandomDirection()
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

Wander.xstep = {-1.0, 1.0,  0.0, 0.0}
Wander.ystep = { 0.0, 0.0, -1.0, 1.0}

--- Selects a random direction and returns a (x,y) vector.
--- @return number dx # X step
--- @return number dy # Y step
function Wander.RandomDirection()
    local direction = math.random(4)
    return Wander.xstep[direction], Wander.ystep[direction]
end


------------------------------------------
-- Export Public API to NPC Editor      --
------------------------------------------

EntityBehavior.Create(Wander.RunAsync):InstallGlobalHooks()

scripting.AddEntityParameterHint("OnLoad", ParamWanderStep, "Float")
scripting.AddEntityParameterHint("OnLoad", ParamWanderDelay, "Float")
scripting.AddEntityParameterHint("OnLoad", ParamWanderSpeed, "Float")
